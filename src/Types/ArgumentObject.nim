type ArgumentObject* = ref object of RootObj
    term: string
    download: bool
    search: bool
    toSkip: bool
    isContinuous: bool
    isRanged: bool
    downloadRange: array[2, int]
