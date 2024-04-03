
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