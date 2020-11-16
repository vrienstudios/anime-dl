const { EventEmitter } = require('events')
const fetch = require('node-fetch');
const cheerio = require('cheerio');

module.exports.source = class extends EventEmitter {

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
        let hits = JSON.parse(json.hits);
        if(hits.length < 1) {
            return {
                error: 'Could not find the desired term in HAnime, try with a more specific search.'
            }
        }
        req = await fetch(`https://hanime.tv/videos/hentai/${hits[0].slug}`);
        let page = await req.text();
        let $ = cheerio.load(page);
        let window = {};
        // todo: parse json in a much safer way
        let manifestId = eval($("#__nuxt")[0].next.children[0].data).state.data.video.videos_manifest.servers[0].streams[0].id;
        let url = `https://weeb.hanime.tv/weeb-api-cache/api/v8/m3u8s/${manifestId}.m3u8`
        console.log(hits)
        return hits[0];
    }

    async download() {
        let episodesToDownload = ['episode 1', 'episode 2']
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
        return failedEpisodes;
    }
}

/* 
    module.exports.data is optional.
    it also might have as much parameters as you want. They will all be displayed on the 
    -lsc.
*/
module.exports.data = {
    name: 'Hanime',
    website: 'hanime.tv',
    description: 'Watch hentai online free download HD on mobile phone tablet laptop desktop. Stream online, regularly released uncensored, subbed, in 720p and 1080p!',
    language: 'English'
}
