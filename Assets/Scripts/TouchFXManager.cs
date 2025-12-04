using UnityEngine;

public class TouchFXManager : MonoBehaviour
{
    public static TouchFXManager Instance;

    [Header("Prefab")]
    public GameObject clickEffectPrefab; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SpawnEffect();
        }
    }

    void SpawnEffect()
    {
        if (clickEffectPrefab == null) return;

        Vector3 mousePos = Input.mousePosition;
        // Kameraya uzaklık (Z derinliği)
        mousePos.z = 10f; 

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        
        // --- Z EKSENİNİ SIFIRLA ---
        worldPos.z = 0f; 

        Instantiate(clickEffectPrefab, worldPos, Quaternion.identity);
    }
}