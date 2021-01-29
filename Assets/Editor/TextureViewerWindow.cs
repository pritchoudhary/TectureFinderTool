namespace TextureTool
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    //Textture list tool in project
    internal class TextureViewerWindow : EditorWindow
    {
        // TreeView Layout
        private static readonly GUILayoutOption[] TreeViewLayoutOptions = new GUILayoutOption[]
        {
            GUILayout.ExpandHeight(true)
        };

        [SerializeField] private TextureTreeViewState treeViewState = null; // State of TreeView
        [SerializeField] private TextureColumnHeaderState headerState = null; // Header of TreeView
        [SerializeField] private SearchState[] columnSearchStates = new SearchState[0]  ; // Column search staus
        [System.NonSerialized] private Texture2D[] textures = new Texture2D[0]; // Texture loaded
        [System.NonSerialized] private TextureImporter[] textureImporters = new TextureImporter[0];
        private TextureTreeView treeView = null; // TreeView 
        private SearchField searchField = null; // Search window

        [SerializeField] MultiColumnHeaderState columnHeaderState;
        private bool isLoadingTexture = false;
        private bool isCreatingTreeView = false;

        //Open Window
        [MenuItem("Tools/TexTools/TextureEditor")]
        private static void OpenWindow()
        {
            var window = GetWindow<TextureViewerWindow>();
            window.titleContent = ToolConfig.WindowTitle;

            var position = window.position;
            position.width = ToolConfig.InitialHeaderTotalWidth + 50f;
            position.height = 400f;
            window.position = position;

            window.CreateTreeView(); // Create TreeView immediately after set up 
        }

        //Window Drawing
        private void OnGUI()
        {
            MyStyle.CreateGUIStyleIfNull();
            if (treeView == null)
            {
                CreateTreeView();
            }

            DrawHeader();
            DrawTreeView();

        }

        //Drawing TreeView
        private void DrawTreeView()
        {
            var rect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandHeight(true));

            if (isCreatingTreeView)
            {
                EditorGUI.BeginDisabledGroup(true);
                treeView?.OnGUI(rect);
                EditorGUI.EndDisabledGroup();

                rect.position += MyStyle.LoadingLabelPosition;
                EditorGUI.LabelField(rect, ToolConfig.CreatingMessage, MyStyle.LoadingLabel);
            }
            else
            if (isLoadingTexture)
            {
                EditorGUI.BeginDisabledGroup(true);
                treeView?.OnGUI(rect);
                EditorGUI.EndDisabledGroup();

                rect.position += MyStyle.LoadingLabelPosition;
                EditorGUI.LabelField(rect, ToolConfig.LoadingMessage, MyStyle.LoadingLabel);
            }
            else
            {
                treeView?.OnGUI(rect);
            }
        }

        //Draw header at top of the window
        private void DrawHeader()
        {
            var defaultColor = GUI.backgroundColor;

            // Draw search window
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUI.backgroundColor = Color.green;
                DrawReloadButton();
                GUI.backgroundColor = defaultColor;

                GUILayout.Space(100);

                GUILayout.FlexibleSpace();
            }

            GUI.backgroundColor = defaultColor;
        }

        //Rrfresh
        private void DrawReloadButton()
        {
            if (treeView == null) { return; }

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                CreateTreeView();
                RefreshTexture();

                headerState.ResetSearch();

                treeView.SetTexture(textures, textureImporters);
                treeView.Reload(); // RefreshTexture when you call BuildRoot is executed, then BuildRows will be executed。


                EditorApplication.delayCall += () => treeView.searchString = TextureTreeView.defaultSearchString;
            }
        }

        //Update TreeView
        private void CreateTreeView()
        {
            if (treeView != null) { return; }
            if (isCreatingTreeView) { return; }
            isCreatingTreeView = true;
            Repaint();

            EditorApplication.delayCall += () =>
            {
                if (columnSearchStates == null || columnSearchStates.Length != ToolConfig.HeaderColumnNum)
                {
                    columnSearchStates = new SearchState[ToolConfig.HeaderColumnNum];
                    for (int i = 0; i < ToolConfig.HeaderColumnNum; i++)
                    {
                        columnSearchStates[i] = new SearchState();
                    }
                }


                treeViewState = treeViewState ?? new TextureTreeViewState();
                headerState = headerState ?? new TextureColumnHeaderState(ToolConfig.HeaderColumns, columnSearchStates);
                headerState.ResetSearch();

                // Create TreeView
                treeView = treeView ?? new TextureTreeView(treeViewState, headerState);
                treeView.searchString = TextureTreeView.defaultSearchString;
                treeView.Reload(); // RefreshTexture when you call BuildRoot is executed, then BuildRows will be executed。

                //Initailize SearchField
                searchField = new SearchField();
                searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;

                isCreatingTreeView = false;
            };
        }

        //Get asset list from specified directory
        public static IEnumerable<string> GetAssetPaths(string[] directories, string filter = "")
        {
            for (int i = 0; i < directories.Length; i++)
            {
                var directory = directories[i];
                if (directory[directory.Length - 1] == '/')
                {
                    directory = directory.Substring(0, directory.Length - 1);
                }
            }

            var paths = AssetDatabase.FindAssets(filter, directories)
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => !string.IsNullOrEmpty(x))
                .OrderBy(x => x);

            return paths;
        }

        private void OnFocus()
        {
            if (treeView != null)
            {
                treeView.UpdateDataSize();
            }
        }

        //Loading Texture
        public void RefreshTexture()
        {
            if (isLoadingTexture) { return; }

            isLoadingTexture = true;
            CustomUI.DisplayProgressLoadTexture();

            // Load texture from specified directory
            var paths = GetAssetPaths(ToolConfig.TargetDirectories, "t:texture2d");
            var textureList = new List<Texture2D>();
            var importerList = new List<TextureImporter>();
            foreach (var path in paths)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null) continue;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                textureList.Add(texture);
                importerList.Add(importer);
            }
            textures = textureList.ToArray();
            textureImporters = importerList.ToArray();

            EditorUtility.ClearProgressBar();

            isLoadingTexture = false;
        }
    }
}