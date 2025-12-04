using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 
using TMPro; 
using System.Collections; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Referanslar")]
    public Camera mainCamera; 
    
    [Header("Paneller")]
    public GameObject settingsPanel;
    public GameObject confirmationPanel;

    [Header("Audio Ayarları")]
    public Toggle musicToggle;
    public Toggle soundToggle;
    public Toggle vibrationToggle;

    [Header("Tema Butonu Görselleri")]
    public Image themeButtonImage; 
    public Sprite iconSun;         
    public Sprite iconMoon;        

    // --- COMBO GÖRSELLERİ ---
    [Header("Combo UI")]
    public TextMeshProUGUI popupComboText; 
    
    // --- YENİ: POPUP SKOR YAZISI ---
    public TextMeshProUGUI popupScoreText; // <-- BUNU EDİTÖRDEN ATA (+450 yazısı)
    // -------------------------------
    
    // SKORUN YANINDAKİ ALEV
    public Image comboFireImage; 
    
    // --- GÜNCELLEME: NO MOVES LEFT KUTUSU ---
    [Header("Game Over UI")]
    public GameObject noMovesBannerObject; 
    // ---------------------------------------

    // --- COMBO YAZISI AYARLARI ---
    [Header("Combo Text Settings")]
    public Color[] comboUnderlayColors; 
    public Vector2 popupSafeArea = new Vector2(2.5f, 3.5f); 
    
    // --- HYPE ANİMASYON AYARLARI ---
    [Header("Hype Animation Settings")]
    public AnimationCurve elasticPopCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.4f, 1.2f), new Keyframe(1, 1)); 
    public float heroSlamDuration = 0.25f;
    public float slideSpeedNormal = 15f;  
    public float slideSpeedFast = 30f;    
    public float godShakeAmount = 15f;    

    // --- ALEV EFEKT AYARLARI ---
    [Header("Fire Effect Settings")]
    public float punchScaleAmount = 0.4f;   
    public float shakeRotationAmount = 15f; 
    public float impactDuration = 0.5f;     

    private enum ActionType { None, GoHome, Restart }
    private ActionType _pendingAction = ActionType.None;

    private Coroutine _comboPopupCoroutine;
    private Coroutine _scorePopupCoroutine; // Yeni: Skor animasyonu için
    private Coroutine _shakeCoroutine;
    private Coroutine _fireEffectCoroutine; 

    private Vector3 _originalCamPos; 
    private int _currentComboLevel = 0; 
    private int _lastComboLevel = 0;
    private Color _defaultComboColor; 
    private Color _currentFireColor = Color.white;

    private float _currentImpactTime = 0f;
    private Quaternion _baseFireRotation;
    private Material _baseFontMaterial;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) _originalCamPos = mainCamera.transform.position;
        
        if (AudioManager.Instance != null)
        {
            if (musicToggle != null) musicToggle.isOn = AudioManager.Instance.isMusicOn;
            if (soundToggle != null) soundToggle.isOn = AudioManager.Instance.isSoundOn;
            if (vibrationToggle != null) vibrationToggle.isOn = AudioManager.Instance.isVibrationOn;
        }

        if (popupComboText != null) 
        {
            _defaultComboColor = popupComboText.color; 
            if (popupComboText.fontSharedMaterial != null)
            {
                _baseFontMaterial = popupComboText.fontSharedMaterial;
            }
            popupComboText.gameObject.SetActive(false);
            popupComboText.alpha = 0;
        }
        
        // --- YENİ: Skor yazısını başlangıçta gizle ---
        if (popupScoreText != null)
        {
            popupScoreText.gameObject.SetActive(false);
            popupScoreText.alpha = 0;
        }
        // --------------------------------------------

        if (comboFireImage != null)
        {
            comboFireImage.gameObject.SetActive(false);
            _baseFireRotation = comboFireImage.transform.localRotation;
        }

        if (noMovesBannerObject != null)
        {
            noMovesBannerObject.SetActive(false);
        }
    }

    public void ShowNoMovesBanner()
    {
        if (noMovesBannerObject != null)
        {
            noMovesBannerObject.SetActive(true);
        }
    }

    public void HideNoMovesBanner()
    {
        if (noMovesBannerObject != null)
        {
            noMovesBannerObject.SetActive(false);
        }
    }

    public void UpdateFireColor(Color newColor)
    {
        _currentFireColor = newColor;
        if (comboFireImage != null && comboFireImage.gameObject.activeSelf)
        {
            comboFireImage.color = _currentFireColor;
        }
    }

    public void UpdateComboVisuals(int comboCount)
    {
        if (comboFireImage == null) return;

        if (comboCount > _lastComboLevel && comboCount >= 2)
        {
            _currentImpactTime = impactDuration;
        }
        
        _lastComboLevel = comboCount;
        _currentComboLevel = comboCount;

        if (comboCount < 2)
        {
            if (comboFireImage.gameObject.activeSelf)
            {
                if (_fireEffectCoroutine != null) StopCoroutine(_fireEffectCoroutine);
                comboFireImage.transform.localScale = Vector3.one;
                comboFireImage.transform.localRotation = _baseFireRotation;
                comboFireImage.gameObject.SetActive(false);
            }
            return;
        }

        if (!comboFireImage.gameObject.activeSelf)
        {
            comboFireImage.gameObject.SetActive(true);
            comboFireImage.color = _currentFireColor;
            _fireEffectCoroutine = StartCoroutine(FirePulseRoutine());
        }
    }

    private IEnumerator FirePulseRoutine()
    {
        float timer = 0f;
        
        while (true)
        {
            timer += Time.deltaTime;

            float comboProgress = Mathf.Clamp01((float)(_currentComboLevel - 2) / 13f);
            float pulseSpeed = Mathf.Lerp(5f, 35f, comboProgress);
            float smoothPulse = Mathf.Sin(timer * pulseSpeed) * 0.07f;

            float impactScaleAddon = 0f;
            float impactRotationZ = 0f;

            if (_currentImpactTime > 0)
            {
                _currentImpactTime -= Time.deltaTime;
                float t = _currentImpactTime / impactDuration;
                t = t * t; 
                impactScaleAddon = punchScaleAmount * t;
                impactRotationZ = Mathf.Sin(Time.time * 40f) * shakeRotationAmount * t;
            }

            float extraBaseScale = Mathf.Lerp(0f, 0.25f, comboProgress);
            Vector3 currentBaseScale = Vector3.one * (1f + extraBaseScale);

            float totalScaleFactor = 1f + smoothPulse + impactScaleAddon;
            comboFireImage.transform.localScale = currentBaseScale * totalScaleFactor;

            comboFireImage.transform.localRotation = _baseFireRotation * Quaternion.Euler(0, 0, impactRotationZ);
            comboFireImage.color = _currentFireColor;

            yield return null;
        }
    }

    public void TriggerScreenShake(float duration, float magnitude)
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        if (mainCamera == null) yield break;
        float elapsed = 0.0f;
        float strength = magnitude * 0.1f; 
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;
            mainCamera.transform.position = new Vector3(_originalCamPos.x + x, _originalCamPos.y + y, _originalCamPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = _originalCamPos;
    }

    // --- GÜNCELLENDİ: ARTIK PUANI DA ALIYOR ---
    public void ShowComboPopup(int comboCount, Vector3 worldPosition, int scoreEarned)
    {
        if (popupComboText == null) return;

        if (_comboPopupCoroutine != null) StopCoroutine(_comboPopupCoroutine);
        // Skor coroutine'ini de durdur
        if (_scorePopupCoroutine != null && popupScoreText != null) StopCoroutine(_scorePopupCoroutine);

        float clampedX = Mathf.Clamp(worldPosition.x, -popupSafeArea.x, popupSafeArea.x);
        float clampedY = Mathf.Clamp(worldPosition.y, -popupSafeArea.y, popupSafeArea.y);
        Vector3 comboPos = new Vector3(clampedX, clampedY, 0);
        
        popupComboText.rectTransform.position = comboPos; 
        popupComboText.text = $"<color=white><size=80%>Combo</size></color> <color=#EDC22E><size=150%>{comboCount}</size></color>";
        
        if (comboUnderlayColors != null && comboUnderlayColors.Length > 0 && _baseFontMaterial != null)
        {
            Color nextColor = comboUnderlayColors[(comboCount - 1) % comboUnderlayColors.Length];
            Material newMaterial = new Material(_baseFontMaterial);
            newMaterial.EnableKeyword("UNDERLAY_ON");
            newMaterial.SetColor("_UnderlayColor", nextColor);
            popupComboText.fontMaterial = newMaterial;
        }

        popupComboText.gameObject.SetActive(true);
        popupComboText.alpha = 1f;
        popupComboText.transform.localRotation = Quaternion.identity; 

        _comboPopupCoroutine = StartCoroutine(AnimateComboHype(comboCount));

        // --- YENİ: SKOR YAZISINI GÖSTERME ---
        if (popupScoreText != null)
        {
            // Konum Belirleme: Patlama soldaysa sağa, sağdaysa sola koy.
            float offsetDir = (worldPosition.x < 0) ? 1f : -1f;
            Vector3 scorePos = comboPos + new Vector3(offsetDir * 2.5f, -0.5f, 0); // Yana ve biraz aşağı

            // Ekran dışına çıkmasın diye onu da clamp yapalım
            float sX = Mathf.Clamp(scorePos.x, -popupSafeArea.x, popupSafeArea.x);
            float sY = Mathf.Clamp(scorePos.y, -popupSafeArea.y, popupSafeArea.y);
            popupScoreText.rectTransform.position = new Vector3(sX, sY, 0);

            popupScoreText.text = "+" + scoreEarned;
            popupScoreText.gameObject.SetActive(true);
            popupScoreText.alpha = 1f;
            
            _scorePopupCoroutine = StartCoroutine(AnimateScorePopup());
        }
        // ------------------------------------
    }

    // --- YENİ: SKOR ANİMASYONU (Yavaş sönme) ---
    private IEnumerator AnimateScorePopup()
    {
        float timer = 0f;
        float duration = 2.0f; // Combo yazısından daha uzun kalsın (2 saniye)

        Vector3 startPos = popupScoreText.rectTransform.position;
        Vector3 endPos = startPos + new Vector3(0, 1.0f, 0); // Yavaşça yukarı

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Yumuşakça yukarı kay
            popupScoreText.rectTransform.position = Vector3.Lerp(startPos, endPos, t);

            // Son %30'luk kısımda sönmeye başla
            if (t > 0.7f)
            {
                float fadeT = (t - 0.7f) / 0.3f;
                popupScoreText.alpha = Mathf.Lerp(1f, 0f, fadeT);
            }

            yield return null;
        }

        popupScoreText.gameObject.SetActive(false);
    }
    // -------------------------------------------

    private IEnumerator AnimateComboHype(int comboCount)
    {
        if (comboCount < 5)
        {
            float timer = 0f;
            float duration = 1.5f; 
            Vector3 startPos = popupComboText.rectTransform.position;
            Vector3 endPos = startPos + new Vector3(0, 1.0f, 0); 

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                
                float curveT = Mathf.Clamp01(t * 1.5f); 
                float scale = elasticPopCurve.Evaluate(curveT);
                popupComboText.transform.localScale = Vector3.one * scale;

                popupComboText.rectTransform.position = Vector3.Lerp(startPos, endPos, t);
                
                if (t > 0.8f) 
                {
                    float fadeT = (t - 0.8f) / 0.2f;
                    popupComboText.alpha = Mathf.Lerp(1f, 0f, fadeT);
                }

                yield return null;
            }
        }
        else
        {
            bool isGodMode = (comboCount >= 10);
            float slamTimer = 0f;
            Vector3 normalScale = Vector3.one * (isGodMode ? 1.5f : 1.2f); 
            Vector3 bigScale = normalScale * 3.0f; 

            while (slamTimer < heroSlamDuration)
            {
                slamTimer += Time.deltaTime;
                float t = slamTimer / heroSlamDuration;
                popupComboText.transform.localScale = Vector3.Lerp(bigScale, normalScale, t * t);
                
                if (isGodMode)
                {
                    float zShake = Random.Range(-godShakeAmount, godShakeAmount);
                    popupComboText.transform.localRotation = Quaternion.Euler(0, 0, zShake);
                }
                yield return null;
            }
            
            popupComboText.transform.localScale = normalScale;
            if(!isGodMode) popupComboText.transform.localRotation = Quaternion.identity;

            float waitDuration = 1.0f; 
            float waitTimer = 0f;
            while(waitTimer < waitDuration)
            {
                waitTimer += Time.deltaTime;
                if (isGodMode)
                {
                    float wobble = Mathf.Sin(Time.time * 20f) * (godShakeAmount * 0.5f);
                    popupComboText.transform.localRotation = Quaternion.Euler(0, 0, wobble);
                }
                yield return null;
            }

            float currentX = popupComboText.rectTransform.position.x;
            float direction = (currentX > 0) ? -1f : 1f; 
            float speed = isGodMode ? slideSpeedFast : slideSpeedNormal;
            
            float exitTimer = 0f;
            while (exitTimer < 0.5f) 
            {
                exitTimer += Time.deltaTime;
                popupComboText.rectTransform.position += new Vector3(direction * speed * Time.deltaTime, 0, 0);
                if(isGodMode) popupComboText.transform.Rotate(0, 0, -direction * 300f * Time.deltaTime);
                if (Mathf.Abs(popupComboText.rectTransform.position.x) > 6f) break;
                yield return null;
            }
        }

        popupComboText.gameObject.SetActive(false);
        popupComboText.transform.localRotation = Quaternion.identity; 
    }

    public void OnThemeButtonClick() { if (ThemeManager.Instance != null) ThemeManager.Instance.SetRandomTheme(); }
    public void OnMusicToggle(bool isOn) { if (AudioManager.Instance != null) AudioManager.Instance.ToggleMusic(isOn); }
    public void OnSoundToggle(bool isOn) { if (AudioManager.Instance != null) AudioManager.Instance.ToggleSound(isOn); }
    public void OnVibrationToggle(bool isOn) { if (AudioManager.Instance != null) AudioManager.Instance.ToggleVibration(isOn); }

    public void ToggleSettingsPanel()
    {
        bool isActive = settingsPanel.activeSelf;
        bool newState = !isActive;
        settingsPanel.SetActive(newState);
        if (isActive) confirmationPanel.SetActive(false); 
        if (GameManager.Instance != null) GameManager.Instance.isPaused = newState;
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.isPaused = false;
    }

    public void OnHomeClicked() { _pendingAction = ActionType.GoHome; OpenConfirmationPanel(); }
    public void OnRestartClicked() { _pendingAction = ActionType.Restart; OpenConfirmationPanel(); }
    
    void OpenConfirmationPanel() 
    { 
        if (confirmationPanel != null) confirmationPanel.SetActive(true); 
        if (GameManager.Instance != null) GameManager.Instance.isPaused = true;
    }

    public void OnConfirmationYes()
    {
        if (_pendingAction == ActionType.GoHome)
        {
            if (confirmationPanel != null) confirmationPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            if (GridManager.Instance != null) GridManager.Instance.PlayOutroAndLoadMenu();
            else SceneManager.LoadScene("MainMenu");
        }
        else if (_pendingAction == ActionType.Restart)
        {
            GameManager.Instance.RestartGame();
            if (confirmationPanel != null) confirmationPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        _pendingAction = ActionType.None;
    }

    public void OnConfirmationNo()
    {
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        _pendingAction = ActionType.None;
        if (settingsPanel != null && !settingsPanel.activeSelf)
        {
            if (GameManager.Instance != null) GameManager.Instance.isPaused = false;
        }
    }

    public bool IsInputBlocked { get { return (settingsPanel != null && settingsPanel.activeSelf) || (confirmationPanel != null && confirmationPanel.activeSelf); } }
}