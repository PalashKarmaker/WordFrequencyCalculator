using NUnit.Framework;
using System;
using System.Linq;

namespace WordProcessor.Tests
{
    [TestFixture()]
    public class FrequencyCalculatorTests
    {
        [TestCase("https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9", 10)]
        [Test()]
        public void CalculateWordFrequenceTest(Uri uri, int top)
        {
            var fc = new FrequencyCalculator();
            var f = fc.CalculateWordFrequency(uri, top).ToList();
            Assert.IsNotNull(f);
        }

        [TestCase("https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9", 10)]
        [Test()]
        public void CalculateWordPairFrequencyTest(Uri uri, int top)
        {
            var fc = new FrequencyCalculator();
            var f = fc.CalculateWordPairFrequency(uri, top).ToList();
            Assert.Greater(f.Count, 0);
        }
    }
}