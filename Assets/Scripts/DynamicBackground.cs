using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DynamicBackground : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject particlePrefab; // Uçuşacak obje (Particle_Dot)
    public int particleCount = 30;    // Kaç tane olsun?
    public float speed = 20f;         // Ne kadar hızlı uçsunlar?
    public Color particleColor = new Color(1, 1, 1, 0.5f); // Toz rengi (Yarı saydam beyaz)

    [Header("Alan Ayarları")]
    // Ekranda hangi sınırlar içinde uçsunlar? (Canvas boyutuna göre ayarla)
    public Vector2 xLimits = new Vector2(-500, 500); 
    public Vector2 yLimits = new Vector2(-900, 900);

    private List<RectTransform> _particles = new List<RectTransform>();
    private List<float> _speeds = new List<float>(); // Her birine farklı hız verelim
    private List<float> _alphaOffsets = new List<float>(); // Yanıp sönme için

    void Start()
    {
        // Parçacıkları Yarat
        for (int i = 0; i < particleCount; i++)
        {
            SpawnParticle();
        }
    }

    void SpawnParticle()
    {
        GameObject obj = Instantiate(particlePrefab, transform);
        RectTransform rect = obj.GetComponent<RectTransform>();

        // Rastgele Konum
        float randX = Random.Range(xLimits.x, xLimits.y);
        float randY = Random.Range(yLimits.x, yLimits.y);
        rect.anchoredPosition = new Vector2(randX, randY);

        // Rastgele Boyut (Kimisi küçük kimisi büyük olsun)
        float randScale = Random.Range(0.3f, 1.0f);
        rect.localScale = Vector3.one * randScale;

        // Rengini Ayarla
        Image img = obj.GetComponent<Image>();
        img.color = particleColor;

        // Listelere Ekle
        _particles.Add(rect);
        _speeds.Add(Random.Range(speed * 0.5f, speed * 1.5f)); // Hız çeşitliliği
        _alphaOffsets.Add(Random.Range(0f, 10f)); // Yanıp sönme zamanlaması
    }

    void Update()
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            RectTransform particle = _particles[i];

            // 1. HAREKET (YUKARI DOĞRU SÜZÜLME)
            particle.anchoredPosition += new Vector2(0, _speeds[i] * Time.deltaTime);

            // Eğer ekranın tepesinden çıktıysa, en alta geri ışınla
            if (particle.anchoredPosition.y > yLimits.y)
            {
                float randX = Random.Range(xLimits.x, xLimits.y);
                particle.anchoredPosition = new Vector2(randX, yLimits.x);
            }

            // 2. PARLAMA / YANIP SÖNME (TWINKLE)
            // Sinüs dalgası ile Alpha (Görünürlük) değeriyle oyna
            Image img = particle.GetComponent<Image>();
            Color c = img.color;
            // 0.2 ile 0.8 arasında gidip gelen bir parlaklık
            float alpha = 0.2f + Mathf.PingPong(Time.time + _alphaOffsets[i], 0.6f); 
            c.a = alpha;
            img.color = c;
        }
    }
}