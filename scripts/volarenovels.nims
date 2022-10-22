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

proc SetID*(id: int) =
  scriptID = id

proc AddHeader*(k: string, v: string) =
  defaultHeaders.add((k, v))
proc getHeaders*(): seq[tuple[key: string, value: string]] =
  return defaultHeaders
proc procHttpTest*(): string =
  return processHttpRequest("newtab", scriptID, defaultHeaders, true)

proc GetMetaData*(uri: string): MetaData =