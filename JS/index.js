import commands from './commands.js';
import sources from './utils/sources.js';
import asyncForEach from './utils/asyncForEach.js';
import path from 'path';
import { fileURLToPath } from 'url';

global.NO_WARNS = true;

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// TODO: Move to another file
if(global.NO_WARNS) {
    const {emitWarning} = process;

    process.emitWarning = (warning, ...args) => {
        if (args[0] === 'ExperimentalWarning') {
            return;
        }

        if (args[0] && typeof args[0] === 'object' && args[0].type === 'ExperimentalWarning') {
            return;
        }

        return emitWarning(warning, ...args);
    };
}

const defaultSource = "vidstreaming";
const defaultDownloadFormat = "%episodenumber%-%name%-%res%.%ext%";

const displayHelp = () => {
    console.log(`Help:\n${commands.sort((a,b) => (a.option > b.option) ? 1 : ((b.option > a.option) ? -1 : 0))
        .map(cmd => `${cmd.option} ${cmd.requiresArgs ? cmd.displayArgs + ' ' : ''}- ${cmd.description}`).join('\n')}`);
    process.exit();
}
if(process.argv.length <= 2) {
    console.log('Too few arguments.')
    displayHelp();
} else {
    let argsObj = {};
    process.argv.forEach((arg, i) => {
        let argument = arg.toLowerCase();
        let command = commands.find(command => (command.option === argument) || (command.aliases.indexOf(argument) !== -1));
        if(command) {
            if(command.requiresArgs) {
                if(process.argv[i+1] ? process.argv[i+1].startsWith('-') : true) {
                    argsObj[command.setVar] = null;  
                } else {
                    argsObj[command.setVar] = process.argv[i+1] || null;
                }
            } else {
                argsObj[command.setVar] = true;
            }
        }
    });
    (async () => {
        const sites = await sources.readSourcesFrom(__dirname + '/sites');

        if(argsObj.lsc) {
            console.log(`Sources:\n\n${sites.map(site => `${Object.keys(site.data).map(key => `${key === 'name' ? '- ' : '\t'+key.charAt(0).toUpperCase() + key.slice(1)+': '}${site.data[key]}`).join('\n')}`).join('\n\n')}`)
            return;
        } else if(!argsObj.searchTerm) {
            console.log('No search term found.');
            displayHelp();
        } else {
            if(!argsObj.source) argsObj.source = defaultSource;
            let source = sites.find(site => site.data.name.toLowerCase() === argsObj.source.toLowerCase())
            if(!source) {
                console.log('Invalid source. Use -lsc to check the available sources.');
                displayHelp();
            }
            source = new source.source(argsObj, defaultDownloadFormat);
                
            source.on('chapterProgress', m => process.stdout.write(m))
            source.on('chapterDone', m => process.stdout.write(m))
                
            let episodes = await source.getEpisodes(argsObj.searchTerm);
                
            if(episodes.error) {
                console.log(episodes.error);
                displayHelp();
            }

            if(argsObj.fileName) {
                const fs = require('fs');
                console.log('\nSaving into ' + argsObj.fileName);
                fs.writeFileSync(argsObj.fileName, episodes.join('\n'));
                console.log('Done!')
            }

            if((argsObj.download) || argsObj.download === null) {
                let failedUrls = await source.download();
                if(failedUrls.length !== 0) {
                    console.log('\nSome downloads failed:\n');
                    console.log(failedUrls.join('\n'))
                }
            } else if(!argsObj.listRes) {
                console.log(`\n\nNext step is to copy these links into a text file and run youtube-dl!\nSample command: youtube-dl.exe -o "%(autonumber)${argsObj.searchTerm}.%(ext)s" -k --no-check-certificate -i -a dwnld.txt\n\n`);
                console.log(episodes.join('\n'))
                setInterval(() => {}, 100000);
            }
        }
    })()   
}