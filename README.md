(In partnership with http://simple-anime.herokuapp.com/)

# anime-dl
> <strong>The front-end for the anime-dl project</strong>
## Table of Contents
- [Motive & Vision](#motive--vision)
- [Site Status](#site-status)
- [Installation](#installation)
- [Building](#building)
- [Usage](#usage)

## Motive & Vision
Though internet is available nearly everywhere, what happens, when, say, it drops for a week?
What happens if power is gone for a week?

You lose access to all the content within the internet; novels, tv shows, movies.
With the popularity of subscription services and licensing, more and more content is being hosted entirely online, without the ability to download or archive it.

This is also a huge problem within the software and gaming industries as a whole;
a large reliance on online drm has ruined offline gaming and productivity.

The goal here is to provide a solution related to online media, when there's no internet.

We want to ship a lightweight and expandable executable via NimScript,
which will allow you to download and store media locally from, virtually, any site with a bit of work.

If you share our vision, feel free to head over to [ADLCore](https://github.com/vrienstudios/ADLCore), the backbone of this project. Or, feel free to contribute here too.

Have any ideas or an issue? Feel free to create an issue or talk to us in the [Discord](https://discord.gg/WYTxbt2).
## Site Status

| SITE                 | Search | Download |
|----------------------|--------|----------|
| NovelHall [Novel]    | YES    | YES      |
| LightNovelPub        | (WIP)  | (WIP)    |
| VidStream [Anime]    | YES    | YES      |
| HAnime [Hentai]      | YES    | YES      |
| MangaKakalot [Manga] | YES    | YES      |

| NOVELS        | VIDEO     | MANGA         |
|---------------|-----------|---------------|
| NovelHall     | VidStream | MangaKakalot  |
| LightNovelPub | HAnime    |               |

## Installation
Download the latest release from the [releases page](https://github.com/vrienstudios/anime-dl/releases)

## Building
Requirements:
* [nim >= 1.6.6](https://nim-lang.org/install.html)
* nimble (should come preinstalled with nim)
* [git](https://git-scm.com/)
* OpenSSL
    * Linux:
        * ``sudo pacman -S openssl``
        * ``sudo apt install openssl``
    * Windows:
        * https://wiki.openssl.org/index.php/Binaries

<strong>(Modification required for Windows Building)</strong>

<br>1. Clone the repo<br>
```
git clone https://github.com/vrienstudios/anime-dl.git && cd anime-dl
```
<br>2. Install required nim modules:<br>
```
nimble installdeps
```
<br>3. Build with SSL support:<br>
```
nimble build -d:ssl
```
## Usage
It should be self explanatory, if you use the UI by simply executing the executable.




https://user-images.githubusercontent.com/13159328/185725402-1425974b-2b15-4a79-99b4-ed4634f67c23.mp4

