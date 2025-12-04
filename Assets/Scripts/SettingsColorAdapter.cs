using UnityEngine;
using UnityEngine.UI;

public class SettingsColorAdapter : MonoBehaviour
{
    [Header("Boyanacak Objeler")]
    public Image panelBackground; // Panelin arkası
    public Image[] frameElements; // Butonlar, alt paneller vb.
    
    // Gölge eklemek istersen Shadow componenti olan objeleri de buraya alabiliriz
    // ama Shadow rengi genelde siyahtır, o yüzden gerek yok.

    void Start()
    {
        // Sahne açıldığında veya Obje aktif olduğunda boyama yap
        ApplyColors();
    }
    
    // Panel her açıldığında da rengi kontrol etsin (Garanti olsun)
    void OnEnable()
    {
        ApplyColors();
    }

    public void ApplyColors()
    {
        // ThemeManager yoksa işlem yapma
        if (ThemeManager.Instance == null) return;

        // Mevcut renk paletini çek
        var palette = ThemeManager.Instance.currentPalette;

        // 1. Arka Planı Boya (Background Color)
        if (panelBackground != null)
        {
            panelBackground.color = palette.backgroundColor;
        }

        // 2. Butonları/Çerçeveleri Boya (Frame Color)
        if (frameElements != null)
        {
            foreach (var img in frameElements)
            {
                if (img != null) img.color = palette.frameColor;
            }
        }
    }
}