namespace Sandbox.UI;

/// <summary>
/// Utility classes for UI functionality
/// </summary>
public static class Utility
{
	/// <summary>
	/// Easing functions for animations/transitions
	/// </summary>
	public static class Easing
	{
		/// <summary>
		/// Try to get an easing function by name
		/// </summary>
		public static bool TryGetFunction(string name, out object? function)
		{
			function = null;
			
			// Common CSS easing function names
			switch (name.ToLowerInvariant())
			{
				case "linear":
				case "ease":
				case "ease-in":
				case "ease-out":
				case "ease-in-out":
				case "step-start":
				case "step-end":
					return true;
				default:
					// Also support cubic-bezier
					if (name.StartsWith("cubic-bezier"))
						return true;
					return false;
			}
		}
	}
}
