import commands from './commands.js';
import sources from './utils/sources.js';
import asyncForEach from './utils/asyncForEach.js';
import log from './utils/log.js';
import path from 'path';
import { fileURLToPath } from 'url';

global.NO_WARNS = true;

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// TODO: Move to another file
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

const defaultSource = "vidstreaming";
const defaultDownloadFormat = "%episodenumber%-%name%-%res%.%ext%";

const displayCommands = () => {
    global.logger.info(`Commands:\n${commands.sort((a,b) => (a.option > b.option) ? 1 : ((b.option > a.option) ? -1 : 0))
    .map(cmd => `${cmd.option} ${cmd.requiresArgs ? cmd.displayArgs + ' ' : ''}- ${cmd.description}`).join('\n')}`);;
    process.exit();
}

const findCommand = find => command => (command.option === find) || (command.aliases.indexOf(find) !== -1);
const helpFindCommand = find => {
    find = "-" + find
    return command => (command.option === find) || (command.aliases.indexOf(find) !== -1);
} 

const showHelpAndQuit = () => {
    console.log(`\nUse -help (or -h) for a list of commands`);
    process.exit();
}

const commandHelp = (helpCmd) => {
    const command = commands.find(helpFindCommand(helpCmd.toLowerCase()));
    if(command) {
        global.logger.info(`${command.option} ${command.displayArgs}:\n\tDescription: ${command.description}\n\tAliases: ${command.aliases.join(', ')}`)
    } else {
        global.logger.info(`Unknown command "${helpCmd}".`);
    }
    showHelpAndQuit();
}

if(process.argv.length <= 2) {
    console.log('Too few arguments.')
    showHelpAndQuit();
} else {
    let argsObj = {};
    process.argv.forEach((arg, i) => {
        let argument = arg.toLowerCase();
        let command = commands.find(findCommand(argument));
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
    const level = Number(argsObj.logLevel);
    global.logger = new log(level);
    global.logger.debug(`Arguments: ${JSON.stringify(argsObj)}`);
    (async () => {
        const sites = await sources.readSourcesFrom(__dirname + '/sites');
        if(argsObj.helpCommand !== undefined) {
            typeof argsObj.helpCommand == "string" ? commandHelp(argsObj.helpCommand) : displayCommands();
        } else if(argsObj.lsc) {
            global.logger.info(`Sources:\n\n${sites.map(site => `${Object.keys(site.data).map(key => `${key === 'name' ? '- ' : '\t'+key.charAt(0).toUpperCase() + key.slice(1)+': '}${site.data[key]}`).join('\n')}`).join('\n\n')}`)
            return;
        } else if(!argsObj.searchTerm) {
            global.logger.error('Please specify an anime to search with -search.');
            showHelpAndQuit();
        } else {
            if(!argsObj.source) argsObj.source = defaultSource;
            let source = sites.find(site => site.data.name.toLowerCase() === argsObj.source.toLowerCase())
            if(!source) {
                global.logger.error('Invalid source. Use -lsc to check the available sources.');
                showHelpAndQuit();
            }
            source = new source.source(argsObj, defaultDownloadFormat);
                
            source.on('chapterProgress', m => process.stdout.write(m))
            source.on('chapterDone', m => process.stdout.write(m))
                
            let episodes = await source.getEpisodes(argsObj.searchTerm);
                
            if(episodes.error) {
                console.log(episodes.error);
                showHelpAndQuit();
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
            }
        }
    })()   
}