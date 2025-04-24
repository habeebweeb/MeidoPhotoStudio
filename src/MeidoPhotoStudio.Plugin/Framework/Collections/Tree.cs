namespace MeidoPhotoStudio.Plugin.Framework.Collections;

public class Tree<T>(params Tree<T>.Node[] children) : IEnumerable<Tree<T>.Node>
{
    private readonly List<Node> nodes = [.. children];

    public void Add(Node child) =>
        nodes.Add(child ?? throw new ArgumentNullException(nameof(child)));

    public IEnumerator<Node> GetEnumerator() =>
        nodes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public class Node(T value, params Node[] children) : IEnumerable<Node>
    {
        private readonly List<Node> children = [.. children];

        public T Value { get; set; } = value;

        public void Add(Node node) =>
            children.Add(node ?? throw new ArgumentNullException(nameof(node)));

        public IEnumerator<Node> GetEnumerator() =>
            children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
