using System.IO;

namespace UnityFileAdder
{
    public static class PathHelper
    {
        public static readonly char PathSeparator = Path.AltDirectorySeparatorChar;

        public static string TrimPathSeparators(string path) =>
            path.Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        public static bool IsFilePath(string path) =>
            !path.EndsWith(Path.DirectorySeparatorChar) &&
            !path.EndsWith(Path.AltDirectorySeparatorChar);
    }
}
