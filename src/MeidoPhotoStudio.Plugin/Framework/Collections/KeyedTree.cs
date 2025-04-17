namespace MeidoPhotoStudio.Plugin.Framework.Collections;

public class KeyedTree<TKey, TValue>(params KeyedTree<TKey, TValue>.Node[] children) : IEnumerable<KeyedTree<TKey, TValue>.Node>
{
    private readonly Dictionary<TKey, Node> nodes = children.ToDictionary(static node => node.Key, static node => node);

    public Node this[TKey key]
    {
        get => nodes[key];
        set => nodes[key] = value;
    }

    public bool ContainsKey(TKey key) =>
        nodes.ContainsKey(key);

    public bool RemoveNode(TKey key) =>
        nodes.Remove(key);

    public bool TryGetNode(TKey key, out Node node) =>
        nodes.TryGetValue(key, out node);

    public IEnumerator<Node> GetEnumerator() =>
        nodes.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public class Node(TKey key, TValue value, params Node[] children) : IEnumerable<Node>
    {
        private readonly Dictionary<TKey, Node> nodes = children.ToDictionary(static node => node.Key, static node => node);

        public TKey Key { get; } = key;

        public TValue Value { get; set; } = value;

        public Node this[TKey key]
        {
            get => nodes[key];
            set => nodes[key] = value;
        }

        public bool ContainsKey(TKey key) =>
            nodes.ContainsKey(key);

        public bool TryGetNode(TKey key, out Node node) =>
            nodes.TryGetValue(key, out node);

        public IEnumerator<Node> GetEnumerator() =>
            nodes.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
