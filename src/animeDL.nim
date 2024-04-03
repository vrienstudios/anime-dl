
import strutils, httpclient, terminal, os, osproc, xmltree, times
import ADLCore, EPUB

# Scan for scripts
var 
  scripts: seq[InfoTuple]
  aniScripts: seq[InfoTuple]
  nvlScripts: seq[InfoTuple]
  mngScripts: seq[InfoTuple]

proc getOccupiedMB(): string =
  return $(getOccupiedMem() / 1000000)
proc buildCoverAndDefaultPage(epub3: Epub3, novelObj: DownloaderContext) =
  stdout.styledWriteLine(fgWhite, "Downloading Cover")
  # section[0] should contain all metadata information about the novel including num of chapters.
  let meta = novelObj.sections[0].mdat
  var nodes: seq[TiNode] = @[]
  var coverBytes: string = ""
  try:
    var host: string = meta.coverUri.split("/")[2]
    novelObj.ourClient.headers = newHttpHeaders({
      "User-Agent": "Mozilla/5.0 (X11; Linux x86_64; rv:101.0) Gecko/20100101 Firefox/101.0",
      "Referer": novelObj.defaultPage,
      "Host": host,
      "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,application/json,*/*;q=0.8"
    })
    coverBytes = novelObj.ourClient.getContent(meta.coverUri)
    novelObj.ourClient.headers = novelObj.defaultHeaders
    let img = Image(fileName: "cover.jpeg", kind: ImageKind.cover, path: coverBytes, isPathData: true)
    epub3.add img
    nodes.add TiNode(kind: NodeKind.ximage, image: img, customPath: "../../cover.jpeg")
  except:
    stdout.styledWriteLine(fgRed, "Could not get novel cover, does it exist?")
    novelObj.ourClient.headers = novelObj.defaultHeaders
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Title: " & meta.name)
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Author: " & meta.author)
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Synopsis: " & meta.description)
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Created with anime-dl (https://github.com/vrienstudios/anime-dl)")
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Scraped from: " & meta.uri)
  nodes.add TiNode(kind: NodeKind.paragraph, text: "Number of pages: " & $novelObj.sections[0].parts.len)
  epub3.add Page(name: "info", nodes: nodes)
proc SetupEpub(mdataObj: MetaData): Epub3 =
  var epub: Epub3
  let appDir = getAppDir()
  let potentialPath = appDir / mdataObj.name & ".epub"
  if fileExists(potentialPath):
    # Check if DIR exists if file also exists.
    if dirExists(appDir / mdataObj.name):
      echo "loading from dir instead of file"
      epub = LoadEpubFromDir(appDir / mdataObj.name)
    else:
      echo "loading from file"
      epub = LoadEpubFile(potentialPath)
    echo "loading TOC"
    epub.loadTOC()
    epub.beginExport()
    return epub
  if dirExists(appDir / mdataObj.name):
    echo "loading from dir"
    epub = LoadEpubFromDir(appDir / mdataObj.name)
    echo "loading TOC"
    epub.loadTOC()
    epub.beginExport()
    return epub
  epub = CreateNewEpub(mdataObj.name, appDir / mdataObj.name)
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
  styledWrite(stdout, fgGreen, ">")
  return readLine(stdin)
proc printHelp() =
  styledWriteLine(stdout, fgGreen, "\r\n~ HELP ~")
  styledWriteLine(stdout, fgWhite, "down: Download\r\n  down host|url search|url\r\n    Example: down novelhall.com DairyCow\r\n    Example 2: down https://www.novelhall.com/novels/dairycow\r\nsearch: Search (returns metadata,url,VidSrcUrl)\r\n  search host|url searchTerm\r\n", fgGreen, "To continue, hit enter.")
  discard getUserInput()
  return
proc beginInteraction() =
  styledWriteLine(stdout, fgGreen, " ~ anime-dl ~ ")
  styledWriteLine(stdout, fgWhite, "Anime - Novel - Manga")
  styledWriteLine(stdout, fgWhite, " (Hint) Type \"help\"")
  let userInput = getUserInput()
  if $userInput[0..3] == "help":
    printHelp()
    return
  return
while true:
  beginInteraction()