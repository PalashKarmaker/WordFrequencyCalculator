using System.Collections.Generic;

namespace WordProcessor
{
    public class PhraseFrequencyComparer : IEqualityComparer<PhraseFrequency>
    {
        public bool Equals(PhraseFrequency x, PhraseFrequency y) => x.Word == y.Word && x.Frequency == y.Frequency;

        public int GetHashCode(PhraseFrequency obj) => obj.Word.GetHashCode();
    }
}
