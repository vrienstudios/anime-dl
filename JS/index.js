const fetch = require('node-fetch');
const cheerio = require('cheerio');
const fs = require('fs')

// Bad way of doing this but it requires less alglorithsm and that stuff
const commandsOption = ['-s', '-o', '-test'];
const commandAliases = [['-search'], ['-output', '-save', '-file']]
const commandsDescription = [
    'Search option incase you only know the name of the show.',
    'Output urls to a text file.',
    'Test arguments'
]
const commandsDisplayArgs = ['[term]', '[filename]', null];
const commandsRequiresArgs = [true, true, false];
const commandsSetVarToNextArg = ['searchTerm', 'fileName', 'test'];

const displayHelp = () => {
    console.log(`Help:\n${commandsOption.map((op, i) => `${op} ${commandsRequiresArgs[i] ? commandsDisplayArgs[i] + ' ' : ''}- ${commandsDescription[i]}`).join('\n')}`);
    process.exit();
}
if(process.argv.length <= 2) {
    console.log('Too few arguments.')
    displayHelp();
} else {
    let argsObj = {};
    process.argv.forEach((arg, i) => {
        let argument = arg.toLowerCase();
        let argIndexInCmdOp = commandsOption.indexOf(argument);
        if(argIndexInCmdOp !== -1) {
            if(commandsRequiresArgs[argIndexInCmdOp]) {
                argsObj[commandsSetVarToNextArg[argIndexInCmdOp]] = process.argv[i+1];
            } else {
                argsObj[commandsSetVarToNextArg[argIndexInCmdOp]] = true;
            }
        }
    })
    if(argsObj.test) {
        console.log('Pinche dawn')
    }
    if(!argsObj.searchTerm) {
        console.log('No search term found.');
        displayHelp();
    } else {
        (async () => {
            let req = await fetch(`https://vidstreaming.io/ajax-search.html?keyword=${argsObj.searchTerm.split(' ').join('+')}`, {
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
            if(argsObj.fileName) {
                console.log('\nSaving into ' + argsObj.fileName);
                fs.writeFileSync(argsObj.fileName, urls.join('\n'));
                console.log('Done!')
            }
            console.log(`\n\nNext step is to copy these links into a text file and run youtube-dl!\nSample command: youtube-dl.exe -o "%(autonumber)${id}.%(ext)s" -k --no-check-certificate -i -a dwnld.txt\n\n`);
            console.log(urls.join('\n'))
            setInterval(() => {}, 100000);
        })()   
    }
}