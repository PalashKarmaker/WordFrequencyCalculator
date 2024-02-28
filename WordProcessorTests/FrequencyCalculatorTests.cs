using NUnit.Framework;
using System;
using System.Linq;

namespace WordProcessor.Tests;

[TestFixture()]
public class FrequencyCalculatorTests
{
    [TestCase("https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9", 10)]
    [Test()]
    public void CalculateWordFrequenceTest(Uri uri, int top)
    {
        
        var f = FrequencyCalculator.CalculateWordFrequency(uri, top).ToList();
        Assert.That(f, Is.Not.Null);
    }

    [TestCase("https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9", 10)]
    [Test()]
    public void CalculateWordPairFrequencyTest(Uri uri, int top)
    {
        
        var f = FrequencyCalculator.CalculateWordPairFrequency(uri, top).ToList();
        Assert.That(f.Count, Is.GreaterThan(0));
    }
}