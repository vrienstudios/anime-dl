import fs from 'fs';

const readSourcesFrom = dir => {
    try {
        let resultSources = [];
        const sourcesFolder = fs.readdirSync(dir);

        return new Promise(async (res, rej) => {
            await sourcesFolder.asyncForEach(async folder => {
                const folderContents = fs.readdirSync(dir + '/' + folder);
                if(folderContents.includes('index.js')) {
                    let source = (await import(`../sites/${folder}/index.js`)).default;
                    if(!source.source) return;
                    if(!source.data) source.data = {}
                    if(!source.data.name) source.data.name = folder;
                    resultSources.push(source)
                }
            })
            
            res(resultSources);
        })
    } catch(err) {
        console.warn('An error ocurred while loading a source.')
        throw err;
    }
    
}

export default {
    readSourcesFrom
}