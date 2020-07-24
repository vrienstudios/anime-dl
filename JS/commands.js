// Bad way of doing this but it requires less alglorithsm and that stuff
const commandsOption = ['-s', '-o', '-download', '-test'];
const commandsAliases = [['-search'], ['-output', '-save', '-file'], [], []]
const commandsDescription = [
    'Search option incase you only know the name of the show.',
    'Output urls to a text file.',
    'Download the videos automatically with a default or specified filename.\n\t%episodenumber% - Will be replaced by the episode number\n\t%name% - Will be replaced by the show name\n\t%ext% - Will be replaced with the extension of the file downloading',
    'Test arguments'
]
const commandsDisplayArgs = ['[term]', '[filename]', '[format] (Optional)', null];
const commandsRequiresArgs = [true, true, true, false];
const commandsSetVarToNextArg = ['searchTerm', 'fileName', 'download', 'test'];

module.exports = {
    commandsOption, commandsAliases, commandsDescription, commandsDisplayArgs, commandsRequiresArgs, commandsSetVarToNextArg
}