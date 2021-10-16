<img src="https://repository-images.githubusercontent.com/280795543/9ae82b00-2747-11eb-83a2-49a3d467c9ec?width=100&height=100" alt="anime-dl" width="150" height="130"><br>

# anime-dl (JS)

> A version of anime-dl written in JavaScript

## Table of Contents

-   [Installation](#installation)
-   [Usage](#usage)
-   [Examples](#examples)
    -   [Getting urls](#getting-urls)
    -   [Saving urls to a file](#saving-urls-to-a-file)
        -   [gogo-stream (vidstreaming)](#gogo-stream-vidstreaming)
        -   [HAnime](#hanime)
    -   [Downloading the videos natively](#downloading-the-videos-natively)
        -   [gogo-stream (vidstreaming)](#gogo-stream-vidstreaming-1)
        -   [HAnime](#hanime-1)
-   [Contributing](#contributing)
    -   [Found a bug?](#found-a-bug)
    -   [Implement a site](#implement-a-site)

# Installation

```sh
$ git clone https://github.com/vrienstudios/anime-dl.git
$ cd anime-dl/JS
$ npm install
$ node index
```

# Usage

```sh
$ node index
```

This will give you all the commands currently available.

This is the JS version and can be used on linux and windows.

# Examples

## Getting urls

```sh
$ node index -S "bakemonogatari"
```

This will get the urls for all the episodes from the series `bakemonogatari` and display them in the console.

## Saving urls to a file

### gogo-stream (vidstreaming)

```sh
$ node index -S "bakemonogatari" -O "bakemonogatari.txt"
```

This will get the urls for all the episodes from the series `bakemonogatari` in gogo-stream, display them in the console and saving them into `bakemonogatari.txt`.<br>

### HAnime

```sh
$ node index -S "fuzzy lips" -src hanime -O "fuzzy.txt"
```

This will get the urls for all the episodes from the series `fuzzy lips` in HAnime, display them in the console and saving them into `fuzzy.txt`.<br>

## Downloading the videos natively

#### WARNING: This feature is in progress and might not work as expected and as of now, only works with .mp4 and .m3u/.m3u8 files. With the .m3u/.m3u8 files downloading being unstable or buggy, although it should do the work.

#### WARNING 2: Downloading videos natively in HAnime requires installing node-forge to decrypt the videos. Use "npm i node-forge" to install it.

### gogo-stream (vidstreaming)

```sh
$ node index -S "bakemonogatari" -download "%episodenumber%-%name%.%ext%"
```

This will get the urls for all the episodes from the series `bakemonogatari` in gogo-stream and download them using node-fetch. To get more information about the templates that can be used in the download filename format use `node index`.<br>

### HAnime

```sh
$ node index -S "fuzzy lips" -download "%episodenumber%-%name%.%ext%"
```

This will get the urls for all the episodes from the series `fuzzy lips` in HAnime and download them using node-fetch. To get more information about the templates that can be used in the download filename format use `node index`.

# Contributing

## Found a bug?

Add a new issue, explaining how the bug affects the program, and how to reproduce it, also specifing the vidstreamdownloader version (JS or C#).

## Implement a site

Implemeting a site in vidstreamdownloader is easy! <br><br>All you have to do is add a folder to sites/ with an index.js file. See sites/ztest/index.js for an example of what you should do.<br><br> Once you have implemented the site, and it's working properly, you can add a pull request and we'll be sure to look at it :)
