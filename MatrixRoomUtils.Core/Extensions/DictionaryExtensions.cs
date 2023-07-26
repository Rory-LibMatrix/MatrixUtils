namespace MatrixRoomUtils.Core.Extensions;

public static class DictionaryExtensions {
    public static bool ChangeKey<TKey, TValue>(this IDictionary<TKey, TValue> dict,
        TKey oldKey, TKey newKey) {
        TValue value;
        if (!dict.Remove(oldKey, out value))
            return false;

        dict[newKey] = value; // or dict.Add(newKey, value) depending on ur comfort
        return true;
    }

    public static Y GetOrCreate<X, Y>(this IDictionary<X, Y> dict, X key) where Y : new() {
        if (dict.TryGetValue(key, out var value)) {
            return value;
        }

        value = new Y();
        dict.Add(key, value);
        return value;
    }

    public static Y GetOrCreate<X, Y>(this IDictionary<X, Y> dict, X key, Func<X, Y> valueFactory) {
        if (dict.TryGetValue(key, out var value)) {
            return value;
        }

        value = valueFactory(key);
        dict.Add(key, value);
        return value;
    }
}
