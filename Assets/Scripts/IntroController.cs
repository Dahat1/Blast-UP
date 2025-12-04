using UnityEngine;
using System.Collections;

public class IntroController : MonoBehaviour
{
    public float waitTime = 2.0f; // Logoda ne kadar dursun?

    void Start()
    {
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        // Biraz bekle (Logo görünsün)
        yield return new WaitForSeconds(waitTime);

        // Transition Manager varsa onu kullan, yoksa direkt geç
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            // Eğer TransitionManager sahnede yoksa düz yükle (Hata vermesin)
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}