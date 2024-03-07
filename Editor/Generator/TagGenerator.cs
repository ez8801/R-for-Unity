using System.CodeDom;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace R.Editor.Tags
{
    public class TagGenerator : ICodeGenerator
    {
        [MenuItem("Tools/Code Generation/Tags")]
        private static void Execute()
        {
            var generator = new TagGenerator();
            generator.Generate();
        }

        public void Generate()
        {
            string[] tags = InternalEditorUtility.tags
                .OrderBy(x => x)
                .ToArray();

            var compileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace();
            var classDeclaration = new CodeTypeDeclaration("Tags")
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };

            foreach (string tag in tags)
            {
                var field = new CodeMemberField
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Const,
                    Name = Utilities.GetScreamName(tag),
                    Type = new CodeTypeReference(typeof(string)),
                    InitExpression = new CodePrimitiveExpression(tag)
                };
                classDeclaration.Members.Add(field);
            }

            codeNamespace.Types.Add(classDeclaration);
            compileUnit.Namespaces.Add(codeNamespace);

            Utilities.GenerateToFile(compileUnit, Application.dataPath + "/Generated", "Tags.cs");
        }
    }
}
