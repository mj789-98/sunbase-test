using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SunbaseTest.Models;
using DG.Tweening;

namespace SunbaseTest.UI
{
    /// <summary>
    /// UI component for individual client list items
    /// </summary>
    public class ClientListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button button;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(0.9f, 0.9f, 1f);
        
        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutQuad;

        private ClientData clientData;
        private bool isSelected;

        /// <summary>
        /// Event triggered when this client list item is clicked
        /// </summary>
        public System.Action<ClientData> OnItemClicked;

        private void Awake()
        {
            // Attach click handler to button
            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            // Clean up event listener
            button.onClick.RemoveListener(HandleClick);
        }

        /// <summary>
        /// Sets up the list item with client data
        /// </summary>
        public void Setup(ClientData clientData)
        {
            this.clientData = clientData;
            labelText.text = clientData.Label;
            pointsText.text = clientData.Points.ToString();
            
            // Reset selection state
            SetSelected(false, false);
        }

        /// <summary>
        /// Sets the selection state of this item with optional animation
        /// </summary>
        public void SetSelected(bool selected, bool animate = true)
        {
            isSelected = selected;
            Color targetColor = selected ? selectedColor : normalColor;
            
            if (animate)
            {
                backgroundImage.DOColor(targetColor, animationDuration).SetEase(animationEase);
                
                if (selected)
                {
                    transform.DOScale(1.05f, animationDuration).SetEase(animationEase);
                }
                else
                {
                    transform.DOScale(1f, animationDuration).SetEase(animationEase);
                }
            }
            else
            {
                backgroundImage.color = targetColor;
                transform.localScale = selected ? new Vector3(1.05f, 1.05f, 1.05f) : Vector3.one;
            }
        }

        /// <summary>
        /// Animates the item's entry into the list
        /// </summary>
        public void AnimateEntry(float delay)
        {
            // Set initial state
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
            
            // Animate entry
            canvasGroup.DOFade(1f, animationDuration).SetDelay(delay).SetEase(animationEase);
            transform.DOScale(1f, animationDuration).SetDelay(delay).SetEase(Ease.OutBack);
        }

        private void HandleClick()
        {
            OnItemClicked?.Invoke(clientData);
        }
        
        /// <summary>
        /// Gets the client data associated with this list item
        /// </summary>
        public ClientData GetClientData()
        {
            return clientData;
        }
    }
} 