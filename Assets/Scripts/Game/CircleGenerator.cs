using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SunbaseTest.Game
{
    /// <summary>
    /// Component responsible for generating random circles for the game
    /// </summary>
    public class CircleGenerator : MonoBehaviour
    {
        [Header("Circle Settings")]
        [SerializeField] private GameObject circlePrefab;
        [SerializeField] private int minCircles = 5;
        [SerializeField] private int maxCircles = 10;
        [SerializeField] private float minCircleSize = 0.5f;
        [SerializeField] private float maxCircleSize = 1.5f;
        [SerializeField] private float edgePadding = 1f;
        [SerializeField] private float minDistanceBetweenCircles = 1f;
        
        [Header("Animation")]
        [SerializeField] private float spawnAnimationDuration = 0.3f;
        [SerializeField] private float delayBetweenSpawns = 0.1f;
        [SerializeField] private Ease spawnEase = Ease.OutBack;
        
        // List of currently active circles
        private List<GameObject> activeCircles = new List<GameObject>();
        
        // Camera reference for screen bounds calculation
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        /// <summary>
        /// Generates a new set of random circles
        /// </summary>
        public void GenerateCircles()
        {
            // Clear existing circles
            ClearCircles();
            
            // Calculate screen bounds considering padding and circle sizes
            Bounds cameraBounds = CalculateCameraBounds();
            float maxCircleRadius = maxCircleSize / 2f;
            
            float minX = cameraBounds.min.x + maxCircleRadius + edgePadding;
            float maxX = cameraBounds.max.x - maxCircleRadius - edgePadding;
            float minY = cameraBounds.min.y + maxCircleRadius + edgePadding;
            float maxY = cameraBounds.max.y - maxCircleRadius - edgePadding;
            
            // Determine number of circles to spawn
            int circleCount = Random.Range(minCircles, maxCircles + 1);
            List<Vector3> circlePositions = new List<Vector3>();
            
            // Generate circle positions
            for (int i = 0; i < circleCount; i++)
            {
                Vector3 position = GetValidPosition(minX, maxX, minY, maxY, circlePositions);
                circlePositions.Add(position);
                
                // Create circle
                float size = Random.Range(minCircleSize, maxCircleSize);
                SpawnCircle(position, size, i * delayBetweenSpawns);
            }
        }

        /// <summary>
        /// Calculates the camera bounds in world space
        /// </summary>
        private Bounds CalculateCameraBounds()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("No camera found!");
                    return new Bounds(Vector3.zero, new Vector3(10, 10, 0));
                }
            }
            
            float distanceToTarget = Mathf.Abs(mainCamera.transform.position.z);
            float halfHeight;
            float halfWidth;
            
            if (mainCamera.orthographic)
            {
                // Orthographic camera
                halfHeight = mainCamera.orthographicSize;
                halfWidth = halfHeight * mainCamera.aspect;
            }
            else
            {
                // Perspective camera
                halfHeight = Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distanceToTarget;
                halfWidth = halfHeight * mainCamera.aspect;
            }
            
            Vector3 center = mainCamera.transform.position + (mainCamera.transform.forward * distanceToTarget);
            return new Bounds(center, new Vector3(halfWidth * 2f, halfHeight * 2f, 1));
        }

        /// <summary>
        /// Gets a valid random position that doesn't overlap with existing circles
        /// </summary>
        private Vector3 GetValidPosition(float minX, float maxX, float minY, float maxY, List<Vector3> existingPositions)
        {
            Vector3 position;
            bool validPosition = false;
            int maxAttempts = 50; // Prevent infinite loop
            int attempts = 0;
            
            do
            {
                position = new Vector3(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY),
                    0
                );
                
                validPosition = IsValidPosition(position, existingPositions);
                attempts++;
                
            } while (!validPosition && attempts < maxAttempts);
            
            return position;
        }

        /// <summary>
        /// Checks if a position is valid (not too close to existing circles)
        /// </summary>
        private bool IsValidPosition(Vector3 position, List<Vector3> existingPositions)
        {
            foreach (var existingPos in existingPositions)
            {
                if (Vector3.Distance(position, existingPos) < minDistanceBetweenCircles)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Spawns a circle at the specified position with animation
        /// </summary>
        private void SpawnCircle(Vector3 position, float size, float delay)
        {
            GameObject circle = Instantiate(circlePrefab, position, Quaternion.identity, transform);
            circle.transform.localScale = Vector3.zero;
            
            // Set size with proper Vector3 (not just a single float value)
            Vector3 targetScale = new Vector3(size, size, 1f);
            
            // Get the Circle component
            Circle circleComponent = circle.GetComponent<Circle>();
            
            // Animate spawn
            circle.transform.DOScale(targetScale, spawnAnimationDuration)
                .SetEase(spawnEase)
                .SetDelay(delay)
                .OnComplete(() => {
                    // Initialize the circle's scale reference after animation completes
                    if (circleComponent != null)
                    {
                        circleComponent.InitializeScale();
                    }
                });
            
            // Add to active circles list
            activeCircles.Add(circle);
        }

        /// <summary>
        /// Removes a circle with animation
        /// </summary>
        /// <returns>True if circle was successfully found and removal started</returns>
        public bool RemoveCircle(GameObject circle)
        {
            if (circle == null)
            {
                return false;
            }
                
            if (!activeCircles.Contains(circle))
            {
                // Circle already removed or not part of our managed circles
                return false;
            }
            
            try
            {
                // Animate removal
                circle.transform.DOScale(Vector3.zero, spawnAnimationDuration / 2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => {
                        if (circle != null)
                        {
                            activeCircles.Remove(circle);
                            Destroy(circle);
                        }
                    });
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error removing circle: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clears all active circles
        /// </summary>
        public void ClearCircles()
        {
            foreach (var circle in activeCircles)
            {
                if (circle != null)
                {
                    Destroy(circle);
                }
            }
            
            activeCircles.Clear();
        }

        /// <summary>
        /// Gets the list of currently active circles
        /// </summary>
        public List<GameObject> GetActiveCircles()
        {
            return activeCircles;
        }
    }
} 