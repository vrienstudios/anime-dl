# anime-dl
> <strong>The front-end for the anime-dl project</strong>
## Table of Contents
- [Motive & Vision](#motive--vision)
- [Site Status](#site-status)
- [Installation](#installation)
- [Building](#building)
- [Usage](#usage)

```
The backing library: [ADLCore](https://github.com/vrienstudios/ADLCore)

Have any ideas or an issue? Feel free to create an issue or talk to us in the [Discord](https://discord.gg/WYTxbt2)
```
## Site Status

| SITE                 | Search   | Download |
|----------------------|----------|----------|
| NovelHall.com [Novel]    | YES      | YES      |
| anihdplay.com [Anime]    | YES      | YES      |
| movstreamhd.pro [Tv]          | YES      | (SOME)      |
| HAnime.tv [Hentai]      | YES      | YES      |
| MangaKakalot.com [Manga] | YES      | YES      |

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
animeDL sel flags
    e.x animeDL nvl -url url -d
        animeDL nvl -d -c NovelHall -s Nicht

-sel (Selectors) | Choose from "nvl" and "ani" components.
-url url | Specify the url of the item you want downloaded.
-d | Is the download flag; if not passed, it will only return metadata.
-lim num:num | Is to specify a range of chapters/episodes you want to download.
-c name | (Testing) Set a custom downloader to call instead of default-- works with scripts.
-cauto | (Incomplete) Automatically set a downloader based on the url
-dblk | (Testing) Download entire series instead of single episode.
-res h (highest) or l (lowest) or widthxheight| (Testing) Specify a resolution to try to download.
-ds | (Incomplete) Do not delete temporary folders generated for EPUB.
-s term | Requires -c to be set.
```



https://user-images.githubusercontent.com/13159328/185725402-1425974b-2b15-4a79-99b4-ed4634f67c23.mp4

