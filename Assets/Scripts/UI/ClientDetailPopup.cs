using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SunbaseTest.Models;
using DG.Tweening;

namespace SunbaseTest.UI
{
    /// <summary>
    /// Popup component to display client details
    /// </summary>
    public class ClientDetailPopup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private TextMeshProUGUI addressText;
        [SerializeField] private TextMeshProUGUI managerStatusText;
        [SerializeField] private Button closeButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupRect;
        
        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;
        [SerializeField] private float overlayFadeDuration = 0.2f;
        
        // Background overlay
        [SerializeField] private Image backgroundOverlay;
        [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.5f);
        
        private bool isVisible = false;
        private Sequence currentAnimation;

        private void Awake()
        {
            // Initialize
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // Setup close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
            
            // Setup background overlay for click outside
            if (backgroundOverlay != null)
            {
                Button overlayButton = backgroundOverlay.GetComponent<Button>();
                if (overlayButton == null)
                {
                    overlayButton = backgroundOverlay.gameObject.AddComponent<Button>();
                    ColorBlock colors = overlayButton.colors;
                    colors.normalColor = Color.clear;
                    colors.highlightedColor = Color.clear;
                    colors.pressedColor = Color.clear;
                    colors.selectedColor = Color.clear;
                    colors.disabledColor = Color.clear;
                    overlayButton.colors = colors;
                }
                overlayButton.onClick.AddListener(Hide);
            }
            
            // Set initial state
            SetVisibleState(false, false);
        }

        private void OnDestroy()
        {
            // Clean up
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
            }
            
            if (backgroundOverlay != null)
            {
                Button overlayButton = backgroundOverlay.GetComponent<Button>();
                if (overlayButton != null)
                {
                    overlayButton.onClick.RemoveListener(Hide);
                }
            }
            
            // Kill any running animations
            currentAnimation?.Kill();
        }

        /// <summary>
        /// Shows the popup with client data
        /// </summary>
        public void Show(ClientData client)
        {
            // Update UI with client data
            if (nameText != null) nameText.text = "Name: " + (client.Name ?? client.Label); // Fall back to label if name not available
            if (pointsText != null) pointsText.text = "Points: " + client.Points.ToString();
            if (addressText != null) addressText.text = "Address: " + (client.Address ?? "No address available");
            if (managerStatusText != null) 
            {
                managerStatusText.text = client.IsManager ? "Manager" : "Non-Manager";
                managerStatusText.color = client.IsManager ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.6f, 0.2f, 0.2f);
            }
            
            // Show with animation
            SetVisibleState(true, true);
        }

        /// <summary>
        /// Hides the popup
        /// </summary>
        public void Hide()
        {
            // Hide with animation
            SetVisibleState(false, true);
        }

        /// <summary>
        /// Sets the popup visibility state with optional animation
        /// </summary>
        private void SetVisibleState(bool visible, bool animate)
        {
            isVisible = visible;
            
            // Kill any running animations
            currentAnimation?.Kill();
            
            if (animate)
            {
                currentAnimation = DOTween.Sequence();
                
                if (visible)
                {
                    // Setup before animation
                    gameObject.SetActive(true);
                    canvasGroup.alpha = 0f;
                    popupRect.localScale = Vector3.zero;
                    
                    // Background animation
                    if (backgroundOverlay != null)
                    {
                        backgroundOverlay.gameObject.SetActive(true);
                        backgroundOverlay.color = Color.clear;
                        currentAnimation.Join(backgroundOverlay.DOColor(overlayColor, overlayFadeDuration));
                    }
                    
                    // Popup animation
                    currentAnimation.Join(canvasGroup.DOFade(1f, animationDuration));
                    currentAnimation.Join(popupRect.DOScale(1f, animationDuration).SetEase(showEase));
                }
                else
                {
                    // Background animation
                    if (backgroundOverlay != null)
                    {
                        currentAnimation.Join(backgroundOverlay.DOColor(Color.clear, overlayFadeDuration));
                    }
                    
                    // Popup animation
                    currentAnimation.Join(canvasGroup.DOFade(0f, animationDuration));
                    currentAnimation.Join(popupRect.DOScale(0f, animationDuration).SetEase(hideEase));
                    
                    // Disable after animation
                    currentAnimation.OnComplete(() => {
                        gameObject.SetActive(false);
                        if (backgroundOverlay != null)
                        {
                            backgroundOverlay.gameObject.SetActive(false);
                        }
                    });
                }
            }
            else
            {
                // Set without animation
                gameObject.SetActive(visible);
                canvasGroup.alpha = visible ? 1f : 0f;
                popupRect.localScale = visible ? Vector3.one : Vector3.zero;
                
                if (backgroundOverlay != null)
                {
                    backgroundOverlay.gameObject.SetActive(visible);
                    backgroundOverlay.color = visible ? overlayColor : Color.clear;
                }
            }
        }
    }
} 