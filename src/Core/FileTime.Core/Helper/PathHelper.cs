using FileTime.Core.Models;

namespace FileTime.Core.Helper
{
    public static class PathHelper
    {
        public static string GetLongerPath(string? oldPath, string? newPath)
        {
            var oldPathParts = oldPath?.Split(Constants.SeparatorChar) ?? Array.Empty<string>();
            var newPathParts = newPath?.Split(Constants.SeparatorChar) ?? Array.Empty<string>();

            var commonPathParts = new List<string>();

            var max = oldPathParts.Length > newPathParts.Length ? oldPathParts.Length : newPathParts.Length;

            for (var i = 0; i < max; i++)
            {
                if (newPathParts.Length <= i)
                {
                    commonPathParts.AddRange(oldPathParts.Skip(i));
                    break;
                }
                else if (oldPathParts.Length <= i || oldPathParts[i] != newPathParts[i])
                {
                    commonPathParts.AddRange(newPathParts.Skip(i));
                    break;
                }
                else if (oldPathParts[i] == newPathParts[i])
                {
                    commonPathParts.Add(oldPathParts[i]);
                }
            }

            return string.Join(Constants.SeparatorChar, commonPathParts);
        }
        public static string GetCommonPath(string? path1, string? path2)
        {
            var path1Parts = path1?.Split(Constants.SeparatorChar) ?? Array.Empty<string>();
            var path2Parts = path2?.Split(Constants.SeparatorChar) ?? Array.Empty<string>();

            var commonPathParts = new List<string>();

            var max = path1Parts.Length > path2Parts.Length ? path2Parts.Length : path1Parts.Length;

            for (var i = 0; i < max; i++)
            {
                if (path1Parts[i] != path2Parts[i]) break;

                commonPathParts.Add(path1Parts[i]);
            }

            return string.Join(Constants.SeparatorChar, commonPathParts);
        }
    }
}