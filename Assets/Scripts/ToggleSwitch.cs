using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Animasyon (Coroutine) için gerekli

public class ToggleSwitch : MonoBehaviour
{
    [Header("Bileşenler")]
    public Toggle toggle;
    public RectTransform handle; // Top (Düğme)
    public Image backgroundImage; // Arka Plan

    [Header("Ayarlar")]
    public Color onColor = new Color(0.3f, 0.8f, 0.3f); // Açıkken Yeşil
    public Color offColor = new Color(0.5f, 0.5f, 0.5f); // Kapalıyken Gri
    
    [Tooltip("Topun ne kadar hızlı kayacağı (Düşük = Hızlı)")]
    public float animationDuration = 0.2f; 

    // Topun duracağı X koordinatları
    private float handlePositionX; 
    private Coroutine _animateRoutine;

    void Start()
    {
        // Toggle'ın genişliğine göre topun ne kadar kayacağını hesapla
        // Top, arka planın genişliğinin yarısı kadar sağa veya sola gidecek
        float backgroundWidth = backgroundImage.rectTransform.rect.width;
        float handleWidth = handle.rect.width;
        
        // Kenarlardan biraz boşluk bırakarak hesapla
        handlePositionX = (backgroundWidth / 2) - (handleWidth / 2) - 5f;

        // Toggle'a dinleyici ekle: Değer değişince OnSwitch çalışsın
        toggle.onValueChanged.AddListener(OnSwitch);

        // Başlangıç durumunu ayarla (Animasyonsuz, direkt oturt)
        if (toggle.isOn)
        {
            handle.anchoredPosition = new Vector2(handlePositionX, 0);
            backgroundImage.color = onColor;
        }
        else
        {
            handle.anchoredPosition = new Vector2(-handlePositionX, 0);
            backgroundImage.color = offColor;
        }
    }

    void OnSwitch(bool isOn)
    {
        // Eğer bir animasyon zaten çalışıyorsa durdur
        if (_animateRoutine != null) StopCoroutine(_animateRoutine);
        
        // Yeni animasyonu başlat
        _animateRoutine = StartCoroutine(AnimateSwitch(isOn));
    }

    // Kayma Animasyonu Yapan Fonksiyon
    IEnumerator AnimateSwitch(bool isOn)
    {
        float timer = 0f;
        
        // Hedef Pozisyon ve Hedef Renk
        Vector2 startPos = handle.anchoredPosition;
        Vector2 endPos = isOn ? new Vector2(handlePositionX, 0) : new Vector2(-handlePositionX, 0);
        
        Color startColor = backgroundImage.color;
        Color endColor = isOn ? onColor : offColor;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;

            // Yumuşak geçiş (SmoothStep)
            t = t * t * (3f - 2f * t);

            // Pozisyonu ve Rengi yavaş yavaş değiştir
            handle.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            backgroundImage.color = Color.Lerp(startColor, endColor, t);

            yield return null; // Bir sonraki kareye geç
        }

        // Emin olmak için tam değerlere sabitle
        handle.anchoredPosition = endPos;
        backgroundImage.color = endColor;
    }
}