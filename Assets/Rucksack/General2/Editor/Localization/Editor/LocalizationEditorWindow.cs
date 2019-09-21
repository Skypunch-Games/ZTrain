using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Devdog.General2.Localization.Editors
{
    public class LocalizationEditorWindow : EditorWindow
    {
        private static LocalizationDatabase[] _dbs = new LocalizationDatabase[0];
        private static string _searchQuery = "";
        private static Vector2 _scrollPos;
        private static string[] _stringKeys = new string[0];
        private static string[] _textureKeys = new string[0];
        private static string[] _spriteKeys = new string[0];
        private static string[] _audioClipKeys = new string[0];
        private static string[] _objectKeys = new string[0];

        private static float _colWidth = 200f;

        private static bool _isSearching
        {
            get { return string.IsNullOrEmpty(_searchQuery) == false; }
        }

        [MenuItem("Tools/Devdog/Localization Editor", false, 0)]
        public static void ShowWindow()
        {
            var w = GetWindow<LocalizationEditorWindow>();
            w.minSize = new Vector2(700, 400);
            w.titleContent = new GUIContent("Localization Editor");
            w.OnEnable();
            w.Show();
        }

        protected void OnEnable()
        {
            _dbs = LocalizationManager.GetAvailableLanguageDatabases();
            _colWidth = Mathf.Max(200f, _dbs.Length / 4f);

            if (LocalizationManager.defaultDatabase != null)
            {
                _stringKeys = LocalizationManager.defaultDatabase._EditorGetAllStrings().Select(o => o.Key).ToArray();
                _textureKeys = LocalizationManager.defaultDatabase._EditorGetAllObjects().Where(o => o.Value is Texture2D).Select(o => o.Key).ToArray();
                _spriteKeys = LocalizationManager.defaultDatabase._EditorGetAllObjects().Where(o => o.Value is Sprite).Select(o => o.Key).ToArray();
                _audioClipKeys = LocalizationManager.defaultDatabase._EditorGetAllObjects().Where(o => o.Value is AudioClip).Select(o => o.Key).ToArray();
                _objectKeys = LocalizationManager.defaultDatabase._EditorGetAllObjects().Select(o => o.Key).ToArray();
            }
        }

        private bool IsLanguageChecked(LocalizationDatabase db)
        {
            return EditorPrefs.GetBool(db.lang + "_Checked", true);
        }

        private void SetLanguageChecked(LocalizationDatabase db, bool result)
        {
            EditorPrefs.SetBool(db.lang + "_Checked", result);
        }

        protected void OnGUI()
        {
            DrawLanguagePicker(20f);

            var keysCount = LocalizationManager.defaultDatabase._EditorGetAllStrings().Count + LocalizationManager.defaultDatabase._EditorGetAllObjects().Count;
            var searchRect = new Rect(position.width - 210f, 0f, 200f, EditorGUIUtility.singleLineHeight);
            _searchQuery = General2.Editors.EditorStyles.SearchBar(searchRect, _searchQuery, this, _isSearching);
            
            var rect = new Rect(0f, 30f, position.width, position.height - 30f);
            _scrollPos = GUI.BeginScrollView(rect, _scrollPos, new Rect(0, 0, 200f * (_dbs.Length + 1), 150 + (keysCount + 2) * EditorGUIUtility.singleLineHeight), false, true);

            foreach (var db in _dbs)
            {
                int indexCounter = 0;
                indexCounter += DrawKeys<string>(_stringKeys, indexCounter);
                indexCounter += DrawKeys<Texture>(_textureKeys, indexCounter);
                indexCounter += DrawKeys<Sprite>(_spriteKeys, indexCounter);
                indexCounter += DrawKeys<AudioClip>(_audioClipKeys, indexCounter);
                indexCounter += DrawKeys<UnityEngine.Object>(_objectKeys, indexCounter);
            }

            for (var i = 0; i < _dbs.Length; i++)
            {
                var db = _dbs[i];
                if (IsLanguageChecked(db) == false)
                {
                    continue;
                }

                EditorGUI.LabelField(new Rect(_colWidth * (i + 1), 0f, _colWidth, EditorGUIUtility.singleLineHeight), db.lang);

                GUI.BeginGroup(new Rect(_colWidth * (i + 1), 0f, _colWidth, (keysCount + 2) * EditorGUIUtility.singleLineHeight), (GUIStyle) "box");

                int counter2 = 0;
                EditorGUI.BeginChangeCheck();
                counter2 += DrawLocalizationDataString(db._EditorGetAllStrings(), _stringKeys, counter2);
                counter2 += DrawLocalizationData<Texture2D>(db._EditorGetAllObjects(), _textureKeys, counter2);
                counter2 += DrawLocalizationData<Sprite>(db._EditorGetAllObjects(), _spriteKeys, counter2);
                counter2 += DrawLocalizationData<AudioClip>(db._EditorGetAllObjects(), _audioClipKeys, counter2);
                counter2 += DrawLocalizationData<UnityEngine.Object>(db._EditorGetAllObjects(), _objectKeys, counter2);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(db);
                }
                
                GUI.EndGroup();
            }

            GUI.EndScrollView();
        }

        private void DrawLanguagePicker(float height)
        {
            var rect = new Rect(0f, 0f, position.width, height);
            GUI.BeginGroup(rect, (GUIStyle) "box");
            EditorGUILayout.BeginHorizontal();

            foreach (var db in _dbs)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUIUtility.labelWidth = 50f;
                var result = EditorGUILayout.ToggleLeft(db.lang, IsLanguageChecked(db), GUILayout.Width(80f));
                if (EditorGUI.EndChangeCheck())
                {
                    SetLanguageChecked(db, result);
                }

                EditorGUIUtility.labelWidth = 0f;
            }

            EditorGUILayout.EndHorizontal();
            GUI.EndGroup();
        }

        /// <returns>Returns the amount of keys drawn (increments startCount)</returns>
        private static int DrawKeys<T>(string[] keys, int startCount)
        {
            startCount++;
            EditorGUI.LabelField(new Rect(0, startCount * EditorGUIUtility.singleLineHeight, 200f, EditorGUIUtility.singleLineHeight), typeof(T).Name, EditorStyles.boldLabel);
            startCount++;
            foreach (var key in keys)
            {
                GUI.Label(new Rect(0, startCount * EditorGUIUtility.singleLineHeight, 200f, EditorGUIUtility.singleLineHeight), key);
                startCount++;
            }

            return startCount;
        }

        private static int DrawLocalizationDataString(Dictionary<string, string> data, string[] keys , int startCount)
        {
            foreach (var key in keys)
            {
                string val;
                data.TryGetValue(key, out val);
                val = val ?? "";

                if (_isSearching && val.Contains(_searchQuery) == false)
                {
                    continue;
                }

                EditorGUI.BeginChangeCheck();
                val = EditorGUI.TextField(new Rect(0f, startCount * EditorGUIUtility.singleLineHeight + 30f, _colWidth, EditorGUIUtility.singleLineHeight), val);
                if (EditorGUI.EndChangeCheck())
                {
                    data[key] = val;
                }

                startCount++;
            }

            return startCount;
        }

        private static int DrawLocalizationData<T>(Dictionary<string, UnityEngine.Object> data, string[] keys, int startCount)
            where T : UnityEngine.Object
        {
            startCount++;
            startCount++;

            foreach (var key in keys)
            {
                UnityEngine.Object val;
                data.TryGetValue(key, out val);
                if (_isSearching && val != null && val.name.Contains(_searchQuery) == false)
                {
                    continue;
                }

                if (val is T == false)
                {
                    continue;
                }

                EditorGUI.BeginChangeCheck();
                val = (T)EditorGUI.ObjectField(new Rect(0f, startCount * EditorGUIUtility.singleLineHeight + 30f, _colWidth, EditorGUIUtility.singleLineHeight), val, typeof(T), false);
                if (EditorGUI.EndChangeCheck())
                {
                    data[key] = val;
                    GUI.changed = true;
                }

                startCount++;
            }

            return startCount;
        }
    }
}
