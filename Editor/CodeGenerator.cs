using UnityEngine;
using UnityEngine.U2D;
using UnityEditorInternal;
using UnityEditor;
//using UnityEditor.AddressableAssets;
//using UnityEditor.AddressableAssets.Settings;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using R.Editor.Utils;
using R.Editor.Settings;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.CodeDom;

namespace R.Editor.Legacy
{
    public class CodeGenerator
    {
        public delegate int HashConverter(string name);

        static string[] _searchInFolders = new[] { "Assets/Data" };

        const string Indent = "    ";

        const string _constantStringFormat =
"        public const string {0} = \"{1}\";";
        const string _constantIntFormat =
"        public const int {0} = {1};";
        const string _customStructFieldFormat =
"        public readonly static {0} {1} = \"{2}\";";

        const string MenuPath = "Tools/Util/Generate Code/";

        [MenuItem(MenuPath + "All %#F9", priority = -10)]
        private static void GenerateAll()
        {
            //GenerateAddressable();
            GenerateAnimatorParameters();
            GenerateAnimatorStates();
            //GenerateAudioClips();
            GenerateLayers();
            GenerateShaderNameIDs();
            GenerateSortingLayers();
            GenerateTags();

            Debug.Log("Complete GenerateAll");
        }

        const string SceneNames = "SceneNames";
        [MenuItem(MenuPath + SceneNames)]
        private static void GenerateSceneNames()
        {
            string className = SceneNames;
            var scenes = AssetDatabase.FindAssets("t:scene", PathUtils.SceneFolders());
            var contents = GUIDToEnumContents(scenes);
            var template = ResGenSettings.Default.ResGenEnumTemplate.text;
            WriteCode(className, contents, template);
        }

        /*
        const string SoundCategory = "SoundCategory";
        [MenuItem(MenuPath + SoundCategory)]
        private static void GenerateSoundCategory()
        {
            string className = SoundCategory;
            var categories = ResGenSettings.Default.AudioControllerPrefab.AudioCategories.Select(x => x.Name);
            var contents = NamesToEnumContents(categories);
            var template = ResGenSettings.Default.ResGenEnumTemplate.text;
            WriteCode(className, contents, template);
        }

        const string Addressables = "Addressables";
        [MenuItem(MenuPath + Addressables + " %F9")]
        private static void GenerateAddressable()
        {
            string className = Addressables;
            
            var names = new List<string>();
            for (int i = 0; i < AddressableAssetSettingsDefaultObject.Settings.groups.Count; ++i)
            {
                var group = AddressableAssetSettingsDefaultObject.Settings.groups[i];
                if (group.ReadOnly == true)
                    continue;

                var iter = group.entries.GetEnumerator();
                while (iter.MoveNext())
                {
                    var entry = iter.Current;
                    RecursiveEntry(entry, ref names);

                    void RecursiveEntry(AddressableAssetEntry entry, ref List<string> names)
                    {
                        if (entry.SubAssets != null && entry.SubAssets.Count > 0)
                        {
                            for (int i = 0; i < entry.SubAssets.Count; ++i)
                            {
                                var sub = entry.SubAssets[i];
                                RecursiveEntry(sub, ref names);
                            }
                        }
                        else
                        {
                            if (entry.IsSubAsset == false)
                                names.Add(entry.address);
                            else
                            {
                                const string extPrefab = ".prefab";
                                string ext = Path.GetExtension(entry.AssetPath);
                                if (ext.Equals(extPrefab) == true ||
                                    entry.MainAsset is Texture ||
                                    entry.MainAsset is SpriteAtlas)
                                    names.Add(entry.address);
                            }
                        }
                    }
                }
            }

            string contents = NamesToContents(names);
            WriteCode(className, contents);
        }
        */

        const string AnimatorParameters = "AnimatorParameters";
        [MenuItem(MenuPath + AnimatorParameters)]
        private static void GenerateAnimatorParameters()
        {
            string className = AnimatorParameters;
            string contents = NamesToHashContents(GetAnimatorParameters(), Animator.StringToHash);
            WriteCode(className, contents);
        }

        const string AnimatorState = "AnimatorState";
        [MenuItem(MenuPath + AnimatorState)]
        private static void GenerateAnimatorStates()
        {
            string className = AnimatorState;
            string contents = NamesToHashContents(GetAnimatorStates(), Animator.StringToHash);
            WriteCode(className, contents);
        }

        /*
        const string AudioClips = "AudioClips";
        [MenuItem(MenuPath + AudioClips)]
        private static void GenerateAudioClips()
        {
            string className = AudioClips;
            var settings = ResGenSettings.Default;
            var categories = settings.AudioControllerPrefab.AudioCategories.Select(x => x.Name);
            string contents = AudioAttrsToContents(GetAudioClips( categories));
            WriteCode(className, contents);
        }
        */

        const string Layers = "Layers";
        [MenuItem(MenuPath + Layers)]
        private static void GenerateLayers()
        {
            string className = Layers;
            var layers = GetLayers();
            string contents = NamesToContents(layers);

            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var layer in layers)
            {
                string code = string.Format(_constantIntFormat, MakeIdentifier(string.Format("{0}Index", layer)), LayerMask.NameToLayer(layer));
                sb.AppendLine(code);
            }

            contents += sb.ToString();

            WriteCode(className, contents);
        }

        const string ShaderNameIDs = "ShaderNameIDs";
        [MenuItem(MenuPath + ShaderNameIDs)]
        private static void GenerateShaderNameIDs()
        {
            string className = ShaderNameIDs;
            string contents = NamesToHashContents(GetShaderNameIDs(), Shader.PropertyToID);
            WriteCode(className, contents);
        }

        const string SortingLayers = "SortingLayers";
        [MenuItem(MenuPath + SortingLayers)]
        private static void GenerateSortingLayers()
        {
            string className = SortingLayers;
            string contents = NamesToContents(GetSortingLayers());
            WriteCode(className, contents);
        }

        const string Tags = "Tags";
        [MenuItem(MenuPath + Tags)]
        private static void GenerateTags()
        {
            string className = Tags;
            string contents = NamesToContents(GetTags());
            WriteCode(className, contents);
        }

        /*
        const string CanvasSortingOrders = "CanvasSortingOrders";
        [MenuItem(MenuPath + CanvasSortingOrders)]
        private static void GenerateCanvasSortingOrders()
        {
            string className = CanvasSortingOrders;
            string ToContents(IReadOnlyList<string> sortOrders)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < sortOrders.Count; i++)
                {
                    var sortOrder = sortOrders[i];
                    var code = string.Format(_constantIntFormat, MakeIdentifier(sortOrder), i);
                    sb.AppendLine(code);
                }
                return sb.ToString();
            }
            string contents = ToContents(GetVirtualSortOrders());
            WriteCode(className, contents);
        }
        */

        static string NamesToEnumContents(IEnumerable<string> enums)
        {
            var sb = new StringBuilder();
            foreach (var @enum in enums)
            {
                sb.AppendLine($"{Indent}{Indent}{@enum},");
            }
            return sb.ToString();
        }

        static string GUIDToEnumContents(IEnumerable<string> guidList)
        {
            var sb = new StringBuilder();
            foreach (var guid in guidList)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                sb.AppendLine($"{Indent}{Indent}{fileName},");
            }
            return sb.ToString();
        }

        static string NamesToContents(IReadOnlyList<string> names)
        {
            var sb = new StringBuilder();
            foreach (string name in names)
            {
                string code = string.Format(_constantStringFormat, MakeIdentifier(name), EscapeDoubleQuote(name));
                sb.AppendLine(code);
            }
            return sb.ToString();
        }

        /*
        static string AudioAttrsToContents(IReadOnlyList<AudioAttribute> attrs)
        {
            var sb = new StringBuilder();
            foreach (var attr in attrs)
            {
                sb.Append(Indent);
                sb.Append(Indent);
                sb.Append($"public readonly static AudioAttribute {MakeIdentifier(attr.Name)} = ");
                sb.AppendLine($"new(SceneNames.{attr.SceneNames}, SoundCategory.{attr.Category}, nameof({attr.Name}));");
            }

            return sb.ToString();
        }
        */

        static string NamesToHashContents(IReadOnlyList<string> names, HashConverter converter)
        {
            var sb = new StringBuilder();
            //string prefix = "String";
            //foreach (string name in names)
            //{
            //    string code = string.Format("    public const string {0}{1} = \"{2}\";", prefix, MakeIdentifier(name), EscapeDoubleQuote(name));
            //    sb.AppendLine(code);
            //}
            //sb.Append(Environment.NewLine);

            foreach (string name in names)
            {
                string code = string.Format(_constantIntFormat, MakeIdentifier(name), converter.Invoke(name));
                sb.AppendLine(code);
            }
            return sb.ToString();
        }

        static List<string> GetAnimatorParameters()
        {
            var names = new List<string>();
            var anims = AssetDatabase.FindAssets("t:animatorcontroller", _searchInFolders);
            foreach (var anim in anims)
            {
                var path = AssetDatabase.GUIDToAssetPath(anim);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
                if (asset == null)
                    continue;

                asset.parameters.ToList().ForEach(x => names.Add(x.name));
            }

            return names.Distinct().ToList();
        }

        static List<string> GetAnimatorStates()
        {
            var names = new List<string>();
            var anims = AssetDatabase.FindAssets("t:animatorcontroller", _searchInFolders);
            foreach (var anim in anims)
            {
                var path = AssetDatabase.GUIDToAssetPath(anim);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
                if (asset == null)
                    continue;

                asset.layers.ToList().ForEach(x => x.stateMachine.states.ToList().ForEach(y => names.Add(y.state.name)));
            }

            return names.Distinct().ToList();
        }

        static List<string> GetTags()
        {
            return InternalEditorUtility.tags.ToList();
        }

        static List<string> GetLayers()
        {
            return Enumerable.Range(0, 32).Select(x => LayerMask.LayerToName(x)).Where(y => y.Length > 0).ToList();
        }

        static List<string> GetSortingLayers()
        {
            return SortingLayer.layers.ToList().Select(x => x.name).ToList();
        }

        static List<string> GetShaderNameIDs()
        {
            var names = new List<string>();
            var shaders = AssetDatabase.FindAssets("t:shader", _searchInFolders);
            foreach (var shader in shaders)
            {
                var path = AssetDatabase.GUIDToAssetPath(shader);
                var asset = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (asset == null)
                    continue;

                var count = ShaderUtil.GetPropertyCount(asset);
                for (int i = 0; i < count; i++)
                {
                    var name = ShaderUtil.GetPropertyName(asset, i);
                    names.Add(name);
                }
            }

            return names.Distinct().ToList();
        }

        // "[rootPath](Assets/Data/Audio)/[Scene]/[Category]/file.extension"
        /*
        static List<string> GetVirtualSortOrders()
        {
            var rootPrefab = ResGenSettings.Default.UIRootPrefab;
            if (rootPrefab == null) return null;

            var canvasObj = rootPrefab.transform.Find("Canvas/SafeAreaAnchor");
            if (canvasObj == null) return null;

            var virtualSortOrders = new List<string>();
            for (int i = 0; i < canvasObj.childCount; i++)
            {
                var child = canvasObj.GetChild(i);
                virtualSortOrders.Add(child.gameObject.name);
            }
            return virtualSortOrders;
        }

        static List<AudioAttribute> GetAudioClips(IEnumerable<string> categories)
        {
            var results = new List<AudioAttribute>();
            var folders = PathUtils.GetAudioClipFolderPath();
            var clips = AssetDatabase.FindAssets("t:audioclip", folders);
            foreach (var clip in clips)
            {
                var path = AssetDatabase.GUIDToAssetPath(clip);
                //if (path.StartsWith(rootPath) == false)
                //    continue;

                var splits = path.Split('/');
                if (splits.Length < 5)
                    continue;

                var categoryName = splits[4];
                if (categories.Any(f => f == categoryName) == false)
                    continue;

                var sceneName = splits[3];
                var fileName = Path.GetFileNameWithoutExtension(path);

                if (false == System.Enum.TryParse<SceneNames>(sceneName, out var scene))
                    scene = R.SceneNames.App;

                if (false == System.Enum.TryParse<SoundCategory>(categoryName, out var category))
                    category = R.SoundCategory.SFX;

                var attr = new AudioAttribute(scene, category, fileName);
                results.Add(attr);
            }

            return results;
        }
        */

        static bool DoCreateDirectory(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir) == false)
            {
                if (DoCreateDirectory(dir) == false)
                    return false;

                Directory.CreateDirectory(dir);
                System.Threading.Thread.Sleep(100);
            }
            return true;
        }

        public static void WriteCode(string className, string contents, string template)
        {
            string path = PathUtils.GetResClassPath(className);

            if (DoCreateDirectory(path) == false)
            {
                Debug.LogError("failed create directory");
                return;
            }

            try
            {
                using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(template, className, contents);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            AssetDatabase.Refresh();

            Debug.Log("Complete Generate : " + className);
        }

        public static void WriteCode(string className, string contents)
        {
            var template = ResGenSettings.Default.ResGenScriptTemplate.text;
            WriteCode(className, contents, template);
        }

        private static string MakeIdentifier(string str)
        {
            var result = Regex.Replace(str, "[^a-zA-Z0-9]", "_");
            if ('0' <= result[0] && result[0] <= '9')
            {
                result = result.Insert(0, "_");
            }

            return result;
        }

        private static string EscapeDoubleQuote(string str)
        {
            return str.Replace("\"", "\"\"");
        }

        public static void GenerateToFile(CodeCompileUnit unit, string directory, string filename)
        {
            var codeProvider = new CSharpCodeProvider();
            var options = new CodeGeneratorOptions
            {
                BracingStyle = "C"
            };

            var writer = new StringWriter();
            codeProvider.GenerateCodeFromCompileUnit(unit, writer, options);
            writer.Flush();
            string output = writer.ToString();

            string directoryPath = directory;
            string filePath = directoryPath + "/" + filename;
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            File.WriteAllText(filePath, output);
            AssetDatabase.Refresh();
        }
    }
}
