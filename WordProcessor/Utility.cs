using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordProcessor
{
    public class Utility
    {
        public static string CleanHtml(string input)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(input ?? "");
            return Regex.Replace(doc.DocumentNode.InnerText.Trim(), @"\s+", " ");
        }
        static int maxDepth = 4;
        //public static string CleanHtml(string s) => s.Replace( Regex.Replace(s, "<[^>]*>", string.Empty, RegexOptions.Multiline);
        public static string GetTextFromUrl(Uri link, List<string> visited, int level = 0)
        {
            if (visited.Contains(link.ToString()))
                return string.Empty;
            using var wc = new WebClient();
            var html = wc.DownloadString(link);
            visited.Add(link.ToString().ToLower());
            if (level == maxDepth)
                return CleanHtml(html);
            level++;
            var hyperLinks = GetHyperLinks(html, link.Host, visited);
            html = CleanHtml(html);
            if (!hyperLinks.Any())
                return html;
            var htmlBuilder = new StringBuilder(html);
            do
            {
                var hl = hyperLinks.Dequeue();
                var txt = GetTextFromUrl(hl, visited, level);
                if (!string.IsNullOrWhiteSpace(txt))
                    htmlBuilder.AppendLine(txt);
            } while (hyperLinks.Any());
            return htmlBuilder.ToString();
        }

        public static async Task GetTextFromUrlAsync(Uri link, ConcurrentDictionary<string, string> visited, int level = 0)
        {
            var linkString = link.ToString().ToLower();
            if (visited.ContainsKey(linkString))
                return;
            using var wc = new WebClient();
            var html = await wc.DownloadStringTaskAsync(link);
            var cleaned = CleanHtml(html);
            if (!visited.ContainsKey(linkString))
                visited.TryAdd(linkString, cleaned);
            if (level == maxDepth)
                return;
            level++;
            var hyperLinks = GetHyperLinks(html, link.Host, visited.Keys);
            if (!hyperLinks.Any())
                return;
            var downloaadTasks = new List<Task>();
            do
            {
                var hl = hyperLinks.Dequeue();
                downloaadTasks.Add(GetTextFromUrlAsync(hl, visited, level));
            } while (hyperLinks.Any());
            if (downloaadTasks.Count > 0)
                Task.WaitAll(downloaadTasks.ToArray());
            return;
        }
        static Regex hyperlinkPattern = new Regex(@"(?<=\<a.*\bhref\=\"")[^\""]*", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        private static Queue<Uri> GetHyperLinks(string html, string host, IEnumerable<string> visited) => new Queue<Uri>(hyperlinkPattern.Matches(html).Select(p => p.Value.ToLower()).Where(p => p.Contains(host) && !visited.Contains(p)).Select(p => new Uri(p)));

        public static string GetTextFromUrl(Uri link) => GetTextFromUrl(link, new List<string>());

        public static async Task<string> GetTextFromUrlAsync(Uri link)
        {
            var visited = new ConcurrentDictionary<string, string>();
            await GetTextFromUrlAsync(link, visited);
            return String.Join(" ", visited.Select(p => p.Value));
        }
    }
}