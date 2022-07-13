import os, strutils
import illwill

import ./Types/ArgumentObject


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

illwillInit(fullscreen=true)
setControlCHook(exitProc)
hideCursor()

var tb: TerminalBuffer

var strSelector: seq[seq[string]] = @[@[],
                                      @["<Help>", "<Anime>", "<Novel>", "<Manga>"],
                                      @["<Search>", "<Read>", "<Download>"]]
var cSelected: int = 0

proc MainHeadInfo(): void =
    tb.write(0, 0, "<ESC> Goes back, Q, Control + C, quits")

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

var currScene: int = 1

while true:
    var key = getKey()
    case key:
    of Key.Escape, Key.Left:
      if(currScene == 0):
        exitProc()
      else:
        dec currScene
    of Key.Q: exitProc()
    of Key.Tab, Key.Down:
      if(cSelected >= len(strSelector[currScene]) - 1):
        cSelected = 0
      else:
        inc cSelected
    of Key.Up:
      if(cSelected == 0):
        cSelected = len(strSelector[currScene])
      dec cSelected
    of Key.Enter, Key.Right:
      case currScene:
        of 0:
          currScene = 1
        # Welcome Page
        of 1:
          case cSelected:
            # HELP
            of 0:
              currScene = 0
            # ANIME
            of 1:
              currScene = 1
            # NOVEL
            of 2:
              currScene = 2
            # MANGA
            of 3: discard
            else: discard
        # novel-dl submodule
        of 2: discard
        else: discard
    else: discard
    case currScene:
      of 0:
        HelpScreen()
      of 1:
        WelcomeScreen()
      of 2:
        NovelScreen()
      else: echo key


    tb.display()

    sleep(20)
