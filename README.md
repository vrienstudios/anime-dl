# anime-dl
> <strong>The front-end for the anime-dl project</strong>

# NOTICE
Embtaku is down & Only certain sites work (see status)

> It may take awhile before I implement a new anime or movie site, since I don't consume this content that much anymore.

>Every now and then I will come back and continue working on adding a new anime site; to see current progress, look at issues.

>I forgot this, when I redesigned the project, but I will re-add the manga downloader over the weekend--

## Table of Contents
- [Site Status](#site-status)
- [Installation](#installation)
- [Building](#building)
- [Usage](#usage)


<br>The backing library: [ADLCore](https://github.com/vrienstudios/ADLCore)

<br>Have any ideas or an issue? Feel free to create an issue or talk to us in the [Discord](https://discord.gg/WYTxbt2)

> ## Site Status

| SITE                 | Search   | Download |
|----------------------|----------|----------|
| [Novel]    |       |       |
| NovelHall.com    | YES      | YES      |
| Shuba    | WIP      | WIP      |
| RoyalRoad.com  | NO | YES |
| [Manga] |       |       |
| MangaKakalot.com | WIP      | WIP      |
| [Anime]    | X      | X      |
| booster    | WIP      | WIP      |
| animeid.live (Espanol)    | NO      | YES      |
| [NSFW]    |       |       |
| HAnime.tv      | YES      | YES      |

## Installation
Download the latest release from the [releases page](https://github.com/vrienstudios/anime-dl/releases)

## Building
Requirements:
* [nim >= 1.6.6](https://nim-lang.org/install.html)
* nimble (should come preinstalled with nim)
* [git](https://git-scm.com/)
* OpenSSL
    * Linux:
        * (Arch-based) ``sudo pacman -S openssl``
        * (Debian-based) ``sudo apt install openssl``
    * Windows (If you don't want to use the ones we provide):
        * https://wiki.openssl.org/index.php/Binaries

<br>1. Clone the repo<br>
```
git clone https://github.com/vrienstudios/anime-dl.git && cd anime-dl
```
<br>2. Install required nim modules:<br>
> Note: It is recommended you to check out the dependencies in the nimble file before doing this.
```
nimble installdeps
```
<br>3. Build with these commands: <br>
```nimble build -d:ssl --threads:on```

## Usage
There are two ways to use the program--

You can simply execute the executable and follow the prompts, or you can follow the instructions below for simpler usage.
> Note: The documentation on the program arguments are subject to change and are not encompassing.

```
Help: 
	./animeDL selector options
	./animeDL ani -d -c HAnime -res x720 -url url
	./animeDL ani -d -c vidstreamAni -res 1920x1080 -url url
(Selectors)
	ani (Denominates video)
	nvl (Denominates text)
	mng (Denominates pictures)
(Options)
	-d (specifies to download)
	-lim num:num (limit the amount of episodes/chapters)
	-c name (Set a custom downloader, useful for scripts)
	-dblk (specify to download more than one episode)
	-res wxh (Can be buggy at times)
```



https://user-images.githubusercontent.com/13159328/185725402-1425974b-2b15-4a79-99b4-ed4634f67c23.mp4

