# Package

version       = "3.1.2"
author        = "VrienStudio"
description   = "Downloader/Scraper for anime"
license       = "GPLv3"
srcDir        = "src"
bin           = @["animeDL"]

# Build tasks

# Managing git dependencies this way allows for easier development
# This task could be done in fancier ways, but this will do until we add more modules ;)
task installdeps, "Installs anime-dl dependencies from github":
    echo("Cloning dependencies from git...")
    exec("git clone https://github.com/ShujianDou/nim-HLSManager")
    exec("git clone https://github.com/ShujianDou/nim-epub")
    exec("git clone https://github.com/vrienstudios/ADLCore.git")
    exec("git clone https://github.com/ShujianDou/halonium.git")
    echo("Installing dependencies...")
    # It is important for the dependencies to be installed in this order.
    withDir "nim-HLSManager":
        exec("nimble install -Y")
    withDir "nim-epub":
        exec("nimble install -Y")
    withDir "ADLCore":
        exec("nimble install -Y")
    withDir "halonium": # This will remove your previous halonium.
      exec("nimble install -Y")

# Dependencies

requires "nim >= 1.6.6"
requires "ADLCore == 0.2.0"
requires "EPUB == 0.3.0"
requires "compiler"
