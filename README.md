(In partnership with http://simple-anime.herokuapp.com/)

# anime-dl
The front-end for the anime-dl project from VrienStudio 
## Porting Progress

| SITE              | Search | Download      |
|-------------------|--------|---------------|
| NovelHall         | YES    | YES           |
| VidStream (anime) | NO     | MANIFEST ONLY |
| MangaKakalot      | NO     | NO            |

| NOVELS          | VIDEO           | MANGA |
|-----------------|-----------------|-------|
| Not Implemented | Not Implemented | NONE  |

## Goal Of This Port

The goal of this port is to greatly increase code readability, modularity, and increase ease of use.

The main backing library will be made public eventually for usage by others.

An experimental release will be released within the next weeks.

## Building
Requirements:
* [nim >= 1.6.6](https://nim-lang.org/install.html)
* nimble (should come preinstalled with nim)
* libzip

<br>Clone the repo, and build with SSL support:<br>
```
nimble build -d:ssl
```
## Usage
It should be self explanatory, if you use the UI by simply executing the executable.

Here's the general flags for the CLI invocation.

<h6>
<ul>
    <li>-e&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;| Export (only affects text based modules)</li>
    <li>"uri"&nbsp;&nbsp;&nbsp;&nbsp;| Downloads from uri</li>
    <li>"term"&nbsp;| Searches for valid term, if object within quotes is not an uri</li>
</ul>
</h6>

<br>
<br>
~Long Live Rhodesia~
