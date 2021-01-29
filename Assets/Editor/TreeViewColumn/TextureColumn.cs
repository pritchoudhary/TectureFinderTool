namespace TextureTool
{
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    internal class TextureColumn : MultiColumnHeaderState.Column
    {
        public TextureColumn(string label, float width) : base()
        {
            base.width = width;
            autoResize = false;
            headerContent = new GUIContent(label);
        }
    }
}