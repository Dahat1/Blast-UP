using System.Collections;
using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
    // Bekleme süresi
    public float beklemeSuresi = 10f;

    void Start()
    {
        // Oyun başlar başlamaz döngüyü başlat
        StartCoroutine(ZiplaDongusu());
    }

    IEnumerator ZiplaDongusu()
    {
        // Sonsuz döngü: Oyun açık olduğu sürece devam eder
        while (true)
        {
            // 1. ADIM: 10 Saniye bekle
            yield return new WaitForSeconds(beklemeSuresi);

            // Orijinal boyutunu hafızaya al (genelde 1,1,1)
            Vector3 normalBoyut = Vector3.one; 

            // 2. ADIM: İÇE BÜZÜŞME (Enerji toplama)
            // Boyutu 0.8'e indiriyoruz (küçülüyor)
            float sure = 0f;
            while (sure < 1f)
            {
                sure += Time.deltaTime * 5f; // Hız çarpanı
                transform.localScale = Vector3.Lerp(normalBoyut, new Vector3(0.8f, 0.8f, 1f), sure);
                yield return null;
            }

            // 3. ADIM: ANİDEN BÜYÜME (Pıt diye fırlama)
            // Boyutu 1.2'ye çıkarıyoruz (normalden büyük oluyor)
            sure = 0f;
            while (sure < 1f)
            {
                sure += Time.deltaTime * 10f; // Çok hızlı büyüsün diye hız 10
                transform.localScale = Vector3.Lerp(new Vector3(0.8f, 0.8f, 1f), new Vector3(1.2f, 1.2f, 1f), sure);
                yield return null;
            }

            // 4. ADIM: YERİNE OTURMA (Lastik gibi geri gelme)
            // Tekrar normal boyuta (1.0) dönüyor
            sure = 0f;
            while (sure < 1f)
            {
                sure += Time.deltaTime * 8f;
                transform.localScale = Vector3.Lerp(new Vector3(1.2f, 1.2f, 1f), normalBoyut, sure);
                yield return null;
            }
            
            // Garanti olsun diye boyutu tam 1 yapıyoruz
            transform.localScale = normalBoyut;
        }
    }
}