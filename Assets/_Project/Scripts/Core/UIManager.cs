// File: Assets/_Project/Scripts/Core/UIManager.cs
// Fix: Added an OnDestroy method to unsubscribe from static events,
// preventing memory leaks and errors in the editor.

using UnityEngine;
using TMPro;

namespace Platformer
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Text Elements")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI homeScoreText;
        [SerializeField] private TextMeshProUGUI awayScoreText;
        [SerializeField] private TextMeshProUGUI gameStateText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            GameStateManager.OnTimerUpdated += UpdateTimer;
            GameStateManager.OnScoreUpdated += UpdateScore;
            GameStateManager.OnGameStateChanged += UpdateGameState;
        }

        private void OnDisable()
        {
            // OnDisable is still good practice for enabled/disabled toggling
            UnsubscribeEvents();
        }

        // THE FIX IS HERE: This method ensures that when the UIManager object is
        // destroyed, it properly cleans up its event subscriptions.
        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void UnsubscribeEvents()
        {
            GameStateManager.OnTimerUpdated -= UpdateTimer;
            GameStateManager.OnScoreUpdated -= UpdateScore;
            GameStateManager.OnGameStateChanged -= UpdateGameState;
        }

        private void UpdateTimer(float time)
        {
            if (timerText == null) return;
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private void UpdateScore(int homeScore, int awayScore)
        {
            if (homeScoreText != null) homeScoreText.text = homeScore.ToString();
            if (awayScoreText != null) awayScoreText.text = awayScore.ToString();
        }

        private void UpdateGameState(GameStateManager.GameState state)
        {
            if (gameStateText != null) gameStateText.text = state.ToString();
        }
    }
}