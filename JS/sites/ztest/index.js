// Example class.
const { EventEmitter } = require('events')

/* 
    Required, or else the module wont be recognized by vidstreamdownloader.
    This class should have these methods (or else something bad might happen!): 
    getEpisodes() - Should also emit events chapterDone and chapterProgress when required.
    download()
    search(term)
    In a future this class will be inherited to save some time.
*/
module.exports.source = class extends EventEmitter {
    constructor() {

    }

    getEpisodes() {

    }

    download() {

    }

    search(term) {

    }
}

/* 
    module.exports.data is optional.
    it also might have as much parameters as you want. They will all be displayed on the 
    -lsc.
*/
