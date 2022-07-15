import asyncdispatch, sets
import abstract_clipboard
import x11/[xlib, x, xatom]

# Good read: https://www.uninformativ.de/blog/postings/2017-04-02/0/POSTING-en.html

type X11Clipboard = ref object of Clipboard
  display: PDisplay
  window: Window
  utf8Atom: Atom
  clipboardAtom: Atom
  myPropertyName: Atom
  targetsAtom: Atom
  incrAtom: Atom
  dataType: string
  data: seq[byte]
  dataTargets: seq[uint32]

const maxXPropLen = 1024 * 1024 div 4 # Max length in 32-bit multiples

proc pbWrite(pb: Clipboard, dataType: string, data: seq[byte]) =
  let pb = X11Clipboard(pb)
  pb.dataType = dataType
  pb.data = data
  pb.dataTargets.setLen(0)

  discard XSetSelectionOwner(pb.display, pb.clipboardAtom, pb.window, CurrentTime)
  discard XFlush(pb.display)

proc getAtomName(d: PDisplay, a: Atom): string =
  let s = XGetAtomName(d, a)
  result = $s
  discard XFree(s)

proc getAtomNames(pb: X11Clipboard, atoms: openarray[Atom], output: var HashSet[string]) =
  for a in atoms:
    if a != pb.targetsAtom:
      output.incl(getAtomName(pb.display, a))

proc getXAvailableFormats(pb: X11Clipboard): HashSet[string] =
  let display = pb.display
  let r = XConvertSelection(display, pb.clipboardAtom, pb.targetsAtom, pb.myPropertyName, pb.window, CurrentTime)

  var selType: TAtom
  var selFormat: cint
  var nitems: culong = 0
  var overflow: culong = 0
  var src: PAtom

  var ev: XEvent
  discard XNextEvent(display, addr ev)

  if XGetWindowProperty(display, pb.window, pb.myPropertyName, 0,
      maxXPropLen, false.XBool, XA_ATOM, addr selType,
      addr selFormat, addr nitems, addr overflow, cast[PPcuchar](addr src)) == Success:
    if selType == XA_ATOM:
      pb.getAtomNames(toOpenArray(cast[ptr UncheckedArray[Atom]](src), 0, nitems.int - 1), result)
    discard XFree(src)
  discard XDeleteProperty(display, pb.window, pb.myPropertyName)

proc bestFormat(origFormat: string, availableFormats: HashSet[string]): string =
  if origFormat in availableFormats:
    result = origFormat
  else:
    var conversions = conversionsToType(origFormat).toHashSet()
    let supportedFormats = intersection(availableFormats, conversions)
    if supportedFormats.len == 0: return

    for f in ["UTF8_STRING"]:
      if f in supportedFormats:
        result = f
        break

    if result.len == 0:
      for f in supportedFormats:
        result = f
        break

proc pbRead(pb: Clipboard, dataType: string, output: var seq[byte]): bool =
  let pb = X11Clipboard(pb)
  let display = pb.display
  if pb.display.isNil: return

  if XGetSelectionOwner(display, pb.clipboardAtom) == pb.window:
    if dataType == pb.dataType:
      output = pb.data
      result = true
    else:
      result = convertData(pb.dataType, dataType, pb.data, output)
    return

  let requestDataType = bestFormat(dataType, pb.getXAvailableFormats())
  if requestDataType.len == 0: return

  let format = XInternAtom(display, requestDataType, 0)
  let r = XConvertSelection(display, pb.clipboardAtom, format, pb.myPropertyName, pb.window, CurrentTime)

  var selType: TAtom
  var selFormat: cint
  var nitems: culong = 0
  var overflow: culong = 0
  var src: pointer

  var ev: XEvent
  discard XNextEvent(display, addr ev)

  if XGetWindowProperty(display, pb.window, pb.myPropertyName, 0.clong,
      maxXPropLen, false.XBool, format, addr selType,
      addr selFormat, addr nitems, addr overflow, cast[PPcuchar](addr src)) == Success:
    if overflow != 0:
      echo "OVERFLOW NOT IMPLEMENTED: ", overflow
    if selType == pb.incrAtom:
      echo "INCR NOT IMPLEMENTED"
    if selType == format:
      var data = newSeq[byte](nitems)
      if nitems != 0:
        copyMem(addr data[0], src, nitems)
      result = convertData(requestDataType, dataType, data, output)
    discard XFree(src)
  discard XDeleteProperty(display, pb.window, pb.myPropertyName)

proc getTargets(pb: X11Clipboard, output: var seq[uint32]) =
  template addTarget(a: Atom) =
    output.add(uint32(a))

  template addTarget(s: string) =
    addTarget(XInternAtom(pb.display, s, 0))

  addTarget(pb.dataType)
  let c = conversionsFromType(pb.dataType)
  for i in c:
    addTarget(i)

  addTarget(pb.targetsAtom)

proc sendSelectionNotify(request: ptr XSelectionRequestEvent, property: Atom) =
  var ssev: XSelectionEvent
  ssev.theType = SelectionNotify
  ssev.requestor = request.requestor
  ssev.selection = request.selection
  ssev.target = request.target
  ssev.property = property
  ssev.time = request.time
  discard XSendEvent(request.display, request.requestor, true.XBool, NoEventMask, cast[PXEvent](addr ssev))

proc onXEvent(pb: X11Clipboard, ev: PXEvent) =
  let display = pb.display
  case ev.theType
  of SelectionClear:
    pb.data = @[]
    pb.dataType = ""
    pb.dataTargets.setLen(0)
    # echo "CLEAR"
  of SelectionRequest:
    let e = addr ev.xselectionRequest
    if e.target == pb.targetsAtom:
      # Reply with TARGETS
      if pb.dataTargets.len == 0:
        pb.getTargets(pb.dataTargets)
      # for t in pb.dataTargets:
      #   echo "fmt: ", getAtomName(e.display, t)
      discard XChangeProperty(e.display, e.requestor, e.property, e.target, 32, PropModeReplace, cast[Pcuchar](addr pb.dataTargets[0]), pb.dataTargets.len.cint)
      sendSelectionNotify(e, e.property)
    else:
      let targetFormat = getAtomName(e.display, e.target)
      var data: seq[byte]
      if convertData(pb.dataType, targetFormat, pb.data, data):
        # Send data
        let sz = data.len.cint
        let pdata = if sz != 0: addr data[0] else: nil
        discard XChangeProperty(e.display, e.requestor, e.property, e.target, 8, PropModeReplace, cast[Pcuchar](pdata), sz)
        sendSelectionNotify(e, e.property)
      else:
        # Send none
        sendSelectionNotify(e, None)
    discard XFlush(display)
  else:
    discard

proc onSocket(pb: X11Clipboard): bool =
  var ev: XEvent
  let display = pb.display
  while XPending(display) != 0:
    discard XNextEvent(display, addr ev)
    onXEvent(pb, addr ev)

proc pbFormats(pb: Clipboard): seq[string] =
  let pb = X11Clipboard(pb)
  let display = pb.display
  if pb.display.isNil: return

  if XGetSelectionOwner(display, pb.clipboardAtom) == pb.window:
    result = conversionsFromType(pb.dataType)
    result.add(pb.dataType)
    return

  let fmts = pb.getXAvailableFormats()
  var ownFormats = initHashSet[string]()
  for f in fmts:
    let conv = conversionsFromType(f)
    ownFormats.incl(conv.toHashSet())

  ownFormats.incl(fmts)
  for f in ownFormats:
    result.add(f)

proc registerSocket(pb: X11Clipboard) =
  let fd = XConnectionNumber(pb.display).AsyncFd
  register(fd)
  addRead(fd) do(fd: AsyncFD) -> bool:
    onSocket(pb)

proc newX11Clipboard(): X11Clipboard =
  let r = X11Clipboard()
  r.display = XOpenDisplay(nil)
  if not r.display.isNil:
    r.window = XCreateSimpleWindow(r.display, DefaultRootWindow(r.display), 0, 0, 1, 1, 0, 0, 0)
    r.utf8Atom = XInternAtom(r.display, "UTF8_STRING", 0)
    r.clipboardAtom = XInternAtom(r.display, "CLIPBOARD", 0)
    r.myPropertyName = XInternAtom(r.display, "nimclip", 0)
    r.targetsAtom = XInternAtom(r.display, "TARGETS", 0)
    r.incrAtom = XInternAtom(r.display, "INCR", 0)
  r.readImpl = pbRead
  r.writeImpl = pbWrite
  r.availableFormatsImpl = pbFormats

  discard XFlush(r.display)
  registerSocket(r)

  r

var gClipboard: Clipboard

proc clipboardWithName*(name: string): Clipboard =
  if gClipboard.isNil:
    gClipboard = newX11Clipboard()
  gClipboard

proc noConvert(fromType, toType: string, data: seq[byte]): seq[byte] = data

registerTypeConversion([
  ("text/plain", "UTF8_STRING"), ("UTF8_STRING", "text/plain"),
  ("text/plain", "STRING"), ("STRING", "text/plain"),
  ("text/plain", "text/plain;charset=utf-8"), ("text/plain;charset=utf-8", "text/plain"),
  ], noConvert)
