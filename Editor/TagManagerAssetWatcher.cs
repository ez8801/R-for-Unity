using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace R.Editor
{
    public class TagManagerAssetWatcher : AssetModificationProcessor
    {
        public const string TagManagerAssetPath = "ProjectSettings/TagManager.asset";

        static string Aggregate(IEnumerable<string> strings)
        {
            if (strings == null || !strings.Any())
                return string.Empty;
            return strings.Aggregate((x, y) => $"{x}\n{y}");
        }

        static bool HasChanged(Assembly loadedAssembly, string typeName, string[] current)
        {
            var type = loadedAssembly.GetType(typeName, false, true);
            if (type == null)
                return true;
            
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            var previous = fields.Select(x => x.GetValue(null).ToString());
            
            var addedItems = current.Except(previous).ToList();
            var removedItems = previous.Except(current).ToList();

            Debug.Log("addedItems: " + Aggregate(addedItems));
            Debug.Log("removedItems: " + Aggregate(removedItems));

            if (addedItems.Count > 0 || removedItems.Count > 0) 
                return true;
            return false;
        }

        static string[] GetSortingLayerNames()
        {
            return SortingLayer.layers.Select(x => x.name).ToArray();
        }

        static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path == TagManagerAssetPath)
                {
                    var runtimeDllPath = Path.Combine(Environment.CurrentDirectory, "Library/ScriptAssemblies/R.dll");
                    var loadedAssembly = Assembly.LoadFile(runtimeDllPath);

                    if (HasChanged(loadedAssembly, "R.LayerNames", InternalEditorUtility.layers)
                        || HasChanged(loadedAssembly, "R.Tags", InternalEditorUtility.tags)
                        || HasChanged(loadedAssembly, "R.SortingLayers", GetSortingLayerNames()))
                    {
                        if (EditorUtility.DisplayDialog(string.Empty, "There have been changes, do you want to renew ?", "ok", "cancel"))
                        {
                            Debug.Log("Renew");
                            CodeGenerator.GenerateAll();
                        }
                    }
                }
            }
            return paths;
        }
    }
}
