using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VampireLike
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text survivedTimeText;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
        }

        private void OnEnable() => GameManager.OnGameOver += HandleGameOver;
        private void OnDisable() => GameManager.OnGameOver -= HandleGameOver;

        private void HandleGameOver()
        {
            if (panelRoot != null) panelRoot.SetActive(true);

            if (survivedTimeText != null && GameManager.Instance != null)
            {
                float t = GameManager.Instance.ElapsedTime;
                survivedTimeText.text = $"力尽きた…\n生存時間 {Mathf.FloorToInt(t / 60f):00}:{Mathf.FloorToInt(t % 60f):00}";
            }
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
