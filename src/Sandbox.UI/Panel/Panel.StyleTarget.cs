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
				yield break;

			foreach (var rule in Style.LastActiveRules)
			{
				if (rule?.Block != null)
					yield return rule.Block;
			}
		}
	}
}
