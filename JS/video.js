const fetch = require('node-fetch');
const fs = require('fs')

module.exports.download = (url, format, name, episodenumber) => {
    // Can download normally...
    return new Promise((resolve, rej) => {
        if(url.endsWith('.mp4')) {
            let ext = url.split('.')[url.split('.').length-1]
            fetch(url).then(res => {
                const dest = fs.createWriteStream(`./${format.replace('%episodenumber%', episodenumber).replace('%name%', name).replace('%ext%', ext)}`);
                res.body.pipe(dest);
                res.body.on('end', () => {
                    resolve();
                })
                res.body.on('error', err => {
                    rej({m: err, url})
                })
            })
        } else {
            rej({m: 'For now, vidstreamdownloader (JS) only supports downloading .mp4 files!', url})
        }
    })
    
}