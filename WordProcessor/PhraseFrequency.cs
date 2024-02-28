namespace WordProcessor
{
    /// <summary>
    /// Model class to facilitate the calculation of frequency of occurrence of a particular phrase in a given text content
    /// </summary>
    /// <seealso cref="FrequencyCalculator"/>
    public class PhraseFrequency
    {
        /// <summary>
        /// Frequency of occurrence of a particular phrase in a given text content
        /// </summary>
        public int Frequency { get; }
        /// <summary>
        /// Phrase in a given text content
        /// </summary>
        public string Word { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="word"></param>
        public PhraseFrequency(int frequency, string word)
        {
            Frequency = frequency;
            Word = word;
        }
    }
}