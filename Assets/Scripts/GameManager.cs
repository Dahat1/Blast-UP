using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro; 
using System.Collections; 
using System.Collections.Generic; // <-- BU EKSİKTİ, EKLENDİ!
using System.IO; // Dosya işlemleri için gerekli

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Standart Game Over Paneli")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;     
    public TextMeshProUGUI finalBestScoreText; 

    // --- YENİ: BEST SCORE GAME OVER PANELİ ---
    [Header("New Best Score Game Over Paneli")]
    public GameObject newBestScorePanel;      
    public TextMeshProUGUI newRecordScoreText; 
    // -----------------------------------------

    [Header("UI Textleri")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;

    [Header("UI Görselleri")]
    public Image scoreBackgroundImage; 

    [Header("Best Score Animasyonu")]
    public GameObject bestScoreNotificationPrefab; 
    public Transform notificationCanvasParent;     
    private bool _hasShownBestScoreAnim = false;   

    public bool isGameOver = false;
    public bool isPaused = false;
    
    [HideInInspector] public bool isIntroPlaying = true;

    private int _currentScore = 0;
    private int _bestScore = 0;

    private float _displayedScore = 0;
    private float _displayedBestScore = 0;
    
    private bool _isNewRecordSet = false; 

    private Coroutine _scoreRoutine;
    private Coroutine _bestScoreRoutine;

    public int CurrentScore => _currentScore;

    private int _comboCount = 0;       
    private int _movesWithoutClear = 0; 
    private const int MOVES_TO_RESET = 4; 

    [HideInInspector] public Vector3 LastShapePosition;

    void Awake()
    {
        Instance = this;
        Screen.orientation = ScreenOrientation.Portrait;
    }

    void Start()
    {
        _bestScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // --- KAYIT KONTROLÜ ---
        if (HasSaveFile())
        {
            LoadGame();
        }
        else
        {
            // Normal Başlangıç
            _displayedScore = 0;
            _displayedBestScore = _bestScore;
            if (scoreText != null) scoreText.text = "0";
            if (bestScoreText != null) bestScoreText.text = _bestScore.ToString();
            
            _hasShownBestScoreAnim = false;
            _isNewRecordSet = false; 

            if (newBestScorePanel != null) newBestScorePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            if(UIManager.Instance != null)
                UIManager.Instance.UpdateComboVisuals(_comboCount);
            
            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.SetRandomTheme();
                UpdateUIColors(ThemeManager.Instance.currentPalette.blockColor, ThemeManager.Instance.currentPalette.explosionColor);
            }
        }
    }

    // --- SAVE / LOAD SİSTEMİ ---
    [System.Serializable]
    public class GameSaveData
    {
        public int score;
        public int combo;
        public int themeIndex;
        public List<bool> gridData; 
        public List<string> currentShapeNames; 
    }

    string SavePath => Application.persistentDataPath + "/savegame.json";

    bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    public void SaveGame()
    {
        if (isGameOver) return; 

        GameSaveData data = new GameSaveData();
        data.score = _currentScore;
        data.combo = _comboCount;
        
        if (ThemeManager.Instance != null) 
            data.themeIndex = ThemeManager.Instance.palettes.IndexOf(ThemeManager.Instance.currentPalette);

        if (GridManager.Instance != null) 
            data.gridData = GridManager.Instance.GetGridData();

        if (ShapeSpawner.Instance != null) 
            data.currentShapeNames = ShapeSpawner.Instance.GetShapeNames();

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(SavePath, json);
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        _currentScore = data.score;
        _comboCount = data.combo;
        
        _displayedScore = _currentScore;
        _displayedBestScore = _bestScore;
        if (scoreText != null) scoreText.text = _currentScore.ToString();
        if (bestScoreText != null) bestScoreText.text = _bestScore.ToString();

        if (ThemeManager.Instance != null && data.themeIndex >= 0 && data.themeIndex < ThemeManager.Instance.palettes.Count)
        {
            ThemeManager.Instance.ApplyTheme(ThemeManager.Instance.palettes[data.themeIndex]);
        }

        if (GridManager.Instance != null)
        {
            isIntroPlaying = false; 
            GridManager.Instance.LoadGridFromData(data.gridData);
        }

        if (ShapeSpawner.Instance != null)
        {
            ShapeSpawner.Instance.LoadShapesFromData(data.currentShapeNames);
        }

        if(UIManager.Instance != null) UIManager.Instance.UpdateComboVisuals(_comboCount);
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
    // ---------------------------

    public void UpdateUIColors(Color textColor, Color imageColor)
    {
        if (scoreText != null) scoreText.color = textColor;
        if (scoreBackgroundImage != null) scoreBackgroundImage.color = imageColor;
    }

    public void StartActualGame()
    {
        Debug.Log("Intro bitti, oyun başlıyor!");
        isIntroPlaying = false;
        if (ShapeSpawner.Instance != null) ShapeSpawner.Instance.SpawnNewShapes();
    }

    public void ProcessTurn(int linesCleared, bool isAllClear, Vector3 centerPos)
    {
        if (isGameOver) return;

        if (linesCleared > 0)
        {
            _movesWithoutClear = 0; 
            _comboCount++;          

            float baseScore = linesCleared * 100f;
            float multiBonus = (linesCleared > 1) ? (linesCleared - 1) * 50f : 0f;

            float rawTurnScore = baseScore + multiBonus;
            float comboPercent = 0.30f + ((_comboCount - 1) * 0.10f);
            float finalTurnScore = rawTurnScore + (rawTurnScore * comboPercent);

            if (isAllClear)
            {
                Debug.Log("TAHTA TERTEMİZ! TEMA DEĞİŞİYOR!");
                finalTurnScore *= 1.5f; 
                if (ThemeManager.Instance != null) ThemeManager.Instance.SetRandomTheme();
            }

            int pointsEarned = Mathf.RoundToInt(finalTurnScore);
            AddScore(pointsEarned);

            if (UIManager.Instance != null)
            {
                if (_comboCount > 1) 
                {
                    UIManager.Instance.ShowComboPopup(_comboCount, centerPos, pointsEarned);
                }

                float shakeMagnitude = Mathf.Min(_comboCount, 10) * 0.5f; 
                if (_comboCount == 1) shakeMagnitude = 0.5f;
                UIManager.Instance.TriggerScreenShake(0.2f, shakeMagnitude);
            }
        }
        else
        {
             _movesWithoutClear++;
             if (_movesWithoutClear >= MOVES_TO_RESET) 
             {
                 _comboCount = 0;
                 _movesWithoutClear = 0;
             }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateComboVisuals(_comboCount);
        }
        
        SaveGame(); // Her hamlede kaydet
    }

    public void AddScore(int amount)
    {
        _currentScore += amount;

        if (_scoreRoutine != null) StopCoroutine(_scoreRoutine);
        _scoreRoutine = StartCoroutine(AnimateScoreChange(scoreText, _currentScore, false));

        if (_currentScore > _bestScore)
        {
            _bestScore = _currentScore;
            PlayerPrefs.SetInt("HighScore", _bestScore);
            PlayerPrefs.Save();

            _isNewRecordSet = true; 

            if (_bestScoreRoutine != null) StopCoroutine(_bestScoreRoutine);
            _bestScoreRoutine = StartCoroutine(AnimateScoreChange(bestScoreText, _bestScore, true));

            if (!_hasShownBestScoreAnim && _currentScore > 0)
            {
                if (bestScoreNotificationPrefab != null && notificationCanvasParent != null)
                {
                     GameObject notif = Instantiate(bestScoreNotificationPrefab, notificationCanvasParent);
                     RectTransform rect = notif.GetComponent<RectTransform>();
                     if(rect!=null) rect.anchoredPosition = new Vector2(0, 150);
                }
                _hasShownBestScoreAnim = true; 
            }
        }
    }

    private IEnumerator AnimateScoreChange(TextMeshProUGUI targetText, int targetValue, bool isBestScore)
    {
        float startValue = isBestScore ? _displayedBestScore : _displayedScore;
        float timer = 0f;
        float duration = 0.5f; 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float smoothT = t * t * (3f - 2f * t);
            float animatedValue = Mathf.Lerp(startValue, targetValue, smoothT);
            
            if (isBestScore) _displayedBestScore = animatedValue;
            else _displayedScore = animatedValue;

            if (targetText != null) targetText.text = Mathf.RoundToInt(animatedValue).ToString();
            yield return null;
        }

        if (isBestScore) _displayedBestScore = targetValue;
        else _displayedScore = targetValue;

        if (targetText != null) targetText.text = targetValue.ToString();
    }

    private IEnumerator CountUpToTarget(TextMeshProUGUI targetText, int targetValue)
    {
        if (targetText == null) yield break;
        float timer = 0f;
        float duration = 1.5f; 
        float startValue = 0f; 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = 1f - Mathf.Pow(1f - t, 5f); 

            float val = Mathf.Lerp(startValue, targetValue, t);
            targetText.text = Mathf.RoundToInt(val).ToString();
            yield return null;
        }
        targetText.text = targetValue.ToString();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        DeleteSaveFile(); // Oyun bittiği için kaydı sil

        if (UIManager.Instance != null) UIManager.Instance.ShowNoMovesBanner();

        if (GridManager.Instance != null) GridManager.Instance.PlayGameOverFillAnimation();
        else ShowGameOverPanel();
    }

    public void ShowGameOverPanel()
    {
        if (_isNewRecordSet && newBestScorePanel != null) StartCoroutine(AnimatePanelRoutine(newBestScorePanel, true));
        else StartCoroutine(AnimatePanelRoutine(gameOverPanel, false));
    }

    private IEnumerator AnimatePanelRoutine(GameObject panelToShow, bool isNewRecordPanel)
    {
        if (UIManager.Instance != null) UIManager.Instance.HideNoMovesBanner();

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            panelToShow.transform.localScale = Vector3.zero;

            float timer = 0f;
            float duration = 0.5f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                float scale = 0f;
                if (t < 0.8f) scale = Mathf.Lerp(0f, 1.1f, t / 0.8f); 
                else scale = Mathf.Lerp(1.1f, 1.0f, (t - 0.8f) / 0.2f);

                panelToShow.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            
            panelToShow.transform.localScale = Vector3.one;

            if (isNewRecordPanel)
            {
                if (newRecordScoreText != null) StartCoroutine(CountUpToTarget(newRecordScoreText, _currentScore));
            }
            else
            {
                if (finalScoreText != null) StartCoroutine(CountUpToTarget(finalScoreText, _currentScore));
                if (finalBestScoreText != null) StartCoroutine(CountUpToTarget(finalBestScoreText, _bestScore)); 
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1; 
        DeleteSaveFile(); // Restart yapınca da silinsin
        
        if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.ReloadCurrentScene();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}