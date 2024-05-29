import system, strutils, httpclient, terminal, os, osproc, xmltree, times, uri
import ADLCore

# Scan for scripts
var 
  scripts: seq[InfoTuple]
  aniScripts: seq[InfoTuple]
  nvlScripts: seq[InfoTuple]
  mngScripts: seq[InfoTuple]

proc getOccupiedMB(): string =
  return $(getOccupiedMem() / 1000000)
proc getUserInput(): string =
  styledWrite(stdout, fgGreen, ">")
  return readLine(stdin)
proc awaitInput() =
  styledWriteLine(stdout, fgGreen, "To continue, hit enter.")
  discard getUserInput()
  return
proc printErr(err: string) =
  styledWriteLine(stdout, fgRed, err)
proc printHelp() =
  styledWriteLine(stdout, fgGreen, "\r\n~ HELP ~")
  styledWriteLine(stdout, fgWhite, "down: Download\r\n  down hostName|url searchTerm|url\r\n    Example: down www.novelhall.com DairyCow\r\n    Example 2: down https://www.novelhall.com/novels/dairycow\r\nsearch: meta (returns metadata,url,VidSrcUrl)\r\n  meta host|url searchTerm|Url\r\n")
  return
proc printOptions() =
  var idx, lineTrack: int16 = 0
  while idx < siteList.len:
    inc idx
    if idx mod 3 == 0:
      inc lineTrack
    styledWrite(stdout, fgWhite, "   ", $idx, "):", siteList[idx - 1].identifier, "\r\n".repeat(lineTrack))
proc extractMetaContent(ctx: var DownloaderContext) =
  return
proc promptSelectionChoice(ctx: var DownloaderContext) =
  return
proc promptResolutionChoice(ctx: var DownloaderContext) =
  echo "Select a resolution!"
  for i in ctx.chapter.mainStream.subStreams:
    echo "$#) $# | $#" % [i.id, i.resolution, i.uri]
  let 
    usr = getUserInput()
  ctx.selectResolution(usr)
  return
proc downloadVideo(ctx: var DownloaderContext) =
  if ctx.chapter.selStream.len == 0:
    ctx.promptResolutionChoice()
    assert ctx.chapter.selStream.len != 0
  let startPath = "./" & ctx.chapter.metadata.name
  var 
    file: File = open(startPath & ".ts", fmWrite)
    #track: File = open(startPath & ".track", fmWrite)
    idx: int = 0
  for data in ctx.walkVideoContent():
    echo "got part $# out of $#" % [$ctx.chapter.streamIndex, $ctx.chapter.selStream.len]
    file.write data.text
    #track.write "got\n"
  file.flushFile()
  file.close()
  #track.flushFile()
  #track.close()
  # TODO: Check if ffmpeg is installed and use it to correct container.
  # TODO: Implement Tracking | loading/saving
  removeFile(startPath & ".track")
  return
proc downloadContent(ctx: var DownloaderContext) =
  if ctx.sections.len > 0 and ctx.section.sResult:
    ctx.promptSelectionChoice()
  assert ctx.setMetadata()
  assert ctx.setParts()
  if ctx.doPrep():
    ctx.downloadVideo()
    return
  var epub: Epub3 = setupEpub(ctx.sections[0].mdat)
  ctx.buildCoverAndDefaultPage(epub)
  for section in ctx.walkSections():
    if section.parts.len == 0: continue
    for chapter in ctx.walkChapters():
      styledWriteLine(stdout, fgWhite, "Got: ", chapter.metadata.name)
      assert ctx.setContent()
      epub += (chapter.metadata.name, chapter.contentSeq)
      chapter.contentSeq = @[]
    epub.write()
  return
proc searchContent(ctx: var DownloaderContext, term: string) =
  assert ctx.setSearch(term)
  return
proc processInput(input: string, path: string = "", take: seq[int] = @[]) =
  let splitTerms = input.split(' ')
  if $input[0..3] == "help":
    printHelp()
    return
  if splitTerms.len < 2:
    printErr("No args?")
    return
  var ctx: DownloaderContext = generateContext(splitTerms[1])
  case splitTerms[0]:
    of "down":
      if splitTerms.len == 2:
        if not splitTerms[1].isUrl():
          printErr("arg (1) is not a URL and no other args")
          return
      elif splitTerms.len == 3:
        if not splitTerms[2].isUrl():
          searchContent(ctx, splitTerms[2])
      downloadContent(ctx)
    of "meta":
      if splitTerms.len > 2:
        if splitTerms[2].isUrl():
          extractMetaContent(ctx)
          return
        searchContent(ctx, splitTerms[2])
        extractMetaContent(ctx)
    else: 
      printErr("arg error")
      return
proc beginInteraction(defaultInput: string = "") =
  try:
    var input = defaultInput
    styledWriteLine(stdout, fgGreen, " ~ anime-dl ~ ")
    styledWriteLine(stdout, fgWhite, "Anime - Novel - Manga")
    styledWriteLine(stdout, fgWhite, " (Hint) Type \"help\"")
    if input == "":
      input = getUserInput()
    processInput(input)
    awaitInput()
  except:
    styledWriteLine(stdout, fgRed, "there was an error")

let ps: int = paramCount()
var pidx: int = 0

if ps > 0:
  var
    url: string
    dPath: string
    take: seq[int] = @[]
    metaOnly: bool
  while pidx < ps:
    var cstr = paramStr(pidx)
    if not cstr.isUrl():
      case cstr:
        of "-l":
          inc pidx
          let limit = paramStr(pidx).split('-')
          for l in limit:
            take.add parseInt(l)
        of "-m":
          metaOnly = true
        else:
          if not dirExists(cstr): 
            inc pidx
            continue
          dPath = cstr
    inc pidx
  processInput(if metaOnly: "meta " else: "down " &  url, dPath, take)
  quit(0)
while true:
  beginInteraction()