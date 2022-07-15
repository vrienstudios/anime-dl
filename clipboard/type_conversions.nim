import tables, sets

type
  ConvCb = proc(fromType, toType: string, data: seq[byte]): seq[byte] {.nimcall, gcsafe.}
  TypeConverter* = object
    typeIndexes: Table[string, uint16]
    typeNames: seq[string]
    converters: seq[ConvCb]
    convertToType: seq[seq[tuple[withType, fn: uint16]]]
    convertFromType: seq[seq[tuple[withType, fn: uint16]]]

proc addType(c: var TypeConverter, t: string): uint16 =
  assert(c.convertFromType.len == c.convertToType.len)
  assert(c.convertFromType.len == c.typeNames.len)
  result = c.convertFromType.len.uint16
  c.convertFromType.add(@[])
  c.convertToType.add(@[])
  c.typeNames.add(t)
  c.typeIndexes[t] = result

proc addConversion(c: var TypeConverter, fromType, toType, cb: uint16) =
  c.convertFromType[fromType].add((toType, cb))
  c.convertToType[toType].add((fromType, cb))

proc registerTypeConversion*(c: var TypeConverter, typePairs: openarray[tuple[fromType, toType: string]], convert: ConvCb) =
  let convertIdx = c.converters.len.uint16
  c.converters.add(convert)

  for (fromType, toType) in typePairs:
    var fromTypeIdx = c.typeIndexes.getOrDefault(fromType, uint16.high)
    var toTypeIdx = c.typeIndexes.getOrDefault(toType, uint16.high)
    let fromTypeExisted = fromTypeIdx != uint16.high
    let toTypeExisted = toTypeIdx != uint16.high
    if not fromTypeExisted:
      fromTypeIdx = addType(c, fromType)

    if not toTypeExisted:
      toTypeIdx = addType(c, toType)

    c.addConversion(fromTypeIdx, toTypeIdx, convertIdx)

proc collectConversions(forType: uint16, types: seq[seq[tuple[withType, fn: uint16]]], output: var HashSet[uint16]) =
  for (t, _) in types[forType]:
    if not output.containsOrIncl(t):
      collectConversions(t, types, output)

proc conversionsForType(c: TypeConverter, forType: string, types: seq[seq[tuple[withType, fn: uint16]]]): seq[string] =
  let typeIdx = c.typeIndexes.getOrDefault(forType, uint16.high)
  if typeIdx == uint16.high: return

  var hs = initHashSet[uint16]()
  collectConversions(typeIdx, types, hs)
  hs.excl(typeIdx)

  result = newSeqOfCap[string](hs.len)
  for t in hs: result.add(c.typeNames[t])

proc conversionsFromType*(c: TypeConverter, fromType: string): seq[string] =
  c.conversionsForType(fromType, c.convertFromType)

proc conversionsToType*(c: TypeConverter, toType: string): seq[string] =
  c.conversionsForType(toType, c.convertToType)

type PathNode = object
  children: seq[PathNode]
  toType: uint16
  cb: uint16
  depth: uint16

proc collectPath(c: TypeConverter, fromType, toType: uint16, n: var PathNode, hs: var HashSet[uint16]) =
  var maxDepth = 0
  var minDepth = uint16.high
  for (t, cb) in c.convertFromType[fromType]:
    if not containsOrIncl(hs, t):
      n.children.add(PathNode(toType: t, cb: cb))
      if t != toType:
        c.collectPath(t, toType, n.children[^1], hs)
      let d = n.children[^1].depth
      if d < minDepth: minDepth = d

  if minDepth == uint16.high:
    n.depth = uint16.high
    n.children = @[]
  else:
    n.depth = minDepth + 1

proc getPath(n: PathNode, fromType: uint16, output: var seq[tuple[fromType, toType, cb: uint16]]) =
  let d = n.depth - 1
  for c in n.children:
    if c.depth == d:
      output.add((fromType, c.toType, c.cb))
      getPath(c, c.toType, output)
      break

proc conversionPath(c: TypeConverter, fromType, toType: string): seq[tuple[fromType, toType, cb: uint16]] =
  let fromTypeIdx = c.typeIndexes.getOrDefault(fromType, uint16.high)
  if fromTypeIdx == uint16.high: return
  let toTypeIdx = c.typeIndexes.getOrDefault(toType, uint16.high)
  if toTypeIdx == uint16.high: return

  var hs = initHashSet[uint16]()
  var n: PathNode
  collectPath(c, fromTypeIdx, toTypeIdx, n, hs)

  if toTypeIdx notin hs: return
  getPath(n, fromTypeIdx, result)

proc convertData*(c: TypeConverter, fromType, toType: string, data: seq[byte], output: var seq[byte]): bool =
  let cp = c.conversionPath(fromType, toType)
  if cp.len == 0: return

  var temp1 = data
  var temp2: seq[byte]

  for (fromType, toType, cb) in cp:
    temp2 = c.converters[cb](c.typeNames[fromType], c.typeNames[toType], temp1)
    swap(temp1, temp2)

  swap(temp1, output)
  true

when isMainModule:
  proc toString(d: seq[byte]): string =
    result = newString(d.len)
    copyMem(addr result[0], unsafeAddr d[0], d.len)
  proc conv(f, t: string, d: seq[byte]): seq[byte] =
    # echo "Converting"
    var s = toString(d)
    s &= " + (" & f & " -> " & t & ")"
    return cast[seq[byte]](s)

  proc toData(s: string): seq[byte] = cast[seq[byte]](s)

  var c: TypeConverter
  c.registerTypeConversion([("t1", "t2"), ("t1", "t3"), ("t2", "t4"), ("t0", "t4"), ("t4", "t1")], conv)

  echo "convertionsFromType t1: ", c.conversionsFromType("t1")
  echo "convertionsToType t4: ", c.conversionsFromType("t4")
  var output: seq[byte]
  if c.convertData("t0", "t4", toData("data"), output):
    echo "Done: ", output.toString
