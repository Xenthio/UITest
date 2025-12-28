namespace Sandbox.UI;

/// <summary>
/// A CSS rule - ie ".chin { width: 100%; height: 100%; }"
/// </summary>
public interface IStyleBlock
{
	/// <summary>
	/// The filename of the file containing this style block (or null if none)
	/// </summary>
	public string FileName { get; }

	/// <summary>
	/// The absolute on disk filename for this style block (or null if not on disk)
	/// </summary>
	public string AbsolutePath { get; }

	/// <summary>
	/// The line in the file containing this style block
	/// </summary>
	public int FileLine { get; }

	/// <summary>
	/// A list of selectors
	/// </summary>
	public IEnumerable<string> SelectorStrings { get; }

	/// <summary>
	/// Get the list of raw style values
	/// </summary>
	public List<StyleProperty> GetRawValues();

	/// <summary>
	/// Update a raw style value
	/// </summary>
	public bool SetRawValue( string key, string value, string originalValue = null );


	public struct StyleProperty
	{
		/// <summary>
		/// Name of the property, ie "color" or "width"
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Current value of the property (which is being rendered)
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// The value that was loaded from the .scss file
		/// </summary>
		public string OriginalValue { get; set; }

		/// <summary>
		/// The line in the file containing this value
		/// </summary>
		public int Line { get; set; }

		/// <summary>
		/// If parsing this property was successful or failed
		/// </summary>
		public bool IsValid { get; set; }
	}
}
