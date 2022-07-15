## Clipboards
## Writing a string to clipboard
## let p = clipboardWithName(PboardGeneral)
## p.writeString("Hello, world!")
##
## Reading a string
## var myString: string
## if p.readString(myString):
##   echo "Got string: ", myString

import ./type_conversions

type
  Clipboard* {.inheritable.} = ref object
    writeImpl*: proc(pb: Clipboard, dataType: string, data: seq[byte]) {.nimcall.}
    readImpl*: proc(pb: Clipboard, dataType: string, output: var seq[byte]): bool {.nimcall.}
    availableFormatsImpl*: proc(pb: Clipboard): seq[string] {.nimcall.}

const
  CboardGeneral* = "__CboardGeneral"
  CboardFont* = "__CboardFont"
  CboardRuler* = "__CboardRuler"
  CboardFind* = "__CboardFind"
  CboardDrag* = "__CboardDrag"

var typeConverter: TypeConverter

proc registerTypeConversion*(typePairs: openarray[tuple[fromType, toType: string]], convert: proc(fromType, toType: string, data: seq[byte]): seq[byte] {.nimcall, gcsafe.}) =
  typeConverter.registerTypeConversion(typePairs, convert)

proc registerTypeConversion*(fromType, toType: string, convert: proc(fromType, toType: string, data: seq[byte]): seq[byte] {.nimcall, gcsafe.}) =
  registerTypeConversion([(fromType, toType)], convert)

proc conversionsFromType*(fromType: string): seq[string] =
  {.gcsafe.}:
    typeConverter.conversionsFromType(fromType)

proc conversionsToType*(toType: string): seq[string] =
  {.gcsafe.}:
    typeConverter.conversionsToType(toType)

proc convertData*(fromType, toType: string, data: seq[byte], output: var seq[byte]): bool =
  if fromType == toType:
    output = data
    result = true
  else:
    {.gcsafe.}:
      result = typeConverter.convertData(fromType, toType, data, output)

proc writeData*(pb: Clipboard, dataType: string, data: seq[byte]) {.inline.} =
  assert(not pb.writeImpl.isNil)
  pb.writeImpl(pb, dataType, data)

proc readData*(pb: Clipboard, dataType: string, output: var seq[byte]): bool {.inline.} =
  assert(not pb.readImpl.isNil)
  pb.readImpl(pb, dataType, output)

proc availableFormats*(pb: Clipboard): seq[string] =
  assert(not pb.availableFormatsImpl.isNil)
  pb.availableFormatsImpl(pb)

proc writeString*(pb: Clipboard, s: string) =
  pb.writeData("text/plain", cast[seq[byte]](s))

proc readString*(pb: Clipboard, output: var string): bool =
  var d: seq[byte]
  result = pb.readData("text/plain", d)
  let sz = d.len
  output.setLen(sz)
  if sz != 0:
    copyMem(addr output[0], addr d[0], sz)
