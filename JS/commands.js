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
        option: '-sc',
        aliases: ['-source', '-website', '-web'],
        description: 'Specify where you want to download anime from.\n\tDefaults to "vidstreaming"',
        displayArgs: '[source]',
        requiresArgs: true,
        setVar: 'source'
    },

    {
        option: '-lsc',
        aliases: ['-listsources', '-ls', '-sources'],
        description: 'List of available sources that can be used to download anime from.',
        displayArgs: null,
        requiresArgs: false,
        setVar: 'lsc'
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
        aliases: ['-pd', '-d'],
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
        description: 'List available resolution for m3u/m3u8 files.',
        displayArgs: null,
        requiresArgs: false,
        setVar: 'listRes'
    }
]

module.exports = commands;