using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro kullanacağız

public class MainMenuManager : MonoBehaviour
{
    [Header("Leaderboard UI")]
    public GameObject leaderboardPanel;
    public TextMeshProUGUI highScoreText; // Paneldeki sayı texti

    public void PlayGame()
    {
        SceneManager.LoadScene("Play"); // Senin sahne adın "Play" idi
    }

    public void OpenLeaderboard()
    {
        // Paneli aç
        leaderboardPanel.SetActive(true);

        // Hafızadan skoru çek ve yazdır
        int bestScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = bestScore.ToString();
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }
}