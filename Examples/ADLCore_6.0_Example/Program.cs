using ADLCore;

Console.Write("Give me an anime link: ");

var animeLink = Console.ReadLine();

//All logging will be sent through the linearUpdater delegate, so it is advised to monitor or write it to console.
ADLCore.Interfaces.Main.QuerySTAT($"ani {animeLink} -d", o => { Console.WriteLine(o); });