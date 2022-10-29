import strutils, httpclient, terminal, os, osproc
import ADLCore, ADLCore/genericMediaTypes, ADLCore/Novel/NovelTypes, ADLCore/Video/VideoType, ADLCore/Interp
import EPUB/[types, EPUB3]

# Process scripts.
var scripts: seq[Interp.InfoTuple] = ScanForScriptsInfoTuple("./scripts/")
var aniScripts: seq[Interp.InfoTuple]
var nvlScripts: seq[Interp.InfoTuple]
var mngScripts: seq[Interp.InfoTuple]

for scr in scripts:
  case scr.scraperType:
    of "ani": aniScripts.add scr
    of "nvl": nvlScripts.add scr
    of "mng": mngScripts.add scr
    else: continue

proc loopVideoDownload(videoObj: Video) =
  stdout.styledWriteLine(fgWhite, "Downloading video for " & videoObj.metaData.name)
  while videoObj.downloadNextVideoPart("./$1.mp4" % [videoObj.metaData.name]):
    eraseLine()
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Got ", ForegroundColor.fgRed, $videoObj.videoCurrIdx, fgWhite, " of ", fgRed, $(videoObj.videoStream.len), " ", fgGreen, "Mem: ", $getOccupiedMem(), "/", $getFreeMem())
    cursorUp 1
  cursorDown 1
  if videoObj.audioStream.len > 0:
    stdout.styledWriteLine(fgWhite, "Downloading audio for " & videoObj.metaData.name)
    while videoObj.downloadNextAudioPart("./$1.ts" % [videoObj.metaData.name]):
      stdout.styledWriteLine(ForegroundColor.fgWhite, "Got ", ForegroundColor.fgRed, $videoObj.audioCurrIdx, fgWhite, " of ", fgRed, $(videoObj.audioStream.len), " ", fgGreen, "Mem: ", $getOccupiedMem(), "/", $getFreeMem())
      cursorUp 1
      eraseLine()
    cursorDown 1
proc downloadCheck(videoObj: Video): string =
  if fileExists("./$1.dfo" % [videoObj.metaData.name]):
    var data: seq[string] = split(readAll(open("./$1.dfo" % [videoObj.metaData.name], fmRead)), '@')
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
  if dirExists("./" & mdataObj.name):
    try:
      return OpenEpub3AndRebuild(mdataList, "./" & mdataObj.name)
    except:
      # If the directory isn't a valid EPUB, remove it and create a new one.
      removeDir("./" & mdataObj.name)
      return CreateEpub3(mdataList, "./" & mdataObj.name)
  else:
    return CreateEpub3(mdataList, "./" & mdataObj.name)

block cmld:
  var argList: tuple[sel: string, dwnld: bool, url: string, limit: bool, lrLimit: array[2, int], custom: bool, customName: string] =
    ("", false, "", false, [0, 0], false, "")
  proc NovelDownload(novelObj: var SNovel) =
    novelObj.setMetaData()
    novelObj.setChapterSequence()
    var epb = SetupEpub(novelObj.metaData)
    var i: int = 0
    var r: int = novelObj.chapters.len
    if argList.limit:
      i = argList.lrLimit[0]
      r = argList.lrLimit[1]
    while i < r:
      eraseLine()
      stdout.styledWriteLine(fgRed, $i, "/", $r, " ", fgWhite, novelObj.chapters[i].name[0..10], " ", fgGreen, "Mem: ", $getOccupiedMem(), "/", $getFreeMem())
      cursorUp 1
      if epb.CheckPageExistance(novelObj.chapters[i].name):
        continue
      var nodes: seq[TiNode] = novelObj.getNodes(novelObj.chapters[i])
      AddPage(epb, GeneratePage(novelObj.chapters[i].name, nodes))
      inc i
    cursorDown 1
    var coverBytes: string = ""
    try:
      coverBytes = novelObj.ourClient.getContent(novelObj.metaData.coverUri)
    except:
      stdout.styledWriteLine(fgRed, "Could not get novel cover, does it exist?")
    AssignCover(epb, Image(name: "cover.jpeg", imageType: ImageType.jpeg, bytes: coverBytes))
    FinalizeEpub(epb)
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
      novelObj = (SNovel)GenerateNewNovelInstance("NovelHall", argList.url)
    block engage:
      if argList.dwnld:
        NovelDownload(novelObj)
        break engage
      setMetaData(novelObj)
      echo $novelObj.metaData
  if paramCount() <= 1:
    break cmld
  block argLoop:
    var i: int = 0
    while i < paramCount():
      inc i
      case paramStr(i):
        of "-d":
          argList.dwnld = true
          inc i
          argList.url = paramStr(i)
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
        else:
          argList.sel = paramStr(i)
    break argLoop
  case argList.sel:
    of "nvl":
      NovelManager()
    else:
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
                    AnimeSelector, Anime, AnimeSearch, AnimeUrlInput, AnimeDownload,
                    Manga, MangaSearch, MangaUrlInput, MangaDownload
  
  var usrInput: string
  var downBulk: bool
  var curSegment: Segment = Segment.Welcome
  var novelObj: SNovel
  var videoObj: Video

  proc SetUserInput() =
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
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
    var idx: int = 1
    var vLines: seq[string] = @["0) NovelHall"]
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
      if usrInt == 0:
        curSegment = Segment.Novel
        return
      # Minus 1, since we have to account for ADLCore default, NovelHall
      novelObj = SNovel(script: GenNewScript(nvlScripts[usrInt - 1].scriptPath))
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
        if novelObj == nil:
          novelObj = (SNovel)GenerateNewNovelInstance("NovelHall", "")
        curSegment = Segment.NovelSearch
        break
      elif usrInput[0] == '2':
        curSegment = Segment.NovelUrlInput
        break
  proc NovelSearchScreen() =
    stdout.styledWrite(ForegroundColor.fgWhite, "Enter Search Term:")
    SetUserInput()
    var mSeq: seq[MetaData] = @[]
    mSeq = novelObj.searchDownloader(usrInput)
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
      if novelObj.script == nil: novelObj = (SNovel)GenerateNewNovelInstance("NovelHall", mSeq[parseInt(usrInput)].uri)
      else: novelObj.defaultPage = mSeq[parseInt(usrInput)].uri
      curSegment = Segment.NovelDownload
      break
  proc NovelUrlInputScreen() =
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Paste/Type URL:")
    stdout.styledWrite(ForegroundColor.fgGreen, "0 > ")
    usrInput = readLine(stdin)
    if novelObj == nil: novelObj = (SNovel)GenerateNewNovelInstance("NovelHall",  usrInput)
    else: novelObj.defaultPage = usrInput
    curSegment = Segment.NovelDownload
  proc NovelDownloadScreen() =
    var mdataObj: MetaData
    var chpSeq: seq[Chapter] = @[]
    block novelData:
      novelObj.setChapterSequence()
      novelObj.setMetaData()
      chpSeq = novelObj.chapters
      mdataObj = novelObj.metaData
    var idx: int = 1
    var epub3 = SetupEpub(mdataObj)
    for chp in chpSeq:
      eraseLine()
      stdout.styledWriteLine(fgRed, $idx, "/", $chpSeq.len, " ", fgWhite, chp.name, " ", fgGreen, "Mem: ", $getOccupiedMem(), "/", $getFreeMem())
      cursorUp 1
      if epub3.CheckPageExistance(chp.name):
        continue
      var nodes: seq[TiNode] = novelObj.getNodes(chp.uri)
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
    stdout.styledWriteLine(fgWhite, "1) VidStream\t2)HAnime")
    while true:
      SetUserInput()
      if usrInput == "1":
        currScraperString = "vidstreamAni"
      elif usrInput == "2":
        currScraperString = "HAnime"
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
      SetUserInput()
      if usrInput.len > 1 or ord(usrInput[0]) <= ord('0') and ord(usrInput[0]) >= ord('8'):
        stdout.styledWriteLine(ForegroundColor.fgRed, "ERR: Doesn't seem to be valid input 0-8")
        continue
      videoObj = GenerateNewVideoInstance(currScraperString, mSeq[parseInt(usrInput)].uri)
      discard videoObj.getMetaData()
      discard videoObj.getStream()
      curSegment = Segment.AnimeDownload
      break
  proc AnimeUrlInputScreen() =
    stdout.styledWriteLine(ForegroundColor.fgWhite, "Paste/Type URL:")
    SetUserInput()
    videoObj = GenerateNewVideoInstance(currScraperString,  usrInput)
    discard videoObj.getMetaData()
    discard videoObj.getStream()
    curSegment = Segment.AnimeDownload
      # TODO: merge formats.
  proc AnimeDownloadScreen() =
    # Not Finalized
    assert videoObj != nil
    if downBulk == false:
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
      videoObj.selResolution(selMedia)
      loopVideoDownload(videoObj)
    else:
      let mData = videoObj.getEpisodeSequence()
      for meta in mData:
        videoObj = GenerateNewVideoInstance("vidstreamAni", meta.uri)
        discard videoObj.getMetaData()
        discard videoObj.getStream()
        let mResL = videoObj.listResolution()
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
        videoObj.selResolution(mResL[selector])
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
    discard novelObj.getChapterSequence
    discard novelObj.getMetaData()
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
      let nodes = novelObj.getNodes(chp)
      AddPage(epub3, GeneratePage(chp.name, nodes))
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
      of Segment.AnimeDownload: AnimeDownloadScreen()
      
      of Segment.Manga: MangaScreen()
      of Segment.MangaSearch: MangaSearchScreen()
      of Segment.MangaUrlInput: MangaUrlInputScreen()
      of Segment.MangaDownload: MangaDownloadScreen()
