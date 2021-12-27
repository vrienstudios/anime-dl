import { EventEmitter } from 'events';
import fetch from 'node-fetch';

const source = class extends EventEmitter {
    constructor(argsObj, defaultDownloadFormat) {
        super();
    }

    async getEpisodes(searchTerm) {
        const search = await this.search(searchTerm);
        if(search.error) {
            return {
                error: search.error
            }
        }
        console.log(search)
        return [];
    }

    download() {

    }

    async search(term) {
        const req = await fetch("https://www3.animeflv.net/api/animes/search", {
            "headers": {
              "content-type": "application/x-www-form-urlencoded; charset=UTF-8"
            },
            "body": `value=${term.split(' ').join('+')}`,
            "method": "POST"
        })
        const result = await req.text();
        console.log(result)
        if(result.length < 1) {
            return {
                error: 'Could not find the desired term in AnimeFLV, try with a more specific search.'
            }
        }
        return result[0];
    }
}

const data = {
    name: 'AnimeFLV',
    website: 'www3.animeflv.net',
    description: 'El mejor portal de anime online para latinoamérica, encuentra animes clásicos, animes del momento, animes más populares y mucho más, todo en animeflv, tu fuente de anime diaria.',
    language: 'Español Sub/Dub'
}

export default { source, data }