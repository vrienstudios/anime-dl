const fetch = require('node-fetch');
const m3uLib = require('./m3u');
const fs = require('fs')

module.exports.listResolutions = (url) => {
    return new Promise((resolve, rej) => {
        fetch(url).then(res => res.text()).then(m3u => {
            let parsedFile = m3uLib.parse(m3u);
            resolve(parsedFile.map(line => {
                if((line.info) && (line.info.NAME)) {
                    return line.info.NAME
                }
            }).filter(l => l !== undefined ? true : false).join(', '));
        })
    })
}

const formatName = (format, episodenumber, name, ext) => {
    return format.replace('%episodenumber%', episodenumber).replace('%name%', name).replace('%ext%', ext)
}

const showDownloadingProgress = (received, part, total, dm, res) => {
    process.stdout.write("\033[0G");
    process.stdout.write(`${dm} ${received} bytes downloaded. (${part+1}/${total} parts, ${res.info.NAME || res.info.RESOLUTION})`);
}


module.exports.download = (url, format, name, episodenumber, m3ures, downloadm) => {
    // Can download normally...
    return new Promise((resolve, rej) => {
        if(url.endsWith('.mp4')) {
            let ext = url.split('.')[url.split('.').length-1]
            fetch(url).then(res => {
                const dest = fs.createWriteStream(`./${formatName(format, episodenumber, name, ext)}`);
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
                let endpoint = url.split('/')
                endpoint.pop();
                endpoint = endpoint.join('/');
                
                let parsedFile = m3uLib.parse(m3u);
                let res = m3ures;
                
                if(m3ures === 'highest') {
                    // SO FUCKING STUPID FUCK FUCK FUCK I HATE THIS 
                    // I JUST WANNA FINISH THIS FUCK SORRY THIS IS AWFUL
                    let resolutions = [];
                    parsedFile.forEach(lines => {
                        if(lines.type === 'header') {
                            if(lines.info) {
                                if(lines.info.NAME) {
                                    let asdasdas = lines.info.NAME.split('')
                                    asdasdas.pop();
                                    resolutions.push(Number(asdasdas.join('')))
                                }
                            }
                        }
                    });
                    res = resolutions[0] + 'p';
                    
                }
                if(!res.info) {
                    res = parsedFile.filter(o => {
                        if((o.type === 'header') && ((o.info.RESOLUTION === res) || (o.info.NAME === res))) { 
                            return true
                        } else {
                            return false
                        }
                    })[0];
                }

                fetch(`${endpoint}/${res.info.FILE}`).then(res => res.text()).then(subm3u => {
                    parsedFile = m3uLib.parse(subm3u);
                    // ts should be compatible with mp4 but just to be safe lets use the .ts extension
                    const dest = fs.createWriteStream(`./${formatName(format, episodenumber, name, 'ts')}`);
                    let recieved = 0;
                    let i = 0;
                    dest.on('pipe', (readable) => {
                        readable.on('data', (chunk) => {
                            recieved+=chunk.length
                            showDownloadingProgress(recieved, i+1, parsedFile.length, downloadm, res);
                        })
                    })
                    const asyncForEach = () => {
                        if(i <= parsedFile.length-1) {
                            fetch(`${endpoint}/${parsedFile[i].info.name}`).then(tsres => {
                                tsres.body.pipe(dest, {end: false})
                                tsres.body.on('end', () => {
                                    asyncForEach();
                                })
                                // TODO: Retry when error
                                tsres.body.on('error', (err) => {
                                    rej({m: err, url})
                                })
                            })
                        } else {
                            finished();
                        }
                        i++
                    }
                    const finished = () => {
                        dest.end();
                        resolve();
                    }
                    asyncForEach();
                })   
            })
        } else {
            rej({m: 'For now, vidstreamdownloader (JS) only supports downloading .mp4 and .m3u/.m3u8 files!', url})
        }
    })
    
}