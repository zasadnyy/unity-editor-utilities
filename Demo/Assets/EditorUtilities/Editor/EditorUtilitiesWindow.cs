/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2015 Vitaliy Zasadnyy
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. 
 */
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Zasadnyy.Editors
{
    public class EditorUtilitiesWindow : EditorWindow
    {
        private const string SETTINGS_FILE_NAME = "EditorUtilitiesSettings.asset";
        private const string SETTINGS_DIRECTORY = "Assets/EditorUtilities/Editor/";

        private EditorUtilitiesSettings _settings;
        private Vector2 _scrollViewPosition;
        private bool _isInSettingsMode;
        private string _currentState = "";

        private GUIStyle _editorStateLableStyle = new GUIStyle();

        [MenuItem("Window/Editor Utilities")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<EditorUtilitiesWindow>(false, "Editor Utilities", true);
        }

        #region lifecycle methods
        void OnEnable()
        {
            _settings = LoadSettings(); 
            
            _editorStateLableStyle.fontSize = 24;
            _editorStateLableStyle.alignment = TextAnchor.MiddleCenter;
            _editorStateLableStyle.padding = new RectOffset(10, 10, 10, 10);
            _editorStateLableStyle.normal.textColor = Color.gray;
        }

        void OnGUI()
        {
            if (_settings == null)
            {
                _settings = LoadSettings();
                if (_settings == null)
                {
                    Debug.LogWarning("_setting are not loaded yet");
                    return;
                }
            }
            
            DrawToolbar();
            DrawWindowContent();
        }

        void Update()
        {
            _currentState = GetCurrentState();
            Repaint();
        }
        #endregion

        #region UI drawing methods
        void DrawToolbar()
        {
            if (_isInSettingsMode)
            {
                DrawSettingsToolbar();
            }
            else
            {
                DrawUtilitiesToolbar();
            }
        }

        void DrawWindowContent()
        {
            _scrollViewPosition = EditorGUILayout.BeginScrollView (_scrollViewPosition, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView);
            if (_isInSettingsMode)
            {
                DrawSettings();
            }
            else
            {
                DrawUtilities();
            }
            EditorGUILayout.EndScrollView ();
        }

        private void DrawSettings()
        {
            EditorGUILayout.Space();
            
            _settings.showEditorState = GUILayout.Toggle(_settings.showEditorState, "Show Editor State");
            _settings.showSceneShortcuts = GUILayout.Toggle(_settings.showSceneShortcuts, "Show Scene Schortcuts");
            
            EditorGUI.BeginDisabledGroup(!_settings.showSceneShortcuts);
            
            EditorGUILayout.Space();
            DrawList("Launch Scenes", ref _settings.launchSceneShortcuts,
                     drawListItem: (scene) => DrawScenePathField(scene),
                     createListItem: () => "Assets/Scenes/");
            
            EditorGUILayout.Space();
            DrawList("Goto Scenes", ref _settings.workingSceneShortcuts,
                     drawListItem: (scene) => DrawScenePathField(scene),
                     createListItem: () => "Assets/Scenes/");
            
            EditorGUI.EndDisabledGroup();
        }

        private string DrawScenePathField(string path)
        {
            EditorGUILayout.BeginVertical();
            
            var newPath = EditorGUILayout.TextField(path);
            
            var isSceneValid = File.Exists(newPath);
            if (!isSceneValid)
            {
                EditorGUILayout.HelpBox ("Scene doesn't exist", MessageType.Error);
            }
            
            EditorGUILayout.EndVertical();
            return newPath;
        }

        private void DrawSettingsToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.FlexibleSpace();
            
            var backUpColor = GUI.color;
            GUI.color = Color.green;
            if (GUILayout.Button("Done", EditorStyles.toolbarButton))
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
                _isInSettingsMode = false;
            }
            GUI.color = backUpColor;
            
            GUILayout.EndHorizontal();
        }

        private void DrawUtilities()
        {
            if (_settings.showEditorState)
            {
                DrawEditorState();
            }
            
            if (_settings.showSceneShortcuts)
            {
                DrawSceneShortcuts();
            }
        }

        private void DrawUtilitiesToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Settings", EditorStyles.toolbarButton))
            {
                _isInSettingsMode = true;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawEditorState()
        {
            GUILayout.Space(36f);
            EditorGUILayout.LabelField(_currentState, _editorStateLableStyle);
            GUILayout.Space(36f);
        }

        private void DrawSceneShortcuts()
        {
            EditorGUILayout.LabelField("Launch Scene:", EditorStyles.boldLabel);
            if (_settings.launchSceneShortcuts.Count > 0)
            {
                foreach (var scene in _settings.launchSceneShortcuts)
                {
                    if (GUILayout.Button(scene)) {
                        OpenScene(scene);
                        PlayOpenedScene();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No scenes added. Go to setting to add one.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Goto Scene:", EditorStyles.boldLabel);
            if (_settings.workingSceneShortcuts.Count > 0)
            {
                foreach (var scene in _settings.workingSceneShortcuts)
                {
                    if (GUILayout.Button(scene)) {
                        OpenScene(scene);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No scenes added. Go to setting to add one.", MessageType.Info);
            }
        }
        #endregion

        #region helper methods
        private EditorUtilitiesSettings LoadSettings()
        {
            if (!File.Exists(SETTINGS_DIRECTORY + SETTINGS_FILE_NAME))
            {
                CreateAsset<EditorUtilitiesSettings>(SETTINGS_DIRECTORY, SETTINGS_FILE_NAME);
            }
            
            return AssetDatabase.LoadAssetAtPath(SETTINGS_DIRECTORY + SETTINGS_FILE_NAME, typeof(EditorUtilitiesSettings)) as EditorUtilitiesSettings;
        }

        private void DrawList<T>(string title, ref List<T> list, Func<T,T> drawListItem, Func<T> createListItem)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            EditorGUILayout.LabelField(title);
            
            EditorGUILayout.HelpBox("Sample path: Assets/Scenes/MainScene.unity", MessageType.None);
            
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list [i] = drawListItem(list [i]);
                if (GUILayout.Button("x", EditorStyles.miniButtonMid, GUILayout.Width(20f), GUILayout.Height(17f)))
                {
                    list.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", EditorStyles.miniButtonMid, GUILayout.Width(20f), GUILayout.Height(17f)))
            {
                list.Add(createListItem());
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void CreateAsset<T>(string directory, string name) where T : ScriptableObject
        {
            var conf = ScriptableObject.CreateInstance<T>();
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            AssetDatabase.CreateAsset(conf, directory + name);
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
        }

        private string GetCurrentState()
        {
            var state = "Editing";
            _editorStateLableStyle.normal.textColor = Color.gray;
            
            if (EditorApplication.isCompiling)
            {
                state = "Compiling";
                _editorStateLableStyle.normal.textColor = Color.red;
            }
            else if (EditorApplication.isPaused)
            {
                state = "Paused";
                _editorStateLableStyle.normal.textColor = Color.yellow;
            }
            else if (EditorApplication.isPlaying)
            {
                state = "Playing";
                _editorStateLableStyle.normal.textColor = Color.green;
            }
            else if (EditorApplication.isUpdating)
            {
                state = "Updating";
                _editorStateLableStyle.normal.textColor = Color.gray;
            } 
            
            return state;
        }

        private void OpenScene(string scenePath)
        {
            var needSave = !(EditorApplication.currentScene.Equals (scenePath) || string.IsNullOrEmpty(EditorApplication.currentScene));
            if (needSave)
            {
                EditorApplication.SaveScene();
            }
            EditorApplication.OpenScene(scenePath);
        }

        private void PlayOpenedScene()
        {
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
        #endregion
    }
}