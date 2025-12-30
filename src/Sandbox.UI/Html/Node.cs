using AngleSharp.Html.Parser;
using AngleSharp.Dom;

namespace Sandbox.Html;

/// <summary>
/// HTML node type enumeration
/// </summary>
public enum NodeType
{
    Element,
    Text,
    Document
}

/// <summary>
/// Wrapper around AngleSharp HTML parsing to match S&box's Sandbox.Html.Node API
/// </summary>
public class Node
{
    private readonly INode _node;
    
    public NodeType NodeType
    {
        get
        {
            if (_node is IDocument) return NodeType.Document;
            if (_node is IElement) return NodeType.Element;
            return NodeType.Text;
        }
    }
    
    public string Name => (_node as IElement)?.TagName?.ToLower() ?? "";
    
    public IEnumerable<Node> ChildNodes
    {
        get
        {
            return _node.ChildNodes.Select(n => new Node(n));
        }
    }
    
    public IEnumerable<Attribute> Attributes
    {
        get
        {
            var element = _node as IElement;
            if (element == null) return Enumerable.Empty<Attribute>();
            
            return element.Attributes.Select(a => new Attribute { Name = a.Name, Value = a.Value });
        }
    }
    
    public string TextContent => _node.TextContent;
    
    private Node(INode node)
    {
        _node = node;
    }
    
    public static Node Parse(string html)
    {
        var parser = new HtmlParser();
        var document = parser.ParseDocument(html);
        return new Node(document);
    }
    
    public class Attribute
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
