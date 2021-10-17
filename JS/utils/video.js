import fetch from 'node-fetch';
import fs from 'fs';
import m3uLib from './m3u.js';

const listResolutions = (url) => {
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
    process.stdout.write("\x1B[0G");
    process.stdout.write(`${dm} ${received} bytes downloaded. (${part}/${total} parts, ${res.info.NAME || res.info.RESOLUTION})`);
}

const DownloadingProgress = (recieved, total, dm, exactProgress) => {
    process.stdout.write("\x1B[0G");
    process.stdout.write(`${dm} ${exactProgress ? `${recieved/1e+6}/${total/1e+6} megabytes recieved` : Math.floor((recieved / total)*100) + "%"}`)
}

const download = (url, format, name, episodenumber, m3ures, downloadm, exactProgress) => {
    return new Promise((resolve, rej) => {
        if(url.endsWith('.mp4')) {
            // Can download normally...
            fetch(url).then(res => {
                // url ends with .mp4, we can assume it is an .mp4 file
                const dest = fs.createWriteStream(`./${formatName(format, episodenumber, name, 'mp4')}`);
                const size = res.headers.get("content-length");
                res.body.pipe(dest);
                let recieved = 0;
                res.body.on('data', chunk => {
                    recieved+=chunk.length;
                    DownloadingProgress(recieved, size, downloadm, exactProgress);
                })
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

                fetch(`${endpoint}/${res.info.FILE}`).then(res => res.text()).then(async subm3u => {
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
                            fetch(`${endpoint}/${parsedFile[i].info.NAME}`).then(tsres => {
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

export default {
    listResolutions, download
}