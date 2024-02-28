using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WordProcessor
{
    /// <summary>
    /// Class to download content from given url traversing up to a certain level of depth of the hyperlinks of same domain
    /// </summary>
    /// <remarks>
    /// Constructor for Downloaded
    /// </remarks>
    /// <param name="depth"></param>
    public partial class Downloader(byte depth)
    {
        /// <summary>
        /// Regex to find hyperlink inside html content
        /// </summary>
        static readonly Regex hyperlinkPattern = hyperLinkExp();

        readonly byte maxDepth = depth;

        private float presentLevel = 0;
        /// <summary>
        /// Present Level
        /// </summary>
        public float PresentLevel => presentLevel;

        /// <summary>
        /// Outputs string stripping off html tags and extra spaces
        /// </summary>
        /// <param name="input"></param>
        /// <returns>string</returns>
        public static string CleanHtml(string input)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(input ?? "");
            return spaceExp().Replace(doc.DocumentNode.InnerText.Trim(), " ");
        }
        /// <summary>
        /// Synchronous call to fetch html cleaned content from given url.
        /// It traverses up to a certain level of depth of the hyperlinks of same domain
        /// </summary>
        /// <param name="link"></param>
        /// <param name="visited">Keep track already visited links</param>
        /// <param name="level">Level of depth of the hyperlinks of same domain to traverse</param>
        /// <returns></returns>
        /// <seealso cref="GetTextFromUrlAsync(Uri, ConcurrentDictionary{string, string}, byte)"/>
        /// <seealso cref="GetTextFromUrl(Uri)"/>
        public string GetTextFromUrl(Uri link, List<string> visited, byte level = 0)
        {
            var linkString = link.ToString().ToLower();
            if (visited.Contains(linkString))
                return string.Empty;
            string html = string.Empty;
            using var wc = new HttpClient();
            try
            {
                html = wc.GetStringAsync(link).Result;
            }
            catch (WebException)
            {
                return string.Empty;
            }
            finally
            {
                visited.Add(linkString);
            }
            if (level == maxDepth)
                return CleanHtml(html);
            level++;
            var hyperLinks = GetHyperLinks(html, link.Host, visited);
            html = CleanHtml(html);
            if (hyperLinks.Count == 0)
                return html;
            var htmlBuilder = new StringBuilder(html);
            do
            {
                var hl = hyperLinks.Dequeue();
                var txt = GetTextFromUrl(hl, visited, level);
                if (!string.IsNullOrWhiteSpace(txt))
                    htmlBuilder.AppendLine(txt);
            } while (hyperLinks.Count != 0);
            return htmlBuilder.ToString();
        }
        /// <summary>
        /// Synchronous call to fetch html cleaned content from given url.
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        /// <seealso cref="GetTextFromUrl(Uri, List{string}, byte)"/>
        /// <seealso cref="GetTextFromUrlAsync(Uri)"/>
        public string GetTextFromUrl(Uri link) => GetTextFromUrl(link, []);

        /// <summary>
        /// Asynchronous multithreaded call to fetch html cleaned content from given url.
        /// </summary>
        /// <param name="link"></param>
        /// <param name="visited">Keep track already visited links and corresponding contents</param>
        /// <param name="level">Level of depth of the hyperlinks of same domain to traverse</param>
        /// <returns></returns>
        /// <seealso cref="GetTextFromUrl(Uri, List{string}, byte)"/>
        /// <seealso cref="GetTextFromUrlAsync(Uri)"/>
        public async Task GetTextFromUrlAsync(Uri link, ConcurrentDictionary<string, string> visited, byte level = 0)
        {
            var linkString = link.ToString().ToLower();
            if (visited.ContainsKey(linkString))
                return;
            using var wc = new HttpClient();
            string html = string.Empty;
            var cleaned = string.Empty;
            try
            {
                html = await wc.GetStringAsync(link).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(html))
                    cleaned = CleanHtml(html);
                if (!visited.ContainsKey(linkString))
                    visited.TryAdd(linkString, cleaned);
            }
            if (level == maxDepth)
                return;
            level++;
            var lastProgress = 1.0f * level / maxDepth;
            var hyperLinks = GetHyperLinks(html, link.Host, visited.Keys);
            if (hyperLinks.Count == 0)
            {
                if (presentLevel < lastProgress)
                    Interlocked.Exchange(ref presentLevel, lastProgress);
                return;
            }
            var increment = 1.0f / (maxDepth * hyperLinks.Count);
            var opts = new ParallelOptions { MaxDegreeOfParallelism = 3 };
            Parallel.ForEach(hyperLinks, opts, hl =>
            {
                var t = GetTextFromUrlAsync(hl, visited, level);
                lastProgress += increment;
                if (presentLevel < lastProgress)
                    Interlocked.Exchange(ref presentLevel, lastProgress);
                t.Wait();
            });
        }
        /// <summary>
        /// ASynchronous multithreaded call to fetch html cleaned content from given url.
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        /// <seealso cref="GetTextFromUrlAsync(Uri, ConcurrentDictionary{string, string}, byte)"/>
        /// <seealso cref="GetTextFromUrl(Uri)"/>
        public async Task<string> GetTextFromUrlAsync(Uri link)
        {
            presentLevel = 0;
            var visited = new ConcurrentDictionary<string, string>();
            await GetTextFromUrlAsync(link, visited);
            return String.Join(" ", visited.Select(p => p.Value));
        }

        /// <summary>
        /// Find hyperlinks in a page for the the given hostname excluding the already visited link
        /// </summary>
        /// <param name="html"></param>
        /// <param name="host"></param>
        /// <param name="visited">Collection of already visited and to be excluded links</param>
        /// <returns></returns>
        private static Queue<Uri> GetHyperLinks(string html, string host, IEnumerable<string> visited) => new(hyperlinkPattern.Matches(html).Select(p => p.Value.ToLower()).Where(p => p.Contains(host) && !visited.Contains(p)).Select(p => new Uri(p)));
        [GeneratedRegex(@"(?<=\<a.*\bhref\=\"")[^\""]*", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, "en-IN")]
        private static partial Regex hyperLinkExp();

        [GeneratedRegex(@"\s+")]
        private static partial Regex spaceExp();
    }
}