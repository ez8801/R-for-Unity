using R.Editor.Utils;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;

namespace R.Editor.Tags
{
    public class TagGenerator : CodeGenerator, ICodeGenerator
    {
        public const string Name = "Tags";

        [MenuItem(MenuPath + Name)]
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

            var codeCompileUnit = new CodeCompileUnit();
            var classDeclaration = new CodeTypeDeclaration(Name)
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };

            foreach (string tag in tags)
            {
                var field = new CodeMemberField
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Const,
                    Name = NameUtils.ToName(tag),
                    Type = new CodeTypeReference(typeof(string)),
                    InitExpression = new CodePrimitiveExpression(tag)
                };
                classDeclaration.Members.Add(field);
            }

            _codeNamespace.Types.Add(classDeclaration);
            codeCompileUnit.Namespaces.Add(_codeNamespace);

            var path = PathUtils.GetResClassPath(Name);
            GenerateToFile(codeCompileUnit, path);
        }
    }
}
