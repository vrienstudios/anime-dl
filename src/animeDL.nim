type
  StreamError = object of CatchableError
  MissingStreamError = object of StreamError
  ResolutionStreamError = object of StreamError
  StreamDefect = object of Defect

import strutils, httpclient, terminal, os, osproc
import ADLCore/Interp, ADLCore
import EPUB/EPUB3

# Process scripts.
var scripts: seq[Interp.InfoTuple] = ScanForScriptsInfoTuple("./scripts/")
var aniScripts: seq[Interp.InfoTuple]
var nvlScripts: seq[Interp.InfoTuple]
var mngScripts: seq[Interp.InfoTuple]
let workingDirectory: string = getCurrentDir()

template resComparer(res: seq[MediaStreamTuple], body: untyped) =
  var hRes {.inject.}: int = 0
  var current {.inject.}: MediaStreamTuple
  for stream in res:
    var b {.inject.} = parseInt(stream.resolution.split('x')[1])
    if body:
      continue
    current = stream
    hRes = b
func resCompare(resolutions: seq[MediaStreamTuple], option: char): MediaStreamTuple =
  result =
    if option == 'h':
      try:
        resComparer(resolutions, (b < hRes))
        current
      except:
        raise(ref ResolutionStreamError)(msg: "Something went wrong in comparison")
    elif option == 'l':
      try:
        resComparer(resolutions, (b > hRes))
        current
      except:
        raise(ref ResolutionStreamError)(msg: "Something went wrong in comparison")
    else: raise(ref ResolutionStreamError)(msg: "No h or l option set in res comparison; you shouldn't see this.")
func findStream(tuples: seq[MediaStreamTuple], resolution: string): MediaStreamTuple =
  if resolution == "highest" or resolution == "h":
    return resCompare(tuples, 'h')
  if resolution == "lowest" or resolution == "l":
    return resCompare(tuples, 'l')
  for stream in tuples:
    if stream.resolution == resolution:
      return stream
  raise (ref MissingStreamError)(msg: "Unable to deduce stream with given resolution; Maybe the resolution doesn't exist?")

for scr in scripts:
  case scr.scraperType:
    of "ani": aniScripts.add scr
    of "nvl": nvlScripts.add scr
    of "mng": mngScripts.add scr
    else: continue

proc loopVideoDownload(videoObj: Video) =
  stdout.styledWriteLine(fgWhite, "Downloading video for " & videoObj.metaData.name)
  while DownloadNextVideoPart(videoObj, (workingDirectory / "$1.mp4" % [videoObj.metaData.name])):
    eraseLine()
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Got ", ForegroundColor.fgRed, $videoObj.videoCurrIdx, fgWhite, " of ", fgRed, $(videoObj.videoStream.len), " ", fgGreen, "Mem: ", $getOccupiedMem(), "/", $getFreeMem())
    cursorUp 1
  cursorDown 1
  if videoObj.audioStream.len > 0:
    stdout.styledWriteLine(fgWhite, "Downloading audio for " & videoObj.metaData.name)
    while DownloadNextAudioPart(videoObj, (workingDirectory / "$1.ts" % [videoObj.metaData.name])):
      stdout.styledWriteLine(ForegroundColor.fgWhite, "Got ", ForegroundColor.fgRed, $videoObj.audioCurrIdx, fgWhite, " of ", fgRed, $(videoObj.audioStream.len), " ", fgGreen, "Mem: ", $getOccupiedMem(), "/", $getFreeMem())
      cursorUp 1
      eraseLine()
    cursorDown 1
proc downloadCheck(videoObj: Video): string =
  if fileExists(workingDirectory / "$1.dfo" % [videoObj.metaData.name]):
    var data: seq[string] = split(readAll(open(workingDirectory / "$1.dfo" % [videoObj.metaData.name], fmRead)), '@')
    if data.len > 1:
      videoObj.videoCurrIdx = parseInt(data[1])
      return data[0]
  return ""
proc SetupEpub(mdataObj: MetaData): EPUB3 =
  let mdataList: seq[metaDataList] = @[
    (metaType: MetaType.dc, name: "title", attrs: @[("id", "title")], text: mdataObj.name),
    (metaType: MetaType.dc, name: "creator", attrs: @[("id", "creator")], text: mdataObj.author),
    (metaType: MetaType.dc, name: "language", attrs: @[], text: "en"),
    (metaType: MetaType.meta, name: "", attrs: @[("property", "dcterms:modified")], text: "2022-01-02T03:50:100"),
    (metaType: MetaType.dc, name: "publisher", attrs: @[], text: "animedl")]
  if dirExists(workingDirectory / mdataObj.name):
    return OpenEpub3AndRebuild(mdataList, workingDirectory / mdataObj.name)
  #  try:
  #    return OpenEpub3AndRebuild(mdataList, "./" & mdataObj.name)
  #  except:
  #    # If the directory isn't a valid EPUB, remove it and create a new one.
  #    removeDir("./" & mdataObj.name)
  #    return CreateEpub3(mdataList, "./" & mdataObj.name)
  else:
    return CreateEpub3(mdataList, workingDirectory / mdataObj.name)

var usrInput: string
proc SetUserInput() =
  stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
  usrInput = readLine(stdin)

block cmld:
  var argList: tuple[sel: string, dwnld: bool, url: string, limit: bool, lrLimit: array[2, int], custom: bool, customName: string, dblk: bool, res: string, skipDelete: bool, search: bool] =
    ("", false, "", false, [0, 0], false, "", false, "", false, false)
  proc NovelDownload(novelObj: var SNovel) =
    discard GetMetaData(novelObj)
    discard GetChapterSequence(novelObj)
    var epb = SetupEpub(novelObj.metaData)
    var i: int = 0
    var r: int = novelObj.chapters.len
    var bf: int = 0
    if argList.limit:
      i = argList.lrLimit[0]
      r = argList.lrLimit[1]
      bf = 1
    while i < r:
      eraseLine()
      let name =
        if novelObj.chapters[i].name.len > 20:
          novelObj.chapters[i].name[0..20]
        else:
          novelObj.chapters[i].name
      stdout.styledWriteLine(fgRed, $i, "/", $(r - bf), " ", fgWhite, name, " ", fgGreen, "Mem: ", $getOccupiedMem(), "/", $getFreeMem())
      cursorUp 1
      if epb.CheckPageExistance(novelObj.chapters[i].name):
        inc i
        continue
      var nodes: seq[TiNode] = GetNodes(novelObj, novelObj.chapters[i])
      AddPage(epb, GeneratePage(novelObj.chapters[i].name, nodes))
      inc i
    cursorDown 1
    var coverBytes: string = ""
    try:
      coverBytes = novelObj.ourClient.getContent(novelObj.metaData.coverUri)
    except:
      stdout.styledWriteLine(fgRed, "Could not get novel cover, does it exist?")
    AssignCover(epb, Image(name: "cover.jpeg", imageType: ImageType.jpeg, bytes: coverBytes))
    FinalizeEpub(epb, argList.skipDelete)
  proc NovelManager() =
    var novelObj: SNovel
    var script: NScript
    block sel:
      if argList.custom and argList.customName != "":
        for scr in nvlScripts:
          if scr.name == argList.customName:
            script = GenNewScript(scr.scriptPath)
            novelObj = SNovel(script: script, defaultPage: argList.url)
            break sel
        quit(-1)
      novelObj = GenerateNewNovelInstance("NovelHall", argList.url).toSNovel()
    block engage:
      if argList.dwnld:
        NovelDownload(novelObj)
        break engage
      echo "Getting MetaData Only"
      echo $GetMetaData(novelObj)
  proc AnimeDownloader(videoObj: var SVideo) =
    var selMedia: MediaStreamTuple
    if argList.dblk == false:
      let mediaStreams: seq[MediaStreamTuple] = ListResolutions(videoObj)
      var streamIndex: int = 0
      if argList.res == "":
        var mVid: seq[MediaStreamTuple] = @[]
        for stream in mediaStreams:
          if stream.isAudio == false: mVid.add(stream)
          stdout.styledWriteLine(ForegroundColor.fgWhite, "$1) $2:$3" % [$len(mVid), stream.id, stream.resolution])
          inc streamIndex
        while true:
          stdout.styledWriteLine(ForegroundColor.fgWhite, "Please select a resolution:")
          SetUserInput()
          if usrInput.len > 1 or ord(usrInput[0]) <= ord('0') and ord(usrInput[0]) >= ord(($streamIndex)[0]):
            stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: Doesn't seem to be valid input 0-^1")
            continue
          break
        selMedia = mVid[parseInt(usrInput) - 1]
      else:
        selMedia = findStream(mediaStreams, argList.res)
      SelResolution(videoObj, selMedia)
      loopVideoDownload(videoObj)
      return
    let episodes = GetEpisodeSequence(videoObj)
    for episode in episodes:
      videoObj = (SVideo)GenerateNewVideoInstance(argList.customName, episode.uri)
      discard GetMetaData(videoObj)
      discard GetStream(videoObj)
      let hResolution = resCompare(ListResolutions(videoObj), 'h')
      SelResolution(videoObj, hResolution)
      loopVideoDownload(videoObj)
  proc AnimeManager() =
    var videoObj: SVideo
    var script: NScript
    block sel:
      if argList.custom:
        if argList.customName == "hanime" or argList.customName == "vidstream" or argList.customName == "membed":
          videoObj = (SVideo)GenerateNewVideoInstance(argList.customName, argList.url)
          break sel
        for scr in aniScripts:
          if scr.name == argList.customName:
            script = GenNewScript(scr.scriptPath)
            videoObj = SVideo(script: script)
            break sel
    block engage:
      if argList.dwnld:
        AnimeDownloader(videoObj)
        break engage
      echo $GetMetaData(videoObj)
  if paramCount() <= 1:
    break cmld
  block argLoop:
    var i: int = 1
    argList.sel = paramStr(i)
    inc i
    argList.url = paramStr(i)
    while i < paramCount():
      inc i
      case paramStr(i):
        of "-d":
          argList.dwnld = true
        of "-lim":
          argList.limit = true
          inc i
          let s: seq[string] = split(paramStr(i), ":")
          argList.lrLimit[0] = parseInt(s[0])
          argList.lrLimit[1] = parseInt(s[1])
        of "-c":
          inc i
          argList.custom = true
          argList.customName = paramStr(i)
        of "-cauto": # Experimental, will eventually replace -c as default.
          arglist.custom = true
        of "-dblk":
          argList.dblk = true
        of "-res":
          inc i
          argList.res = paramStr(i)
        of "-ds":
          arglist.skipDelete = true
        of "-s":
          argList.search = true
        else:
          continue
    break argLoop
  case argList.sel:
    of "nvl":
      NovelManager()
    of "ani":
      AnimeManager()
    else:
      echo "Help: "
      echo "animeDL {tag} {url} {options}"
      echo "(OPTIONS)\n\t-d (sets download to true)\n\t-lim {num}:{num} (selects a range to download from *starts at 0)"
      echo "\t-c {downloader name} (sets a custom downloader name)\n\t-cauto (for test purposes, so we can drop the tag selector and site selector later)"
      echo "\t-dblk (choose to download an entire series from membed/gogoplay)\n\t-res {h|l|numxnum} (if not set, it defaults to ask)"
      quit(-1)
  quit(1)

# TODO: Implement params/commandline arguments.
block interactive:
  # When Windows, redirect program to cmd
  var isWnOpen: bool = false
  if paramCount() == 1 and paramStr(1) == "con":
    isWnOpen = true
  when defined(windows):
    if isWnOpen:
      discard execProcess("cmd $1 con" % [getAppDir() / getAppFilename()])
      quit(0)
  type Segment = enum 
                    Quit, Welcome, 
                    NovelSelector, Novel, NovelSearch, NovelDownload, NovelUrlInput,
                    AnimeSelector, Anime, AnimeSearch, AnimeUrlInput, AnimeDownload, AnimeSearchSelector
                    Manga, MangaSearch, MangaUrlInput, MangaDownload

  var downBulk: bool
  var curSegment: Segment = Segment.Welcome
  var novelObj: SNovel
  var videoObj: Video
  var currScraperString: string

  proc WelcomeScreen() =
    stdout.styledWriteLine(ForegroundColor.fgRed, "Welcome to anime-dl 3.0")
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t1) Anime")
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t2) Novel")
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t3) Manga")
    while true:
      stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
      usrInput = readLine(stdin)
      if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('3'):
        stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: put isn't 1, 2, 3")
        continue
      if usrInput[0] == '1':
        curSegment = Segment.AnimeSelector
        break
      if usrInput[0] == '2':
        curSegment = Segment.NovelSelector
        break
      if usrInput[0] == '3':
        curSegment = Segment.Manga
        break

  proc NovelSelector() =
    var idx: int = 2
    var vLines: seq[string] = @["1) NovelHall"]
    for scr in nvlScripts:
      if idx mod 4 == 0:
        vLines.add "$1) $2" % [$idx, $scr.name]
        inc idx
        continue
      vLines[^1] = vLines[^1] & "\t$1) $2" % [$idx, $scr.name]
      inc idx
    for ln in vLines:
      stdout.styledWriteLine(fgWhite, ln)
    SetUserInput()
    try:
      let usrInt = parseInt(usrInput)
      if usrInt == 1:
        novelObj = GenerateNewNovelInstance("NovelHall", "")
        curSegment = Segment.Novel
        return
      novelObj = SNovel(script: GenNewScript(nvlScripts[usrInt - 2].scriptPath))
      curSegment = Segment.Novel
      return
    except:
      stdout.styledWriteLine(fgRed, "Error in input")

  proc NovelScreen() =
    stdout.styledWriteLine(ForegroundColor.fgRed, "novel-dl")
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t1) Search")
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t2) Download")
    while true:
      SetUserInput()
      if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('2'):
        stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: put isn't 1, 2")
        continue
      if usrInput[0] == '1':
        curSegment = Segment.NovelSearch
        break
      elif usrInput[0] == '2':
        curSegment = Segment.NovelUrlInput
        break
  proc NovelSearchScreen() =
    stdout.styledWrite(ForegroundColor.fgWhite, "Enter Search Term:")
    SetUserInput()
    var mSeq: seq[MetaData] = @[]
    mSeq = SearchDownloader(novelObj, usrInput)
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
      if novelObj.script == nil: novelObj = GenerateNewNovelInstance("NovelHall", mSeq[parseInt(usrInput)].uri)
      else: novelObj.defaultPage = mSeq[parseInt(usrInput)].uri
      curSegment = Segment.NovelDownload
      break
  proc NovelUrlInputScreen() =
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Paste/Type URL:")
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    if novelObj.script != nil: novelObj = GenerateNewNovelInstance("NovelHall",  usrInput)
    else: `defaultPage=`(novelObj, usrInput)
    curSegment = Segment.NovelDownload
  proc NovelDownloadScreen() =
    var mdataObj: MetaData
    var chpSeq: seq[Chapter] = @[]
    block novelData:
      discard GetMetaData(novelObj)
      discard GetChapterSequence(novelObj)
      chpSeq = novelObj.chapters
      mdataObj = novelObj.metaData
    var idx: int = 1
    var epub3 = SetupEpub(mdataObj)
    for chp in chpSeq:
      eraseLine()
      stdout.styledWriteLine(fgRed, $idx, "/", $chpSeq.len, " ", fgWhite, chp.name, " ", fgGreen, "Mem: ", $getOccupiedMem(), "/", $getFreeMem())
      cursorUp 1
      if epub3.CheckPageExistance(chp.name):
        inc idx
        continue
      var nodes: seq[TiNode] = GetNodes(novelObj, chp)
      AddPage(epub3, GeneratePage(chp.name, nodes))
      inc idx
    cursorDown 1
    var coverBytes: string = ""
    try:
      coverBytes = novelObj.getDefHttpClient.getContent(novelObj.metaData.coverUri)
    except:
      stdout.styledWriteLine(fgRed, "Could not get novel cover, does it exist?")
    AssignCover(epub3, Image(name: "cover.jpeg", imageType: ImageType.jpeg, bytes: coverBytes))
    FinalizeEpub(epub3)
    curSegment = Segment.Quit
  proc AnimeSelector() =
    stdout.styledWriteLine(fgRed, "Please choose a video scraper!")
    stdout.styledWriteLine(fgWhite, "1) VidStream\t2) HAnime\t3) Membed")
    while true:
      SetUserInput()
      if usrInput == "1":
        currScraperString = "vidstreamAni"
      elif usrInput == "2":
        currScraperString = "HAnime"
      elif usrInput == "3":
        currScraperString = "Membed"
      else:
        continue
      curSegment = Segment.Anime
      break
  proc AnimeScreen() =
    stdout.styledWriteLine(ForegroundColor.fgRed, "anime-dl ($1)" % [currScraperString])
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t1) Search")
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t2) Download (individual)")
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t3) Download (bulk)")
    while true:
      SetUserInput()
      if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('2'):
        stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: put isn't 1, 2")
        continue
      if usrInput[0] == '1':
        videoObj = GenerateNewVideoInstance(currScraperString,  "")
        curSegment = Segment.AnimeSearch
        break
      elif usrInput[0] == '2':
        curSegment = Segment.AnimeUrlInput
        break
      elif usrInput[0] == '3':
        curSegment = Segment.AnimeUrlInput
        downBulk = true
        break
  proc AnimeSearchScreen() =
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Enter Search Term:")
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    let mSeq = SearchDownloader(videoObj, usrInput)
    var idx: int = 1
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
      SetUserInput()
      if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('9'):
        stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: Doesn't seem to be valid input 0-8")
        continue
      videoObj = GenerateNewVideoInstance(currScraperString, mSeq[parseInt(usrInput) - 1].uri)
      # TODO: normalize HAnime in to the same system.
      if currScraperString == "Membed" or currScraperString == "vidstreamAni":
        curSegment = Segment.AnimeSearchSelector
        break
      discard GetMetaData(videoObj)
      discard GetStream(videoObj)
      curSegment = Segment.AnimeDownload
      break
  proc AnimeSearchSeasonSelector() =
    try:
      var episodes = GetEpisodeSequence(videoObj)
      stdout.styledWriteLine(fgWhite, "Select an Episode:")
      if episodes.len == 1:
        stdout.styledWriteLine(fgWhite, "1 Episode, auto-continuing")
        videoObj = GenerateNewVideoInstance(currScraperString, episodes[0].uri)
        discard GetMetaData(videoObj)
        discard GetStream(videoObj)
        curSegment = Segment.AnimeDownload
        return
      var idx: int = 1
      for ep in episodes:
        stdout.styledWriteLine(ForegroundColor.fgGreen, $idx, fgWhite, " | ", fgWhite, ep.name)
        inc idx
      while true:
        SetUserInput()
        try:
          videoObj = GenerateNewVideoInstance(currScraperString, episodes[parseInt(usrInput) - 1].uri)
          break
        except: stdout.styledWriteLine(fgRed, "Attempt to enter an index number.")
      #stdout.styledWriteLine(fgWhite, "Download/Stream?")
      #stdout.styledWriteLine(fgGreen, "1) Download")
      #stdout.styledWriteLine(fgWhite, "2) Exec VLC")
      #stdout.styledWriteLine(fgWhite, "3) Exec MPV")
      #stdout.styledWriteLine(fgWhite, "4) Raw link")
      #while true:
      #  try:
      #    SetUserInput()
      #    case parseInt(usrInput)
      #      of 2: execCmd("vlc" & res)
      #  except: stdout.styledWriteLine(fgRed, "I didn't understsand that.")
      discard GetMetaData(videoObj)
      discard GetStream(videoObj)
    except:
      stdout.styledWriteLine(fgRed, "Failed to retrieve episodes; attempting auto-select")
    curSegment = Segment.AnimeDownload
  proc AnimeUrlInputScreen() =
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Paste/Type URL:")
    SetUserInput()
    videoObj = GenerateNewVideoInstance(currScraperString,  usrInput)
    discard GetMetaData(videoObj)
    discard GetStream(videoObj)
    curSegment = Segment.AnimeDownload
      # TODO: merge formats.
  proc AnimeDownloadScreen() =
    # Not Finalized
    assert videoObj != nil
    if downBulk == false:
      let mStreams: seq[MediaStreamTuple] = ListResolutions(videoObj)
      var mVid: seq[MediaStreamTuple] = @[]
      var idx: int = 0
      for obj in mStreams:
        if obj.isAudio:
          continue
        else:
          mVid.add(obj)
          when not defined(release):
            stdout.styledWriteLine(ForegroundColor.fgWhite, "$1) $2:$3|$4" % [$len(mVid), obj.id, obj.resolution, obj.uri])
          else:
            stdout.styledWriteLine(ForegroundColor.fgWhite, "$1) $2:$3" % [$len(mVid), obj.id, obj.resolution])
          inc idx
      while true and downBulk == false:
        stdout.styledWriteLine(ForegroundColor.fgWhite, "Please select a resolution:")
        SetUserInput()
        if usrInput.len > 1 or ord(usrInput[0]) <= ord('0') and ord(usrInput[0]) >= ord(($idx)[0]):
          stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: Doesn't seem to be valid input 0-^1")
          continue
        break
      let selMedia = mVid[parseInt(usrInput) - 1]
      if downloadCheck(videoObj) == selMedia.resolution:
        echo ""
      SelResolution(videoObj, selMedia)
      loopVideoDownload(videoObj)
    else:
      let mData = GetEpisodeSequence(videoObj)
      for meta in mData:
        videoObj = GenerateNewVideoInstance(currScraperString, meta.uri)
        discard GetMetaData(videoObj)
        discard GetStream(videoObj)
        let mResL = ListResolutions(videoObj)
        var hRes: int = 0
        var indexor: int = 0
        var selector: int = 0
        for res in mResL:
          inc indexor
          let b = parseInt(res.resolution.split('x')[1])
          if b < hRes: continue
          hRes = b
          selector = indexor - 1
        stdout.styledWriteLine(ForegroundColor.fgGreen, "Got resolution: $1 for $2" % [mResL[selector].resolution, videoObj.metaData.name])
        SelResolution(videoObj, mResL[selector])
        loopVideoDownload(videoObj)
    curSegment = Segment.Quit

  proc MangaScreen() =
    stdout.styledWriteLine(ForegroundColor.fgRed, "manga-dl (Utilizing MangaKakalot, for now)")
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t1) Search")
    stdout.styledWriteLine(ForegroundColor.fgWhite, "\t2) Download")
    while true:
      stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
      usrInput = readLine(stdin)
      if usrInput.len > 1 or ord(usrInput[0]) <= ord('1') and ord(usrInput[0]) >= ord('2'):
        stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: put isn't 1, 2")
        continue
      if usrInput[0] == '1':
        novelObj = GenerateNewNovelInstance("MangaKakalot", "")
        curSegment = Segment.MangaSearch
        break
      elif usrInput[0] == '2':
        curSegment = Segment.MangaUrlInput
        break
  proc MangaSearchScreen() =
    stdout.styledWrite(ForegroundColor.fgWhite, "Enter Search Term:")
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    let mSeq = SearchDownloader(videoObj, usrInput)
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
      stdout.styledWriteLine(ForegroundColor.fgWhite, "Select Manga:")
      stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
      usrInput = readLine(stdin)
      if usrInput.len > 1 or ord(usrInput[0]) <= ord('0') and ord(usrInput[0]) >= ord('8'):
        stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: Doesn't seem to be valid input 0-8")
        continue
      novelObj = GenerateNewNovelInstance("MangaKakalot", mSeq[parseInt(usrInput)].uri)
      curSegment = Segment.MangaDownload
      break
  proc MangaUrlInputScreen() =
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Paste/Type URL:")
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    novelObj = GenerateNewNovelInstance("MangaKakalot",  usrInput)
    curSegment = Segment.MangaDownload
  proc MangaDownloadScreen() =
    discard GetChapterSequence(novelObj)
    discard GetMetaData(novelObj)
    var idx: int = 1
    let mdataList: seq[metaDataList] = @[
      (metaType: MetaType.dc, name: "title", attrs: @[("id", "title")], text: novelObj.metaData.name),
      (metaType: MetaType.dc, name: "creator", attrs: @[("id", "creator")], text: novelObj.metaData.author),
      (metaType: MetaType.dc, name: "language", attrs: @[], text: "?"),
      (metaType: MetaType.dc, name: "identifier", attrs: @[("id", "pub-id")], text: ""),
      (metaType: MetaType.meta, name: "", attrs: @[("property", "dcterms:modified")], text: "2022-01-02T03:50:100"),
      (metaType: MetaType.dc, name: "publisher", attrs: @[], text: "animedl")]
    var epub3: EPUB3 = CreateEpub3(mdataList, "./" & novelObj.metaData.name)
    for chp in novelObj.chapters:
      eraseLine()
      stdout.styledWriteLine(fgRed, $idx, "/", $novelObj.chapters.len, " ", fgWhite, chp.name, " ", fgGreen, "Mem: ", $getOccupiedMem(), "/", $getFreeMem())
      cursorUp 1
      let nodes = GetNodes(novelObj, chp)
      AddGenPage(epub3, chp.name, nodes)
      inc idx
    cursorDown 1
    var coverBytes: string = ""
    try:
      coverBytes = novelObj.ourClient.getContent(novelObj.metaData.coverUri)
    except:
      stdout.styledWriteLine(fgRed, "Could not get manga cover, does it exist?")
    AssignCover(epub3, Image(name: "cover.jpeg", imageType: ImageType.jpeg, bytes: coverBytes))
    FinalizeEpub(epub3)
    curSegment = Segment.Quit

  while true:
    case curSegment:
      of Segment.Quit:
        quit(1)
      of Segment.Welcome: WelcomeScreen()

      of Segment.NovelSelector: NovelSelector()
      of Segment.Novel: NovelScreen()
      of Segment.NovelSearch: NovelSearchScreen()
      of Segment.NovelUrlInput: NovelUrlInputScreen()
      of Segment.NovelDownload: NovelDownloadScreen()

      of Segment.AnimeSelector: AnimeSelector()
      of Segment.Anime: AnimeScreen()
      of Segment.AnimeSearch: AnimeSearchScreen()
      of Segment.AnimeUrlInput: AnimeUrlInputScreen()
      of Segment.AnimeSearchSelector: AnimeSearchSeasonSelector()
      of Segment.AnimeDownload: AnimeDownloadScreen()
      
      of Segment.Manga: MangaScreen()
      of Segment.MangaSearch: MangaSearchScreen()
      of Segment.MangaUrlInput: MangaUrlInputScreen()
      of Segment.MangaDownload: MangaDownloadScreen()
