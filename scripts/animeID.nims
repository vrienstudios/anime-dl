# name:animeID (Spanish)
# scraperType:ani
# version:0.0.1
# siteUri:https://animeid.live/

var defaultHeaders: seq[tuple[key: string, value: string]] = @[
  ("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0"),
  ("Accept-Encoding", "identity"), ("Accept", "*/*")]

var 
    page: XmlNode
    scriptID: int
    baseUri: string = "https://animeid.live/"
    defaultPage: string = ""
    currPage: string = ""

proc SetID*(id: int) =
  scriptID = id
proc SetDefaultPage*(page: string) =
  defaultPage = page
proc AddHeader*(k: string, v: string) =
  defaultHeaders.add((k, v))
proc getHeaders*(): seq[tuple[key: string, value: string]] =
  return defaultHeaders
proc procHttpTest*(): string =
  return processHttpRequest("newtab", scriptID, defaultHeaders, true)
proc GetHLSStream*(): HLSStream =
    var 
        ajaxUri = baseUri & "ajax.php?" & (parseHtml(SeekNode($page, "<div class=\"play-video\">")).child("iframe").attr("src").split("?")[^1]) & "&refer=none"
        jsonRespObj = parseJson(processHttpRequest(ajaxUri, scriptID, defaultHeaders, false))
        m3Uri = jsonRespObj["source"].getElems()[0]["file"].getStr()
        hls = parseManifestInterp(processHttpRequest(m3Uri, scriptID, defaultHeaders, false))
    hls.baseUri = join(m3Uri.split('/')[0..^2], "/") & "/"
    return hls
proc GetMetaData*(): MetaData =
  var mdata = MetaData()
  if currPage != defaultPage:
    page = parseHtml(processHttpRequest(defaultPage, scriptID, defaultHeaders, false))
    currPage = defaultPage
  let 
    videoInfoLeft: XmlNode = parseHtml(SeekNode($page, "<div class=\"video-info-left\">"))
    videoDetails = parseHtml(SeekNode($videoInfoLeft, "<div class=\"video-details\">"))
  assert videoInfoLeft != nil
  mdata.name = videoInfoLeft.child("h1").innerText
  mdata.series = sanitizeString(videoDetails.child("span").innerText)
  mdata.description = sanitizeString(videoDetails.child("div").child("div").innerText)
  return mdata
proc GetResolutions*(mainStream: HLSStream): seq[MediaStreamTuple] =
  var medStream: seq[MediaStreamTuple] = @[]
  var index: int = 0
  for segment in mainStream.parts:
    if segment.header == "#EXT-X-MEDIA:":
      var id: string
      var language: string
      var uri: string
      for param in segment.values:
        case param.key:
          of "GROUP-ID": id = param.value
          of "LANGUAGE": language = param.value
          of "URI":
            uri = param.value
          else: discard
      medStream.add((id: id, resolution: "", uri: uri, language: language, isAudio: true, bandWidth: ""))
    elif segment.header == "#EXT-X-STREAM-INF:":
      var bandwidth: string
      var resolution: string
      var id: string
      var uri: string
      for param in segment.values:
        case param.key:
          of "BANDWIDTH": bandwidth = param.value
          of "RESOLUTION": resolution = param.value
          of "AUDIO": id = param.value
          else: discard
      uri = indexStreamHead(mainStream.parts[index + 1], "URI")
      medStream.add((id: id, resolution: resolution, uri: mainStream.baseUri & uri, language: "", isAudio: false, bandWidth: bandwidth))
    inc index
  return medStream
proc Search*(str: string): seq[MetaData] =
  let content = processHttpRequest(baseUri & "ajax-search.html?keyword=" & str & "&id=-1", scriptID, defaultHeaders, false)
  let json = parseJson(content)
  var results: seq[MetaData] = @[]
  page = parseHtml(json["content"].getStr())
  currPage = baseUri
  for a in page.findAll("a"):
    var data = MetaData()
    data.name = a.innerText
    data.uri = baseUri & a.attr("href")
    results.add(data)
  return results
proc SetResolution*(mBase: tuple[s1: MediaStreamTuple, s2: string]): tuple[video: seq[string], audio: seq[string]] =
  var vManifest = parseManifestInterp(processHttpRequest(mBase.s1.uri, scriptID, defaultHeaders, false), mBase.s2)
  var vSeq: seq[string] = @[]
  for part in vManifest.parts:
    if part.header == "URI":
      vSeq.add(part.values[0].value)
  return (vSeq, @[])
proc GetNextVideoPart*(idx: int, videoStream: seq[string]): string =
  return processHttpRequest(videoStream[idx], scriptID, defaultHeaders, false)