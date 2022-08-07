using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DeepFreeze.Packages.GameEditorWindow.Editor
{
    public class GameEditorWindow : EditorWindow
    {
        private const string LastWindowKey = "gameeditorwindow_lastWindow";
        private const string ExpandedPrefKey = "gameeditorwindow_expanded";

        private const float SidebarWidthCollapsed = 30;
        private const float SidebarWidthExpanded = 200;
        
        public static GameEditorWindow Instance { get; private set; }

        public static bool SidebarExpanded
        {
            get => EditorPrefs.GetBool(ExpandedPrefKey, false);
            set => EditorPrefs.SetBool(ExpandedPrefKey, value);
        }

        public static string LastWindow
        {
            get => EditorPrefs.GetString(LastWindowKey, string.Empty);
            set => EditorPrefs.SetString(LastWindowKey, value);
        }
        
        private static readonly List<IGameEditorWindow> Windows = new();
        private static IGameEditorWindow _currentWindow;
        
        private static GUIStyle _styleSidebarButton;
        public static GUIStyle StyleSidebarButton
        {
            get
            {
                if (_styleSidebarButton == null)
                {
                    _styleSidebarButton = new GUIStyle("Button")
                    {
                        alignment = TextAnchor.MiddleLeft,
                        
                    };
                }

                return _styleSidebarButton;
            }
        }
        
        public static Vector2 InstanceSize => Instance != null ? Instance.position.size : Vector2.one;
        
        private Vector2 _scrollPosSidebar;
        private Vector2 _scrollPosMainArea;
        
        [MenuItem("Window/Game Editor")]
        public static void Initialize()
        {
            Instance = GetWindow<GameEditorWindow>();
            Instance.titleContent = new GUIContent("Game Editor");
            Instance.Show();
        }
        
        [InitializeOnLoadMethod]
        private static void RefreshWindows()
        {
            _currentWindow?.OnFocusLost();
            Windows.Clear();

            EditorApplication.delayCall += DelayRefreshWindows;
        }

        private static void DelayRefreshWindows()
        {
            Windows.Clear();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(t => typeof(IGameEditorWindow).IsAssignableFrom(t));
                Windows.AddRange(from type in types where !type.IsInterface select (IGameEditorWindow) Activator.CreateInstance(type));
            }
            
            Windows.Sort(new GameEditorWindowComparer());
            _currentWindow = string.IsNullOrEmpty(LastWindow)
                ? null
                : Windows.FirstOrDefault(w =>
                    w.GetType().Name.Equals(LastWindow, StringComparison.InvariantCultureIgnoreCase));
            _currentWindow?.OnFocus();
        }

        private void OnEnable()
        {
            
        }

        private void OnFocus()
        {
            _currentWindow?.OnFocus();
        }

        private void OnLostFocus()
        {
            _currentWindow?.OnFocusLost();
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
                DrawMainContent();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(SidebarExpanded ? SidebarWidthExpanded : SidebarWidthCollapsed));
            {
                foreach (var window in Windows)
                {
                    GUI.color = _currentWindow != null && _currentWindow == window ? Color.cyan : Color.white;
                    if (GUILayout.Button(SidebarExpanded ? window.IconExpanded : window.IconCollapsed, StyleSidebarButton, GUILayout.Width(SidebarExpanded ? SidebarWidthExpanded - 4 : SidebarWidthCollapsed - 4), GUILayout.Height(SidebarWidthCollapsed - 4)))
                    {
                        _currentWindow?.OnFocusLost();
                        _currentWindow = _currentWindow != null && _currentWindow == window ? null : window;
                        LastWindow = _currentWindow != null ? _currentWindow.GetType().Name : string.Empty;
                        _currentWindow?.OnFocus();
                        GUI.FocusControl(null);
                        _scrollPosMainArea = Vector2.zero;
                    }
                    GUI.color = Color.white;
                }
                
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    if (SidebarExpanded)
                    {
                        GUILayout.FlexibleSpace();
                    }

                    if (GUILayout.Button(SidebarExpanded ? "<" : ">"))
                    {
                        SidebarExpanded = !SidebarExpanded;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawMainContent()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    _currentWindow?.ToolbarLeft();
                    GUILayout.FlexibleSpace();
                    _currentWindow?.ToolbarRight();
                }
                EditorGUILayout.EndHorizontal();
                
                _scrollPosMainArea = EditorGUILayout.BeginScrollView(_scrollPosMainArea);
                {
                    _currentWindow?.MainContent();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        
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
    }
}
