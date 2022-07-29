import strutils, httpclient, terminal
import ADLCore, ADLCore/genericMediaTypes, ADLCore/Novel/NovelTypes, ADLCore/Video/VideoType
import EPUB, EPUB/genericHelpers

var usrInput: string
var curSegment: int = 0
var novelObj: Novel
var videoObj: Video

proc WelcomeScreen() =
  stdout.styledWriteLine(ForegroundColor.fgRed, "Welcome to anime-dl 3.0")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t1) Anime")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t2) Novel")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t3) (SOON) Manga")
  while true:
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('3'):
      stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: put isn't 1, 2, 3")
      continue
    if usrInput[0] == '1':
      curSegment = 6
      break
    if usrInput[0] == '2':
      curSegment = 2
      break
    if usrInput[0] == '3':
      stdout.styledWriteLine(ForegroundColor.fgRed, "MANGA NOT AVAILABLE RIGHT NOW")
proc NovelScreen() =
  stdout.styledWriteLine(ForegroundColor.fgRed, "novel-dl (Utilizing NovelHall, for now)")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t1) Search")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t2) Download")
  while true:
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('2'):
      stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: put isn't 1, 2")
      continue
    if usrInput[0] == '1':
      novelObj = GenerateNewNovelInstance("NovelHall", "")
      curSegment = 3
      break
    elif usrInput[0] == '2':
      curSegment = 5
      break
proc NovelSearchScreen() =
  stdout.styledWrite(ForegroundColor.fgWhite, "Enter Search Term:")
  stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
  usrInput = readLine(stdin)
  let mSeq = novelObj.searchDownloader(usrInput)
  var idx: int = 0
  var mSa: seq[MetaData]
  if mSeq.len > 9:
    mSa = mSeq[0..9]
  else:
    mSa = mSeq
  for mDat in mSa:
    stdout.styledWriteLine(ForegroundColor.fgGreen, $idx, fgWhite, " | ", fgWhite, mDat.name, " | " & mDat.author)
    inc idx
  while true:
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Select Novel:")
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    if usrInput.len > 1 or ord(usrInput[0]) <= ord('0') and ord(usrInput[0]) >= ord('8'):
      stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: Doesn't seem to be valid input 0-8")
      continue
    novelObj = GenerateNewNovelInstance("NovelHall", mSeq[parseInt(usrInput)].uri)
    curSegment = 4
    break
proc NovelUrlInputScreen() =
  stdout.styledWriteLine(ForegroundColor.fgWhite, "Paste/Type URL:")
  stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
  usrInput = readLine(stdin)
  novelObj = GenerateNewNovelInstance("NovelHall",  usrInput)
  curSegment = 4
proc NovelDownloadScreen() =
  discard novelObj.getChapterSequence
  discard novelObj.getMetaData()
  var idx: int = 1
  ## WARNING: AUTHOR NEEDS UPDATING IN EPUB.NIM
  var epb: Epub = Epub(title: novelObj.metaData.name, author: novelObj.metaData.author)
  discard epb.StartEpubExport("./" & novelObj.metaData.name & ".epub")
  for chp in novelObj.chapters:
    stdout.styledWriteLine(fgGreen, $idx, $novelObj.chapters.len, fgWhite, chp.name)
    let nodes = novelObj.getNodes(chp)
    discard epb.AddPage(GeneratePage(nodes, chp.name))
  var coverBytes: string = try: novelObj.ourClient.getContent(novelObj.metaData.coverUri)
      except:
        ""
      finally:
        stdout.styledWriteLine(fgRed, "Could not get novel cover, does it exist?")
  discard epb.EndEpubExport("001001", "ADLCore", coverBytes)
  curSegment = -1
proc AnimeScreen() =
  stdout.styledWriteLine(ForegroundColor.fgRed, "anime-dl (Utilizing vidstream, for now)")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t1) Search")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t2) Download (individual)")
  stdout.styledWriteLine(ForegroundColor.fgWhite, "\t3) Download (bulk)")
  while true:
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('2'):
      stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: put isn't 1, 2")
      continue
    if usrInput[0] == '1':
      videoObj = GenerateNewVideoInstance("vidstreamAni",  "")
      curSegment = 7
      break
    elif usrInput[0] == '2':
      curSegment = 8
      break
proc AnimeSearchScreen() =
  stdout.styledWriteLine(ForegroundColor.fgWhite, "Enter Search Term:")
  stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
  usrInput = readLine(stdin)
  let mSeq = videoObj.searchDownloader(usrInput)
  var idx: int = 0
  var mSa: seq[MetaData]
  if mSeq.len > 9:
    mSa = mSeq[0..9]
  else:
    mSa = mSeq
  for mDat in mSa:
    stdout.styledWriteLine(ForegroundColor.fgGreen, $idx, fgWhite, " | ", fgWhite, mDat.name)
    inc idx
  while true:
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Select Video:")
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    if usrInput.len > 1 or ord(usrInput[0]) <= ord('0') and ord(usrInput[0]) >= ord('8'):
      stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: Doesn't seem to be valid input 0-8")
      continue
    videoObj = GenerateNewVideoInstance("vidstreamAni", mSeq[parseInt(usrInput)].uri)
    curSegment = 9
    break
proc AnimeUrlInputScreen() =
  stdout.styledWriteLine(ForegroundColor.fgWhite, "Paste/Type URL:")
  stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
  usrInput = readLine(stdin)
  videoObj = GenerateNewVideoInstance("vidstreamAni",  usrInput)
  curSegment = 9
proc AnimeDownloadScreen() =
  # Not Finalized
  assert videoObj != nil
  let mStreams: seq[MediaStreamTuple] = videoObj.listResolution()
  var mVid: seq[MediaStreamTuple] = @[]
  var idx: int = 0
  for obj in mStreams:
    if obj.isAudio:
      continue
    else:
      mVid.add(obj)
      stdout.styledWriteLine(ForegroundColor.fgWhite, "$1) $2:$3" % [$len(mVid), obj.id, obj.resolution])
      inc idx
  while true:
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Please select a resolution:")
    stdout.styledWriteLine(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    if usrInput.len > 1 or ord(usrInput[0]) <= ord('0') and ord(usrInput[0]) >= ord(($idx)[0]):
      stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: Doesn't seem to be valid input 0-^1")
      continue
    break
  let selMedia = mVid[parseInt(usrInput)]
  videoObj.selResolution(selMedia)
  while videoObj.downloadNextVideoPart("./videoObj.ts"):
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Downloaded A Video Part, continuing.")
  while videoObj.downloadNextAudioPart("./audioObj.ts"):
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Downloaded An Audio Part, continuing.")
  curSegment = -1
while true:
  case curSegment:
    of -1:
      quit(1)
    of 0: WelcomeScreen()
    of 2: NovelScreen()
    of 3: NovelSearchScreen()
    of 4: NovelDownloadScreen()
    of 5: NovelUrlInputScreen()
    of 6: AnimeScreen()
    of 7: AnimeSearchScreen()
    of 8: AnimeUrlInputScreen()
    of 9: AnimeDownloadScreen()
    else:
      quit(-1)
