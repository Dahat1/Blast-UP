using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance;

    [System.Serializable]
    public struct ColorPalette
    {
        public string name;             
        public Color backgroundColor;   
        public Color frameColor;        
        public Color gridColor;         
        public Color blockColor;        
        public Color explosionColor;    
        public Color boardFillColor;    
    }

    [Header("Palet Listesi")]
    public List<ColorPalette> palettes; 

    [Header("Mevcut Durum")]
    public ColorPalette currentPalette; 

    [Header("Referanslar (Oyun Alanı)")]
    public Camera mainCamera;
    public SpriteRenderer[] boardBorders; 
    public SpriteRenderer boardBacking;   

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

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferences();
        ApplyTheme(currentPalette);
    }

    void Start()
    {
        RefreshReferences();
        SetRandomTheme(); 
    }

    void RefreshReferences()
    {
        mainCamera = Camera.main;

        GameObject board = GameObject.Find("BoardBackground");
        if (board != null)
        {
            boardBacking = board.GetComponent<SpriteRenderer>();

            List<SpriteRenderer> bordersList = new List<SpriteRenderer>();
            SpriteRenderer[] allRenderers = board.GetComponentsInChildren<SpriteRenderer>();

            foreach (var sr in allRenderers)
            {
                if (sr != boardBacking)
                {
                    bordersList.Add(sr);
                }
            }
            boardBorders = bordersList.ToArray();
        }
    }

    public void SetRandomTheme()
    {
        if (palettes == null || palettes.Count == 0) return;

        ColorPalette newPalette = palettes[Random.Range(0, palettes.Count)];

        if (palettes.Count > 1 && newPalette.name == currentPalette.name)
        {
            SetRandomTheme();
            return;
        }

        ApplyTheme(newPalette);
    }

    public void ApplyTheme(ColorPalette palette)
    {
        currentPalette = palette;

        // 1. Oyun Alanı
        if (mainCamera != null) mainCamera.backgroundColor = palette.backgroundColor;

        if (boardBorders != null)
        {
            foreach (var border in boardBorders)
            {
                if(border != null) border.color = palette.frameColor;
            }
        }

        if (boardBacking != null) boardBacking.color = palette.boardFillColor;

        // 2. Grid ve Spawner
        if (GridManager.Instance != null) GridManager.Instance.UpdateGridColors(palette.gridColor); 
        if (ShapeSpawner.Instance != null) ShapeSpawner.Instance.UpdateSpawnedShapesColor(palette.blockColor);

        // 3. UI Genel
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUIColors(palette.blockColor, palette.explosionColor);
        }

        // 4. UI Efekt
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateFireColor(palette.explosionColor);
        }
        
        // 5. SAHNEDEKİ "SETTINGS COLOR ADAPTER"LARI BUL VE GÜNCELLE
        // Bu satır, sahnedeki tüm adaptörleri bulur ve renklerini güncellemelerini söyler.
        SettingsColorAdapter[] adapters = FindObjectsOfType<SettingsColorAdapter>();
        foreach(var adapter in adapters)
        {
            adapter.ApplyColors();
        }
    }
}