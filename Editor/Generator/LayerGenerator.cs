using R.Editor.Utils;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace R.Editor.Layers
{
    public class LayerGenerator : CodeGenerator, ICodeGenerator
    {
        public const string Name = "Layers";

        [MenuItem(MenuPath + Name)]
        private static void Execute()
        {
            var generator = new LayerGenerator();
            generator.Generate();
        }

        public CodeTypeDeclaration CreateLayers(List<string> layers)
        {
            var classDeclaration = new CodeTypeDeclaration(Name)
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };

            for (int i = 0; i < layers.Count; i++)
            {
                string layer = layers[i];
                string layerName = NameUtils.ToName(layer);
                int layerValue = LayerMask.NameToLayer(layer);

                var layerField = new CodeMemberField(typeof(int), layerName)
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Const,
                    InitExpression = new CodePrimitiveExpression(layerValue)
                };

                classDeclaration.Members.Add(layerField);
            }

            return classDeclaration;
        }

        public CodeTypeDeclaration CreateLayerNames(List<string> layers)
        {
            var classDeclaration = new CodeTypeDeclaration("LayerNames")
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };

            for (int i = 0; i < layers.Count; i++)
            {
                string layer = layers[i];
                string layerName = NameUtils.ToName(layer);
                var layerField = new CodeMemberField(typeof(string), layerName)
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Const,
                    InitExpression = new CodePrimitiveExpression(layer)
                };

                classDeclaration.Members.Add(layerField);
            }

            return classDeclaration;
        }

        public void Generate()
        {
            List<string> layers = InternalEditorUtility.layers
                .ToList();

            var codeCompileUnit = new CodeCompileUnit();
            var layerDeclaration = CreateLayers(layers);
            var layerNameDeclaration = CreateLayerNames(layers);
            _codeNamespace.Types.Add(layerDeclaration);
            _codeNamespace.Types.Add(layerNameDeclaration);
            codeCompileUnit.Namespaces.Add(_codeNamespace);

            var path = PathUtils.GetResClassPath(Name);
            GenerateToFile(codeCompileUnit, path);
        }
    }
}
