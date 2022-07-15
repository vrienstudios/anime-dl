import sets, strutils
import abstract_clipboard

import darwin/app_kit

type MacClipboard = ref object of Clipboard
  p: NSPasteboard

const NonUtiPrefix = "nim-clip."

proc finalizePboard(p: MacClipboard) = p.p.release()

proc nativePboardName(n: string): NSString =
  case n
  of CboardGeneral: result = NSPasteboardNameGeneral
  of CboardFont: result = NSPasteboardNameFont
  of CboardRuler: result = NSPasteboardNameRuler
  of CboardFind: result = NSPasteboardNameFind
  of CboardDrag: result = NSPasteboardNameDrag
  else: result = n

proc toUti(s: string): string =
  if '.' notin s or '/' in s:
    result = NonUtiPrefix
    for c in s:
      case c
      of '.': result &= ".d"
      of '/': result &= ".s"
      of ';': result &= ".c"
      of '&': result &= ".a"
      of '?': result &= ".q"
      of '!': result &= ".b"
      of '_': result &= ".u"
      else: result &= c
  else:
    result = s

proc fromUti(s: string): string =
  if s.startsWith(NonUtiPrefix):
    var i = NonUtiPrefix.len
    while i < s.len:
      if s[i] == '.':
        inc i
        if i < s.len:
          case s[i]
          of 'd': result &= '.'
          of 's': result &= '/'
          of 'c': result &= ';'
          of 'a': result &= '&'
          of 'q': result &= '?'
          of 'b': result &= '!'
          of 'u': result &= '_'
          else: result &= s[i]
        else:
          result &= '.'
      else:
        result &= s[i]
      inc i
  else:
    result = s

proc setDataForType(pi: NSPasteboardItem, data: seq[byte], dataType: string) =
  let utiDataType = toUti(dataType)
  let nsdata = NSData.withBytes(data)
  discard pi.setDataForType(nsdata, utiDataType.toNSString())

proc pbWrite(pb: Clipboard, dataType: string, data: seq[byte]) =
  let pb = MacClipboard(pb)
  pb.p.clearContents()
  let npi = NSPasteboardItem.alloc().init()
  npi.setDataForType(data, dataType)
  var buf: seq[byte]
  for f in conversionsFromType(dataType):
    buf.setLen(0)
    if convertData(dataType, f, data, buf):
      npi.setDataForType(buf, f)
  pb.p.writeObjects(arrayWithObjects(npi))
  npi.release()

proc getAvailableFormats(pi: NSPasteboardItem): HashSet[string] =
  let typs = pi.`types`()
  if not typs.isNil:
    for t in typs:
      result.incl(fromUti($t))

proc getFirstItem(pb: MacClipboard): NSPasteboardItem =
  let its = pb.p.pasteboardItems
  if not its.isNil and its.len != 0:
    result = its[0]

proc getAvailableFormats(pb: MacClipboard): HashSet[string] =
  let pi = pb.getFirstItem()
  if not pi.isNil:
    result = getAvailableFormats(pi)

proc bestFormat(origFormat: string, availableFormats: HashSet[string]): string =
  if origFormat in availableFormats:
    result = origFormat
  else:
    var conversions = conversionsToType(origFormat).toHashSet()
    let supportedFormats = intersection(availableFormats, conversions)
    if supportedFormats.len == 0: return

    for f in [$NSPasteboardTypeString]:
      if f in supportedFormats:
        result = f
        break

    if result.len == 0:
      for f in supportedFormats:
        result = f
        break

proc pbRead(pb: Clipboard, dataType: string, output: var seq[byte]): bool =
  let pb = MacClipboard(pb)
  let pi = pb.getFirstItem()
  if not pi.isNil:
    let requestDataType = bestFormat(dataType, pi.getAvailableformats())
    if requestDataType.len != 0:
      let d = pi.dataForType(toUti(requestDataType).toNSString)
      if not d.isNil:
        let sz = d.len
        var buf = newSeq[byte](sz)
        if sz != 0:
          d.getBytes(addr buf[0], sz)
        result = convertData(requestDataType, dataType, buf, output)

proc pbAvailableFormats(pb: Clipboard): seq[string] =
  let pb = MacClipboard(pb)
  let fmts = pb.getAvailableFormats()
  var ownFormats = initHashSet[string]()
  for f in fmts:
    let conv = conversionsFromType(f)
    ownFormats.incl(conv.toHashSet())
  ownFormats.incl(fmts)
  for f in ownFormats:
    result.add(f)

proc clipboardWithName*(name: string): Clipboard =
  var res: MacClipboard
  res.new(finalizePboard)
  res.p = NSPasteboard.withName(nativePboardName(name)).retain()
  res.writeImpl = pbWrite
  res.readImpl = pbRead
  res.availableFormatsImpl = pbAvailableFormats
  res

proc noconv(fromType, toType: string, data: seq[byte]): seq[byte] = data

proc reg =
  let ns = $NSPasteboardTypeString
  registerTypeConversion([
    ("text/plain", ns),
    (ns, "text/plain")], noconv)

reg()
