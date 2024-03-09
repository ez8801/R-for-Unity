using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace R.Editor
{
    public class CodeGenerator
    {
        public const string MenuPath = "Tools/Code Generation/";

        protected CodeNamespace _codeNamespace;

        public CodeGenerator()
        {
            _codeNamespace = new CodeNamespace("R");
        }

        [MenuItem(MenuPath + "Generate All", false, int.MaxValue)]
        private static void GenerateAll()
        {
            Type typeDefinition = typeof(ICodeGenerator);

            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> types = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeDefinition))
                .ToList();

            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract)
                    continue;

                var instance = (ICodeGenerator)Activator.CreateInstance(type);
                instance.Generate();
            }
        }

        bool EnsureDirectory(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (false == Directory.Exists(directory))
            {
                if (false == EnsureDirectory(directory))
                    return false;

                Directory.CreateDirectory(directory);
                System.Threading.Thread.Sleep(100);
            }
            return true;
        }

        public void GenerateToFile(CodeCompileUnit unit, string fileFullPath)
        {
            var codeProvider = new CSharpCodeProvider();
            var options = new CodeGeneratorOptions
            {
                BracingStyle = "C",
                BlankLinesBetweenMembers = false
            };

            var writer = new StringWriter();
            codeProvider.GenerateCodeFromCompileUnit(unit, writer, options);
            writer.Flush();

            string output = writer.ToString();

            if (EnsureDirectory(fileFullPath))
            {
                File.WriteAllText(fileFullPath, output);
                AssetDatabase.Refresh();
            }
        }
    }
}
