using System.Linq;

namespace Sandbox.UI;

public partial class Panel : IStyleTarget
{
	string IStyleTarget.ElementName => ElementName;
	string IStyleTarget.Id => Id;
	PseudoClass IStyleTarget.PseudoClass => PseudoClass;
	IStyleTarget IStyleTarget.Parent => Parent;
	IReadOnlyList<IStyleTarget> IStyleTarget.Children => _children?.AsReadOnly();
	bool IStyleTarget.HasClasses( string[] classes ) => HasClasses( classes );
	int IStyleTarget.SiblingIndex => SiblingIndex;

	/// <summary>
	/// Get the currently active style blocks for this panel (for debugging/inspection)
	/// </summary>
	public IEnumerable<IStyleBlock> ActiveStyleBlocks
	{
		get
		{
			if (Style?.LastActiveRules == null)
				return Enumerable.Empty<IStyleBlock>();

			// Create a defensive copy to prevent "Collection was modified" errors
			// when the style is rebuilt during iteration
			var rulesCopy = Style.LastActiveRules.ToList();
			return rulesCopy.Where(x => x?.Block != null).Select(x => x.Block);
		}
	}
}
