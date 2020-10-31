using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WordProcessor
{
    public class FrequencyCalculator
    {
        public IEnumerable<PhraseFrequency> CalculateWordFrequence(string content, int? top = null)
        {
            content = content.ToLower();
            var wordPattern = new Regex(@"\w+", RegexOptions.IgnoreCase);
            return CalculateFrequency(content, wordPattern, top);
        }

        private static IEnumerable<PhraseFrequency> CalculateFrequency(string content, Regex wordPattern, int? top = null)
        {
            var words = wordPattern.Matches(content).Select(p => p.Value);
            var q = words.GroupBy(p => p, k => k).Select(g => new PhraseFrequency(g.Count(), g.Key)).OrderByDescending(p => p.Frequency);
            if (top.HasValue)
                return q.Take(top.Value);
            return q;
        }

        public IEnumerable<PhraseFrequency> CalculateWordFrequence(Uri uri, int? top = null) => CalculateWordFrequence(new Utility(4).GetTextFromUrl(uri), top);

        public IEnumerable<PhraseFrequency> CalculateWordPairFrequency(Uri uri, int top) => CalculateWordPairFrequency(new Utility(4).GetTextFromUrlAsync(uri).Result, top);

        public IEnumerable<PhraseFrequency> CalculateWordPairFrequency(string content, int top)
        {
            content = content.ToLower();
            var wordFrequency = new Queue<PhraseFrequency>(CalculateWordFrequence(content));
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
                var freqs = consecutivePatterns.Select(p => CalculateFrequency(content, p, top)).SelectMany(p => p).OrderByDescending(p => p.Frequency).Take(top).ToList();
                if (freqs == null || freqs.Count <= 0)
                    continue;
                if (consecutiveFrequency.Count == 0)
                    consecutiveFrequency.AddRange(freqs);
                else
                {
                    if (freqs.First().Frequency <= consecutiveFrequency.Last().Frequency)
                        continue;
                    consecutiveFrequency = consecutiveFrequency.Union(freqs, new PhraseFrequencyComparer()).OrderByDescending(p => p.Frequency).Take(top).ToList();
                }
            }
            return consecutiveFrequency;
        }
    }
}