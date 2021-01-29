namespace TextureTool
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    internal static class MyStyle
    {
        public static GUIStyle DefaultLabel => EditorStyles.label;
        public static GUIStyle YellowLabel { get; private set; } 
        public static GUIStyle RedLabel { get; private set; } 
        public static GUIStyle LoadingLabel { get; private set; } 
        public static GUIStyle MessageLabel { get; private set; } 
        public static GUIStyle TreeViewColumnHeader { get; private set; } 
        public static GUIStyle ColumnHeader { get; private set; }

        public static readonly Vector2 LoadingLabelPosition = new Vector2(14f, -8f); 

        public static void CreateGUIStyleIfNull()
        {
            if (LoadingLabel == null)
            {
                LoadingLabel = new GUIStyle(EditorStyles.label);
                LoadingLabel.alignment = TextAnchor.MiddleCenter;
                LoadingLabel.fontSize = 64;
            }

            if (MessageLabel == null)
            {
                MessageLabel = new GUIStyle(EditorStyles.label);
                MessageLabel.alignment = TextAnchor.UpperLeft;
            }

            if (YellowLabel == null)
            {
                YellowLabel = new GUIStyle(EditorStyles.label);
                YellowLabel.normal.textColor = new Color(1f, 0.35f, 0f);
            }

            if (RedLabel == null)
            {
                RedLabel = new GUIStyle(EditorStyles.label);
                RedLabel.normal.textColor = new Color(1f, 0f, 0f);
                //RedLabel.fontStyle = FontStyle.Bold;
            }

            if (TreeViewColumnHeader == null)
            {
                TreeViewColumnHeader = new GUIStyle(MultiColumnHeader.DefaultStyles.columnHeader);
                TreeViewColumnHeader.alignment = TextAnchor.LowerLeft;
            }
        }
    }
}