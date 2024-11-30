using System.Reflection;

namespace StationEntranceVisuals.Utils
{
    public static class ReflectionUtils
    {

        public static readonly BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.GetProperty;

    }
}
