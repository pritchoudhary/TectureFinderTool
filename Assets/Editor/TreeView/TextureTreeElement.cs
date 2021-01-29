namespace TextureTool
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    //Elements of TreeView
    internal class TextureTreeElement
    {
        private ulong textureByteLength = 0;
        private string textureDataSizeText = "";
        public string AssetPath { get; set; } // Asset path
        public string AssetName { get; set; } // Asset name
        public ulong TextureByteLength => textureByteLength; // Texture data size (Byte)
        public string TextureDataSizeText => textureDataSizeText;// Texture data size text
        public Texture2D Texture { get; set; } // Loaded textures
        public TextureImporter TextureImporter { get; set; } // Import settings
        public int Index { get; set; } 
        public TextureTreeElement Parent { get; private set; } 
        public List<TextureTreeElement> Children { get; } = new List<TextureTreeElement>();

        //Get GUIStyle
        public GUIStyle GetLabelStyle(EHeaderColumnId id)
        {
            GUIStyle labelStyle = MyStyle.DefaultLabel;
            switch (id)
            {
                case EHeaderColumnId.TextureName:
                case EHeaderColumnId.TextureType:
                    break;
                case EHeaderColumnId.NPot:
                    if (TextureImporter.npotScale == TextureImporterNPOTScale.None)
                    {
                        labelStyle = MyStyle.RedLabel;
                    }
                    break;
                case EHeaderColumnId.MaxSize:
                    if (TextureImporter.maxTextureSize > ToolConfig.RedMaxTextureSize)
                    {
                        labelStyle = MyStyle.RedLabel;
                    }
                    break;
                case EHeaderColumnId.GenerateMips:
                    if (TextureImporter.mipmapEnabled == true)
                    {
                        labelStyle = MyStyle.RedLabel;
                    }
                    break;
                case EHeaderColumnId.AlphaIsTransparency:
                    break;
                case EHeaderColumnId.TextureSize:
                    switch (Mathf.Min(Texture.width, Texture.height))
                    {
                        case int minSize when minSize > ToolConfig.RedTextureSize:
                            labelStyle = MyStyle.RedLabel;
                            break;
                        case int minSize when minSize > ToolConfig.YellowTextureSize:
                            labelStyle = MyStyle.YellowLabel;
                            break;
                        default:
                            labelStyle = MyStyle.DefaultLabel;
                            break;
                    }
                    break;
                case EHeaderColumnId.DataSize:
                    switch ((int)TextureByteLength)
                    {
                        case int len when len > ToolConfig.RedDataSize:
                            labelStyle = MyStyle.RedLabel;
                            break;
                        //case int len when len > ToolConfig.YellowDataSize:
                        //    labelStyle = MyStyle.YellowLabel;
                        //    break;
                        default:
                            labelStyle = MyStyle.DefaultLabel;
                            break;
                    }
                    break;

            }
            return labelStyle;
        }

        //Data to display
        public object GetDisplayData(EHeaderColumnId id)
        {
            switch (id)
            {
                case EHeaderColumnId.TextureName:
                    return Texture.name;
                case EHeaderColumnId.TextureType:
                    return TextureImporter.textureType;
                case EHeaderColumnId.NPot:
                    return TextureImporter.npotScale;
                case EHeaderColumnId.MaxSize:
                    return TextureImporter.maxTextureSize;
                case EHeaderColumnId.GenerateMips:
                    return TextureImporter.mipmapEnabled;
                case EHeaderColumnId.AlphaIsTransparency:
                    return TextureImporter.alphaIsTransparency;
                case EHeaderColumnId.TextureSize:
                    return new Vector2Int(Texture.width, Texture.height);
                case EHeaderColumnId.DataSize:
                    return TextureByteLength;
                default:
                    return -1;
            }
        }

        //Get Text to display
        public string GetDisplayText(EHeaderColumnId id)
        {
            switch (id)
            {
                case EHeaderColumnId.TextureName:
                    return Texture.name;
                case EHeaderColumnId.TextureType:
                    return TextureImporter.textureType.ToString();
                case EHeaderColumnId.NPot:
                    return TextureImporter.npotScale.ToString();
                case EHeaderColumnId.MaxSize:
                    return TextureImporter.maxTextureSize.ToString();
                case EHeaderColumnId.GenerateMips:
                    return TextureImporter.mipmapEnabled.ToString();
                case EHeaderColumnId.AlphaIsTransparency:
                    return TextureImporter.alphaIsTransparency.ToString();
                case EHeaderColumnId.TextureSize:
                    return $"{Texture.width}x{Texture.height}";
                case EHeaderColumnId.DataSize:
                    return textureDataSizeText;
                default:
                    return "---";
            }
        }

        //Update data size
        public void UpdateDataSize()
        {
            textureByteLength = (Texture != null) ? (ulong)Texture?.GetRawTextureData().Length : 0;
            textureDataSizeText = Utils.ConvertToHumanReadableSize(textureByteLength);
        }

        //Add Children
        internal void AddChild(TextureTreeElement child)
        {
            // Delete if parent present
            if (child.Parent != null)
            {
                child.Parent.RemoveChild(child);
            }

            // Add child to parent
            Children.Add(child);
            child.Parent = this;
        }

        //Delete child
        public void RemoveChild(TextureTreeElement child)
        {
            if (Children.Contains(child))
            {
                Children.Remove(child);
                child.Parent = null;
            }
        }

        //Column search
        public bool DoesItemMatchSearch(SearchState[] searchState)
        {
            for (int columnIndex = 0; columnIndex < ToolConfig.HeaderColumnNum; columnIndex++)
            {
                if (!DoesItemMatchSearchInternal(searchState, columnIndex))
                {
                    return false;
                }
            }
            return true;
        }

        //Search match
        private bool DoesItemMatchSearchInternal(SearchState[] searchStates, int columnIndex)
        {
            var searchState = searchStates[columnIndex];
            //if (!searchState.HasValue) { return true; }

            return searchState.DoesItemMatch((EHeaderColumnId)columnIndex, this);
        }
    }
}