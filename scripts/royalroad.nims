# name:RoyalRoad
# scraperType:nvl
# version:0.0.1
# siteUri:https://www.royalroad.com/home

import std/[htmlparser, xmltree]
var defaultHeaders: seq[tuple[key: string, value: string]] = @[
  ("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0"),
  ("Accept-Encoding", "identity"), ("Accept", "*/*")]

var page: XmlNode
var currPage: string
var defaultPage: string
var scriptID: int

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

proc GetChapterSequence*(): seq[Chapter] =
  # Assume we are on correct page, and that page var was set.
  var tableBody = parseHtml(SeekNode($page, "<tbody>"))
  var chapters: seq[Chapter] = @[]
  for tableItem in tableBody.items:
    if tableItem.kind != xnElement:
      continue
    let chapterRowInfo = tableItem.child("td").child("a")
    var newChapter: Chapter = Chapter()
    newChapter.name = sanitizeString(chapterRowInfo.innerText)
    newChapter.uri = "https://www.royalroad.com" & chapterRowInfo.attr("href")
    chapters.add newChapter
  return chapters

proc GetNodes*(chapter: Chapter): seq[TiNode] =
  var tinodes: seq[TiNode] = @[]
  let htmlData: string = processHttpRequest(chapter.uri, scriptID, defaultHeaders, false)
  let chapterNode: XmlNode = parseHtml(SeekNode(htmlData, "<div class=\"chapter-inner chapter-content\"/>"))
  # When it becomes available, search for italics and strong identifiers.
  for p in chapterNode.items:
    if p.kind == xnElement:
      tinodes.add TiNode(kind: TextKind.p, text: p.innerText)
  #Nicht dein spiel
  return tinodes
proc GetMetaData*(): MetaData =
  currPage = defaultPage
  let pageContent = processHttpRequest(defaultPage, scriptID, defaultHeaders, false)
  page = parseHtml(pageContent)
  let ovNode: XmlNode = parseHtml(SeekNode(pageContent, "<div class=\"page-content-inner\">")).child("div")
  let coverUri = parseHtml(SeekNode($ovNode, "<div class=\"cover-art-container\">")).child("img").attr("src")
  let authorTitleNodeCombo = parseHtml(SeekNode($ovNode, "<div class=\"col-md-5 col-lg-6 text-center md-text-left fic-title\">")).child("div")
  let title = parseHtml(SeekNode($authorTitleNodeCombo, "<h1 class=\"font-white\">")).innerText
  let author = parseHtml(SeekNode($authorTitleNodeCombo, "<span>")).child("a").innerText
  var mdata: MetaData = MetaData()
  mdata.name = title
  mdata.author = author
  mdata.coverUri = coverUri
  # Not the actual description/synopsis.
  #mdata.description = SeekNode(ndat.innerHtml, "<div class=\"description\">").innerText
  # Details is the first div, TOC is 2nd.
  mdata.description = parseHtml(SeekNode(pageContent, "<div class=\"description\">")).child("div").innerText
  return mdata