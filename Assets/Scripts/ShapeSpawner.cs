using UnityEngine;
using System.Collections; // Coroutine için şart
using System.Collections.Generic;

public class ShapeSpawner : MonoBehaviour
{
    public static ShapeSpawner Instance;

    public Transform[] spawnPoints;

    [Header("Zorluk Kategorileri")]
    public List<GameObject> easyShapes;   
    public List<GameObject> mediumShapes; 
    public List<GameObject> hardShapes;   

    [Header("Akıllı Asistan Ayarı")]
    public int smartAssistLimit = 5000; 

    // --- YENİ: Spawn Animasyon Ayarları ---
    [Header("Spawn Animasyonu")]
    public float spawnDelayBetweenShapes = 0.1f; // Sırayla çıkma hızı
    public float popAnimationDuration = 0.4f;    // Büyüme süresi
    // --------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isIntroPlaying)
        {
            SpawnNewShapes();
        }
    }

    public void SpawnNewShapes()
    {
        // Direkt döngü yerine Coroutine başlatıyoruz (Sıralı çıkış için)
        StartCoroutine(SpawnRoutine());
    }

    // --- YENİ: SIRALI SPAWN RUTİNİ ---
    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point = spawnPoints[i];
            if (point.childCount > 0) continue;

            GameObject shapeToSpawn = null;
            int currentScore = 0;
            if(GameManager.Instance != null) currentScore = GameManager.Instance.CurrentScore;

            // --- AKILLI ASİSTAN MANTIĞI (Aynen Korundu) ---
            if (currentScore < smartAssistLimit)
            {
                if (i == 0)
                {
                      shapeToSpawn = FindShapeThatClearsLine(easyShapes);
                      if (shapeToSpawn == null) shapeToSpawn = GetRandom(easyShapes);
                }
                else
                {
                    List<GameObject> combinedList = new List<GameObject>();
                    combinedList.AddRange(easyShapes);
                    combinedList.AddRange(mediumShapes);
                    shapeToSpawn = FindShapeThatClearsLine(combinedList);
                    if (shapeToSpawn == null) shapeToSpawn = GetRandomShapeByDifficulty();
                }
            }
            else
            {
                if (i == 0) shapeToSpawn = GetRandom(easyShapes);
                else shapeToSpawn = GetRandomShapeByDifficulty();
            }

            if (!GridManager.Instance.CanShapeFitAnywhere(shapeToSpawn))
            {
                shapeToSpawn = FindSafeShape();
            }
            // ---------------------------------------------

            GameObject newShape = Instantiate(shapeToSpawn, point.position, Quaternion.identity);
            newShape.transform.SetParent(point);
            
            // BAŞLANGIÇTA GÖRÜNMEZ (Scale 0)
            newShape.transform.localScale = Vector3.zero; 

            // RENGİ AYARLA
            if (ThemeManager.Instance != null)
            {
                Color themeBlockColor = ThemeManager.Instance.currentPalette.blockColor;
                foreach (Transform child in newShape.transform)
                {
                    SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = themeBlockColor;
                }
            }

            // POP ANİMASYONUNU BAŞLAT (Overshoot Effect)
            StartCoroutine(AnimateShapePop(newShape.transform));

            // Bir sonraki şekil için bekle (Pıtır pıtır efekt)
            yield return new WaitForSeconds(spawnDelayBetweenShapes);
        }

        CheckGameOverCondition();
    }

    private IEnumerator AnimateShapePop(Transform shapeTransform)
    {
        float timer = 0f;
        Vector3 targetScale = Vector3.one * 0.5f; // Normal boyut
        Vector3 overshootScale = targetScale * 1.2f; // %20 büyüme

        while (timer < popAnimationDuration)
        {
            if (shapeTransform == null) yield break; 

            timer += Time.deltaTime;
            float t = timer / popAnimationDuration;

            if (t < 0.7f)
            {
                // İlk %70: Büyüme
                float t1 = t / 0.7f;
                t1 = t1 * t1 * (3f - 2f * t1); 
                shapeTransform.localScale = Vector3.Lerp(Vector3.zero, overshootScale, t1);
            }
            else
            {
                // Son %30: Yerine oturma
                float t2 = (t - 0.7f) / 0.3f;
                shapeTransform.localScale = Vector3.Lerp(overshootScale, targetScale, t2);
            }

            yield return null;
        }

        if (shapeTransform != null) shapeTransform.localScale = targetScale;
    }
    // ----------------------------------------------------

    public void UpdateSpawnedShapesColor(Color newColor)
    {
        foreach (Transform point in spawnPoints)
        {
            if (point.childCount > 0)
            {
                Transform shape = point.GetChild(0);
                foreach (Transform block in shape)
                {
                    SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = newColor;
                }
            }
        }
    }

    // --- AŞAĞIDAKİ YARDIMCI FONKSİYONLAR AYNI KALDI ---

    GameObject FindShapeThatClearsLine(List<GameObject> candidates)
    {
        List<GameObject> shuffled = new List<GameObject>(candidates);
        for (int i = 0; i < shuffled.Count; i++) {
            GameObject temp = shuffled[i];
            int rnd = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[rnd];
            shuffled[rnd] = temp;
        }

        foreach (GameObject shape in shuffled)
        {
            if (GridManager.Instance.CanShapeClearAnyLine(shape))
            {
                return shape;
            }
        }
        return null; 
    }

    GameObject FindSafeShape()
    {
        List<GameObject> candidates = new List<GameObject>(easyShapes);
        for (int i = 0; i < candidates.Count; i++) {
            GameObject temp = candidates[i];
            int rnd = Random.Range(i, candidates.Count);
            candidates[i] = candidates[rnd];
            candidates[rnd] = temp;
        }

        foreach (GameObject shape in candidates)
        {
            if (GridManager.Instance.CanShapeFitAnywhere(shape)) return shape;
        }
        return GetRandom(easyShapes);
    }

    GameObject GetRandomShapeByDifficulty()
    {
        int score = 0;
        if (GameManager.Instance != null) score = GameManager.Instance.CurrentScore;
        float val = Random.value; 

        if (score < 10000)
        {
            if (val < 0.95f) return GetRandom(easyShapes);
            else return GetRandom(mediumShapes);
        }
        else if (score < 20000)
        {
            if (val < 0.85f) return GetRandom(easyShapes);
            else return GetRandom(mediumShapes);
        }
        else
        {
            if (val < 0.75f) return GetRandom(easyShapes);
            else if (val < 0.90f) return GetRandom(mediumShapes);
            else return GetRandom(hardShapes);
        }
    }

    GameObject GetRandom(List<GameObject> list)
    {
        if (list == null || list.Count == 0) 
        {
            if (easyShapes != null && easyShapes.Count > 0) return easyShapes[0];
            return null; 
        }
        return list[Random.Range(0, list.Count)];
    }

    public void CheckIfFieldIsEmpty()
    {
        StartCoroutine(CheckRoutine());
    }

    System.Collections.IEnumerator CheckRoutine()
    {
        yield return new WaitForEndOfFrame();
        bool allEmpty = true;
        foreach (Transform point in spawnPoints)
        {
            if (point.childCount > 0) { allEmpty = false; break; }
        }

        if (allEmpty) SpawnNewShapes();
        else CheckGameOverCondition();
    }

    public void CheckGameOverCondition()
    {
        StartCoroutine(GameOverCheckRoutine());
    }

    System.Collections.IEnumerator GameOverCheckRoutine()
    {
        yield return new WaitForEndOfFrame();
        bool canAnyMove = false;

        foreach (Transform point in spawnPoints)
        {
            if (point.childCount > 0)
            {
                GameObject shape = point.GetChild(0).gameObject;

                if (shape.transform.childCount == 0) continue;
                
                if (GridManager.Instance.CanShapeFitAnywhere(shape))
                {
                    canAnyMove = true;
                    break;
                }
            }
        }

        if (!canAnyMove) GameManager.Instance.GameOver();
    }


    // --- SAVE/LOAD SİSTEMİ İÇİN EKLENENLER ---

    // 1. Bekleyen şekillerin isimlerini listeler
    public List<string> GetShapeNames()
    {
        List<string> names = new List<string>();
        foreach (Transform point in spawnPoints)
        {
            if (point.childCount > 0)
            {
                // "(Clone)" yazısını temizleyerek kaydet ki tekrar bulabilelim
                string rawName = point.GetChild(0).name.Replace("(Clone)", "").Trim();
                names.Add(rawName);
            }
            else
            {
                names.Add("Empty"); // Boş slot
            }
        }
        return names;
    }

    // 2. İsim listesini alır ve şekilleri tekrar yaratır
    public void LoadShapesFromData(List<string> names)
    {
        // Önce var olanları temizle
        foreach (Transform point in spawnPoints)
        {
            if (point.childCount > 0) Destroy(point.GetChild(0).gameObject);
        }

        // Blok rengini al
        Color themeBlockColor = Color.white;
        if (ThemeManager.Instance != null) themeBlockColor = ThemeManager.Instance.currentPalette.blockColor;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (i >= names.Count) break;
            string shapeName = names[i];

            if (shapeName == "Empty") continue;

            // İsme göre prefabı bul
            GameObject prefab = FindPrefabByName(shapeName);
            
            if (prefab != null)
            {
                GameObject newShape = Instantiate(prefab, spawnPoints[i].position, Quaternion.identity);
                newShape.transform.SetParent(spawnPoints[i]);
                newShape.transform.localScale = Vector3.one * 0.5f; // Normal boyut

                // Rengi boya
                foreach (Transform child in newShape.transform)
                {
                    SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = themeBlockColor;
                }
            }
        }
    }

    // İsme göre prefab arayan yardımcı fonksiyon
    GameObject FindPrefabByName(string name)
    {
        // Easy Listesi
        foreach (var go in easyShapes) if (go.name == name) return go;
        // Medium Listesi
        foreach (var go in mediumShapes) if (go.name == name) return go;
        // Hard Listesi
        foreach (var go in hardShapes) if (go.name == name) return go;
        
        return null;
    }
    // -----------------------------------------
}