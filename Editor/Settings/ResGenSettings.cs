using System.IO;
using UnityEngine;
using UnityEditor;

namespace R.Editor.Settings
{
    [CreateAssetMenu(fileName = "ResourceGenerationSettings.asset", menuName = "R/Generation Settings")]
    public class ResGenSettings : ScriptableObject
    {
        private const string kDefaultConfigAssetName = "ResourceGenerationSettings";
        private const string kDefaultConfigFolder = "Assets/Settings/R";
        private const string kDefaultConfigObjectName = "com.ez.R";

        private static string DefaultAssetPath => kDefaultConfigFolder + "/" + kDefaultConfigAssetName + ".asset";

        public Object SceneFolder;
        public TextAsset ResGenScriptTemplate;
        public TextAsset StringConvertableStructTemplate;
        public Object ResGenClassOutputFolder;
        
        public static ResGenSettings Default
        {
            get
            {
                ResGenSettings so;
                if (EditorBuildSettings.TryGetConfigObject(kDefaultConfigObjectName, out so))
                {

                }
                else
                {
                    so = AssetDatabase.LoadAssetAtPath<ResGenSettings>(DefaultAssetPath);
                    if (so == null)
                    {
                        so = CreateInstance<ResGenSettings>();
                        AssetDatabase.CreateAsset(so, DefaultAssetPath);
                        EditorUtility.SetDirty(so);
                        AssetDatabase.SaveAssets();
                    }

                    EditorBuildSettings.AddConfigObject(kDefaultConfigObjectName, so, true);
                }

                return so;
            }
        }

#if UNITY_EDITOR
        public void Reset()
        {
            if (SceneFolder == null)
            {
                var defaultSceneFolderPath = Path.Combine(Application.dataPath, "Scenes");
                if (Directory.Exists(defaultSceneFolderPath))
                {
                    SceneFolder = AssetDatabase.LoadAssetAtPath("Assets/Scenes", typeof(Object));
                }
            }
        }
#endif
    }
}