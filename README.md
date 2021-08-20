
Note on development:
	
	Since a lot of my classes are beginning to start, development of anime-dl may start to stall in regards to update patterns; I will still attempt to work on this in my free time, but between life, school, and a number of projects, I can not guarantee new features or updates for awhile.
	However, I will still fix any bugs that crop up quickly, but I must ask those, who use or modify this project, to help me in finding bugs, thank you!
	Also, since my development environment has moved from mainly windows-10 to linux with a windows guest, expect better support for linux.


Note on Server-Modification:

	This is a version of ADLCore that has been modified so that it can function in a server-environment more easily.

# Features
1. 	Anime downloading on supported sites
	
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
	
3. Supported Manga
	
	reagmanganata.com
	
	mangakakalot.com
	
	manganato.com

## Example Usage (outdated)

[![Example](https://img.youtube.com/vi/YgfuUqdk1fw/0.jpg)](https://www.youtube.com/watch?v=YgfuUqdk1fw)

To download an anime:
'anime-dl ani -s Jerou -d' OR run the EXE and type: 'ani -s Jerou -d'

Explanation: 
The "ani" switch tells the program to select the anime downloader, the -s switch tells the program to search for an anime called "Jerou," the -d switch tells the program to download the anime.


To download a novel and export it to epub:
>'anime -dl nvl -d -e {link to novel page}'

To convert an already downloaded novel to epub:
> 'anime-dl nvl -e {path to novel on disk}'

### Misc
Feel free to create issues as it's lax around here, and you can also [dm me on twitter](https://twitter.com/shujiandou "dm me on twitter") for conversations, support, or with any suggestions.

#### Release Schedule
You can expect a new release with improvements every 1/2 week(s) on Monday

##### Current Command Help

1. Anime
 
2. -c & -skip skip already downloaded videos
3. -cc (HAnime) Continue after downloading one episode to the next available.
4. -S Search flag
5. -gS GoGoAnime search
6. -hS HAnime search
7. -tS Twist.Moe search
8. -stream launch a stream to vlc (experimental)

1. Novels:
2. -e export to epub 

1. General:
2. -d download flag
3. -mt multithreading flag.
4. ani & -aS anime flag
5. nvl & -nS novel flag
6. -h help command (outdated)
7. -l set specific locations to export data.
8. -resume resume downloads (not in use)
9. -range specify ranges 0-10, 100-400, etc.
