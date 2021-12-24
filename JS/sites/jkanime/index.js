import { EventEmitter } from 'events';
import cheerio from 'cheerio';
import fetch from 'node-fetch';
import video from '../../utils/video.js';

const URL = "https://jkanime.net/";

const commonFetch =  {
    "credentials": "include",
    "headers": {
        "User-Agent": "Mozilla/5.0 (X11; Linux x86_64; rv:95.0) Gecko/20100101 Firefox/95.0",
        "Accept": "application/json, text/javascript, */*; q=0.01",
        "Accept-Language": "en-US,en;q=0.5",
        "X-Requested-With": "XMLHttpRequest",
        "Sec-Fetch-Dest": "empty",
        "Sec-Fetch-Mode": "cors",
        "Sec-Fetch-Site": "same-origin",
        "Referer": "https://jkanime.net/"
    },
    "method": "GET",
    "mode": "cors"
};

const source = class extends EventEmitter {
    constructor(argsObj, defaultDownloadFormat) {
        super();
        this.urls = [];
        this.argsObj = argsObj;
        this.defaultDownloadFormat = defaultDownloadFormat;
        this.slug = null;
    }

    async getEpisodes(searchTerm) {
        const search = await this.search(searchTerm);
        if(search.error) {
            return {
                error: search.error
            }
        }
        const { slug } = search;
        this.slug = slug;
        global.logger.debug(`slug is ${slug}`);
        const animeURL = `${URL}${slug}/`;
        const animePageReq = await fetch(animeURL, commonFetch);
        const animePage = await animePageReq.text();
        let $ = cheerio.load(animePage);
        const episodes = parseInt($("span").filter((_, tag) => tag.children ? tag.children[0]?.data === 'Episodios:' : false)[0].next.data.slice(1))
        global.logger.debug(`${episodes} episodes`)
        let i = 0;

        return new Promise(async (res, rej) => {
            const getEpisode = async () => {
                if(i <= episodes-1) {
                    i++;
                    this.emit('urlSlugProgress', {
                        slug,
                        current: i,
                        total: episodes
                    })
                    const episodePageReq = await fetch(`${animeURL}${i}`, commonFetch)
                    const episodePage = await episodePageReq.text();
                    const url = episodePage.split(`video[1] = '`)[1].split('</iframe>')[0].split(`"`)[3];
                    const iframeReq = await fetch(url, commonFetch);
                    const iframeRedirect = await iframeReq.text();
                    $ = cheerio.load(iframeRedirect);
                    const redirectData = $("input")[0].attribs.value
                    const makePostData = (data) => Object.assign(commonFetch, { 
                        method: "POST", 
                        body: data,
                        headers: Object.assign(commonFetch.headers, 
                            {"Content-Type": "application/x-www-form-urlencoded"})
                        
                    });
                    
                    const redirectPostReq = await fetch(`${URL}gsplay/redirect_post.php`, makePostData(`data=${redirectData}`))
                    const dataHash = redirectPostReq.url.split("#")[1];
                    global.logger.debug(`data hash: ${dataHash}`)
                    const makeAPIReq = async () => {
                        const apiReq = await fetch(`${URL}gsplay/api.php`, makePostData(`v=${dataHash}`))
                        const apiJson = await apiReq.json();
                        global.logger.debug(apiJson);
                        if(apiJson.sleep) {
                            if(isNaN(apiJson.sleep)) apiJson.sleep = 3000;
                            global.logger.debug(`asked to sleep ${apiJson.sleep}`);
                            return setTimeout(makeAPIReq, apiJson.sleep);
                        }
                        this.urls.push(apiJson.file);
                        this.emit('urlProgressDone');
                        getEpisode();
                    }
                    await makeAPIReq();
                } else {
                    global.logger.debug('finish!');
                    res(this.urls);
                }
            } // end get episode function  
            
            getEpisode();
        });
    }

    download() {
        return video.downloadWrapper({
            urls: this.urls,
            argsObj: this.argsObj,
            slug: this.slug,
            defaultDownloadFormat: this.defaultDownloadFormat
        })
    }

    async search(term) {
        const req = await fetch(`${URL}ajax/ajax_search/?q=${term.split(' ').join('%20')}`, commonFetch);
        const { animes } = await req.json();
        if(animes.length < 1) {
            return {
                error: 'Could not find the desired term in AnimeFLV, try with a more specific search.'
            }
        }
        return animes[0];
    }
}

const data = {
    name: 'Jkanime',
    website: URL,
    description: 'La mejor pagina para Ver Anime Online Gratis, mira los ultimos capitulos de los animes del momento sin ninguna restriccion | ver Online y descargar',
    language: 'Español Sub/Dub'
}

export default { source, data }