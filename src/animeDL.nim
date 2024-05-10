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
  awaitInput()
  quit(-1)
proc printHelp() =
  styledWriteLine(stdout, fgGreen, "\r\n~ HELP ~")
  styledWriteLine(stdout, fgWhite, "down: Download\r\n  down hostName|url searchTerm|url\r\n    Example: down www.novelhall.com DairyCow\r\n    Example 2: down https://www.novelhall.com/novels/dairycow\r\nsearch: meta (returns metadata,url,VidSrcUrl)\r\n  meta host|url searchTerm|Url\r\n")
  awaitInput()
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
proc downloadVideo(ctx: var DownloaderContext) =
  printErr("VIDEOS UNAVAILABLE RIGHT NOW")
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
proc processInput(input: string) =
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
    else: return
proc beginInteraction(defaultInput: string = "") =
  var input = defaultInput
  styledWriteLine(stdout, fgGreen, " ~ anime-dl ~ ")
  styledWriteLine(stdout, fgWhite, "Anime - Novel - Manga")
  styledWriteLine(stdout, fgWhite, " (Hint) Type \"help\"")
  if input == "":
    input = getUserInput()
  processInput(input)
  awaitInput()
while true:
  when defined(debug):
    quit(0)
  beginInteraction()