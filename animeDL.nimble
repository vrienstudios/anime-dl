# Package

version       = "0.1.0"
author        = "ShujianDou"
description   = "Downloader/Scraper for anime"
license       = "Proprietary"
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
    echo("Installing dependencies...")
    # It is important for the dependencies to be installed in this order.
    withDir "nim-HLSManager":
        exec("nimble install")
    withDir "nim-epub":
        exec("nimble install")
    withDir "ADLCore":
        exec("nimble install")

# Dependencies

requires "nim >= 1.6.6"
requires "ADLCore"
requires "EPUB"
