import { EventEmitter } from 'events';
import fetch from 'node-fetch';
import cheerio from 'cheerio';

let forge;
(async () => {
    try {
        forge = await import('node-forge');
    } catch {
        global.logger.warn("WARNING: node-forge not installed, native downloads for HAnime will not be available. Use \"npm i node-forge\" to install it and support HAnime as it is a requiered dependency.")
    }
    
})();
const getEpManifest = (query) => {
    let window = {};
    // todo: parse json in a much safer way
    let manifest = eval(query("#__nuxt")[0].next.children[0].data);
    return manifest.state.data.video;
}

const loadCheerioEp = async (slug) => {
    let req = await fetch(constEpUrl + slug);
    let page = await req.text();
    return cheerio.load(page);
}

const getEpUrl = (manifest) => `https://weeb.hanime.tv/weeb-api-cache/api/v8/m3u8s/${manifest.videos_manifest.servers[0].streams[0].id}.m3u8`
const constEpUrl = `https://hanime.tv/videos/hentai/`;

const source = class extends EventEmitter {

    constructor(argsObj, defaultDownloadFormat) {
        super();
    }

    async getEpisodes(searchTerm) {
        let req = await fetch("https://search.htv-services.com/", {
            "headers": {
                "content-type": "application/json"
            },
            "body": JSON.stringify({
                "search_text": searchTerm,
                "tags": [],
                "tags_mode": "AND",
                "brands": [],
                "blacklist": [],
                "order_by": "created_at_unix",
                "ordering": "asc",
                "page": 0
            }),
            "method": "POST"
        })

        let json = await req.json();
        //console.log(json)
        let hits = JSON.parse(json.hits);
        if(hits.length < 1) {
            return {
                error: 'Could not find the desired term in HAnime, try with a more specific search.'
            }
        }
        //console.log(hits)
        
        //let eps = $("#rc-section")[2].children[0].data;
        //console.log(eps)
       
        //console.log(manifest.state.data.video)
        //this.emit('chapterProgress', )
        let $ = await loadCheerioEp(hits[0].slug)
        let manifest = getEpManifest($);
        let eps;
        let urls = [];
        try {
            eps = manifest.hentai_franchise_hentai_videos.filter(vid => vid.slug != hits[0].slug)
        } catch {
            eps = [];
        }

        
        
        urls.push(getEpUrl(manifest));

        await eps.asyncForEach(async ep => {
            $ = await loadCheerioEp(ep.slug);
            urls.push(getEpUrl(getEpManifest($)));
        })
        //req = await fetch(url);
        //let m3u8 = await req.text();
        //let seq = 0;
        
        return urls;
    }

    async download() {
        if(!forge) {
            console.log("Skipping download, node-forge not available.");
            return [];
        } else {
            /*let episodesToDownload = ['episode 1', 'episode 2']
            let failedEpisodes = ['episode 3'];
            await episodesToDownload.asyncForEach(async e => {
                process.stdout.write(`Downloading ${e}... `);
                return new Promise((res, rej) => {
                    setTimeout(() => {
                        process.stdout.write('Done!\n');
                        res();
                    }, 2000)
                })
                
            })
            return failedEpisodes;*/
            console.log("Download coming soon.")
            return [];
        }
        
    }
}

/* 
    module.exports.data is optional.
    it also might have as much parameters as you want. They will all be displayed on the 
    -lsc.
*/
const data = {
    name: 'Hanime',
    website: 'hanime.tv',
    description: 'Watch hentai online free download HD on mobile phone tablet laptop desktop. Stream online, regularly released uncensored, subbed, in 720p and 1080p!',
    language: 'English'
}

export default { source, data }
