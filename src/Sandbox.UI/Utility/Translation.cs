namespace Sandbox.UI;

/// <summary>
/// Type conversion utilities (stub for S&box compatibility)
/// </summary>
public static class Translation
{
    public static bool TryConvert(object value, Type targetType, out object? convertedValue)
    {
        try
        {
            if (value == null)
            {
                convertedValue = null;
                return !targetType.IsValueType;
            }
            
            if (targetType.IsAssignableFrom(value.GetType()))
            {
                convertedValue = value;
                return true;
            }
            
            convertedValue = Convert.ChangeType(value, targetType);
            return true;
        }
        catch
        {
            convertedValue = null;
            return false;
        }
    }
}
