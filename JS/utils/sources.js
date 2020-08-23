const fs = require('fs');

module.exports.readSourcesFrom = dir => {
    try {
        let resultSources = [];
        const sourcesFolder = fs.readdirSync(dir);
        sourcesFolder.forEach(folder => {
            const folderContents = fs.readdirSync(dir + '/' + folder);
            if(folderContents.includes('index.js')) {
                let source = require(dir + '/' + folder + '/index.js');
                if(!source.source) return;
                if(!source.data) source.data = {}
                if(!source.data.name) source.data.name = folder;
                resultSources.push(source)
            }
        })
        return resultSources;
    } catch(err) {
        console.warn('An error ocurred while loading a source.')
        throw err;
    }
    
}