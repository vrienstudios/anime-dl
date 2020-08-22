# Vidstream Downloader
This is app is for downloading anime and other shows from Vidstreaming.io. Report all problems to the issues page.

You can download any vidstream anime or vidstreaming media with this.

# How does it work?
Well, it basically acts as a web crawler; it gets every link associated with the series that you wish to download, and then it gets the ids for those videos. Vidstream.io uses an ID system to decide which video the server will serve to you. We can easily get the direct url to the content from this server through a get request.

# How do I download VidStreaming.IO videos with this?
You feed the links thrown into the console to youtube-dl.

# Usage / How to
Right now, Vidstream Downloader is in its infancy, and a lof of features are missing. However, it still serves its purpose.

example usage: VidStreamIORipper.exe -S "Legend of the Galactic Heroes"

Also, before you complain about links being invalid, links expire rougly a day after they were made.

# Available parameters:

Note: Link/Search query should **ALWAYS** be the **LAST** parameter given. **These parameters changes by version**

-S | Search option incase you only know the name of the show.

-pD | Enables progressive download. Only works with the -S parameter at the moment.

-mt | Enables multi threading support; download 3 videos simultaneously. 

example usage: VidStreamIORipper.exe -S -pD "Legend of the Galactic Heroes"

# Trouble shooting:

If you have any issues with the project/application, please reach out! We'll be extremely happy to help. You can create an issue here on the board, and we'll respond nearly immediately.

# I hope you found something interesting/useful here!

# Future - The Anime Download Project

In the future, this utility will support a plethora of sites and services allowing you to download your anime with ease.
