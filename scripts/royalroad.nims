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
var scriptID: int

proc SetID*(id: int) =
  scriptID = id

proc AddHeader*(k: string, v: string) =
  defaultHeaders.add((k, v))
proc getHeaders*(): seq[tuple[key: string, value: string]] =
  return defaultHeaders
proc procHttpTest*(): string =
  return processHttpRequest("newtab", scriptID, defaultHeaders, true)

proc GetChapterSequence(uri: string): seq[Chapter] =
  var chapSequence: seq[Chapter] = @[]
  if uri != currPage:
    currPage = uri
    page = parseHtml(processHttpRequest(uri, scriptID, defaultHeaders, false))
  var idx: int = 0
  let mainChapterNode = parseHtml(SeekNode($page, "<div id=\"TableOfContents\" class=\"tab-pane fade in active\">")).child("div")
  for divider in mainChapterNode.items:
    if divider.kind != xnElement or divider.tag != "div":
      continue
    let body = parseHtml(SeekNode(divider.innerHtml, "<div id=\"heading-0\" class=\"panel-heading\" role=\"tab\">")).child("div")
    for row in body.items:
      if row.kind != xnElement:
        continue
      let liList = parseHtml(SeekNode(row.innerHtml, "<ul class=\"list-unstyled list-chapters\">"))
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
      tinodes.add TiNode(kind: TextKind.p, text: p.innerText)
  #Nicht dein spiel
  return tinodes
proc GetMetaData*(uri: string): MetaData =
  currPage = uri
  let ovNode: XmlNode = parseHtml(SeekNode(processHttpRequest(uri, scriptID, defaultHeaders, false), "<div class=\"page-content-inner\" vocab=\"http://schema.org/\" typeof=\"Book\">"))
  page = ovNode
  let mainNode: XmlNode = page.child("div")
  var mdata: MetaData = MetaData()
  let ndat: XmlNode = mainNode.child("div")
  mdata.name = ndat.child("h3").innerText
  mdata.author = ndat.child("p").innerText
  # Not the actual description/synopsis.
  #mdata.description = SeekNode(ndat.innerHtml, "<div class=\"description\">").innerText
  # Details is the first div, TOC is 2nd.
  mdata.description = parseHtml(SeekNode($ovNode, "<div class=\"recommended discoverWrapper p-30\">")).child("div").child("div").innerText
  return mdata