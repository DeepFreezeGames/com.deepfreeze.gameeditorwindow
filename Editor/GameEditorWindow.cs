using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameEditorWindow.Editor
{
    public class GameEditorWindow : EditorWindow
    {
        private class GameEditorWindowComparer : IComparer<IGameEditorWindow>
        {
            public int Compare(IGameEditorWindow windowA, IGameEditorWindow windowB)
            {
                if (windowA == null)
                {
                    return -1;
                }

                if (windowB == null)
                {
                    return 1;
                }

                return windowA.SortOrder.CompareTo(windowB.SortOrder);
            }
        }
        
        private static readonly List<IGameEditorWindow> Windows = new List<IGameEditorWindow>();
        private IGameEditorWindow _currentWindow;
        
        public static GameEditorWindow Instance { get; private set; }
        public static Vector2 InstanceSize => Instance != null ? Instance.position.size : Vector2.one;
        
        private Vector2 _scrollPosMainArea;
        
        [MenuItem("Window/Game Editor")]
        public static void Initialize()
        {
            Instance = GetWindow<GameEditorWindow>();
            Instance.titleContent = new GUIContent("Game Editor");
            Instance.Show();
        }
        
        [InitializeOnLoadMethod]
        private static void FetchEditorWindows()
        {
            Windows.Clear();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(t => typeof(IGameEditorWindow).IsAssignableFrom(t));
                foreach (var type in types)
                {
                    if (type.IsInterface)
                    {
                        continue;
                    }
                    
                    //Debug.Log($"Found type: {type.Name}");
                    Windows.Add((IGameEditorWindow)Activator.CreateInstance(type));
                }
            }
            
            Windows.Sort(new GameEditorWindowComparer());
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            Instance = null;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void OnGUI()
        {
            if (Instance == null || Instance != this)
            {
                Instance = this;
            }
            
            EditorGUILayout.BeginHorizontal();
            {
                DrawSidebar();
                EditorGUILayout.BeginVertical();
                {
                    DrawToolbar();
                    DrawMainContent();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(30f));
            {
                if (Windows.Count == 0)
                {
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    for (var i = 0; i < Windows.Count; i++)
                    {
                        if (i == Windows.Count - 1)
                        {
                            GUILayout.FlexibleSpace();
                        }

                        var window = Windows[i];
                        GUI.backgroundColor = window == _currentWindow ? Color.cyan : Color.white;
                        if (GUILayout.Button(window.Icon, GUILayout.Width(24f), GUILayout.Height(24f)))
                        {
                            _currentWindow?.OnFocusLost();
                            _currentWindow = _currentWindow == window ? null : window;
                            _currentWindow?.OnFocused();
                        }

                        GUI.backgroundColor = Color.white;
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                _currentWindow?.ToolbarLeft();
                GUILayout.FlexibleSpace();
                _currentWindow?.ToolbarRight();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMainContent()
        {
            EditorGUILayout.BeginVertical();
            {
                _scrollPosMainArea = EditorGUILayout.BeginScrollView(_scrollPosMainArea);
                {
                    _currentWindow?.MainContent();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
