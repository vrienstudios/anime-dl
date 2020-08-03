// Bad way of doing this but it requires less alglorithsm and that stuff
const commandsOption = ['-s', '-o', '-download', '-res', '-f'];
const commandsAliases = [['-search'], ['-output', '-save', '-file'], [], ['-resolution', '-m3ures'], ['-formats', '-listformats', '-lf']]
const commandsDescription = [
    'Search option incase you only know the name of the show.',
    'Output urls to a text file.',
    'Download the videos automatically with a default or specified filename.\n\t%episodenumber% - Will be replaced by the episode number\n\t%name% - Will be replaced by the show name\n\t%ext% - Will be replaced with the extension of the file downloading',
    'Set a resolution to download m3u/m3u8 files',
    'List available resolution for m3u/m3u8 files. If this option is used with -download, -download option will be anulated and only the formats will be displayed.',
]
const commandsDisplayArgs = ['[term]', '[filename]', '[format] (Optional)', '[res]', null];
const commandsRequiresArgs = [true, true, true, true, false];
const commandsSetVarToNextArg = ['searchTerm', 'fileName', 'download', 'm3ures', 'listRes'];

module.exports = {
    commandsOption, commandsAliases, commandsDescription, commandsDisplayArgs, commandsRequiresArgs, commandsSetVarToNextArg
}