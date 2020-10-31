using NUnit.Framework;
using System;

namespace WordProcessor.Tests
{
    [TestFixture()]
    public class UtilityTests
    {
        [Test()]
        [TestCase(@"<span class=""nav-bar-button-chevron"" aria-hidden=""true"">Before test string
				<span class=""docon docon-chevron-down-light expanded-indicator"">This is jus test</span>
			</span>")]
        public void CleanHtmlTest(string html)
        {
            var txt = Utility.CleanHtml(html);
            Assert.Less(txt.IndexOf("<"), 0);
        }

        [Test()]
        [TestCase("https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9", 4)]
        public void GetHtmlFromUrlAsyncTest(Uri uri, byte depth)
        {
            var html = new Utility(depth).GetTextFromUrlAsync(uri).Result;
            Assert.Greater(html.Length, 0);
        }

        [Test()]
        [TestCase("https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9", 4)]
        public void GetTextFromUrlTest(Uri uri, byte depth)
        {
            var txt = new Utility(depth).GetTextFromUrl(uri);
            Assert.Greater(txt.Length, 0);
        }
    }
}