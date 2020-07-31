const fetch = require('node-fetch');
const cheerio = require('cheerio');
const fs = require('fs')

const { commandsOption, commandsAliases, commandsDescription, commandsDisplayArgs, commandsRequiresArgs, commandsSetVarToNextArg } = require('./commands');
const video = require('./video');
const defaultDownloadFormat = "%episodenumber%-%name%.%ext%";

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
                if(process.argv[i+1] ? process.argv[i+1].startsWith('-') : true) {
                    argsObj[commandsSetVarToNextArg[argIndexInCmdOp]] = null;  
                } else {
                    argsObj[commandsSetVarToNextArg[argIndexInCmdOp]] = process.argv[i+1] || null;
                }
            } else {
                argsObj[commandsSetVarToNextArg[argIndexInCmdOp]] = true;
            }
        }
    })
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
            for (var i = 0; i < episodesNumber; i++) {
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
            if(argsObj.listRes) {
                let i = 0;
                let resolutions = [];
                const asyncForEachUrl = () => {
                    if(i <= urls.length-1) {
                        video.listResolutions(urls[i]).then(epres => {
                            resolutions.push(epres);
                            asyncForEachUrl();
                        })
                    } else {
                        finished();
                    }
                    i++
                }
                const finished = () => {
                    console.log('\n\n'+resolutions.map((resolution, i) => `Available resolutions for episode #${i+1}: ${resolution}`).join('\n'))
                }
                asyncForEachUrl(); 
            } else {
                if((argsObj.download) || argsObj.download === null) {
                    console.log('Starting download...')
                    let i = 0;
                    let failedUrls = [];
                    const asyncForEachUrl = () => {
                        
                        if(i <= urls.length-1) {
                            let downloadm = `Downloading ${id}-episode-${i+1} (${i+1}/${episodesNumber})...`;
                            process.stdout.write(downloadm);
                            video.download(urls[i], argsObj.download || defaultDownloadFormat, id, i+1, argsObj.m3ures || 'highest', downloadm).then(() => {
                                process.stdout.write("\033[0G" + `${downloadm} \u001b[32mDone!\u001b[0m` + "\033[K\n")
                                asyncForEachUrl();
                            }).catch(reason => {
                                process.stdout.write("\033[0G" + `${downloadm} \u001b[31m${reason.m}\u001b[0m` + "\033[K\n");
                                failedUrls.push(reason.url)
                                asyncForEachUrl();
                            })
                        } else {
                            finished();
                        }
                        i++
                    }
                    const finished = () => {
                        if(failedUrls.length !== 0) {
                            console.log('\n\nSome downloads failed:\n');
                            console.log(failedUrls.join('\n'))
                        }
                    }
                    asyncForEachUrl();
                    
                } else {
                    console.log(`\n\nNext step is to copy these links into a text file and run youtube-dl!\nSample command: youtube-dl.exe -o "%(autonumber)${id}.%(ext)s" -k --no-check-certificate -i -a dwnld.txt\n\n`);
                    console.log(urls.join('\n'))
                    setInterval(() => {}, 100000);
                }
            }
            
            
        })()   
    }
}