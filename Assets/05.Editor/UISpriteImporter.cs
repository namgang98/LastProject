#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// 03.Asset/UI 폴더에 들어오는 그림을 UI용 스프라이트로 자동 설정
public class UISpriteImporter : AssetPostprocessor
{
    private const string Folder = "Assets/03.Asset/UI/";

    // 파일 이름 → 9-slice 테두리 두께(px). 없으면 테두리 없이 Single
    private static readonly Dictionary<string, int> Borders = new()
    {
        { "ui_panel", 32 },
        { "ui_gauge_bg", 12 },
        { "ui_slider_track", 8 },
        { "ui_slider_fill", 4 },
        { "ui_input", 12 },
        { "ui_button_normal", 16 },
        { "ui_button_hover", 16 },
        { "ui_button_pressed", 16 },
        { "ui_button_disabled", 16 },
    };

    private void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith(Folder))
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        string name = Path.GetFileNameWithoutExtension(assetPath);

        if(!Borders.TryGetValue(name, out int border))
            return;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteBorder = new Vector4(border, border, border, border);
        importer.SetTextureSettings(settings);
    }
}
#endif
