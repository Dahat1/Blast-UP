#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PaletteImporter : MonoBehaviour
{
    [MenuItem("BlastUp/Otomatik Paletleri Yükle")]
    public static void ImportPalettes()
    {
        ThemeManager manager = FindObjectOfType<ThemeManager>();

        if (manager == null)
        {
            Debug.LogError("HATA: Sahnede 'ThemeManager' bulunamadı!");
            return;
        }

        manager.palettes = new List<ThemeManager.ColorPalette>();

        // -------------------------------------------------------------------------
        // GÜNCELLEME: Artık fonksiyonun sonuna "gridAlpha" değeri ekledik.
        // Cream için 0.4f (daha koyu/belirgin), Diğerleri için 0.1f (silik)
        // -------------------------------------------------------------------------


        // 1. GOLDEN NIGHT (Altın Gece) -> Block: MUSTARD YELLOW (#EDC22E)
        manager.palettes.Add(CreatePalette("Golden Night", 
            "#121212", "#262626", "#FFFFFF", "#EDC22E", "#FFFFFF", "#1A1A1A", 0.1f));


	    // 2. ATLANTIS (Atlantis) -> Block: OCEAN BLUE (#3498DB)
        manager.palettes.Add(CreatePalette("Atlantis", 
            "#001014", "#006064", "#FFFFFF", "#8F2121", "#EDC22E", "#001A21", 0.1f));

	    // 3. CREAMY ORANGE (Light Mod)
        manager.palettes.Add(CreatePalette("Creamy Orange", 
            "#FFF3E0", "#BF360C", "#222222", "#F67C5F", "#FFD700", "#FFE0B2", 0.97f));

	    // 4. VOLCANIC ASH
        manager.palettes.Add(CreatePalette("Volcanic Ash", 
            "#1A0505", "#420F0F", "#FFFFFF", "#EDC22E", "#4ECCA3", "#240808", 0.1f));

        // 5. SAPPHIRE DEPTHS
        manager.palettes.Add(CreatePalette("Sapphire Depths", 
            "#0A192F", "#172A45", "#FFFFFF", "#F67C5F", "#FFFFFF", "#0D213F", 0.1f));

        // 6. CHOCO MINT
        manager.palettes.Add(CreatePalette("Choco Mint", 
            "#1F1109", "#3E2723", "#FFFFFF", "#4ECCA3", "#FFFF00", "#2B180F", 0.1f));

        // 7. DEEP TEAL
        manager.palettes.Add(CreatePalette("Deep Teal", 
            "#002626", "#004D40", "#FFFFFF", "#F67C5F", "#FFFFFF", "#003333", 0.1f));


	    // 8. CYBER PUNK
        manager.palettes.Add(CreatePalette("Cyber Punk", 
            "#050510", "#1A237E", "#FFFFFF", "#951F1F", "#00E676", "#0A0A1F", 0.1f));

        // 9. VELVET VIOLET
        manager.palettes.Add(CreatePalette("Velvet Violet", 
            "#12001F", "#311B92", "#FFFFFF", "#97131C", "#FFFF00", "#1A002B", 0.1f));

        // 10. MIDNIGHT BERRY
        manager.palettes.Add(CreatePalette("Midnight Berry", 
            "#0D1B2A", "#1B263B", "#FFFFFF", "#9B59B6", "#EDC22E", "#132338", 0.1f));


        EditorUtility.SetDirty(manager);
        Debug.Log($"BAŞARILI! Izgara şeffaflıkları düzeltildi (Cream daha belirgin).");
    }

    // Yardımcı fonksiyona 'gridAlpha' parametresi eklendi
    private static ThemeManager.ColorPalette CreatePalette(string name, string bg, string frame, string grid, string block, string expl, string fill, float gridAlpha)
    {
        ThemeManager.ColorPalette p = new ThemeManager.ColorPalette();
        p.name = name;
        ColorUtility.TryParseHtmlString(bg, out p.backgroundColor);
        ColorUtility.TryParseHtmlString(frame, out p.frameColor);
        ColorUtility.TryParseHtmlString(grid, out p.gridColor);
        ColorUtility.TryParseHtmlString(block, out p.blockColor);
        ColorUtility.TryParseHtmlString(expl, out p.explosionColor);
        ColorUtility.TryParseHtmlString(fill, out p.boardFillColor);

        // --- IZGARA AYARI ---
        // Artık parametre olarak gelen gridAlpha değerini kullanıyoruz.
        Color gridWithAlpha = p.gridColor;
        gridWithAlpha.a = gridAlpha; 
        p.gridColor = gridWithAlpha;
        // --------------------

        return p;
    }
}
#endif