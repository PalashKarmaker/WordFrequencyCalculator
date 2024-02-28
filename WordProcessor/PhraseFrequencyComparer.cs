using System.Collections.Generic;

namespace WordProcessor;
/// <summary>
/// Comparer defined for calculating frequency of phrase
/// </summary>
internal class PhraseFrequencyComparer : IEqualityComparer<PhraseFrequency>
{
    /// <summary>
    /// Implementation of Equality for <code>PhraseFrequency</code> 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool Equals(PhraseFrequency x, PhraseFrequency y) => x.Word == y.Word && x.Frequency == y.Frequency;
    /// <summary>
    /// <code>GetHashCode</code> override
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode(PhraseFrequency obj) => obj.Word.GetHashCode();
}
