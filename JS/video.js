

module.exports.download = (url) => {
    // Can download normally...
    return new Promise((res, rej) => {
        if(url.endsWith('.mp4')) {
            console.log('Downloading .mp4 file ' + url + '... ');
            res()
        } else {
            rej({m: 'For now, vidstreamdownloader (JS) only supports downloading .mp4 files!', url})
        }
    })
    
}