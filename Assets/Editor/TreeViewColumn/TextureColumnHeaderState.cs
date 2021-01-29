namespace TextureTool
{
    using UnityEditor.IMGUI.Controls;

    internal class TextureColumnHeaderState : MultiColumnHeaderState
    {
        public ColumnSearchField[] SearchFields { get; private set; }
        public SearchState[] SearchStates { get; private set; } 

        public TextureColumnHeaderState(Column[] columns, SearchState[] searchStates) : base(columns)
        {
            //SearchStrings = searchStrings;
            SearchStates = searchStates;
            SearchFields = new ColumnSearchField[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                SearchFields[i] = new ColumnSearchField();
            }
        }

        public void ResetSearch()
        {
            foreach (var state in SearchStates)
            {
                state.ResetSearch();
            }
        }
    }
}