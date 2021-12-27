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

const formatName = (format, episodenumber, name, ext, res) => {
    return format.replace('%episodenumber%', episodenumber).replace('%name%', name).replace('%ext%', ext).replace("%res%", res);
}

const m3uDownloadingProgress = (received, part, total, dm, res) => {
    process.stdout.write("\x1B[0G");
    process.stdout.write(`${dm} ${received} bytes downloaded. (${part}/${total} parts, ${res.info.NAME || res.info.RESOLUTION})`);
}

const DownloadingProgress = (recieved, total, dm, exactProgress) => {
    process.stdout.write("\x1B[0G");
    process.stdout.write(`${dm} ${exactProgress ? `${recieved/1e+6}/${total/1e+6} megabytes recieved` : Math.floor((recieved / total)*100) + "%"}`)
}

const download = (url, format, name, episodenumber, downloadRes, downloadm, exactProgress) => {
    return new Promise((resolve, rej) => {
        if(!url.endsWith('.m3u') || !url.endsWith('.m3u8')) {
            // Can download normally...
            fetch(url).then(res => {
                const dest = fs.createWriteStream(`./${formatName(format, episodenumber, name, 'mp4', downloadRes)}`);
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
        } else {
            fetch(url).then(res => res.text()).then(m3u => {
                let endpoint = url.split('/')
                endpoint.pop();
                endpoint = endpoint.join('/');
                
                let parsedFile = m3uLib.parse(m3u);
                let res = downloadRes;
                
                if(downloadRes === 'highest') {
                    let resolutions = [];
                    parsedFile.forEach(lines => {
                        if(lines.type === 'header') {
                            if(lines.info) {
                                if(lines.info.NAME) {
                                    let numberRes = lines.info.NAME.split('')
                                    numberRes.pop();
                                    resolutions.push(Number(numberRes.join('')))
                                }
                            }
                        }
                    });
                    res = resolutions[0] + 'p';
                    
                }
                if(!res.info) {
                    res = parsedFile.filter(o => {
                        return ((o.type === 'header') && ((o.info.RESOLUTION === res) || (o.info.NAME === res)));
                    })[0];
                }

                fetch(`${endpoint}/${res.info.FILE}`).then(res => res.text()).then(async subm3u => {
                    parsedFile = m3uLib.parse(subm3u);
                    // ts should be compatible with mp4 but just to be safe lets use the .ts extension
                    const dest = fs.createWriteStream(`./${formatName(format, episodenumber, name, 'ts', downloadRes)}`);
                    let recieved = 0;
                    let i = 0;
                    dest.on('pipe', (readable) => {
                        readable.on('data', (chunk) => {
                            recieved+=chunk.length
                            m3uDownloadingProgress(recieved, i+1, parsedFile.length, downloadm, res);
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
        }
    })
    
}

/**
 * Wrapper for outputing video.download information to console in a readable way and downloading multiple URLs
 * @param {object} info Object with the parameters: 
 *                      slug - The slug/anime name/episode (replaces %current% with the current episode being downloaded)
 *                      urls - The urls that will be downloaded
 *                      argsObj - argsObj from the class
 *                      defaultDownloadFormat - defaultDownloadFormat from the class
 * 
 * @returns An array with failed urls (empty if there were none that failed)
 */
const downloadWrapper = async info => { // TODO: Make argsObj available to this function instead of reling on the sources
    let failedUrls = [];
    const cleanLines = `\u001b[0m` + "\u001b[K\n"
    await info.urls.asyncForEach(async (_, i) => {
        let slug = info.slug.replace('%current%', i+1)
        let downloadm = `Downloading ${slug} (${i+1}/${info.urls.length})...`;
        process.stdout.write(downloadm);
        let ddownloadm = "\u001b[0G" + `${downloadm} \u001b[3`
        try {
            await download(
                info.urls[i], 
                info.argsObj.download || info.defaultDownloadFormat, 
                slug, 
                i+1, 
                info.argsObj.downloadRes || 'highest', 
                downloadm,
                info.argsObj.exactProgress
            );
            process.stdout.write(`${ddownloadm}2mDone!${cleanLines}`)

        } catch(reason) {
            global.logger.warn(reason)
            failedUrls.push(reason.url)
            process.stdout.write(`${ddownloadm}1m${reason.m}!${cleanLines}`)
        }
    })
    
    return failedUrls;  
}

export default {
    listResolutions, download, downloadWrapper
}