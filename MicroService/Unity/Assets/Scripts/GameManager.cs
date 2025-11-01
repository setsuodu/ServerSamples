using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Text scoreText;
    public Slider healthSlider;
    public GameObject gameOverPanel;

    private int score = 0;
    private int playerHealth = 3;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
        gameOverPanel.SetActive(false);
    }

    public void AddScore(int value)
    {
        score += value;
        UpdateUI();
    }

    public void PlayerTakeDamage()
    {
        playerHealth--;
        UpdateUI();
        if (playerHealth <= 0) GameOver();
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        healthSlider.value = playerHealth;
    }

    void GameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}