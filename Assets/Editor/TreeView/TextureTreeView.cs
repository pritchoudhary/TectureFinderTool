namespace TextureTool
{
    using System.Linq;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;
    using System.Reflection;

    //TreeView for texture tools
    internal partial class TextureTreeView : TreeView
    {
        public static readonly string defaultSearchString = "HOGE";
        private static readonly TextAnchor fieldLabelAnchor = TextAnchor.MiddleLeft;

        private Texture2D prefabIconTexture = null; 
        private TextureTreeElement[] baseElements = new TextureTreeElement[0];

        public bool IsInitialized => isInitialized;
        public bool IsEmpty => baseElements.Length == 0;
        public int ElementCount => baseElements.Length;

       
        public TextureTreeView(TextureTreeViewState state, TextureColumnHeaderState headerState)
        : base(new TextureTreeViewState(), new TextureColumnHeader(headerState))
        {
            showAlternatingRowBackgrounds = true; // Show background stripes
            showBorder = true; 

            var textureColumnHeader = multiColumnHeader as TextureColumnHeader;
            textureColumnHeader.sortingChanged += OnSortingChanged; 
            textureColumnHeader.searchChanged += CallSearchChanged; 

            foreach (var searchField in headerState.SearchFields)
            {
                //searchField.searchChanged += () => CallSearchChanged("");
                searchField.searchChanged += CallSearchChanged;
            }
        }

        //Draw column rows
        private void DrawRowColumn(RowGUIArgs args, Rect rect, int columnIndex)
        {
            if (args.item.id < 0) { return; }  

            TextureTreeElement element = baseElements[args.item.id];
            if (element == null) { return; }

            var texture = element.Texture;
            var textureImporter = element.TextureImporter;
            if (texture == null || textureImporter == null)
            {
                EditorGUI.LabelField(rect, "(null)");
                return;
            }

            //GUIStyle labelStyle = EditorStyles.label;
            GUIStyle labelStyle = EditorStyles.label;
            switch ((EHeaderColumnId)columnIndex)
            {
                case EHeaderColumnId.TextureName:
                    rect.x += 2f;

                    // Drawn icon
                    Rect toggleRect = rect;
                    toggleRect.y += 2f;
                    toggleRect.size = new Vector2(12f, 12f);
                    GUI.DrawTexture(toggleRect, texture);

                    // Draw text
                    Rect labelRect = new Rect(rect);
                    labelRect.x += toggleRect.width;
                    EditorGUI.LabelField(labelRect, args.label);
                    break;
                default:
                    var text = element.GetDisplayText((EHeaderColumnId)columnIndex);
                    var style = element.GetLabelStyle((EHeaderColumnId)columnIndex);
                    EditorGUI.LabelField(rect, text, style);
                    break;
            }
        }

        public void Clean()
        {
            baseElements = new TextureTreeElement[0];
            Reload();
        }

        protected override void KeyEvent()
        {
            base.KeyEvent();

            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return) // Enter button press
            {
                var selection = this.GetSelection();
                if (selection.Count == 0) { return; }

                int id = selection.ElementAt(0);
                if (id < 0) { return; }

                // Open Prefab
                var path = baseElements[id].AssetPath;
                var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                AssetDatabase.OpenAsset(prefab);
            }
        }

        //Selection changes 
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            if (selectedIds.Count == 0) { return; }

            //selectedIds = selectedIds.Distinct().ToArray();

            Object[] objects = new Object[selectedIds.Count];
            for (int i = 0; i < selectedIds.Count; i++)
            {
                int id = selectedIds.ElementAt(i);
                var path = baseElements[id].AssetPath;
                objects[i] = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            }

            Selection.objects = objects;
            EditorGUIUtility.PingObject(objects[objects.Length - 1]);
        }

        
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);

            // Highlight on double click
            var path = baseElements[id].AssetPath;
            var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            AssetDatabase.OpenAsset(prefab); 
        }

        
        public TextureTreeElement GetElement(int index)
        {
            return baseElements[index];
        }

       
        public void UpdateDataSize()
        {
            foreach (var element in baseElements)
            {
                element.UpdateDataSize();
            }
        }

        public IEnumerable<TextureTreeElement> GetSelectionElement()
        {
            return GetSelection().Select(id => GetElement(id));
        }

       
        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            // Filter down results
            var textureItem = item as TextureTreeViewItem;
            var textureHeaderState = this.multiColumnHeader.state as TextureColumnHeaderState;
            return textureItem.data.DoesItemMatchSearch(textureHeaderState.SearchStates);
        }

        //Check if anything is in search bar
        new bool hasSearch
        {
            get
            {
                var textureHeaderState = this.multiColumnHeader.state as TextureColumnHeaderState;
                for (int i = 0; i < ToolConfig.HeaderColumnNum; i++)
                {
                    if (!textureHeaderState.SearchStates[i].HasValue) { return true; }
                }
                return false;
            }
        }

        //Creating columns
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();

            //var textureHeaderState = this.multiColumnHeader.state as TextureColumnHeaderState;
            //var columnSearchStrings = textureHeaderState.SearchStrings;

            var elements = new List<TreeViewItem>();
            foreach (var baseElement in baseElements)
            {
                var baseItem = CreateTreeViewItem(baseElement) as TextureTreeViewItem;
                baseItem.data = baseElement;
                
                if (DoesItemMatchSearch(baseItem, searchString))
                {
                    root.AddChild(baseItem);
                    rows.Add(baseItem); 
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            return rows;
        }

        
        protected override TreeViewItem BuildRoot()
        {
            return new TextureTreeViewItem { id = -1, depth = -1, displayName = "Root" };
        }

        private TreeViewItem CreateTreeViewItem(TextureTreeElement model)
        {
            return new TextureTreeViewItem { id = model.Index, displayName = model.AssetName };
        }

        public void SetTexture(Texture2D[] textures, TextureImporter[] importers)
        {
            baseElements = new TextureTreeElement[textures.Length];
            for (int i = 0; i < baseElements.Length; i++)
            {
                var path = AssetDatabase.GetAssetPath(textures[i]);
                baseElements[i] = new TextureTreeElement
                {
                    AssetPath = path,
                    AssetName = System.IO.Path.GetFileNameWithoutExtension(path),
                    Texture = textures[i],
                    TextureImporter = importers[i],
                };
            }

            for (int i = 0; i < baseElements.Length; i++)
            {
                var element = baseElements[i];
                element.Index = i;
                element.UpdateDataSize();
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (prefabIconTexture == null)
            {
                prefabIconTexture = EditorGUIUtility.Load("Prefab Icon") as Texture2D;
            }

            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var rect = args.GetCellRect(visibleColumnIndex);
                var columnIndex = args.GetColumn(visibleColumnIndex);
                var labelStye = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStye.alignment = fieldLabelAnchor;

                DrawRowColumn(args, rect, columnIndex);
            }
        }

        public void CallSearchChanged()
        {
            searchString = "";
            searchString = defaultSearchString;

            //Debug.Log("CallSearchChanged : searchString=" + searchString);

            //// TreeViewController m_TreeView
            //var m_TreeView = typeof(TreeView)
            //    .GetField("m_TreeView", BindingFlags.NonPublic | BindingFlags.Instance)
            //    .GetValue(this);
            //Debug.Log(m_TreeView);

            //// public ITreeViewDataSource data { get; set; }
            //var data = m_TreeView.GetType()
            //    .GetProperty("data", BindingFlags.Public | BindingFlags.Instance)
            //    .GetValue(m_TreeView);
            //Debug.Log(data);

            //// void OnSearchChanged();
            //MethodInfo OnSearchChanged = data.GetType()
            //    .GetMethod("OnSearchChanged", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            //Debug.Log(OnSearchChanged);

            //OnSearchChanged.Invoke(data, new object[0]);

            //System.Action<string> searchChanged = m_TreeView.GetType()
            //    .GetField("searchChanged", BindingFlags.NonPublic | BindingFlags.Instance)
            //    .GetValue(m_TreeView) as System.Action<string>;
        }
    }
}