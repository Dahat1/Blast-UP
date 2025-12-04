using UnityEngine;

public class CameraFit : MonoBehaviour
{
    [Header("Ayarlar")]
    public SpriteRenderer boardBackground; // Izgaranın arkasındaki o kare resim
    public float padding = 1f; // Kenarlardan ne kadar boşluk kalsın?

    void Start()
    {
        AdjustCamera();
    }

    void AdjustCamera()
    {
        if (boardBackground == null) return;

        Camera cam = Camera.main;
        
        // Arka planın boyutlarını al
        float targetHeight = boardBackground.bounds.size.y;
        float targetWidth = boardBackground.bounds.size.x;

        // Ekran oranını hesapla
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = targetWidth / targetHeight;

        if (screenRatio >= targetRatio)
        {
            // Ekran yeterince geniş, yüksekliğe göre ayarla
            cam.orthographicSize = (targetHeight / 2) + padding;
        }
        else
        {
            // Ekran dar (Uzun telefon), genişliğe göre ayarla
            // Formül: Size = (TargetWidth / ScreenRatio) / 2
            float differenceInSize = targetRatio / screenRatio;
            cam.orthographicSize = (targetHeight / 2 * differenceInSize) + padding;
        }
    }
}