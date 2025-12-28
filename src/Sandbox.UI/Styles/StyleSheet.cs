namespace Sandbox.UI;

public class StyleSheet
{
	public static List<StyleSheet> Loaded { get; internal set; } = new List<StyleSheet>();

	/// <summary>
	/// Between sessions we clear the stylesheets, so one gamemode can't accidentally
	/// use cached values from another.
	/// </summary>
	internal static void InitStyleSheets()
	{
		foreach ( var sheet in Loaded )
		{
			sheet?.Release();
		}

		Loaded.Clear();
	}

	public List<StyleBlock> Nodes { get; set; } = new List<StyleBlock>();
	public string FileName { get; internal set; }
	public List<string> IncludedFiles { get; set; } = new List<string>();
	public Dictionary<string, string> Variables;
	public Dictionary<string, KeyFrames> KeyFrames = new Dictionary<string, KeyFrames>( StringComparer.OrdinalIgnoreCase );

	/// <summary>
	/// Releases the filesystem watcher so we won't get file changed events.
	/// </summary>
	public void Release()
	{
		// File watching not implemented in this port
	}

	public static StyleSheet FromFile( string filename, IEnumerable<(string key, string value)> variables = null, bool failSilently = false )
	{
		filename = filename.NormalizeFilename();

		var alreadyLoaded = Loaded.FirstOrDefault( x => x.FileName == filename );
		if ( alreadyLoaded != null )
			return alreadyLoaded;

		var sheet = new StyleSheet();
		sheet.UpdateFromFile( filename, failSilently );

		sheet.AddVariables( variables );
		sheet.FileName = filename;

		Loaded.Add( sheet );

		return sheet;
	}

	internal void AddFilename( string filename )
	{
		IncludedFiles.Add( filename );
	}

	public static StyleSheet FromString( string styles, string filename = "none", IEnumerable<(string key, string value)> variables = null )
	{
		try
		{
			return StyleParser.ParseSheet( styles, filename, variables );
		}
		catch ( Exception e )
		{
			Console.WriteLine( $"Error parsing stylesheet: {e.Message}\n{e.StackTrace}" );
			return new StyleSheet();
		}
	}

	internal bool UpdateFromFile( string name, bool failSilently = false )
	{
		// Try to resolve the file path
		var fullPath = name;
		
		// Try different path resolutions
		if ( !System.IO.File.Exists( fullPath ) )
		{
			// Try relative to current directory
			fullPath = System.IO.Path.GetFullPath( name );
		}

		if ( !System.IO.File.Exists( fullPath ) )
		{
			if ( failSilently )
			{
				Nodes = new();
				return true;
			}
			
			Console.WriteLine( $"Error opening stylesheet: {name} (File not found)" );
			Nodes = new();
			return false;
		}

		try
		{
			var text = System.IO.File.ReadAllText( fullPath );
			if ( text is null ) throw new System.IO.FileNotFoundException( "File not found", name );

			return UpdateFromString( text, name, failSilently );
		}
		catch ( Exception e )
		{
			if ( !failSilently )
			{
				Console.WriteLine( $"Error opening stylesheet: {name} ({e.Message})" );
			}

			Nodes = new();
		}

		return false;
	}

	internal bool UpdateFromString( string text, string filename = "none", bool failSilently = false )
	{
		try
		{
			var sheet = FromString( text, filename, null );

			Nodes = sheet.Nodes;
			Variables = sheet.Variables;
			KeyFrames = sheet.KeyFrames;

			// Don't overwrite the included files if the stylesheet
			// failed to load, because it won't be able to hotload
			if ( sheet.IncludedFiles.Any() )
			{
				IncludedFiles = sheet.IncludedFiles;
			}

			sheet.Release();

			return true;
		}
		catch ( Exception e )
		{
			if ( !failSilently )
			{
				Console.WriteLine( $"Error opening stylesheet: {filename} ({e.Message})" );
			}

			Nodes = new();
		}

		return false;
	}

	internal void SetVariable( string key, string value, bool isdefault = false )
	{
		Variables ??= new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

		if ( isdefault && Variables.ContainsKey( key ) ) return;

		// If it's another variable, straight swap it
		value = ReplaceVariables( value );

		Variables[key] = value;
	}

	public string GetVariable( string name, string defaultValue = default )
	{
		if ( Variables == null ) return defaultValue;
		if ( Variables.TryGetValue( name, out var val ) ) return val;
		return null;
	}

	public string ReplaceVariables( string str )
	{
		if ( !str.Contains( '$' ) ) return str; // fast exit

		if ( Variables == null )
		{
			// No variables defined - return unchanged with warning
			Console.WriteLine( $"Warning: CSS contains variable reference but no variables defined: {str}" );
			return str;
		}

		var pairs = Variables.Where( x => str.Contains( x.Key ) ).ToArray();

		bool replaced = false;
		foreach ( var var in pairs.OrderByDescending( x => x.Key.Length ) ) // replace the longest first so $button won't stomp $button-bright
		{
			str = str.Replace( var.Key, var.Value );
			replaced = true;
		}

		if ( !replaced && str.Contains( '$' ) )
		{
			// String still contains $ but no matching variable found - log warning
			Console.WriteLine( $"Warning: Unknown CSS variable in '{str}'" );
		}

		return str;
	}

	internal void AddVariables( IEnumerable<(string key, string value)> variables )
	{
		if ( variables == null ) return;

		foreach ( var var in variables )
		{
			SetVariable( var.key, var.value );
		}
	}

	public void AddKeyFrames( KeyFrames frames )
	{
		KeyFrames[frames.Name] = frames;
	}
}
