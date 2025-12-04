using UnityEngine;
using System.Collections;
using TMPro; // TextMeshPro kullanıyorsan

// Bu scripti, oluşturacağın "BestScoreNotification" prefab'ının en üstteki objesine tak.
[RequireComponent(typeof(CanvasGroup))]
public class BestScoreAnim : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 150f;  // Yukarı çıkış hızı
    public float lifeTime = 2.0f;   // Ekranda kalma süresi

    [Header("Efekt Ayarları")]
    public Vector3 startScale = new Vector3(0.5f, 0.5f, 1f); // İlk çıkış boyutu (küçükten büyüsün)

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        // Obje aktif olduğunda animasyonu başlat
        StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        // 1. Başlangıç Durumu (Küçük ve tam opak)
        transform.localScale = startScale;
        _canvasGroup.alpha = 1f;

        float timer = 0f;

        // 2. "Pop" Efekti (Hızlıca normal boyuta gel)
        while (timer < 0.3f)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / 0.3f;
            // Elastik bir büyüme efekti
            float scale = Mathf.Lerp(startScale.x, 1.2f, t); 
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        // Biraz overshoot yaptıktan sonra normal boyuta dön
        transform.localScale = Vector3.one; 

        // 3. Yukarı Kayma ve Sönme
        timer = 0f;
        while (timer < lifeTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / lifeTime;

            // Yukarı hareket (AnchoredPosition kullanarak)
            _rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.unscaledDeltaTime;

            // Şeffaflaşma (Sonlara doğru başlasın)
            if (t > 0.5f)
            {
                float fadeT = (t - 0.5f) / 0.5f;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeT);
            }

            yield return null;
        }

        // 4. Temizlik
        Destroy(gameObject);
    }
}