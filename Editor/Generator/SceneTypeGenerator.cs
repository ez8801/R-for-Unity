using R.Editor.Layers;
using R.Editor.Utils;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace R.Editor
{
    public class SceneTypeGenerator : CodeGenerator, ICodeGenerator
    {
        public const string Name = "Scene Names";

        [MenuItem(MenuPath + Name)]
        private static void Execute()
        {
            var generator = new SceneTypeGenerator();
            generator.Generate();
        }

        public string GUIDToAssetName(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (false == string.IsNullOrEmpty(assetPath))
                return Path.GetFileNameWithoutExtension(assetPath);
            return string.Empty;
        }

        public void Generate()
        {
            var scenes = AssetDatabase.FindAssets("t:scene", PathUtils.SceneFolders())
                .Select(x => GUIDToAssetName(x))
                .Distinct()
                .ToArray();
            
            var codeCompileUnit = new CodeCompileUnit();
            var enumDeclaration = new CodeTypeDeclaration("SceneNames")
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };

            for (int i = 0; i < scenes.Length; i++)
            {
                string sceneName = scenes[i];
                string variableName = NameUtils.ToName(scenes[i]);
                var sceneField = new CodeMemberField(typeof(string), variableName)
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Const,
                    InitExpression = new CodePrimitiveExpression(sceneName)
                };

                enumDeclaration.Members.Add(sceneField);
            }

            _codeNamespace.Types.Add(enumDeclaration);
            codeCompileUnit.Namespaces.Add(_codeNamespace);

            var path = PathUtils.GetResClassPath("SceneNames");
            GenerateToFile(codeCompileUnit, path);
        }
    }
}
