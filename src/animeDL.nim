import os, strutils
import illwill, clipboard
import ./Types/ArgumentObject
import ADLCore, ADLCore/genericMediaTypes, ADLCore/Novel/NovelTypes

proc processArgs() : string =
    return "Doesnt Accept Arguments Yet"

let r1Length: int = 80
let r1Height: int = 12

var b: int = paramCount() + 1

var argObject = ArgumentObject()
if b > 1:
    var i: int = 0
    while i < b:
        echo paramStr(i)
        inc i

proc exitProc() {.noconv.} =
  illwillDeinit()
  showCursor()
  quit(0)

illwillInit(fullscreen=false)
setControlCHook(exitProc)
hideCursor()

var tb: TerminalBuffer

var isDisplayingTextBox: bool = false
var textBox: seq[char] = @[]

var strSelector: seq[seq[string]] = @[@[],
                                      @["<Help>", "<Anime>", "<Novel>", "<Manga>"],
                                      @["<Search>", "<Read>", "<Download>"]]
var cSelected: int = 0

proc toString(str: seq[char]): string =
  result = newStringOfCap(len(str))
  for ch in str:
    add(result, ch)

proc MainHeadInfo(): void =
    tb.write(0, 0, "<ESC> Goes back, Q, Control + C, quits")

var currScene: int = 4

# Uses num of elements + 4.
proc WritePromptSelList(scene: int, rLength: int, sHeight: int): void =
  let eOff: int = rLength - 2
  var idx: int = 0
  #tb.write(1, sHeight, fgBlue, center("Hit <TAB> to cycle selection", eOff))
  tb.write(1, sHeight, fgBlue, repeat("=", rLength - 1))
  idx = idx + 2
  for st in strSelector[scene]:
    if(cSelected == idx - 2):
      tb.write(1, sHeight + idx, fgGreen, center("!" & st, eOff))
    else:
      tb.write(1, sHeight + idx, fgWhite, center(st, eOff))
    inc idx
  tb.write(1, sHeight + idx + 1, fgBlue, center("Hit <ENTER> to confirm", eOff))

proc WriteHeader(): void =
  MainHeadInfo()
  tb.setForegroundColor(fgGreen)
  tb.drawRect(0, 2, r1Length, 8, doubleStyle=true)

  tb.write(1, 4, fgWhite, center("Welcome To anime-dl!", r1Length - 2))
  tb.write(1, 5, fgWhite, center("anime-dl supports anime", r1Length - 2))
  tb.write(1, 6, fgWhite, center("novels, and manga", r1Length - 2))
  tb.display()

proc HelpScreen(): void =
  var xOff: int = r1Length - 2
  tb = newTerminalBuffer(terminalWidth(), terminalHeight())
  MainHeadInfo()
  tb.setForegroundColor(fgGreen)
  tb.drawRect(0, 2, r1Length, r1Height, doubleStyle = true)
  tb.write(1, 4, fgWhite, center("HELP", xOff))
  tb.write(2, 5, fgWhite, center("Welcome to the help section", xOff))
  tb.write(3, 6, fgWhite, alignLeft("If you pass args, you will be in cli mode", xOff - 1))
  tb.write(3, 7, fgWhite, alignLeft("For a full list of commands, visit the github", xOff - 1))
  tb.write(5, 8, fgWhite, alignLeft("https://github.com/vrienstudios/anime-dl", xOff - 3))

proc WelcomeScreen(): void =
  tb = newTerminalBuffer(terminalWidth(), terminalHeight())
  WriteHeader()
  tb.drawRect(0, 9, r1Length, 19, doubleStyle=true)
  WritePromptSelList(1, r1Length, 11)

proc NovelScreen(): void =
  tb = newTerminalBuffer(terminalWidth(), terminalHeight())
  MainHeadInfo()
  tb.setForegroundColor(fgGreen)
  tb.drawRect(0, 2, r1Length, r1Height, doubleStyle = true)
  tb.write(1, 4, fgWhite, center("novel-dl submodule", r1Length - 2))
  tb.write(1, 5, fgWhite, "Search | (not working)")
  tb.write(1, 6, fgWhite, "   Allows you to search for novels following a search term")
  tb.write(1, 7, fgWhite, "Read | (not working)")
  tb.write(1, 8, fgWhite, "   Allows you to read and keep track of your novels here")
  tb.write(1, 9, fgWhite, "Download | (WIP)")
  tb.write(1, 10, fgWhite, "   Downloads a novel to disk, and also gives an option to export to EPUB.")
  WritePromptSelList(2, 80, 14)

# Scene 6
proc NovelSearchScreenListObjects(site: string, term: string) =
  textBox = @[]
  tb = newTerminalBuffer(terminalWidth(), terminalHeight())
  MainHeadInfo()
  tb.setForegroundColor(fgGreen)
  tb.drawRect(0, 2, r1Length, 6, doubleStyle = true)
  tb.write(1, 4, fgWhite, center("Searching for " & term, r1Length - 2))
  tb.display()
  var novelObj = GenerateNewNovelInstance(site, "")
  var mData: seq[MetaData] = novelObj.searchDownloader(term)
  tb.write(1, 5, fgWhite, "Select a novel to GET")
  var row: int = 7
  for md in mData:
    if md.name.len <= r1Length - 2:
      tb.write(1, row, fgGreen, $(row - 7) & ": " & md.name[0..^1])
    else:
      tb.write(1, row, fgGreen, $(row - 7) & ": " & md.name[0..r1Length - 4 - ($row).len])
    inc row

  tb.drawRect(0, row, r1Length, row + 2, doubleStyle = false)
  tb.write(1, row + 1, fgGreen, "$>")
  while true:
    var k = getKey()
    if k == Key.Enter:
      currScene = 6
      break
    elif k == Key.BackSpace:
      textBox.delete(textBox.len() - 1)
      tb.write(3, row + 1, fgWhite, " ".repeat(r1Length - 3))
    elif k != Key.None:
      textBox.add(char(k.ord))
    tb.write(4, row + 1, fgWhite, toString(textBox))
    tb.display()
    sleep(20)

# Scene 5
proc NovelSearchScreenInputTerm(dwn: int): void =
  textBox = @[]
  tb = newTerminalBuffer(terminalWidth(), terminalHeight())
  MainHeadInfo()
  tb.setForegroundColor(fgGreen)
  tb.drawRect(0, 2, r1Length, 6, doubleStyle = true)
  tb.write(1, 4, fgWhite, center("novel-dl submodule", r1Length - 2))
  tb.write(1, 5, fgWhite, "Enter a search term:")
  isDisplayingTextBox = true
  tb.drawRect(0, 7, r1Length, 9, doubleStyle = false)
  tb.write(1, 8, fgGreen, "$>")
  while true:
    var k = getKey()
    if k == Key.Enter:
      currScene = 6
      break
    elif k == Key.BackSpace:
      textBox.delete(textBox.len() - 1)
      tb.write(3, 8, fgWhite, " ".repeat(r1Length - 3))
    elif k.ord == 22:
      var clipStr: string
      discard clipboardWithName(CboardGeneral).readString(clipStr)
      for c in clipStr:
        textBox.add(c)
    elif k != Key.None:
      textBox.add(char(k.ord))
    tb.write(4, 8, fgWhite, toString(textBox))
    tb.display()
    sleep(20)
  NovelSearchScreenListObjects("NovelHall", toString(textBox))

proc NovelSearchScreen(): void =
  tb = newTerminalBuffer(terminalWidth(), terminalHeight())
  MainHeadInfo()
  tb.setForegroundColor(fgGreen)
  tb.drawRect(0, 2, r1Length, 7, doubleStyle = true)
  tb.write(1, 4, fgWhite, center("novel-dl submodule", r1Length - 2))
  tb.write(1, 5, fgWhite, "Enter a number to select a downloader (default 0)")
  tb.write(1, 6, fgWhite, "0) NovelHall")
  isDisplayingTextBox = true
  tb.drawRect(0, 10, r1Length, 12, doubleStyle = false)
  tb.write(1, 11, fgGreen, "$>")

  while true:
    var k = getKey()
    if k == Key.Enter:
      currScene = 5
      break
    elif k == Key.BackSpace:
      if textBox.len != 0:
        textBox.delete(textBox.len() - 1)
        tb.write(3, 11, fgWhite, " ".repeat(r1Length - 3))
    elif k != Key.None:
      textBox.add(char(k.ord))
    tb.write(4, 11, fgWhite, toString(textBox))
    tb.display()
    sleep(20)
  NovelSearchScreenInputTerm(parseInt(toString(textBox)))

while true:
  #var key = getKey()
  #if key != Key.None:
  #  echo key.ord
  #sleep(20)
  case currScene:
      of 0:
        HelpScreen()
      of 1:
        WelcomeScreen()
      of 2:
        NovelScreen()
      of 4:
        NovelSearchScreen()
      of 5:
        discard
      else: discard


  tb.display()

  sleep(20)
