namespace WordProcessor
{
    public class PhraseFrequency
    {
        public int Frequency { get; }
        public string Word { get; }

        public PhraseFrequency(int frequency, string word)
        {
            Frequency = frequency;
            Word = word;
        }
    }
}