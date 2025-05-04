using UnityEngine;
using DG.Tweening;

namespace SunbaseTest.Game
{
    /// <summary>
    /// Component for circle game objects with visual feedback
    /// </summary>
    public class Circle : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D circleCollider;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 1f);
        [SerializeField] private Color hitColor = new Color(1f, 0.5f, 0.5f);
        
        [Header("Animation")]
        [SerializeField] private float pulseDuration = 0.5f;
        [SerializeField] private float pulseScale = 1.2f;

        private bool isHit = false;
        private Sequence pulseSequence;
        private Vector3 originalScale;
        private bool hasInitializedScale = false;

        private void Awake()
        {
            // Find components if not assigned
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            if (circleCollider == null)
            {
                circleCollider = GetComponent<Collider2D>();
            }
            
            // Set initial color
            if (spriteRenderer != null)
            {
                spriteRenderer.color = defaultColor;
            }
        }
        
        /// <summary>
        /// Called by external systems once the circle's final scale is set
        /// </summary>
        public void InitializeScale()
        {
            if (!hasInitializedScale)
            {
                originalScale = transform.localScale;
                hasInitializedScale = true;
            }
        }

        private void OnMouseEnter()
        {
            // Ensure scale is initialized
            if (!hasInitializedScale)
            {
                InitializeScale();
            }
            
            // Change color on hover
            if (!isHit && spriteRenderer != null)
            {
                spriteRenderer.DOColor(hoverColor, 0.2f);
                
                // Pulse effect on hover
                StartPulseAnimation();
            }
        }

        private void OnMouseExit()
        {
            // Ensure scale is initialized
            if (!hasInitializedScale)
            {
                InitializeScale();
            }
            
            // Reset color on mouse exit
            if (!isHit && spriteRenderer != null)
            {
                spriteRenderer.DOColor(defaultColor, 0.2f);
                
                // Stop pulse animation
                StopPulseAnimation();
            }
        }

        /// <summary>
        /// Called when the circle is hit by a line
        /// </summary>
        public void OnHit()
        {
            if (!isHit)
            {
                isHit = true;
                
                // Ensure scale is initialized
                if (!hasInitializedScale)
                {
                    InitializeScale();
                }
                
                // Visual feedback
                if (spriteRenderer != null)
                {
                    spriteRenderer.DOColor(hitColor, 0.2f);
                }
                
                // Stop pulse animation
                StopPulseAnimation();
                
                // Flash effect using original scale as reference
                transform.DOPunchScale(originalScale * 0.3f, 0.3f, 5, 0.5f);
            }
        }

        /// <summary>
        /// Starts a pulsing animation
        /// </summary>
        private void StartPulseAnimation()
        {
            // Ensure scale is initialized
            if (!hasInitializedScale)
            {
                InitializeScale();
            }
            
            // Stop existing animation
            StopPulseAnimation();
            
            // Create new pulse sequence
            pulseSequence = DOTween.Sequence();
            
            // Scale up and down based on original scale
            pulseSequence.Append(transform.DOScale(originalScale * pulseScale, pulseDuration / 2f).SetEase(Ease.OutQuad));
            pulseSequence.Append(transform.DOScale(originalScale, pulseDuration / 2f).SetEase(Ease.InOutQuad));
            
            // Loop the sequence
            pulseSequence.SetLoops(-1);
        }

        /// <summary>
        /// Stops the pulsing animation
        /// </summary>
        private void StopPulseAnimation()
        {
            // Ensure scale is initialized
            if (!hasInitializedScale)
            {
                InitializeScale();
            }
            
            pulseSequence?.Kill();
            transform.DOScale(originalScale, 0.2f);
        }

        private void OnDestroy()
        {
            // Ensure animations are stopped
            StopPulseAnimation();
            DOTween.Kill(transform);
            DOTween.Kill(spriteRenderer);
        }
    }
} 