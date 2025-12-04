using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource; // Arka plan müziği için
    public AudioSource sfxSource;   // Ses efektleri (Patlama vs.) için

    // Ayarları kaydetmek için anahtarlar
    private const string MUSIC_KEY = "MusicSetting";
    private const string SOUND_KEY = "SoundSetting";
    private const string VIB_KEY = "VibSetting";

    // Diğer scriptlerden erişmek için değişkenler
    public bool isMusicOn;
    public bool isSoundOn;
    public bool isVibrationOn;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            LoadSettings(); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    void LoadSettings()
    {
        isMusicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        isSoundOn = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
        isVibrationOn = PlayerPrefs.GetInt(VIB_KEY, 1) == 1;

        UpdateMusicState();
    }

    public void ToggleMusic(bool isOn)
    {
        isMusicOn = isOn;
        PlayerPrefs.SetInt(MUSIC_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateMusicState();
    }

    public void ToggleSound(bool isOn)
    {
        isSoundOn = isOn;
        PlayerPrefs.SetInt(SOUND_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleVibration(bool isOn)
    {
        isVibrationOn = isOn;
        PlayerPrefs.SetInt(VIB_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void UpdateMusicState()
    {
        if (musicSource == null) return;
        musicSource.mute = !isMusicOn;
        
        if (isMusicOn && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    // --- DIŞARIDAN ÇAĞIRILACAK FONKSİYONLAR ---

    public void PlaySFX(AudioClip clip)
    {
        if (isSoundOn && clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // --- YENİLENMİŞ TİTREŞİM SİSTEMİ ---

    // 1. HAFİF TİTREŞİM (Blok Yerleştirme)
    public void VibrateLight()
    {
        if (!isVibrationOn) return;
        VibrateCustom(40); // 40 milisaniye
    }

    // 2. GÜÇLÜ TİTREŞİM (Satır Kırma)
    public void VibrateHeavy()
    {
        if (!isVibrationOn) return;
        VibrateCustom(90); // 90 milisaniye
    }

    // Platforma göre titreşim yapan yardımcı fonksiyon
    private void VibrateCustom(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android için milisaniye ayarlı titreşim (JNI Kullanımı)
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                    {
                        if (vibrator.Call<bool>("hasVibrator"))
                        {
                            // Android API 26+ için VibrationEffect kullanılabilir ama bu yöntem eski cihazlarda da çalışır
                            vibrator.Call("vibrate", milliseconds);
                        }
                    }
                }
            }
        }
        catch
        {
            // Hata olursa standart titreşimi kullan
            Handheld.Vibrate();
        }
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS için Taptic Engine
        if (milliseconds < 50)
            UnityEngine.iOS.Device.GenerateFeedback(UnityEngine.iOS.HapticFeedbackStyle.Light);
        else
            UnityEngine.iOS.Device.GenerateFeedback(UnityEngine.iOS.HapticFeedbackStyle.Heavy);
#else
        // Bilgisayarda (Editörde) titreşim olmaz, log düşelim
        // Debug.Log("Titreşim (Milisaniye): " + milliseconds);
#endif
    }
}