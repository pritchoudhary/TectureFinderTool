namespace TextureTool
{
    using UnityEditor;

    //UI drawing for editor
    internal static class CustomUI
    {
        public static int RowCount { get; set; } = 1;

        public static void DisplayProgressLoadTexture()
        {
            EditorUtility.DisplayProgressBar(ToolConfig.ProgressTitle, "Getting Textures", 0f);
        }
    }
}