# This is just an example to get you started. You may wish to put all of your
# tests into a single file, or separate them into multiple `test1`, `test2`
# etc. files (better names are recommended, just make sure the name starts with
# the letter 't').
#
# To run these tests, simply execute `nimble test`.

import clipboard
import asyncdispatch, os, times

proc test() =
  let p = clipboardWithName(CboardGeneral)
  p.writeString("hi")
  # echo "wrote"
  # var s: string
  # if p.readString(s):
  #   echo "ok: ", s
  # else:
  #   echo "not ok"

proc main() {.async.} =
  test()
  while true:
    # echo "hi"
    await sleepAsync(1000)
    echo now()
    echo "AVAILABLE FORMATS: ", clipboardWithName(CboardGeneral).availableFormats()
    var s: string
    if clipboardWithName(CboardGeneral).readString(s):
      echo "CONTENT: ", s
    else:
      echo "NO STRING CONTENT"

    var image: seq[byte]
    if clipboardWithName(CboardGeneral).readData("image/png", image):
      echo "IMAGE CONTENT: ", image.len
      # writeFile("/tmp/image.png", image)

waitFor main()
