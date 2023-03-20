using ScryfallApi.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MTGAlphaSortDivider
{
    public static class FindDivisions
    {
        private const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        public static List<HashSet<char>> FindAlphabeticDivisions(Dictionary<char,List<Card>> cardsByAlpha, int divisions)
        {
            return FindAlphabeticDivisionsRecursive(cardsByAlpha,divisions, alphabet.ToList(), 0);
        }

        private static List<HashSet<char>> FindAlphabeticDivisionsRecursive(Dictionary<char, List<Card>> cardsByAlpha, int divisions, List<char> remainingCharacters, int depth)
        {
            if(divisions == 1)
            {
                List<HashSet<char>> result = new();
                result.Add(remainingCharacters.ToHashSet());
                return result;
            } else if(remainingCharacters.Count == 1)
            {
                List<HashSet<char>> result = new();
                result.Add(remainingCharacters.ToHashSet());
                for(int i = 0; i < divisions - 1; i++)
                {
                    result.Add(new());
                }
                return result;
            }
            
            int minimumDifference = int.MaxValue;
            List<HashSet<char>> bestDivisions = new();

            for(int i = 0; i < remainingCharacters.Count; i++)
            {
                List<HashSet<char>> currentResult = FindAlphabeticDivisionsRecursive(cardsByAlpha, divisions - 1, remainingCharacters.TakeLast(remainingCharacters.Count - i).ToList(), depth + 1);
                currentResult.Insert(0, remainingCharacters.Take(i).ToHashSet());

                if(DivisionsMaxSizeDiff(cardsByAlpha, currentResult, divisions) < minimumDifference)
                {
                    minimumDifference = DivisionsMaxSizeDiff(cardsByAlpha, currentResult, divisions);
                    bestDivisions = currentResult;
                }
            }

            return bestDivisions;
        }

        private static int DivisionsMaxSizeDiff(Dictionary<char, List<Card>> cardsByAlpha, List<HashSet<char>> divisions, int divisionsCount)
        {
            if(divisions.Count != divisionsCount)
            {
                return int.MaxValue;
            }

            int smallestDivision = divisions.Min(d => d.Sum(c => cardsByAlpha[c].Count));
            int largestDivision = divisions.Max(d => d.Sum(c => cardsByAlpha[c].Count));

            return largestDivision - smallestDivision;
        }
    }
}
