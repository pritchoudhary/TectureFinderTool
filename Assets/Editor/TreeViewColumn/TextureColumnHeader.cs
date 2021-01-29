namespace TextureTool
{
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    internal class TextureColumnHeader : MultiColumnHeader
    {
        private const float searchY = 4f; 
        private const float searchMarginLeft = 3f;
        private const float searchMarginRight = 6f; 
        private const float searchHeight = 17f; 
        private const float searchSpace = 4f; 
        private const float sortHeight = labelHeight + sortSpace; 
        private const float sortSpace = 6f;
        private const float labelHeight = 32f;
        private const float labelY = 4f; 

        public System.Action searchChanged { get; set; }

       
        public TextureColumnHeader(MultiColumnHeaderState state) : base(state)
        {
            height = searchY + searchHeight + searchSpace + sortHeight;
        }

        //Header drawing of TreeView
        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            headerRect.y += searchY;
            headerRect.height -= searchY;

            Rect searchRect = new Rect(headerRect);
            searchRect.height = searchHeight;
            searchRect.width -= searchMarginLeft + searchMarginRight;
            searchRect.x += searchMarginLeft;

            EditorGUI.BeginChangeCheck();
            var headerState = state as TextureColumnHeaderState;
            var searchField = headerState.SearchFields[columnIndex];
            searchField.OnGUI(searchRect, headerState, columnIndex);
            if (EditorGUI.EndChangeCheck())
            {
                searchChanged?.Invoke();
                searchField.searchChanged?.Invoke();
            }

            if (canSort && column.canSort)
            {
                Rect sortRect = headerRect;
                sortRect.height = sortHeight;
                sortRect.y = searchRect.height + searchSpace;
                SortingButton(column, sortRect, columnIndex);
            }

            Rect labelRect = new Rect(headerRect.x, headerRect.yMax - labelHeight - labelY, headerRect.width, labelHeight);
            GUI.Label(labelRect, column.headerContent, MyStyle.TreeViewColumnHeader);
        }
    }
}