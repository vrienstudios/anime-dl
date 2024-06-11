# Package

version       = "3.1.2"
author        = "VrienStudio"
description   = "Downloader/Scraper for anime"
license       = "GPLv3"
srcDir        = "src"
bin           = @["animeDL"]

task installdeps, "Installs anime-dl dependencies from github":
    echo("Cloning dependencies from git...")
    createDir "libs/"
    withDir "libs/":
        exec("git clone https://github.com/ShujianDou/nim-HLSManager")
        exec("git clone https://github.com/ShujianDou/nim-epub")
        exec("git clone https://github.com/vrienstudios/ADLCore.git")
    echo("Installing dependencies...")
    # It is important for the dependencies to be installed in this order.
    exec "nimble install compiler"
    exec "nimble install halonium"
    exec "nimble install nimcrypto"
    exec "nimble install nimscripter"
    exec "nimble install zippy"
    exec "nimble install checksums"
    withDir "libs/nim-HLSManager":
        exec("nimble install -Y")
    withDir "libs/nim-epub":
        exec("nimble install -Y")
    withDir "libs/ADLCore":
      exec("nimble install -Y")

# Dependencies

requires "nim >= 1.6.6"
requires "ADLCore == 0.2.0"
requires "EPUB == 0.3.0"
requires "compiler"
