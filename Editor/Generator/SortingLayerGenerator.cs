using R.Editor.Utils;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace R.Editor.SortingLayers
{
    public class SortingLayerGenerator : CodeGenerator, ICodeGenerator
    {
        public const string Name = "Sorting Layers";

        [MenuItem(MenuPath + Name)]
        private static void Execute()
        {
            var generator = new SortingLayerGenerator();
            generator.Generate();
        }

        public void Generate()
        {
            string[] layers = SortingLayer.layers
                .Select(x => x.name)
                .ToArray();

            var codeCompileUnit = new CodeCompileUnit();
            var classDeclaration = new CodeTypeDeclaration("SortingLayers")
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };

            for (int i = 0; i < layers.Length; i++)
            {
                string layer = layers[i];
                string layerName = NameUtils.ToName(layer);
                var layerField = new CodeMemberField(typeof(string), layerName)
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Const,
                    InitExpression = new CodePrimitiveExpression(layerName)
                };

                classDeclaration.Members.Add(layerField);
            }

            _codeNamespace.Types.Add(classDeclaration);
            codeCompileUnit.Namespaces.Add(_codeNamespace);

            var path = PathUtils.GetResClassPath("SortingLayers");
            GenerateToFile(codeCompileUnit, path);
        }
    }
}
