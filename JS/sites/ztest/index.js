// Example class. You can also see the vidstreaming class for a complete example.
const { EventEmitter } = require('events')

/* 
    Required, or else the module wont be recognized by vidstreamdownloader.
    This class should have these methods (or else something bad might happen!): 
    getEpisodes() - Should also emit events chapterDone and chapterProgress when required.
    download() - This should return an array with failed urls, in case there were one.
    In a future this class will be inherited to save some time.

    Events:
        chapterProgress should emit a string, that string will be written into the stdout
        chapterDone same as chapter progress.
*/
module.exports.source = class extends EventEmitter {
    /* 
    Vidstreamdownloader passes two arguments to the constructor
      argsObj - An object with command line arguments and their values
      defaultDownloadFormat - The format that can be used to store resulting files,
      in case there is none specified by the user. Check help to see how to replace the %% values.
    */
    constructor(argsObj, defaultDownloadFormat) {

    }

    getEpisodes() {
        function getChapter(c) {
            this.emit('chapterProgress', 'Downloading chapter 2...')
            return c;
        }

        this.emit('chapterDone', ' Done!')
    }

    download() {
        let episodesToDownload = ['episode 1', 'episode 2']
        let failedEpisodes = ['episode 3'];
        return failedEpisodes;
    }
}

/* 
    module.exports.data is optional.
    it also might have as much parameters as you want. They will all be displayed on the 
    -lsc.
*/
