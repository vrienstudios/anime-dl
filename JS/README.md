# Installation
`$ git clone https://github.com/vrienstudios/vidstreamdownloader.git`<br>
`$ cd vidstreamdownloader/JS`<br>
`$ npm install`<br>
`$ node index`


# Usage
`node index`<br>
This will give you all the commands currently available.

This is the JS version and can be used on linux (probably, not tested yet) and windows.

# Examples

## Getting urls
`node index -S "bakemonogatari"`<br>
This will get the urls for all the episodes from the series `bakemonogatari` and display them in the console.


## Saving urls to a file
`node index -S "bakemonogatari" -O "bakemonogatari.txt"`<br>
This will get the urls for all the episodes from the series `bakemonogatari`, display them in the console and saving them into `bakemonogatari.txt`.

## Downloading the videos natively

### WARNING: This feature is in progress and might not work as expected and as of now, only works with .mp4 files.

`node index -S "bakemonogatari" -download "%episodenumber%-%name%.%ext%"`<br>
This will get the urls for all the episodes from the series `bakemonogatari` and download them using node-fetch. To get more information about the templates that can be used in the download filename format use `node index`.