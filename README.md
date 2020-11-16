[![Discord Server](https://img.shields.io/discord/737835739529740308.svg?label=discord)](https://discord.gg/Jzxfy2U) ![CodeQL](https://github.com/vrienstudios/vidstreamdownloader/workflows/CodeQL/badge.svg)

For quick replies, you can dm me on twitter: https://twitter.com/shujiandou

# Anime Downloader
Hello! Welcome to our page, this application is perfect for downloading any anime or hentai series that is on the site vidstreaming.io, hanime, gogo-stream, and many more to come!

# How does it work?
Well, it basically acts as a web crawler; it gets every link associated with the series that you wish to download, and then it gets the ids for those videos. Vidstream.io uses an ID system to decide which video the server will serve to you, so we can easily get the direct url to the content from this server through a get request. We then parse the response to get the video manifest so that we can see all the parts of the video and download them.

# Usage / How to
example usage: VidStreamIORipper.exe -S Legend of the Galactic Heroes -d

Example usage 2:
[![Example](https://img.youtube.com/vi/YgfuUqdk1fw/0.jpg)](https://www.youtube.com/watch?v=YgfuUqdk1fw)

# Available parameters:

-S | (Optional) Search option incase you only know the name of the show. Search query should follow.

-d | (Optional) Enables progressive download.

-mt | (Optional) Enables multi threading support; download 2 videos simultaneously. 

-c | (Optional) Skip any files already downloaded -- does not work with -h

-h | (Optional) Download from HAnime; -C or -mt has no impact on this flag.

example usage: VidStreamIORipper.exe -S Legend of the Galactic Heroes -d

example usage w/ HAnime: VidStreamIORipper.exe -h -d

# Trouble shooting:

If you have any issues with the project/application, please reach out! We'll be extremely happy to help. You can create an issue here on the board, and we'll respond nearly immediately.


# Future - The Anime Download Project

In the future, this utility will support a plethora of sites and services allowing you to download your anime with ease.
