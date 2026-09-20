using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider xpBar;
        [SerializeField] private Text levelText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text idolNameText;

        private void OnEnable()
        {
            PlayerHealth.OnHealthChanged += HandleHealthChanged;
            PlayerLeveling.OnXpChanged += HandleXpChanged;
            GameManager.OnTimeUpdated += HandleTimeUpdated;
            IdolProfile.OnIdolAssigned += HandleIdolAssigned;
        }

        private void OnDisable()
        {
            PlayerHealth.OnHealthChanged -= HandleHealthChanged;
            PlayerLeveling.OnXpChanged -= HandleXpChanged;
            GameManager.OnTimeUpdated -= HandleTimeUpdated;
            IdolProfile.OnIdolAssigned -= HandleIdolAssigned;
        }

        private void HandleIdolAssigned(IdolData idol)
        {
            if (idolNameText != null && idol != null)
            {
                idolNameText.text = idol.idolName;
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (healthBar == null) return;
            healthBar.maxValue = max;
            healthBar.value = current;
        }

        private void HandleXpChanged(int level, float currentXp, float xpToNextLevel)
        {
            if (xpBar != null)
            {
                xpBar.maxValue = xpToNextLevel;
                xpBar.value = currentXp;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv. {level}";
            }
        }

        private void HandleTimeUpdated(float elapsedSeconds)
        {
            if (timerText == null) return;
            int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
            int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
