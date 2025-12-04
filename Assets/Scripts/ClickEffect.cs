using UnityEngine;
using System.Collections;

public class ClickEffect : MonoBehaviour
{
    [Header("Ayarlar")]
    public float animationDuration = 0.5f; 
    public Color effectColor = new Color(1f, 0.0f, 0.8f, 1f); 
    
    [Range(0.01f, 2.0f)]
    public float sizeMultiplier = 0.5f; // Biraz daha görünür olsun diye 0.5 yaptım

    [Header("Referanslar")]
    public SpriteRenderer innerRing; 
    public SpriteRenderer outerRing; 

    private float _timer = 0f;

    void Start()
    {
        // --- KESİN ÇÖZÜM: SORUN BURADAYDI ---
        // Ana objenin boyutu 1 olmalı, yoksa içindekiler gözükmez!
        transform.localScale = Vector3.one; 
        // ------------------------------------

        if (innerRing != null) 
        {
            innerRing.color = effectColor;
            innerRing.sortingOrder = 32000; // En önde
            innerRing.transform.localScale = Vector3.zero; // Sadece halka 0 başlasın
        }
        if (outerRing != null) 
        {
            outerRing.color = effectColor;
            outerRing.sortingOrder = 31999; // Bir arkada
            outerRing.transform.localScale = Vector3.zero; // Sadece halka 0 başlasın
        }
    }

    void Update()
    {
        _timer += Time.unscaledDeltaTime; 
        float t = _timer / animationDuration; 

        if (t >= 1f)
        {
            Destroy(gameObject); 
            return;
        }

        if (innerRing != null)
        {
            float innerT = Mathf.Clamp01(t * 1.5f); 
            float scale = Mathf.Lerp(0f, 1.0f, 1f - Mathf.Pow(1f - innerT, 3f)) * sizeMultiplier; 
            innerRing.transform.localScale = Vector3.one * scale;

            Color c = effectColor;
            c.a = Mathf.Lerp(1f, 0f, innerT); 
            innerRing.color = c;
        }

        if (outerRing != null)
        {
            float outerMax = 1.5f * sizeMultiplier;
            float outerScale = Mathf.Lerp(0f, outerMax, 1f - Mathf.Pow(1f - t, 2f)); 
            outerRing.transform.localScale = Vector3.one * outerScale;

            Color c = effectColor;
            c.a = Mathf.Lerp(0.5f, 0f, t); 
            outerRing.color = c;
        }
    }
}