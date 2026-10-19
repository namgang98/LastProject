#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// 03.Asset/UI 폴더에 들어오는 그림을 UI용 스프라이트로 자동 설정
public class UISpriteImporter : AssetPostprocessor
{
    private const string Folder = "Assets/03.Asset/UI/";

    // 파일 이름 → 9-slice 테두리 (왼쪽, 아래, 오른쪽, 위) px. 없으면 테두리 없이 Single
    private static readonly Dictionary<string, Vector4> Borders = new()
    {
        { "ui_panel", new Vector4(36, 36, 36, 36) },
        { "ui_gauge_bg", new Vector4(12, 12, 12, 12) },
        { "ui_slider_track", new Vector4(8, 8, 8, 8) },
        { "ui_slider_fill", new Vector4(4, 4, 4, 4) },
        { "ui_input", new Vector4(12, 12, 12, 12) },
        { "ui_button_normal", new Vector4(16, 16, 16, 16) },
        { "ui_button_hover", new Vector4(16, 16, 16, 16) },
        { "ui_button_pressed", new Vector4(16, 16, 16, 16) },
        { "ui_button_disabled", new Vector4(16, 16, 16, 16) },
        { "ui_title_banner", new Vector4(16, 16, 16, 16) },
        { "ui_divider", new Vector4(24, 0, 24, 0) },
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

        if(!Borders.TryGetValue(name, out Vector4 border))
            return;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteBorder = border;
        importer.SetTextureSettings(settings);
    }
}
#endif
