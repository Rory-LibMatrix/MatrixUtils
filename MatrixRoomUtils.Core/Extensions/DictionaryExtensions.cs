namespace MatrixRoomUtils.Core.Extensions;

public static class DictionaryExtensions
{
    public static bool ChangeKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, 
        TKey oldKey, TKey newKey)
    {
        TValue value;
        if (!dict.Remove(oldKey, out value))
            return false;

        dict[newKey] = value;  // or dict.Add(newKey, value) depending on ur comfort
        return true;
    }
}