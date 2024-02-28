using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WordProcessor;

/// <summary>
/// 
/// </summary>
/// <seealso cref="PhraseFrequency"/>
/// <seealso cref="Downloader"/>
public static partial class FrequencyCalculator
{
    /// <summary>
    /// Calculate frequency of occurrence of a particular word in a given text content
    /// </summary>
    /// <param name="content"></param>
    /// <param name="n">Take only first set of items with frequency in descending order </param>
    /// <returns>Top n items with frequency in descending order</returns>
    public static IEnumerable<PhraseFrequency> CalculateWordFrequency(string content, int? n = null)
    {
        content = content.ToLower();
        var wordPattern = WordExp();
        return CalculateFrequency(content, wordPattern, n);
    }
    /// <summary>
    /// Calculate frequency of occurrence of a particular w for given url
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="n">Take only top n items with frequency in descending order</param>
    /// <returns>Top n items with frequency in descending order</returns>
    /// <seealso cref="CalculateWordFrequency(string, int?)"/>
    public static IEnumerable<PhraseFrequency> CalculateWordFrequency(Uri uri, int? n = null) => CalculateWordFrequency(new Downloader(4).GetTextFromUrl(uri), n);

    /// <summary>
    /// Calculate frequency of occurrence of a particular word pair in a given url
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="n"></param>
    /// <returns>Set of items with frequency in descending order</returns>
    /// <seealso cref="CalculateWordPairFrequency(string, int)"/>
    public static IEnumerable<PhraseFrequency> CalculateWordPairFrequency(Uri uri, int n) => CalculateWordPairFrequency(new Downloader(4).GetTextFromUrlAsync(uri).Result, n);

    /// <summary>
    /// Calculate frequency of occurrence of a particular word pair in a given text content
    /// </summary>
    /// <param name="content"></param>
    /// <param name="n"></param>
    /// <returns>Top n items with frequency in descending order</returns>
    /// <seealso cref="CalculateWordFrequency(Uri, int?)"/>
    public static IEnumerable<PhraseFrequency> CalculateWordPairFrequency(string content, int n)
    {
        content = content.ToLower();
        var wordFrequency = new Queue<PhraseFrequency>(CalculateWordFrequency(content));
        var consecutiveFrequency = new List<PhraseFrequency>();
        while (wordFrequency.Count != 0)
        {
            var w = wordFrequency.Dequeue();
            if (consecutiveFrequency.Count >= 10 && w.Frequency < consecutiveFrequency[^1].Frequency)
                break;
            var consecutivePatterns = new Regex[] {
            new($"\\b{w.Word}\\s+\\w+", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture),
            new($"\\w+\\s+{w.Word}\\b", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture) };
            var frequencies = consecutivePatterns.Select(p => CalculateFrequency(content, p, n)).SelectMany(p => p).OrderByDescending(p => p.Frequency).Take(n).ToList();
            if (frequencies.Count <= 0)
                continue;
            if (consecutiveFrequency.Count == 0)
                consecutiveFrequency.AddRange(frequencies);
            else
            {
                if (frequencies[0].Frequency <= consecutiveFrequency[^1].Frequency)
                    continue;
                consecutiveFrequency = consecutiveFrequency.Union(frequencies, new PhraseFrequencyComparer()).OrderByDescending(p => p.Frequency).Take(n).ToList();
            }
        }
        return consecutiveFrequency;
    }

    /// <summary>
    /// Calculate frequency of occurrence of a given pattern in a given text content
    /// </summary>
    /// <param name="content"></param>
    /// <param name="wordPattern"></param>
    /// <param name="n">Take only top n items with frequency in descending order</param>
    /// <returns>Top n items with frequency in descending order</returns>
    /// <seealso cref="CalculateWordFrequency(Uri, int?)"/>
    private static IEnumerable<PhraseFrequency> CalculateFrequency(string content, Regex wordPattern, int? n = null)
    {
        var words = wordPattern.Matches(content).Select(p => p.Value);
        var q = words.GroupBy(p => p, k => k).Select(g => new PhraseFrequency(g.Count(), g.Key)).OrderByDescending(p => p.Frequency);
        if (n.HasValue)
            return q.Take(n.Value);
        return q;
    }

    [GeneratedRegex(@"\w+", RegexOptions.IgnoreCase, "en-IN")]
    private static partial Regex WordExp();
}