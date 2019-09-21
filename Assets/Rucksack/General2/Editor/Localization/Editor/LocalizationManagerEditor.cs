using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Devdog.General2.Editors;
using UnityEditor;
using EditorUtility = UnityEditor.EditorUtility;

namespace Devdog.General2.Localization.Editors
{
    [CustomEditor(typeof(LocalizationManager))]
    public class LocalizationManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var t = (LocalizationManager) target;

            if (GUILayout.Button("Generate default database"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Save file", "MainLanguageDb.asset", "asset", "Choose a location where to save the localization database.");
                if (path.EndsWith(".asset"))
                {
                    int indexOf = path.LastIndexOf(Path.DirectorySeparatorChar);
                    if (indexOf == -1)
                    {
                        indexOf = path.LastIndexOf('/');
                    }
                    
                    path = path.Substring(0, indexOf);
                }

                if (string.IsNullOrEmpty(path) == false)
                {
                    var asset = ScriptableObjectUtility.CreateAsset<LocalizationDatabase>(path, "MainLanguageDb.asset");
                    var defaultDb = typeof(LocalizationManager).GetField("_defaultDatabase", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    defaultDb.SetValue(t, asset);

                    var databases = typeof(LocalizationManager).GetField("_databases", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var dbList = ((LocalizationDatabase[])databases.GetValue(t)).ToList();
                    dbList.Add(asset);

                    databases.SetValue(t, dbList.ToArray());
                }
            }

            if (GUILayout.Button("Open Localization Editor"))
            {
                LocalizationEditorWindow.ShowWindow();
            }
        }
    }
}