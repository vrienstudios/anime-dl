import abstract_clipboard
import winlean, sets, os, strscans, strutils

{.pragma: user32, stdcall, dynlib: "user32" .}
{.pragma: kernel32, stdcall, dynlib: "kernel32" .}

type
  LPVOID = pointer
  UINT = cuint

const
  GMEM_MOVEABLE: UINT = 0x0002
  CF_UNICODETEXT_FORMAT_NAME = "_H:000D"

proc openClipboard(hwnd: Handle = 0): WINBOOL {.user32, importc: "OpenClipboard".}
proc closeClipboard(): WINBOOL {.user32, importc: "CloseClipboard".}
proc getClipboardData(uFormat: UINT):Handle {.user32, importc: "GetClipboardData".}
proc setClipboardData(uFormat: UINT, hMem: Handle = 0): Handle {.user32, importc: "SetClipboardData".}
proc emptyClipboard(): WINBOOL {.user32, importc: "EmptyClipboard".}
proc enumClipboardFormats(format: UINT): UINT {.user32, importc: "EnumClipboardFormats".}
proc registerClipboardFormat(lpszFormat: cstring): UINT {.user32, importc: "RegisterClipboardFormatA".}
proc getClipboardFormatName(uFormat: UINT, lpszFormatName: cstring, cchMaxCount: cint): cint {.user32, importc: "GetClipboardFormatNameA".}
proc globalAlloc(uFlags: UINT, dwBytes: csize): Handle {.kernel32, importc: "GlobalAlloc".}
proc globalFree(hMem: Handle): Handle {.kernel32, importc: "GlobalFree".}
proc globalSize(hMem: Handle): csize {.kernel32, importc: "GlobalSize".}
proc globalLock(hMem: Handle): LPVOID {.kernel32, importc: "GlobalLock".}
proc globalUnlock(hMem: Handle): WINBOOL {.kernel32, importc: "GlobalUnlock".}

proc `*`(b: SomeOrdinal): bool = result = b != 0

proc error()=
  raiseOSError(getLastError().OSErrorCode)

proc getClipboardFormatByName(str: string): UINT =
  var v: int
  if scanf(str, "_H:$h", v):
    result = UINT(v)
  else:
    result = registerClipboardFormat(str)

type WindowsClipboard = ref object of Clipboard

proc globalWithData(data: seq[byte]): Handle =
  result = globalAlloc(GMEM_MOVEABLE, data.len)
  if result != 0:
    if data.len != 0:
      let pBuf = globalLock(result)
      if not pBuf.isNil:
        copyMem(pBuf, unsafeAddr data[0], data.len)
        discard globalUnlock(result)
      else:
        discard globalFree(result)
        result = 0

proc copyDataToClipboard(dataType: string, data: seq[byte]) =
  let fKind = getClipboardFormatByName(dataType)
  if fKind == 0: return
  if dataType.startsWith("_H:"):
    assert(data.len == sizeof(Handle))
    var h = cast[ptr Handle](unsafeAddr data[0])[]
    if h != 0:
      discard setClipboardData(fKind, h)
  else:
    var glmem = globalWithData(data)
    if glmem != 0:
      if setClipboardData(fKind, glmem) != 0:
        glmem = 0

    if glmem != 0:
      discard globalFree(glmem)

proc pbWrite(pb: Clipboard, dataType: string, data: seq[byte])=
  if *openClipboard() and *emptyClipboard():
    copyDataToClipboard(dataType, data)
    var buf: seq[byte]
    for f in conversionsFromType(dataType):
      buf.setLen(0)
      if convertData(dataType, f, data, buf):
        copyDataToClipboard(f, buf)
    discard closeClipboard()

  else:
    error()

proc getClipboardFormatName(fmt: UINT): string =
  # According to https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerclipboardformata
  # Registered clipboard formats are identified by values in the range 0xC000 through 0xFFFF.
  if fmt < 0xC000:
    result = "_H:" & toHex(fmt.int, 4)
  else:
    var data: array[512, char]
    let sz = getClipboardFormatName(fmt, cstring(addr data[0]), sizeof(data).cint)
    if sz > 0:
      result = newString(sz)
      copyMem(addr result[0], addr data[0], sz)

proc getClipboardAvailableFormats(): HashSet[string] =
  var fmt = 0.UINT
  while true:
    fmt = enumClipboardFormats(fmt)
    if fmt == 0: break
    let n = getClipboardFormatName(fmt)
    if n.len != 0:
      result.incl(n)

proc bestFormat(origFormat: string, availableFormats: HashSet[string]): string =
  if origFormat in availableFormats:
    result = origFormat
  else:
    var conversions = conversionsToType(origFormat).toHashSet()
    let supportedFormats = intersection(availableFormats, conversions)
    if supportedFormats.len == 0: return

    for f in [CF_UNICODETEXT_FORMAT_NAME]:
      if f in supportedFormats:
        result = f
        break

    if result.len == 0:
      for f in supportedFormats:
        result = f
        break

proc pbRead(pb: Clipboard, dataType: string, output: var seq[byte]): bool =
  if *openClipboard():
    let requestDataType = bestFormat(dataType, getClipboardAvailableformats())
    if requestDataType.len != 0:
      let fKind = getClipboardFormatByName(requestDataType)
      let h = getClipboardData(fKind)
      if requestDataType.startsWith("_H:"):
        if h != 0:
          var buf = newSeq[byte](sizeof(h))
          cast[ptr Handle](addr buf[0])[] = h
          result = convertData(requestDataType, dataType, buf, output)
      elif h != 0:
        let lpstr = globalLock(h)
        if not lpstr.isNil:
          let sz = globalSize(h)
          var buf = newSeq[byte](sz)
          if sz != 0:
            copyMem(addr buf[0], lpstr, sz)
          result = convertData(requestDataType, dataType, buf, output)
        discard globalUnlock(h)
      discard closeClipboard()

  else:
    error()

proc pbAvailableFormats(pb: Clipboard): seq[string] =
  if *openClipboard():
    let fmts = getClipboardAvailableFormats()
    discard closeClipboard()
    var ownFormats = initHashSet[string]()
    for f in fmts:
      let conv = conversionsFromType(f)
      ownFormats.incl(conv.toHashSet())
    ownFormats.incl(fmts)
    for f in ownFormats:
      result.add(f)

proc clipboardWithName*(name: string): Clipboard =
  var res = new(WindowsClipboard)
  res.writeImpl = pbWrite
  res.readImpl = pbRead
  res.availableFormatsImpl = pbAvailableFormats
  result = res

proc multiByteToWideChar(
  codePage: UINT,
  dwFlags: DWORD,
  lpMultiByteStr: ptr byte,
  cbMultiByte: cint,
  lpWideCharStr: ptr byte,
  cchWideChar: cint): cint {.
    stdcall, importc: "MultiByteToWideChar", dynlib: "kernel32".}

proc wideCharToMultiByte(
  codePage: UINT,
  dwFlags: DWORD,
  lpWideCharStr: pointer,
  cchWideChar: cint,
  lpMultiByteStr: pointer,
  cbMultiByte: cint,
  lpDefaultChar: cstring = nil,
  lpUsedDefaultChar: pointer = nil): cint {.
    stdcall, importc: "WideCharToMultiByte", dynlib: "kernel32".}

proc wideToUTF8(fromType, toType: string, data: seq[byte]): seq[byte] =
  const cp = 65001 # UTF8 codepage
  assert(data.len == sizeof(Handle))
  let h = cast[ptr Handle](unsafeAddr data[0])[]
  if h != 0:
    let sz = globalSize(h)
    if sz > 0:
      let buf = globalLock(h)
      if buf != nil:
        let numChars = (sz div 2).cint
        let usz = wideCharToMultiByte(cp, 0, buf, numChars, nil, 0) + 1
        result.setLen(usz)
        discard wideCharToMultiByte(cp, 0, buf, numChars, addr result[0], usz)
        discard globalUnlock(h)

proc utf8ToWide(fromType, toType: string, data: seq[byte]): seq[byte] =
  const cp = 65001 # UTF8 codepage
  var wideStr: seq[byte]
  let dataLen = data.len.cint
  if dataLen != 0:
    let sizeInChars = multiByteToWideChar(cp, 0, unsafeAddr data[0], dataLen, nil, 0) + 1
    let sizeInBytes = sizeInChars * 2
    wideStr.setLen(sizeInBytes)
    discard multiByteToWideChar(cp, 0, unsafeAddr data[0], dataLen, addr wideStr[0], sizeInChars)

  let h = globalWithData(wideStr)
  result.setLen(sizeof(h))
  cast[ptr Handle](addr result[0])[] = h

registerTypeConversion("text/plain", CF_UNICODETEXT_FORMAT_NAME, utf8ToWide)
registerTypeConversion(CF_UNICODETEXT_FORMAT_NAME, "text/plain", wideToUTF8)
