using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace SunbaseTest.Game
{
    /// <summary>
    /// Game manager for the Circle Interaction Game
    /// </summary>
    public class CircleGameManager : MonoBehaviour
    {
        [Header("Game Components")]
        [SerializeField] private CircleGenerator circleGenerator;
        [SerializeField] private LineDrawer lineDrawer;
        
        [Header("UI Components")]
        [SerializeField] private Button restartButton;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private GameObject gameOverPanel;
        
        [Header("Game Settings")]
        [SerializeField] private int pointsPerCircle = 10;
        [SerializeField] private float gameOverDelay = 1f;
        
        private int currentScore = 0;
        private int totalCircles = 0;
        private int destroyedCircles = 0;

        private void Awake()
        {
            // Find components if not assigned
            if (circleGenerator == null)
            {
                circleGenerator = FindObjectOfType<CircleGenerator>();
            }
            
            if (lineDrawer == null)
            {
                lineDrawer = FindObjectOfType<LineDrawer>();
            }
            
            // Setup event listeners
            if (lineDrawer != null)
            {
                lineDrawer.OnCirclesHit += HandleCirclesHit;
            }
            
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
            
            // Hide game over panel initially
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void Start()
        {
            // Start the game
            StartGame();
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (lineDrawer != null)
            {
                lineDrawer.OnCirclesHit -= HandleCirclesHit;
            }
            
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartGame);
            }
        }

        /// <summary>
        /// Starts a new game
        /// </summary>
        private void StartGame()
        {
            // Reset game state
            currentScore = 0;
            destroyedCircles = 0;
            
            // Generate circles
            if (circleGenerator != null)
            {
                circleGenerator.GenerateCircles();
                totalCircles = circleGenerator.GetActiveCircles().Count;
            }
            
            // Update UI
            UpdateScore();
            
            // Hide game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Restarts the game
        /// </summary>
        private void RestartGame()
        {
            StartGame();
        }

        /// <summary>
        /// Handles circles hit by the drawn line
        /// </summary>
        private void HandleCirclesHit(List<GameObject> hitCircles)
        {
            if (hitCircles != null && hitCircles.Count > 0)
            {
                int hitCount = 0;
                
                // Remove hit circles
                foreach (var circle in hitCircles)
                {
                    if (circle != null)
                    {
                        // Use try-catch to ensure we continue processing even if one circle fails
                        try
                        {
                            // Only count this hit if the circle is successfully removed
                            if (circleGenerator.RemoveCircle(circle))
                            {
                                // Update score and count
                                currentScore += pointsPerCircle;
                                hitCount++;
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Error removing circle: {e.Message}");
                        }
                    }
                }
                
                // Only update if we actually hit circles
                if (hitCount > 0)
                {
                    destroyedCircles += hitCount;
                    
                    // Update UI
                    UpdateScore();
                    
                    // Check for game over
                    CheckGameOver();
                }
            }
        }

        /// <summary>
        /// Updates the score display
        /// </summary>
        private void UpdateScore()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {currentScore}";
                
                // Animate score text
                scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
            }
        }

        /// <summary>
        /// Checks if all circles have been destroyed
        /// </summary>
        private void CheckGameOver()
        {
            if (destroyedCircles >= totalCircles)
            {
                // Game over with delay
                DOVirtual.DelayedCall(gameOverDelay, () => {
                    ShowGameOver();
                });
            }
        }

        /// <summary>
        /// Shows the game over screen
        /// </summary>
        private void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                
                // Animate panel
                CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
                }
                
                canvasGroup.alpha = 0f;
                gameOverPanel.transform.localScale = Vector3.zero;
                
                // Animation sequence
                Sequence gameOverSequence = DOTween.Sequence();
                gameOverSequence.Append(canvasGroup.DOFade(1f, 0.5f));
                gameOverSequence.Join(gameOverPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            }
        }
    }
} 