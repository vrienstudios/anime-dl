import clipboard/abstract_clipboard
export abstract_clipboard

when defined(macosx) and not defined(ios):
  import clipboard/clipboard_mac
  export clipboard_mac

elif defined(windows):
  import clipboard/clipboard_win
  export clipboard_win

elif defined(js) or defined(emscripten):
  import clipboard/clipboard_web
  export clipboard_web

elif defined(linux) and not defined(android):
  import clipboard/clipboard_x11
  export clipboard_x11

else:
  proc clipboardWithName*(name: string): Clipboard = result.new()
