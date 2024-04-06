
import strutils, httpclient, terminal, os, osproc, xmltree, times, uri
import ADLCore, ADLCore/utils, EPUB

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
proc downloadContent(ctx: var DownloaderContext) =
  return
proc searchContent(ctx: var DownloaderContext, term: string) =
  return
proc beginInteraction() =
  styledWriteLine(stdout, fgGreen, " ~ anime-dl ~ ")
  styledWriteLine(stdout, fgWhite, "Anime - Novel - Manga")
  styledWriteLine(stdout, fgWhite, " (Hint) Type \"help\"")
  let 
    userInput = getUserInput()
    splitTerms = userInput.split(' ')
  if $userInput[0..3] == "help":
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
        searchContent(ctx)
        extractMetaContent(ctx)
    else: return
  awaitInput()
while true:
  beginInteraction()