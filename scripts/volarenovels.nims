# name:VolareNovels
# scraperType:nvl
# version:0.0.1
# siteUri:https://www.volarenovels.com/

import std/[htmlparser, xmltree]
var defaultHeaders: seq[tuple[key: string, value: string]] = @[
  ("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0"),
  ("Accept-Encoding", "identity"), ("Accept", "*/*")]

var page: XmlNode
var currPage: string
var scriptID: int
# SET THIS, IF YOU DON'T WANT TO CALL  GETMETADATA
var defaultPage: string

proc SetID*(id: int) =
  scriptID = id
proc SetDefaultPage*(page: string) =
  echo "def: " & page
  defaultPage = page

proc AddHeader*(k: string, v: string) =
  defaultHeaders.add((k, v))
proc getHeaders*(): seq[tuple[key: string, value: string]] =
  return defaultHeaders
proc procHttpTest*(): string =
  return processHttpRequest("newtab", scriptID, defaultHeaders, true)

proc GetChapterSequence*(): seq[Chapter] =
  var chapSequence: seq[Chapter] = @[]
  if defaultPage != currPage:
    currPage = defaultPage
    page = parseHtml(processHttpRequest(defaultPage, scriptID, defaultHeaders, false))
  var idx: int = 0
  let mainChapterNode = parseHtml(SeekNode($page, "<div id=\"TableOfContents\" class=\"tab-pane fade in active\">")).child("div")
  for divider in mainChapterNode.items:
    if divider.kind != xnElement or divider.tag != "div":
      continue
    let body = parseHtml(SeekNode(divider.innerText, "<div id=\"heading-0\" class=\"panel-heading\" role=\"tab\">")).child("div")
    for row in body.items:
      if row.kind != xnElement:
        continue
      let liList = parseHtml(SeekNode(row.innerText, "<ul class=\"list-unstyled list-chapters\">"))
      for liEl in liList.items:
        if liEl.kind != xnElement:
          continue
          # Should probably sanitize chapter name.
        chapSequence.add Chapter(name: liEl.child("a").child("span").innerText, number: idx, uri: liEl.child("a").attr("href"))
        inc idx

proc GetNodes*(chapter: Chapter): seq[TiNode] =
  var tinodes: seq[TiNode] = @[]
  let htmlData: string = processHttpRequest(chapter.uri, scriptID, defaultHeaders, false)
  let chapterNode: XmlNode = parseHtml(SeekNode(htmlData, "<div class=\"jfontsize_content fr-view\"/>"))
  # When it becomes available, search for italics and strong identifiers.
  for p in chapterNode.items:
    if p.kind == xnElement:
      tinodes.add TiNode(kind: NodeKind.paragraph, text: p.innerText)
  #Nicht dein spiel
  return tinodes
proc GetMetaData*(): MetaData =
  currPage = defaultPage
  let ovNode: XmlNode = parseHtml(SeekNode(processHttpRequest(currPage, scriptID, defaultHeaders, false), "<div class=\"m-b-60\">"))
  page = ovNode
  let mainNode: XmlNode = parseHtml(SeekNode($ovNode, "<div class=\"md-d-table m-lr-20\">"))
  var mdata: MetaData = MetaData()
  mdata.coverUri = mainNode.child("div").child("img").attr("src")
  let ndat: XmlNode = parseHtml(SeekNode($mainNode, "<div class=\"p-tb-10-rl-30\">"))
  mdata.name = ndat.child("h3").innerText
  mdata.author = ndat.child("p").innerText
  # Not the actual description/synopsis.
  #mdata.description = SeekNode(ndat.innerHtml, "<div class=\"description\">").innerText
  # Details is the first div, TOC is 2nd.
  mdata.description = parseHtml(SeekNode($ovNode, "<div class=\"recommended discoverWrapper p-30\">")).child("div").child("div").innerText
  return mdata