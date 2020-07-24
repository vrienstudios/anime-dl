module.exports.parse = (m3u) => {
    // THICC codeblock i really dont like it but meh
    return m3u.split('\n').map(line => {
        if(line.startsWith('#')) {
            let info = {};
            let l = line.split(',');
            l.shift();
            l.forEach(inf => {
                let retObj = {}
                retObj[inf.split('=')[0]] = inf.split('=')[1];
                Object.assign(info, retObj)
            })
            return {type: 'comment', info}
        } else {
            if(line === '') return;
            return {type: 'file', name: line}
        }
    }).filter(l => l !== undefined ? true : false);
}