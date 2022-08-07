using UnityEngine;

namespace DeepFreeze.Packages.GameEditorWindow.Editor
{
    public interface IGameEditorWindow
    {
        int SortOrder { get; }
        
        GUIContent IconCollapsed { get; }
        GUIContent IconExpanded { get; }

        void OnFocus();

        void OnFocusLost();

        void ToolbarLeft();

        void ToolbarRight();

        void MainContent();
    }
}
