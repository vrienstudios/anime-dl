const fetch = require('node-fetch');
const cheerio = require('cheerio');

const displayHelp = () => {
    console.log('Help:\n-S [term] - Search option incase you only know the name of the show.');
    process.exit();
}
if(process.argv.length <= 2) {
    console.log('Too few arguments.')
    displayHelp();
} else {
    let searchTerm = null;
    process.argv.forEach((arg, i) => {
        if(arg === '-S') {
            searchTerm = process.argv[i+1];
        } 
    })
    if(!searchTerm) {
        console.log('No search term found.');
        displayHelp();
    } else {
        (async () => {
            let req = await fetch(`https://vidstreaming.io/ajax-search.html?keyword=${searchTerm.split(' ').join('+')}`, {
                "headers": {
                    "x-requested-with": "XMLHttpRequest" // appearantly i need this or else it wont give any json output  
                }
            });
            let { content } = await req.json();
            let $ = cheerio.load(content);
            if(content === '') {
                console.log('Could not find the desired term in vidstreaming, try with a more specific search');
                displayHelp();
            }
            let id = $('a')[0].attribs.href.split('/videos/')[1].split('-episode')[0];
            console.log('Found page ' + id)
            
            req = await fetch(`https://vidstreaming.io/videos/${id}-episode-1`);
            episodeHtml = await req.text();
            $ = cheerio.load(episodeHtml);
            let episodesNumber = Number($('ul.listing.items.lists')[0].children.filter(tag => tag.attribs ? tag.attribs.class.includes('video-block') ? true : false : false).length);
            let urls = [];
            if(episodesNumber <= 1) {
                console.log('Only found one episode, no need to get more!');
                episodesNumber = 1;
            } 
            for(var i = 0; i < episodesNumber; i++) {
                process.stdout.write(`Getting url for ${id}-episode-${i+1} (${i+1}/${episodesNumber})...`)
                let epPage = await fetch(`https://vidstreaming.io/videos/${id}-episode-${i+1}`);
                let epHtml = await epPage.text();
                let e$ = cheerio.load(epHtml);
                let eId = e$('iframe')[0].attribs.src.split('id=')[1].split('=')[0]
                let vreq = await fetch(`https://vidstreaming.io/ajax.php?id=${eId}`);
                let json = await vreq.json();
                urls.push(json.source[0].file);
                process.stdout.write(` \u001b[32mDone!\u001b[0m\n`)
            }
            console.log(`\n\nNext step is to copy these links into a text file and run youtube-dl!\nSample command: youtube-dl.exe -o "%(autonumber)${id}.%(ext)s" -k --no-check-certificate -i -a dwnld.txt\n\n`);
            console.log(urls.join('\n'))
            setInterval(() => {}, 100000);
        })()   
    }
}