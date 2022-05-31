(In partnership with http://simple-anime.herokuapp.com/)

# [Wiki](https://github.com/vrienstudios/anime-dl/wiki)

[Commands](https://github.com/vrienstudios/anime-dl/wiki/Commands)

[Differences Between JS and C#](https://github.com/vrienstudios/anime-dl/wiki/Differences-between-C%23-and-JS-versions)

Please do checkout the JS version; there's also a limited Nim version for mobile linux operating systems!

## Supported Sites

# Anime Sites Support
|             | Vidstreaming | Vidstreaming based sites | Hanime    | Twist.moe | Jkanime.net |
|-------------|--------------|--------------------------|-----------|-----------|-------------|
| C#          | ✅            | ✅                        | ✅         | ✅      | ❌           |
| JS          | ✅            | ❌                        | ✅ | ❌       | ✅           |

# Novel Support
| Differences | AsianHobbyist.com | Wuxiaworld.co | Wuxiaworld.com | Scribblehub.com | NovelFull.com | NovelHall.com | volarenovels.com |
|-------------|-------------------|---------------|----------------|-----------------|---------------|---------------|------------------|
| C#          | ✅                 | ✅             | ✅              | ✅               | ✅             | ✅             | ✅                |

# Manga Support
| Differences | reagmanganata.com | mangakakalot.com | manganato.com |
|-------------|-------------------|------------------|---------------|
| C#          | ✅                 | ✅                | ✅             |
| JS          | ❌                 | ❌                | ❌             |

### Example Usage (outdated)

[![Example](https://img.youtube.com/vi/YgfuUqdk1fw/0.jpg)](https://www.youtube.com/watch?v=YgfuUqdk1fw)
[![https://imgur.com/TVNENWX.png](https://imgur.com/TVNENWX.png)](https://imgur.com/TVNENWX.png)

To download an anime:
'anime-dl ani -gS Jerou -d' -- With the advent of our UI, this will become much easier.

Explanation: 
The "ani" switch tells the program to select the anime downloader, the -gS switch tells the program to search for an anime called "Jerou" on gogo-anime the -d switch tells the program to download the anime.


To download a novel and export it to epub:
>'anime -dl nvl -d -e {link to novel page}'

To convert an already downloaded novel to epub:
> 'anime-dl nvl -e {path to novel on disk}'

#### Example Usage (As An API/Library)

```csharp
using ADLCore;

Console.Write("Give me an anime link: ");

var animeLink = Console.ReadLine();

//All logging will be sent through the linearUpdater delegate, so it is advised to monitor or write it to console.
ADLCore.Interfaces.Main.QuerySTAT($"ani {animeLink} -d", o => { Console.WriteLine(o); });
```

