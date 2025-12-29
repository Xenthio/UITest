namespace Sandbox;

/// <summary>
/// Marks a class as a library component that can be instantiated by name in Razor templates.
/// For example, [Library("button")] allows &lt;button&gt; tags in Razor to create instances of this class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class LibraryAttribute : Attribute
{
	/// <summary>
	/// This is the name that will be used to create this class in Razor templates.
	/// If you don't set it via the attribute constructor it will be set
	/// to the name of the class it's attached to.
	/// </summary>
	public string Name { get; internal set; }

	/// <summary>
	/// A nice presentable title to show in editors.
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// A description for documentation or editor display.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Group name for organizing components.
	/// </summary>
	public string? Group { get; set; }

	public LibraryAttribute()
	{
		Name = string.Empty;
	}

	public LibraryAttribute(string name)
	{
		Name = name;
	}
}
