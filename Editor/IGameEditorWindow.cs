using UnityEngine;

namespace DeepFreeze.Packages.GameEditorWindow.Editor
{
    public interface IGameEditorWindow
    {
        GUIContent Icon { get; }

        int SortOrder { get; }

        void OnFocused();

        void OnFocusLost();

        void ToolbarLeft();

        void ToolbarRight();

        void MainContent();
    }
}
