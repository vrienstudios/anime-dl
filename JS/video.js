const fetch = require('node-fetch');
const m3uLib = require('./m3u');
const fs = require('fs')

module.exports.download = (url, format, name, episodenumber, m3ures) => {
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
        } else if(url.endsWith('.m3u') || url.endsWith('.m3u8')) {
            fetch(url).then(res => res.text()).then(m3u => {
                console.log(m3u)
                let parsedFile = m3uLib.parse(m3u);
                let res = m3ures;
                if(m3ures === 'highest') {
                    // this is fucking stupid
                    parsedFile.map(lines => {
                        if(res === 'highest') {
                            if(lines.type === 'comment') {
                                if(lines.info) {
                                    if(lines.info.RESOLUTION || lines.info.NAME) {
                                        res = lines.info.RESOLUTION;
                                    }
                                }
                            }
                        }
                    })
                }
                console.log(res)
                console.log(parsedFile)
                
            })
        } else {
            rej({m: 'For now, vidstreamdownloader (JS) only supports downloading .mp4 files!', url})
        }
    })
    
}