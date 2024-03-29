﻿using NUnit.Framework;
using System;
using WordProcessor;

namespace WordProcessorTests;

[TestFixture()]
public class DownloaderTests
{
    [Test()]
    [TestCase(@"<span class=""nav-bar-button-chevron"" aria-hidden=""true"">Before test string
				<span class=""docon docon-chevron-down-light expanded-indicator"">This is jus test</span>
			</span>")]
    public void CleanHtmlTest(string html)
    {
        var txt = Downloader.CleanHtml(html);
        Assert.That(txt.IndexOf('<'), Is.LessThan(0));
    }

    [Test()]
    [TestCase("https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9", 4)]
    public void GetHtmlFromUrlAsyncTest(Uri uri, byte depth)
    {
        var html = new Downloader(depth).GetTextFromUrlAsync(uri).Result;
        Assert.That(html.Length, Is.GreaterThan(0));
    }

    [Test()]
    [TestCase("https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9", 4)]
    public void GetTextFromUrlTest(Uri uri, byte depth)
    {
        var txt = new Downloader(depth).GetTextFromUrl(uri);
        Assert.That(txt.Length, Is.GreaterThan(0));
    }
}