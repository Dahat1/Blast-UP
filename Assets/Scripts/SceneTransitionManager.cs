using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Ayarlar")]
    public CanvasGroup fadeCanvasGroup; 
    public float fadeDuration = 0.3f; 

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if(GetComponentInChildren<Canvas>() != null) 
                GetComponentInChildren<Canvas>().enabled = false;
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // --- DEĞİŞİKLİK: Başlangıçtaki otomatik Fade kaldırıldı ---
        // Artık oyun açılınca GridManager'ın Intro animasyonu görünecek.
        // Perdeyi manuel olarak şeffaf ve pasif yapıyoruz.
        if (fadeCanvasGroup != null) 
        {
            fadeCanvasGroup.alpha = 0f; 
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    public void ReloadCurrentScene()
    {
        StartCoroutine(TransitionRoutine(SceneManager.GetActiveScene().name));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        if (fadeCanvasGroup != null) fadeCanvasGroup.blocksRaycasts = true;
        yield return StartCoroutine(Fade(0, 1)); // Karar

        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 1f;
        Time.timeScale = 1f; 
        
        SceneManager.LoadScene(sceneName);

        yield return null; 
        yield return null; 

        yield return StartCoroutine(Fade(1, 0)); // Açıl
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadeCanvasGroup != null) fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            float t = timer / fadeDuration;
            t = t * t * (3f - 2f * t);

            if (fadeCanvasGroup != null) 
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            
            yield return null;
        }
        
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = endAlpha;
            if (endAlpha == 0) fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}