using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

namespace SunbaseTest.Utilities
{
    /// <summary>
    /// Utility for handling scene transitions
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Ease fadeEase = Ease.InOutQuad;
        
        private static SceneLoader instance;

        // Scene names
        public const string MainScene = "Main";
        public const string CircleGameScene = "CircleGame";

        public static SceneLoader Instance
        {
            get { return instance; }
        }

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create fade canvas if not assigned
            if (fadeCanvasGroup == null)
            {
                CreateFadeCanvas();
            }
            
            // Initialize fade canvas
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Creates a canvas for fade transitions
        /// </summary>
        private void CreateFadeCanvas()
        {
            // Create canvas
            GameObject fadeCanvas = new GameObject("FadeCanvas");
            fadeCanvas.transform.SetParent(transform);
            
            // Add canvas components
            Canvas canvas = fadeCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            
            CanvasScaler canvasScaler = fadeCanvas.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add image for fade effect
            GameObject fadeImage = new GameObject("FadeImage");
            fadeImage.transform.SetParent(fadeCanvas.transform, false);
            
            RectTransform rectTransform = fadeImage.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            
            Image image = fadeImage.AddComponent<Image>();
            image.color = Color.black;
            
            // Add canvas group
            fadeCanvasGroup = fadeImage.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// Loads a scene with fade transition
        /// </summary>
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        /// <summary>
        /// Coroutine for loading scene with fade transition
        /// </summary>
        private System.Collections.IEnumerator LoadSceneCoroutine(string sceneName)
        {
            // Fade out
            fadeCanvasGroup.blocksRaycasts = true;
            yield return FadeOut();
            
            // Load scene
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
            loadOperation.allowSceneActivation = true;
            
            while (!loadOperation.isDone)
            {
                yield return null;
            }
            
            // Fade in
            yield return FadeIn();
            fadeCanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Fades the screen to black
        /// </summary>
        private System.Collections.IEnumerator FadeOut()
        {
            fadeCanvasGroup.alpha = 0f;
            Tween fadeTween = fadeCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase);
            
            while (fadeTween.IsActive())
            {
                yield return null;
            }
        }

        /// <summary>
        /// Fades the screen from black
        /// </summary>
        private System.Collections.IEnumerator FadeIn()
        {
            fadeCanvasGroup.alpha = 1f;
            Tween fadeTween = fadeCanvasGroup.DOFade(0f, fadeDuration).SetEase(fadeEase);
            
            while (fadeTween.IsActive())
            {
                yield return null;
            }
        }

        /// <summary>
        /// Loads the main scene
        /// </summary>
        public void LoadMainScene()
        {
            LoadScene(MainScene);
        }

        /// <summary>
        /// Loads the circle game scene
        /// </summary>
        public void LoadCircleGameScene()
        {
            LoadScene(CircleGameScene);
        }
    }
} 