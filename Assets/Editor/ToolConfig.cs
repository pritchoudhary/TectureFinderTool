namespace TextureTool
{
    using UnityEngine;
    
    internal static class SizeUnits    
    {
        public const ulong B = 1;
        public const ulong KB = 1024 * B;
        public const ulong MB = 1024 * KB;
        public const ulong GB = 1024 * MB;
        public const ulong TB = 1024 * GB;
    }

    //Tool Configuration
    internal static class ToolConfig
    {
        // Texture directory
        public readonly static string[] TargetDirectories = new string[] { "Assets" };  

        // Text
        public readonly static string ProgressTitle = "Texture window is initializing";
        public readonly static string CreatingMessage = "Creating...";
        public readonly static string LoadingMessage = "Loading...";

        // Window Title
        public readonly static GUIContent WindowTitle = new GUIContent("Texture Viewer"); 

        public const int MB = 1024 * 1024; 
        public const int RedDataSize = 3 * MB; // Warning if data size exceeds limits
        public const int YellowTextureSize = 2000; // Yellow warning
        public const int RedTextureSize = 4000; // Red Warning
        public const int RedMaxTextureSize = 2048; // max texture size warning

        // Number of headers
        public static int HeaderColumnNum => HeaderColumns.Length; // Number of column headers

        // Header definition
        public static readonly TextureColumn[] HeaderColumns = new[] {
            new TextureColumn("Texture", 180f), // 0
            new TextureColumn("Texture Type", 105f), // 1
            new TextureColumn("Non Power of 2", 105f), // 2
            new TextureColumn("Max Size", 70f), // 3
            new TextureColumn("Generate\nMip Maps", 70f), // 4
            new TextureColumn("Alpha is\nTransparency", 96f), // 5
            new TextureColumn("Texture Size", 105f), // 6
            new TextureColumn("Data Size", 80f), // 7
        };

        public static float InitialHeaderTotalWidth
        {
            get
            {
                float width = 0f;
                for (int i = 0; i < HeaderColumns.Length; i++)
                {
                    width += HeaderColumns[i].width;
                }
                return width;
            }
        }

    }
}