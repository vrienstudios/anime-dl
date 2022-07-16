import os, strutils, httpclient, terminal
import clipboard
import ./Types/ArgumentObject
import ADLCore, ADLCore/genericMediaTypes, ADLCore/Novel/NovelTypes
import EPUB, EPUB/genericHelpers, EPUB/Types/genericTypes

var usrInput: string
var curSegment: int = 0
var novelObj: Novel

proc WelcomeScreen() =
  stdout.styledWriteLine(ForegroundColor.fgRed, "Welcome to anime-dl 3.0")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t1) Anime")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t2) Novel")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t3) (SOON) Manga")
  while true:
    stdout.styledWrite(ForegroundColor.fgGreen, ">")
    usrInput = readLine(stdin)
    if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('2'):
      stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: put isn't 1, 2, 3")
      continue
    if usrInput[0] == '2':
      curSegment = 2
      break
proc NovelScreen() =
  stdout.styledWriteLine(ForegroundColor.fgRed, "novel-dl (Utilizing NovelHall, for now)")
  stdout.styledWriteLine(ForegroundColor.fgRed, "\t1) Search")
  stdout.styledWriteLine(ForegroundColor.fgRed, "\t2) Download")
  while true:
    stdout.styledWrite(ForegroundColor.fgGreen, ">")
    usrInput = readLine(stdin)
    if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('2'):
      stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: put isn't 1, 2")
      continue
    if usrInput[0] == '1':
      novelObj = GenerateNewNovelInstance("NovelHall", "")
      curSegment = 3
      break
    elif usrInput[0] == '2':
      curSegment = 4
      break
proc NovelSearchScreen() =
  stdout.styledWrite(ForegroundColor.fgWhite, "Enter Search Term:")
  stdout.styledWrite(ForegroundColor.fgGreen, ">")
  usrInput = readLine(stdin)
  let mSeq = novelObj.searchDownloader(usrInput)
  var idx: int = 0
  for mDat in mSeq[0..9]:
    stdout.styledWrite(ForegroundColor.fgGreen, $idx, fgWhite, mDat.name, ": " & mDat.author)
    inc idx
  while true:
    stdout.styledWrite(ForegroundColor.fgWhite, "Select Novel:")
    stdout.styledWrite(ForegroundColor.fgGreen, ">")
    if usrInput.len > 1 or ord(usrInput[0]) <= ord('0') and ord(usrInput[0]) >= ord('8'):
      stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: Doesn't seem to be valid input 0-8")
      continue
    novelObj = GenerateNewNovelInstance("NovelHall", mDat[parseInt(usrInput)].uri)
    curSegment = 6
    break
proc NovelDownloadScreen() =
  var cSeq: seq[Chapter] = novelObj.getChapterSequence
  

while true:
  case curSegment:
    of -1:
      quit(1)
    of 0: WelcomeScreen()
    of 2: NovelScreen()
    of 3: NovelSearchScreen()
    of 4: NovelDownloadScreen()
    else:
      quit(-1)