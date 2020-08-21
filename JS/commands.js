const commands = [
    {
        option: '-s',
        aliases: ['-search'],
        description: 'Search option incase you only know the name of the show.',
        displayArgs: '[term]',
        requiresArgs: true,
        setVar: 'searchTerm'
    },

    {
        option: '-o',
        aliases: ['-output', '-save', '-file'],
        description: 'Output urls to a text file.',
        displayArgs: '[filename]',
        requiresArgs: true,
        setVar: 'fileName'
    },

    {
        option: '-download',
        aliases: ['-pD'],
        description: 'Download the videos automatically with a default or specified filename.\n\t%episodenumber% - Will be replaced by the episode number\n\t%name% - Will be replaced by the show name\n\t%ext% - Will be replaced with the extension of the file downloading',
        displayArgs: '[format] (Optional)',
        requiresArgs: true,
        setVar: 'download'
    },

    {
        option: '-res',
        aliases: ['-resolution', '-m3ures'],
        description: 'Set a resolution to download m3u/m3u8 files',
        displayArgs: '[res]',
        requiresArgs: true,
        setVar: 'm3ures'
    },

    {
        option: '-f',
        aliases: ['-formats', '-listformats', '-lf'],
        description: 'List available resolution for m3u/m3u8 files. If this option is used with -download, -download option will be anulated and only the formats will be displayed.',
        displayArgs: null,
        requiresArgs: false,
        setVar: 'listRes'
    }
]

module.exports = commands;