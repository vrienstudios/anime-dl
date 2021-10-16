import fetch from 'node-fetch';
import cheerio from 'cheerio';
import { EventEmitter } from 'events';
import video from '../../utils/video.js';


const source = class Vidstreaming extends EventEmitter {
    constructor(argsObj, defaultDownloadFormat) {
        super();
        this.argsObj = argsObj;
        this.defaultDownloadFormat = defaultDownloadFormat;
        this.urls = null;
        this.id = null;
        this.episodesNumber = null;
    }

    async getEpisodes(term) {
        const id = await this.search(term);
        if(id.error) {
            return { error: id.error };
        }
        const req = await fetch(`https://vidstreaming.io/videos/${id}-episode-1`);
        const episodeHtml = await req.text();
        const $ = cheerio.load(episodeHtml);
        let episodesNumber = Number($('ul.listing.items.lists')[0].children.filter(tag => tag.attribs ? tag.attribs.class.includes('video-block') ? true : false : false).length);
        let urls = [];
        if(episodesNumber <= 1) {
            episodesNumber = 1;
        } 
        this.episodesNumber = episodesNumber;
        for (var i = 0; i < episodesNumber; i++) {
            this.emit('chapterProgress', `Getting url for ${id}-episode-${i+1} (${i+1}/${episodesNumber})...`)
            let epPage = await fetch(`https://vidstreaming.io/videos/${id}-episode-${i+1}`);
            let epHtml = await epPage.text();
            let e$ = cheerio.load(epHtml);
            let eId = e$('iframe')[0].attribs.src.split('id=')[1].split('=')[0]
            let vreq = await fetch(`https://vidstreaming.io/ajax.php?id=${eId}`);
            let json = await vreq.json();
            urls.push(json.source_bk[0].file);
            this.emit('chapterDone', ` \u001b[32mDone!\u001b[0m\n`)
        }
        if(this.argsObj.listRes) {
            let resolutions = [];
            await urls.asyncForEach(async url => {
                if((!url.endsWith('.m3u8'))) return resolutions.push('Resolution list only available for .m3u8');
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
                    this.argsObj.m3ures || 'highest', 
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
        const req = await fetch(`https://vidstreaming.io/ajax-search.html?keyword=${term.split(' ').join('+')}`, {
            "headers": {
                "x-requested-with": "XMLHttpRequest"
            }
        });
        const { content } = await req.json();
        const $ = cheerio.load(content);
        if(content === '') {
            return {
                error: 'Could not find the desired term in vidstreaming, try with a more specific search'
            };
        }
        const id = $('a')[0].attribs.href.split('/videos/')[1].split('-episode')[0];
        this.id = id;
        return id;
    }
}

const data = {
    name: 'Vidstreaming',
    website: 'vidstreaming.io',
    description: 'Vidstreaming - Watch anime online anywhere',
    language: 'English'
}

export default { source, data }