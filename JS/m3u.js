module.exports.parse = (m3u) => {
    // THICC codeblock i really dont like it but meh
    let lines = m3u.split('\n');
    return lines.map((line, i) => {
        if(line.startsWith('#EXT-X-STREAM-INF:')) {
            let info = {};
            let l = line.split(',');
            l.shift();
            l.forEach(inf => {
                info[inf.split('=')[0]] = inf.split('=')[1];
            })
            info.FILE = lines[i+1]
            if(info.NAME) {
                info.NAME = info.NAME.replace(/"/g, '');
            }
            return {type: 'header', info}
        }
    }).filter(l => l !== undefined ? true : false);
}