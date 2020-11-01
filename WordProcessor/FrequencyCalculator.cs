using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WordProcessor
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="PhraseFrequency"/>
    /// <seealso cref="Downloader"/>
    public class FrequencyCalculator
    {
        /// <summary>
        /// Calculate frequency of occurance of a particular word in a given text content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="n">Take only first set of items with freequency in descending order </param>
        /// <returns>Top n items with freequency in descending order</returns>
        public IEnumerable<PhraseFrequency> CalculateWordFrequency(string content, int? n = null)
        {
            content = content.ToLower();
            var wordPattern = new Regex(@"\w+", RegexOptions.IgnoreCase);
            return CalculateFrequency(content, wordPattern, n);
        }
        /// <summary>
        /// Calculate frequency of occurance of a given pattern in a given text content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="wordPattern"></param>
        /// <param name="n">Take only top n items with freequency in descending order</param>
        /// <returns>Top n items with freequency in descending order</returns>
        /// <seealso cref="CalculateWordFrequency(Uri, int?)"/>
        private static IEnumerable<PhraseFrequency> CalculateFrequency(string content, Regex wordPattern, int? n = null)
        {
            var words = wordPattern.Matches(content).Select(p => p.Value);
            var q = words.GroupBy(p => p, k => k).Select(g => new PhraseFrequency(g.Count(), g.Key)).OrderByDescending(p => p.Frequency);
            if (n.HasValue)
                return q.Take(n.Value);
            return q;
        }
        /// <summary>
        /// Calculate frequency of occurance of a particular w foordr given url
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="n">Take only top n items with freequency in descending order</param>
        /// <returns>Top n items with freequency in descending order</returns>
        /// <seealso cref="CalculateWordFrequency(string, int?)"/>
        public IEnumerable<PhraseFrequency> CalculateWordFrequency(Uri uri, int? n = null) => CalculateWordFrequency(new Downloader(4).GetTextFromUrl(uri), n);
        /// <summary>
        /// Calculate frequency of occurance of a particular word pair in a given url
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="n"></param>
        /// <returns>Set of items with freequency in descending order</returns>
        /// <seealso cref="CalculateWordPairFrequency(string, int)"/>
        public IEnumerable<PhraseFrequency> CalculateWordPairFrequency(Uri uri, int n) => CalculateWordPairFrequency(new Downloader(4).GetTextFromUrlAsync(uri).Result, n);
        /// <summary>
        /// Calculate frequency of occurance of a particular word pair in a given text content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="n"></param>
        /// <returns>Top n items with freequency in descending order</returns>
        /// <seealso cref="CalculateWordFrequency(Uri, int?)"/>
        public IEnumerable<PhraseFrequency> CalculateWordPairFrequency(string content, int n)
        {
            content = content.ToLower();
            var wordFrequency = new Queue<PhraseFrequency>(CalculateWordFrequency(content));
            var consecutiveFrequency = new List<PhraseFrequency>();
            while (wordFrequency.Any())
            {
                var w = wordFrequency.Dequeue();
                if (consecutiveFrequency.Count >= 10 && w.Frequency < consecutiveFrequency.Last().Frequency)
                    break;
                var consecutivePatterns = new Regex[] {
                new Regex($"\\b{w.Word}\\s+\\w+", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture),
                new Regex($"\\w+\\s+{w.Word}\\b", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture) };
                //var consecutivePattern = new Regex($"((?<=\\b{w.Word}\\s+)\\w+)|(\\w+(?=\\s+{w.Word}\\b))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                var freqs = consecutivePatterns.Select(p => CalculateFrequency(content, p, n)).SelectMany(p => p).OrderByDescending(p => p.Frequency).Take(n).ToList();
                if (freqs == null || freqs.Count <= 0)
                    continue;
                if (consecutiveFrequency.Count == 0)
                    consecutiveFrequency.AddRange(freqs);
                else
                {
                    if (freqs.First().Frequency <= consecutiveFrequency.Last().Frequency)
                        continue;
                    consecutiveFrequency = consecutiveFrequency.Union(freqs, new PhraseFrequencyComparer()).OrderByDescending(p => p.Frequency).Take(n).ToList();
                }
            }
            return consecutiveFrequency;
        }
    }
}