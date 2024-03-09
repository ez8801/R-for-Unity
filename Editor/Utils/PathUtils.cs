using UnityEditor;
using R.Editor.Settings;
using System.IO;

namespace R.Editor.Utils
{
    public class PathUtils
    {
        public static string GetResClassPath(string className)
        {
            var settings = ResGenSettings.Default;
            var path = AssetDatabase.GetAssetPath(settings.ResGenClassOutputFolder);
            return Path.Combine(path, $"{className}.cs");
        }

        public static string[] SceneFolders()
        {
            var settings = ResGenSettings.Default;
            var path = AssetDatabase.GetAssetPath(settings.SceneFolder);
            return new string[1] { path };
        }
    }
}