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
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Zasadnyy.Editors
{
    public class EditRecentProjectsWindow : EditorWindow
    {
        private const string RECENTLY_USED_PROJECT_KEY = "RecentlyUsedProjectPaths-";

        private Dictionary<string, string> recentProjects = new Dictionary<string, string>();

        [MenuItem("Window/Edit Recent Projects")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<EditRecentProjectsWindow>(false, "Edit Recent Projects", true);
        }

        #region lifecycle methods
        void OnEnable()
        {
            LoadRecentProjects();
        }

        void OnGUI()
        {
            GUILayout.Space(15);
            EditorGUILayout.HelpBox("Tired of long list of recent projects in \"File -> Open Project...\" popup?\n" +
                "Now you can remove unneeded projects with one click. Enjoy!", MessageType.None);
            if(recentProjects.Count > 0)
            {
                DrawRecentProjects();
                GUILayout.Space(15);
                DrawClearAllRecentProjects();
            }
            else 
            {
                EditorGUILayout.LabelField("No Recent Projects", EditorStyles.boldLabel);
            }
        }
        #endregion

        #region private methods
        private void DrawRecentProjects()
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField("Recent Projects", EditorStyles.boldLabel);
            
            foreach(var prefKey in recentProjects.Keys.ToArray())
            {
                var recentProject = recentProjects[prefKey];
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                
                if (GUILayout.Button(new GUIContent("x", "Remove project"), EditorStyles.miniButtonMid, GUILayout.Width(20f), GUILayout.Height(17f)))
                {
                    RemoveRecentProject(prefKey);
                }
                
                EditorGUILayout.LabelField(new GUIContent(recentProject, recentProject));
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawClearAllRecentProjects()
        {
            var color = GUI.color;
            GUI.color = Color.red;
            
            if(GUILayout.Button(new GUIContent("Clear All Recent Projects", "WARNING: You can't undone this operation!")))
            {
                RemoveAllRecentProjects();
            }
            
            GUI.color = color;
        }

        private void LoadRecentProjects()
        {
            recentProjects.Clear();
            
            string path = null;
            int i = 0;
            
            while (true)// boo-ha-ha
            {
                var prefKey = RECENTLY_USED_PROJECT_KEY + i;
                
                if (EditorPrefs.HasKey (prefKey))
                {
                    path = EditorPrefs.GetString (prefKey);
                    recentProjects[prefKey] = path;
                    i++;
                }
                else
                {
                    break;
                }
            }
        }

        void RemoveRecentProject(string projectKeyToRemove)
        {
            // clear recent project in preferences
            foreach(var prefKey in recentProjects.Keys)
            {
                EditorPrefs.DeleteKey(prefKey);
            }
            
            recentProjects.Remove(projectKeyToRemove);
            
            // save recent projects with new order
            var recentProjectsValues = recentProjects.Values.ToArray();
            for(int i = 0; i < recentProjectsValues.Length; i++)
            {
                EditorPrefs.SetString(RECENTLY_USED_PROJECT_KEY + i, recentProjectsValues[i]);
            }
        }

        void RemoveAllRecentProjects()
        {
            foreach (var prefKey in recentProjects.Keys)
            {
                EditorPrefs.DeleteKey(prefKey);
            }
            recentProjects.Clear();
        }
        #endregion
    }
}

