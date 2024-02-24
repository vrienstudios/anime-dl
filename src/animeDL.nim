
import strutils, httpclient, terminal, os, osproc, xmltree, times
import ADLCore, EPUB

# Scan for scripts
var 
  scripts: seq[Interp.InfoTuple] = scanForScriptsInfoTuple("./scripts/")
  aniScripts: seq[Interp.InfoTuple]
  nvlScripts: seq[Interp.InfoTuple]
  mngScripts: seq[Interp.InfoTuple]

proc buildCoverAndDefaultPage(epub3: Epub3, novelObj: SNovel) =
  stdout.styledWriteLine(fgWhite, "Downloading Cover")
  let meta = novelObj.metadata
  var nodes: seq[TiNode] = @[]
  var coverBytes: string = ""
  try:
    var host: string = novelObj.metaData.coverUri.split("/")[2]
    novelObj.getDefHttpClient.headers = newHttpHeaders({
      "User-Agent": "Mozilla/5.0 (X11; Linux x86_64; rv:101.0) Gecko/20100101 Firefox/101.0",
      "Referer": novelObj.defaultPage,
      "Host": host,
      "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,application/json,*/*;q=0.8"
    })
    coverBytes = novelObj.getDefHttpClient.getContent(meta.coverUri)
    novelObj.getDefHttpClient.headers = novelObj.defaultHeaders
    let img = Image(fileName: "cover.jpeg", kind: ImageKind.cover, path: coverBytes, isPathData: true)
    epub3.add img
    nodes.add TiNode(kind: NodeKind.ximage, image: img, customPath: "../../cover.jpeg")
  except:
    stdout.styledWriteLine(fgRed, "Could not get novel cover, does it exist?")
    novelObj.getDefHttpClient.headers = novelObj.defaultHeaders
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Title: " & meta.name)
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Author: " & meta.author)
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Synopsis: " & meta.description)
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Created with anime-dl (https://github.com/vrienstudios/anime-dl)")
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Scraped from: " & meta.uri)
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Number of pages: " & $novelObj.chapters.len)
  epub3.add Page(name: "info", nodes: nodes)
proc getOccupiedMB(): string =
  return $(getOccupiedMem() / 1000000)
proc loopVideoDownload(videoObj: SVideo) =
  stdout.styledWriteLine(fgWhite, "Downloading video for " & videoObj.metaData.name)
  while downloadNextVideoPart(videoObj, (workingDirectory / "$1.mp4" % [videoObj.metaData.name])):
    eraseLine()
    stdout.styledWriteLine(fgWhite, "Got ", fgRed, $videoObj.videoCurrIdx, fgWhite, " of ", fgRed, $(videoObj.videoStream.len), " ", fgGreen, "Mem: ", getOccupiedMB(), "MB")
    cursorUp 1
  cursorDown 1
  if videoObj.audioStream.len > 0:
    stdout.styledWriteLine(fgWhite, "Downloading audio for " & videoObj.metaData.name)
    while downloadNextAudioPart(videoObj, (workingDirectory / "$1.ts" % [videoObj.metaData.name])):
      stdout.styledWriteLine(fgWhite, "Got ", fgRed, $videoObj.audioCurrIdx, fgWhite, " of ", fgRed, $(videoObj.audioStream.len), " ", fgGreen, "Mem: ", getOccupiedMB(), "MB")
      cursorUp 1
      eraseLine()
    cursorDown 1
proc downloadCheck(videoObj: SVideo): string =
  if fileExists(workingDirectory / "$1.dfo" % [videoObj.metaData.name]):
    var data: seq[string] = split(readAll(open(workingDirectory / "$1.dfo" % [videoObj.metaData.name], fmRead)), '@')
    if data.len > 1:
      videoObj.videoCurrIdx = parseInt(data[1])
      return data[0]
  return ""
proc SetupEpub(mdataObj: MetaData): Epub3 =
  var epub: Epub3 
  let potentialPath = workingDirectory / mdataObj.name & ".epub"
  if fileExists(potentialPath):
    # Check if DIR exists if file also exists.
    if dirExists(workingDirectory / mdataObj.name):
      echo "loading from dir instead of file"
      epub = LoadEpubFromDir(workingDirectory / mdataObj.name)
    else:
      echo "loading from file"
      epub = LoadEpubFile(potentialPath)
    echo "loading TOC"
    epub.loadTOC()
    epub.beginExport()
    return epub
  if dirExists(workingDirectory / mdataObj.name):
    echo "loading from dir"
    epub = LoadEpubFromDir(workingDirectory / mdataObj.name)
    echo "loading TOC"
    epub.loadTOC()
    epub.beginExport()
    return epub
  epub = CreateNewEpub(mdataObj.name, workingDirectory / mdataObj.name)
  epub.beginExport() # Export while creating
  block addMeta:
    # Title
    epub.metaData.add EpubMetaData(metaType: MetaType.dc, name: "title", attrs: {"id": "title"}.toXmlAttributes(), text: mdataObj.name)
    # Author
    epub.metaData.add EpubMetaData(metaType: MetaType.dc, name: "creator", attrs: {"id": "creator"}.toXmlAttributes(), text: mdataObj.author)
    # Default Language
    epub.metaData.add EpubMetaData(metaType: MetaType.dc, name: "language", text: "en")
    # modification date
    epub.metaData.add EpubMetaData(metaType: MetaType.meta, attrs: {"property": "dcterms:modified"}.toXmlAttributes(), text: $getTime())
    # Publisher (default to us)
    epub.metaData.add EpubMetaData(metaType: MetaType.dc, name: "publisher", text: "anime-dl")
  # Build in memory -- use a different method for epub resumation.
  return epub

proc getUserInput(): string =
    return readLine(stdin)
proc beginInteraction() =
  console.styledWriteLine(stdout, fgGreen, " ~ anime-dl ~ ")
  console.styledWriteLine(stdout, fgWhite, "\tNovel -")
