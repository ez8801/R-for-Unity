using System.CodeDom;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace R.Editor.Layers
{
    public class LayerGenerator : ICodeGenerator
    {
        [MenuItem("Tools/Code Generation/Layers")]
        private static void Execute()
        {
            var generator = new LayerGenerator();
            generator.Generate();
        }

        public void Generate()
        {
            string[] layers = InternalEditorUtility.layers
                .OrderBy(x => x)
                .ToArray();

            var codeCompileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace();
            var classDeclaration = new CodeTypeDeclaration("Layers")
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };

            for (int i = 0; i < layers.Length; i++)
            {
                string layer = layers[i];
                string layerName = Utilities.GetScreamName(layer);
                string maskName = layerName + "_MASK";
                int layerValue = LayerMask.NameToLayer(layer);

                var layerField = new CodeMemberField(typeof(int), layerName)
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Const,
                    InitExpression = new CodePrimitiveExpression(layerValue)
                };

                var maskField = new CodeMemberField(typeof(int), maskName)
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Const,
                    InitExpression = new CodePrimitiveExpression(1 << layerValue)
                };

                classDeclaration.Members.Add(layerField);
                classDeclaration.Members.Add(maskField);
            }

            codeNamespace.Types.Add(classDeclaration);
            codeCompileUnit.Namespaces.Add(codeNamespace);

            Utilities.GenerateToFile(codeCompileUnit, Application.dataPath + "/Generated", "Layers.cs");
        }
    }
}
