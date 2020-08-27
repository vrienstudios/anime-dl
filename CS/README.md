[![Discord Server](https://img.shields.io/discord/737835739529740308.svg?label=discord)](https://discord.gg/Jzxfy2U) 

# Vidstream Downloader
Hello! Welcome to our page, this application is perfect for downloading any anime series that is on the site vidstreaming.io.

# How does it work?
Well, it basically acts as a web crawler; it gets every link associated with the series that you wish to download, and then it gets the ids for those videos. Vidstream.io uses an ID system to decide which video the server will serve to you, so we can easily get the direct url to the content from this server through a get request. We then parse the response to get the video manifest so that we can see all the parts of the video and download them.

# How do I download VidStreaming.IO videos with this?
Well, there are multiple ways you can download videos with this. If you don't want to download from our app, you can leave out the -p and -mt option to just get usable links that can be used with youtube-dl.

# Usage / How to
example usage: VidStreamIORipper.exe -S "Legend of the Galactic Heroes"

Also, before you complain about links being invalid, links expire rougly a day after they were made.

# Available parameters:

Note: You can not currently do links from arguments alone. If you have a link, and not a search term, open the program.

-S | Search option incase you only know the name of the show. Search query should follow.

-d | Enables progressive download. Only works with the -S parameter at the moment.

-mt | Enables multi threading support; download 3 videos simultaneously. 

example usage: VidStreamIORipper.exe -S "Legend of the Galactic Heroes" -d

# Trouble shooting:

If you have any issues with the project/application, please reach out! We'll be extremely happy to help. You can create an issue here on the board, and we'll respond nearly immediately.

# I hope you found something interesting/useful here!

# Future - The Anime Download Project

In the future, this utility will support a plethora of sites and services allowing you to download your anime with ease.
