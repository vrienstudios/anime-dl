import fetch from 'node-fetch';
import cheerio from 'cheerio';
import { EventEmitter } from 'events';
import video from '../../utils/video.js';


const URL = "https://gogoplay1.com/";
const DOWNLOAD_URL = "https://streamani.net/download"
const UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36 Edg/94.0.992.50";
const commonFetch = {
    "headers": {
        "Cookie": "tvshow=8srvmggsochisdd5hv9cjpqjv3; token=61c3ae0b5a08f",
        "User-Agent": UA,
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8",
        "Accept-Language": "en-US,en;q=0.5",
        "Upgrade-Insecure-Requests": "1",
        "Sec-Fetch-Dest": "document",
        "Sec-Fetch-Mode": "navigate",
        "Sec-Fetch-Site": "same-origin",
        "Sec-Fetch-User": "?1",
        "Cache-Control": "max-age=0"
    }
}


const source = class Vidstreaming extends EventEmitter {
    constructor(argsObj, defaultDownloadFormat) {
        super();
        this.argsObj = argsObj;
        this.defaultDownloadFormat = defaultDownloadFormat;
        this.urls = null;
        this.id = null;
        this.episodesNumber = null;
        this.rawUrlObj = {};
    }

    async getEpisodes(term) {
        const id = await this.search(term);
        if(id.error) {
            return { error: id.error };
        }
        const req = await fetch(`${URL}/videos/${id}-episode-1`, commonFetch);
        const episodeHtml = await req.text();
        const $ = cheerio.load(episodeHtml);
        let episodesNumber = Number($('ul.listing.items.lists')[0].children.filter(tag => tag.attribs ? tag.attribs.class.includes('video-block') ? true : false : false).length);
        let urls = [];
        if(episodesNumber <= 1) {
            episodesNumber = 1;
        } 
        this.episodesNumber = episodesNumber;
        for (var i = 0; i < episodesNumber; i++) {
            let epSlug = `${id}-episode-${i+1}`;
            this.emit('chapterProgress', `Getting url for ${epSlug} (${i+1}/${episodesNumber})...`)
            let epPage = await fetch(`${URL}/videos/${epSlug}`, commonFetch);
            let epHtml = await epPage.text();
            let ep$ = cheerio.load(epHtml);
            let downloadQuery = ep$('iframe')[0].attribs.src.split('?')[1]
            let downloadReq = await fetch(`${DOWNLOAD_URL}?${downloadQuery}`, commonFetch);
            
            let dwnHtml = await downloadReq.text();
            let dwn$ = cheerio.load(dwnHtml);
            
            let downloadURL = dwn$(".dowload").filter((idx, div) => div.children[0].children[0].data.includes("Download Xstreamcdn"))[0].children[0].attribs.href;
            
            let fileURLReq = await fetch(downloadURL.replace("/f/", "/api/source/"), {
                // No idea whether these headers affect the download
                // or not but I wont touch them just in case ..
                headers: {
                    "x-requested-with": "XMLHttpRequest",
                    "origin": downloadURL.split('/')[0],
                    "referer": downloadURL,
                    "accept-encoding": "gzip, deflate, br",
                    "user-agent": UA
                },
                method: "POST"
            });
            let fileURLJson = await fileURLReq.json();
            let { data } = fileURLJson;
            this.rawUrlObj = data;

            let availableResolutions = data.map(obj => [obj.label, obj.file]).sort((a, b) => {
                if(Number(a[0].slice(0, a.length-1)) < Number(b[0].slice(0, b.length-1))) return 1
                return -1
            });
            let highestRes = availableResolutions[0];
            let argRes = availableResolutions.filter(res => res[0] === this.argsObj.downloadRes)[0];
            let desiredRes = this.argsObj.downloadRes == 'highest' || !this.argsObj.downloadRes ? highestRes : argRes ? argRes : (() => { process.stdout.write(` "${this.argsObj.downloadRes}" resolution not avaliable, defaulting to highest (${highestRes[0]})... `); return highestRes })();
            urls.push(desiredRes[1]);
            this.emit('chapterDone', ` \u001b[32mDone!\u001b[0m\n`)
        }
        if(this.argsObj.listRes) {
            let resolutions = [];
            await urls.asyncForEach(async url => {
                if((!url.endsWith('.m3u8'))) {
                    resolutions.push(this.rawUrlObj.map(url => url.label).join(', '));
                    return;
                }
                let videoRes = await video.listResolutions(url)
                resolutions.push(videoRes);
            })
            console.log('\n\n'+resolutions.map((resolution, i) => `Available resolutions for episode #${i+1}: ${resolution}`).join('\n'))
        }
        this.urls = [...urls];
        return urls;
    }

    async download() {
        let failedUrls = [];
        const cleanLines = `\u001b[0m` + "\u001b[K\n"
        await this.urls.asyncForEach(async (_, i) => {
            let downloadm = `Downloading ${this.id}-episode-${i+1} (${i+1}/${this.episodesNumber})...`;
            process.stdout.write(downloadm);
            let ddownloadm = "\u001b[0G" + `${downloadm} \u001b[3`
            try {
                await video.download(
                    this.urls[i], 
                    this.argsObj.download || this.defaultDownloadFormat, 
                    this.id, 
                    i+1, 
                    this.argsObj.downloadRes || 'highest', 
                    downloadm,
                    this.argsObj.exactProgress
                );
                process.stdout.write(`${ddownloadm}2mDone!${cleanLines}`)

            } catch(reason) {
                console.log(reason)
                failedUrls.push(reason.url)
                process.stdout.write(`${ddownloadm}1m${reason.m}!${cleanLines}`)
            }
        })
        
        return failedUrls;
    }

    async search(term) {
       const req = await fetch(`${URL}/search.html?keyword=${term.split(' ').join('+')}`, commonFetch);
        const content = await req.text();
        const $ = cheerio.load(content);
        try {
            const id = $('.video-block')[0].children.filter(tag => tag.name === "a")[0].attribs.href.split('/videos/')[1].split('-episode')[0];
            this.id = id;
            return id;
        } catch(err) {
            return {
                error: `Could not find the desired term in vidstreaming, try with a more specific search (${err})`
            };
        }
    }
}

const data = {
    name: 'Vidstreaming',
    website: 'vidstreaming.io',
    description: 'Vidstreaming - Watch anime online anywhere',
    language: 'English'
}

export default { source, data }