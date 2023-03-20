using ScryfallApi.Client.Models;
using System.Reflection.Metadata;
using System.Text.Json;

using MTGAlphaSortDivider;
using static MTGAlphaSortDivider.FindDivisions;

//get all cards, using Scryfall's 
const string path = "C:\\Users\\raven\\Documents\\.projects\\MTGAlphaSortDivider\\oracle-cards-20230311220254.json";

List<Card> allCards;

Console.WriteLine("Reading bulk data...");

using (StreamReader reader = new(path))
{
    string json = reader.ReadToEnd();
    allCards = JsonSerializer.Deserialize<List<Card>>(json);
}

Console.WriteLine($"Bulk data read done! Read {allCards.Count} oracle cards");

Console.WriteLine("Filtering out non-sorted cards...");

//filter out unwanted cards: emblems, tokens, art cards
List<Card> allCardsFiltered = allCards
    .Where(c => c.Layout != "art_series")
    .Where(c => c.Layout != "token")
    .Where(c => c.Layout != "double_faced_token")
    .Where(c => c.Layout != "emblem")
    .ToList();

Console.WriteLine($"Filtering done! Filtered out {allCards.Count - allCardsFiltered.Count} cards.");

Console.WriteLine("Conducting color and land sorting...");

//sort all cards into these top level sorting blocks:
//    White, Blue, Black, Red, Green, Multicolor, Colorless, Lands

Dictionary<TopLevelSortingBlocks, List<Card>> topLevelBlocks = new();

//Checking for lands like this selects some cards that I would not sort as lands, like MDFC cards with a creature and land on different sides. For the purposes of this program, that is not a problem.
topLevelBlocks.Add(TopLevelSortingBlocks.Lands, allCardsFiltered.Where(c => c.TypeLine.Contains("Land")).ToList());
Console.WriteLine($"Sorted {topLevelBlocks[TopLevelSortingBlocks.Lands].Count} lands");

topLevelBlocks.Add(TopLevelSortingBlocks.Multicolor, allCardsFiltered.Where(c => c.ColorIdentity.Length > 1).Where(c => !c.TypeLine.Contains("Land")).ToList());
Console.WriteLine($"Sorted {topLevelBlocks[TopLevelSortingBlocks.Multicolor].Count} multicolors");

topLevelBlocks.Add(TopLevelSortingBlocks.Colorless, allCardsFiltered.Where(c => c.ColorIdentity.Length == 0).Where(c => !c.TypeLine.Contains("Land")).ToList());
Console.WriteLine($"Sorted {topLevelBlocks[TopLevelSortingBlocks.Colorless].Count} colorless");

topLevelBlocks.Add(TopLevelSortingBlocks.White, allCardsFiltered.Where(c => c.ColorIdentity.Length == 1).Where(c => c.ColorIdentity.Contains("W")).Where(c => !c.TypeLine.Contains("Land")).ToList());
Console.WriteLine($"Sorted {topLevelBlocks[TopLevelSortingBlocks.White].Count} white");

topLevelBlocks.Add(TopLevelSortingBlocks.Blue, allCardsFiltered.Where(c => c.ColorIdentity.Length == 1).Where(c => c.ColorIdentity.Contains("U")).Where(c => !c.TypeLine.Contains("Land")).ToList());
Console.WriteLine($"Sorted {topLevelBlocks[TopLevelSortingBlocks.Blue].Count} blue");

topLevelBlocks.Add(TopLevelSortingBlocks.Black, allCardsFiltered.Where(c => c.ColorIdentity.Length == 1).Where(c => c.ColorIdentity.Contains("B")).Where(c => !c.TypeLine.Contains("Land")).ToList());
Console.WriteLine($"Sorted {topLevelBlocks[TopLevelSortingBlocks.Black].Count} black");

topLevelBlocks.Add(TopLevelSortingBlocks.Red, allCardsFiltered.Where(c => c.ColorIdentity.Length == 1).Where(c => c.ColorIdentity.Contains("R")).Where(c => !c.TypeLine.Contains("Land")).ToList());
Console.WriteLine($"Sorted {topLevelBlocks[TopLevelSortingBlocks.Red].Count} red");

topLevelBlocks.Add(TopLevelSortingBlocks.Green, allCardsFiltered.Where(c => c.ColorIdentity.Length == 1).Where(c => c.ColorIdentity.Contains("G")).Where(c => !c.TypeLine.Contains("Land")).ToList());
Console.WriteLine($"Sorted {topLevelBlocks[TopLevelSortingBlocks.Green].Count} green");

//Then sort into these second level blocks:
//    Creature/Artifact, Instant/Sorcery, Enchantment
//Creatures and artifacts are given priority.
Console.WriteLine("\nSorting by second level blocks...");
Dictionary<TopLevelSortingBlocks, Dictionary<SecondLevelBlocks, List<Card>>> secondSortedCards = new();

foreach(var topBlock in topLevelBlocks)
{
    if(topBlock.Key == TopLevelSortingBlocks.Lands)
    {
        //skip lands, we will use top level blocks list for those.
        continue;
    }

    Dictionary<SecondLevelBlocks, List<Card>> secondSortedTopBlock = new();

    secondSortedTopBlock.Add(SecondLevelBlocks.CreaturesArtifacts, topLevelBlocks[topBlock.Key].Where(c => c.TypeLine.Contains("Creature") || c.TypeLine.Contains("Artifact")).ToList());
    Console.WriteLine($"Sorted {secondSortedTopBlock[SecondLevelBlocks.CreaturesArtifacts].Count} {topBlock.Key} cretaures and artifacts");

    secondSortedTopBlock.Add(SecondLevelBlocks.InstantsSorceries, topLevelBlocks[topBlock.Key].Where(c => !(c.TypeLine.Contains("Creature") || c.TypeLine.Contains("Artifact"))).Where(c => c.TypeLine.Contains("Instant") || c.TypeLine.Contains("Sorcery")).ToList());
    Console.WriteLine($"Sorted {secondSortedTopBlock[SecondLevelBlocks.InstantsSorceries].Count} {topBlock.Key} instants and sorceries");

    secondSortedTopBlock.Add(SecondLevelBlocks.Enchantments, topLevelBlocks[topBlock.Key].Where(c => !(c.TypeLine.Contains("Creature") || c.TypeLine.Contains("Artifact"))).Where(c => c.TypeLine.Contains("Enchantment")).ToList());
    Console.WriteLine($"Sorted {secondSortedTopBlock[SecondLevelBlocks.Enchantments].Count} {topBlock.Key} enchantments");

    secondSortedCards.Add(topBlock.Key, secondSortedTopBlock);
}

//now we can figure out how we decide to divide our alphabetical blocks

//these constants decide how many divisions each second level block gets
const int creaturesArtifactsDivisions = 5;
const int instantsSorceriesDivisions = 3;
const int landDivisions = 2;

const string alphaOrder = "abcdefghijklmnopqrstuvwxyz";

Dictionary<char, List<Card>> landsAlphabet = new();
foreach(char letter in alphaOrder)
{
    landsAlphabet.Add(letter, topLevelBlocks[TopLevelSortingBlocks.Lands].Where(c => c.Name.ToLower().StartsWith(letter)).ToList());
    Console.WriteLine($"Found {landsAlphabet[letter].Count} {letter} land cards");
}

var landsResult = FindAlphabeticDivisions(landsAlphabet, landDivisions);

Console.WriteLine("\nFound the following divisions to be optimal for lands:");
foreach(var division in landsResult)
{
    Console.WriteLine(String.Join(',', division));
}

foreach(var topBlock in secondSortedCards)
{
    foreach(var secondBlock in topBlock.Value)
    {
        //there's not enough enchantments to justify dividing them alphabetically
        if (secondBlock.Key == SecondLevelBlocks.Enchantments) continue;

        Console.WriteLine($"\n\nFinding divisions for {topBlock.Key} {secondBlock.Key} cards");
        Dictionary<char, List<Card>> cardsAlphabet = new();
        foreach (char letter in alphaOrder)
        {
            cardsAlphabet.Add(letter, secondBlock.Value.Where(c => c.Name.ToLower().StartsWith(letter)).ToList());
        }

        int divisionsCount = secondBlock.Key == SecondLevelBlocks.CreaturesArtifacts ? creaturesArtifactsDivisions : instantsSorceriesDivisions;

        var result = FindAlphabeticDivisions(cardsAlphabet, divisionsCount);

        Console.WriteLine($"\nFound the following divisions to be optimal for {topBlock.Key} {secondBlock.Key}:");

        foreach(var division in result)
        {
            Console.WriteLine($"{String.Join(',', division)} ({division.Sum(c => cardsAlphabet[c].Count)})");
        }
    }
}