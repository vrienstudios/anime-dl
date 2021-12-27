const commands = [
    {
        option: '-search',
        aliases: ['-s'],
        description: 'Search option incase you only know the name of the show.',
        displayArgs: '[term]',
        requiresArgs: true,
        setVar: 'searchTerm'
    },

    {
        option: '-source',
        aliases: ['-sc', '-website', '-web', '-src'],
        description: 'Specify where you want to download anime from.\n\tDefaults to "vidstreaming"',
        displayArgs: '[source]',
        requiresArgs: true,
        setVar: 'source'
    },

    {
        option: '-list-sources',
        aliases: ['-lsc', '-listsources', '-ls', '-sources'],
        description: 'List of available sources that can be used to download anime from.',
        displayArgs: null,
        requiresArgs: false,
        setVar: 'lsc'
    },

    {
        option: '-output',
        aliases: ['-o', '-save', '-file'],
        description: 'Output urls to a text file.',
        displayArgs: '[filename]',
        requiresArgs: true,
        setVar: 'fileName'
    },

    {
        option: '-download',
        aliases: ['-pd', '-d'],
        description: 'Download the videos automatically with a default or specified filename.\n\t%episodenumber% - Will be replaced by the episode number\n\t%name% - Will be replaced by the show name\n\t%ext% - Will be replaced with the extension of the file downloading\n\t%res% - Will be replaced with the resolution of the file downloading',
        displayArgs: '[format] (Optional)',
        requiresArgs: true,
        setVar: 'download'
    },

    {
        option: '-exact-progress',
        aliases: ['-mb'],
        description: 'Show download progress in megabytes, instead of percentage',
        displayArgs: null,
        requiresArgs: false,
        setVar: 'exactProgress'
    },

    {
        option: '-resolution',
        aliases: ['-res', '-m3ures'],
        description: 'Set a resolution to download files',
        displayArgs: '[res]',
        requiresArgs: true,
        setVar: 'downloadRes'
    },

    {
        option: '-list-formats',
        aliases: ['-formats', '-listformats', '-lf', '-f'],
        description: 'List available resolutions for downloads.',
        displayArgs: null,
        requiresArgs: false,
        setVar: 'listRes'
    },

    {
        option: '-log-level',
        aliases: ['-log', '-level', '-debug', '-dbg'],
        description: 'Set log level for console output.\n\tDefaults to 0 (only downloading related logging and fatal errors)\n\t\t0 - Only downloading related logging and errors\n\t\t1 - Warnings (and previous level)\n\t\t2 - All (including debug)',
        displayArgs: '[level] (Optional)',
        requiresArgs: true,
        setVar: 'logLevel'
    },

    {
        option: '-help',
        aliases: ['-?', '-h', '-cmds', '-commands'],
        description: 'Shows a list of commands or shows help about a certain command.',
        displayArgs: '[command] (Optional)',
        requiresArgs: true,
        setVar: 'helpCommand'
    }
]

export default commands;