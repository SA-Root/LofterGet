using System.ComponentModel;
using System.Reflection;

namespace LofterGet.Model;

internal static class EnumExtensions
{
    extension<T>(T value) where T : Enum
    {
        // Generic attribute-based description reader
        public string GetDescription()
        {
            if (value == null) return string.Empty;
            var fi = value.GetType().GetField(value.ToString());
            var attr = fi?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}