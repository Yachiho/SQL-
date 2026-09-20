using System;
using UnityEngine;

namespace VampireLike
{
    public enum GameState
    {
        Playing,
        Paused,
        LevelingUp,
        GameOver
    }

    /// <summary>
    /// Owns the survival timer and the overall run state. Weapons, spawners
    /// and UI all read GameManager.State instead of Time.timeScale checks
    /// scattered everywhere.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event Action<GameState> OnStateChanged;
        public static event Action<float> OnTimeUpdated;
        public static event Action OnGameOver;

        [SerializeField] private float runDurationSeconds = 20f * 60f; // survive-the-clock target

        public GameState State { get; private set; } = GameState.Playing;
        public float ElapsedTime { get; private set; }
        public float RunDurationSeconds => runDurationSeconds;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (State != GameState.Playing) return;

            ElapsedTime += Time.deltaTime;
            OnTimeUpdated?.Invoke(ElapsedTime);

            if (ElapsedTime >= runDurationSeconds)
            {
                Win();
            }
        }

        public void SetState(GameState newState)
        {
            if (State == newState) return;
            State = newState;

            Time.timeScale = newState is GameState.Paused or GameState.LevelingUp or GameState.GameOver
                ? 0f
                : 1f;

            OnStateChanged?.Invoke(newState);
        }

        public void PauseForLevelUp() => SetState(GameState.LevelingUp);

        public void ResumeAfterLevelUp() => SetState(GameState.Playing);

        public void GameOver()
        {
            SetState(GameState.GameOver);
            OnGameOver?.Invoke();
        }

        private void Win()
        {
            // Placeholder for a victory screen; treated as a special game-over for now.
            SetState(GameState.GameOver);
            OnGameOver?.Invoke();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
