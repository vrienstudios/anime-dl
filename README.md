### NOTE: This is the C# version of anime-dl, there is also a [JS](https://github.com/vrienstudios/anime-dl/tree/JS) version in case you're interested in that. For differences between these two versions please see this entry [Differences between C# and JS versions](https://github.com/vrienstudios/anime-dl/wiki/Differences-between-C%23-and-JS-versions), although you should be generally fine using the C# version.


(In partnership with http://simple-anime.herokuapp.com/)

# C# Features

1. Anime downloading on supported sites
	
	  HAnime
	
	  Vidstreaming Umbrella
	
	  vidcloud9.com
		
	  vidstreaming.io
		
	  gogo-stream.com
		
	  animeid.to
	
	  Twist.Moe

2. Supported Novels

	AsianHobbyist.com
	
	Wuxiaworld.co
	
	Wuxiaworld.com
	
	Scribblehub.com
	
	NovelFull.com

	NovelHall.com
	
	volarenovels.com
	
3. Supported Manga
	
	reagmanganata.com
	
	mangakakalot.com
	
	manganato.com

## Example Usage (outdated)

[![Example](https://img.youtube.com/vi/YgfuUqdk1fw/0.jpg)](https://www.youtube.com/watch?v=YgfuUqdk1fw)
[![https://imgur.com/TVNENWX.png](https://imgur.com/TVNENWX.png)](https://imgur.com/TVNENWX.png)

To download an anime:
'anime-dl ani -gS Jerou -d' -- With the advent of our UI, this will become much easier.

Explanation: 
The "ani" switch tells the program to select the anime downloader, the -gS switch tells the program to search for an anime called "Jerou" on gogo-anime the -d switch tells the program to download the anime.


To download a novel and export it to epub:
>'anime -dl nvl -d -e {link to novel page}'

To convert an already downloaded novel to epub:
> 'anime-dl nvl -e {path to novel on disk}'

### Misc
Feel free to create issues as it's lax around here, and you can also [dm me on twitter](https://twitter.com/shujiandou "dm me on twitter") for conversations, support, or with any suggestions.


-h for help, and you can also view our wiki.

#### UI WIP Images
[![https://media.discordapp.net/attachments/295705408373391361/945525317651558400/unknown.png?width=919&height=479](https://media.discordapp.net/attachments/295705408373391361/945525317651558400/unknown.png?width=919&height=479)

[![https://cdn.discordapp.com/attachments/904850064768389140/956274737905012837/Screenshot_20220323_152436.png](https://cdn.discordapp.com/attachments/904850064768389140/956274737905012837/Screenshot_20220323_152436.png)

The UI in the C# version is a complete WIP, as you can see. However, we hope to have a fully functional UI by 1st of August, 2022

##### C# Status - 27/3/22

Updates to anime-dl, C#, will be extremely slow for the coming months, since I can only find time to work on it on the weekends currently.
