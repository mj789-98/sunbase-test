using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SunbaseTest.Game
{
    /// <summary>
    /// Component for handling line drawing using mouse/touch input
    /// </summary>
    public class LineDrawer : MonoBehaviour
    {
        [Header("Line Settings")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private Color lineColor = Color.white;
        [SerializeField] private int initialVertexCapacity = 100;
        
        [Header("Drawing Settings")]
        [SerializeField] private float minVertexDistance = 0.1f;
        [SerializeField] private LayerMask drawingPlaneLayer;
        
        [Header("Collision")]
        [SerializeField] private LayerMask circleLayer;
        [SerializeField] private float collisionRadius = 0.5f;
        [SerializeField] private bool debugCollisions = false;
        
        [Header("Animation")]
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private Ease fadeOutEase = Ease.InQuad;

        // List of line points for collision checking
        private List<Vector3> linePoints = new List<Vector3>();
        
        // Camera reference for input processing
        private Camera mainCamera;
        
        // Flag to indicate if we are currently drawing
        private bool isDrawing = false;
        
        // Reference to the last position to prevent too many vertices
        private Vector3 lastDrawnPoint;

        // Event for when circles are hit by the line
        public System.Action<List<GameObject>> OnCirclesHit;

        private void Awake()
        {
            mainCamera = Camera.main;
            
            // Initialize line renderer if not already assigned
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }
            
            ConfigureLineRenderer();
        }

        private void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// Configures the line renderer appearance
        /// </summary>
        private void ConfigureLineRenderer()
        {
            lineRenderer.positionCount = 0;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            
            if (lineMaterial != null)
            {
                lineRenderer.material = lineMaterial;
            }
            
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.useWorldSpace = true;
        }

        /// <summary>
        /// Handles input for drawing lines
        /// </summary>
        private void HandleInput()
        {
            // Start drawing
            if (Input.GetMouseButtonDown(0) && !isDrawing)
            {
                StartDrawing();
            }
            
            // Continue drawing
            if (Input.GetMouseButton(0) && isDrawing)
            {
                ContinueDrawing();
            }
            
            // End drawing
            if (Input.GetMouseButtonUp(0) && isDrawing)
            {
                EndDrawing();
            }
        }

        /// <summary>
        /// Starts the line drawing process
        /// </summary>
        private void StartDrawing()
        {
            isDrawing = true;
            
            // Reset line renderer and points list
            lineRenderer.positionCount = 0;
            linePoints.Clear();
            
            // Get initial position
            Vector3 mousePosition = GetMouseWorldPosition();
            
            // Add first point
            AddLinePoint(mousePosition);
            lastDrawnPoint = mousePosition;
        }

        /// <summary>
        /// Continues drawing line based on current input
        /// </summary>
        private void ContinueDrawing()
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            
            // Only add new point if it's far enough from the last one
            if (Vector3.Distance(mousePosition, lastDrawnPoint) >= minVertexDistance)
            {
                AddLinePoint(mousePosition);
                lastDrawnPoint = mousePosition;
            }
        }

        /// <summary>
        /// Ends the drawing process and checks for collisions
        /// </summary>
        private void EndDrawing()
        {
            isDrawing = false;
            
            // Check for collisions with circles
            List<GameObject> hitCircles = CheckCircleCollisions();
            
            // Trigger event for hit circles
            if (hitCircles.Count > 0)
            {
                OnCirclesHit?.Invoke(hitCircles);
            }
            
            // Fade out line
            FadeOutLine();
        }

        /// <summary>
        /// Adds a point to the line
        /// </summary>
        private void AddLinePoint(Vector3 point)
        {
            linePoints.Add(point);
            
            // Update line renderer
            lineRenderer.positionCount = linePoints.Count;
            lineRenderer.SetPosition(linePoints.Count - 1, point);
        }

        /// <summary>
        /// Converts mouse position to world position
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("No camera found!");
                    return Vector3.zero;
                }
            }
            
            Vector3 mousePosition = Input.mousePosition;
            
            // Handle both camera types
            if (mainCamera.orthographic)
            {
                // For orthographic camera
                mousePosition.z = -mainCamera.transform.position.z;
                return mainCamera.ScreenToWorldPoint(mousePosition);
            }
            else
            {
                // For perspective camera
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                // Project the ray to z=0 plane
                float distanceToZPlane = -ray.origin.z / ray.direction.z;
                return ray.origin + ray.direction * distanceToZPlane;
            }
        }

        /// <summary>
        /// Checks for collisions between the drawn line and circles
        /// </summary>
        private List<GameObject> CheckCircleCollisions()
        {
            List<GameObject> hitCircles = new List<GameObject>();
            
            // Skip collision check if too few points
            if (linePoints.Count < 2)
            {
                return hitCircles;
            }
            
            // Calculate bounding area for all line points, not just start and end
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            
            // Find min/max X and Y values across all line points
            foreach (Vector3 point in linePoints)
            {
                minX = Mathf.Min(minX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxX = Mathf.Max(maxX, point.x);
                maxY = Mathf.Max(maxY, point.y);
            }
            
            // Add padding for radius
            minX -= collisionRadius;
            minY -= collisionRadius;
            maxX += collisionRadius;
            maxY += collisionRadius;
            
            // Get all potential circle colliders in the expanded area
            Collider2D[] circleColliders = Physics2D.OverlapAreaAll(
                new Vector2(minX, minY),
                new Vector2(maxX, maxY),
                circleLayer
            );
            
            // Use a HashSet to prevent duplicate entries
            HashSet<GameObject> uniqueHitCircles = new HashSet<GameObject>();
            
            // Check for collisions with line segments
            foreach (var circleCollider in circleColliders)
            {
                if (circleCollider != null && circleCollider.gameObject != null)
                {
                    // Check if circle intersects with any line segment
                    if (IsCircleIntersectingLine(circleCollider.transform.position, circleCollider.bounds.extents.x))
                    {
                        uniqueHitCircles.Add(circleCollider.gameObject);
                        
                        // Also trigger the circle's hit effect for visual feedback
                        Circle circleComponent = circleCollider.GetComponent<Circle>();
                        if (circleComponent != null)
                        {
                            circleComponent.OnHit();
                        }
                    }
                }
            }
            
            // Convert HashSet to List
            return new List<GameObject>(uniqueHitCircles);
        }

        /// <summary>
        /// Checks if a circle intersects with the drawn line
        /// </summary>
        private bool IsCircleIntersectingLine(Vector3 circleCenter, float circleRadius)
        {
            // Check each line segment for intersection
            for (int i = 0; i < linePoints.Count - 1; i++)
            {
                Vector3 start = linePoints[i];
                Vector3 end = linePoints[i + 1];
                
                // Check if circle intersects with this line segment
                if (CircleIntersectsLineSegment(circleCenter, circleRadius, start, end))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a circle intersects with a line segment
        /// </summary>
        private bool CircleIntersectsLineSegment(Vector3 circleCenter, float circleRadius, Vector3 lineStart, Vector3 lineEnd)
        {
            // Find the closest point on the line segment to the circle center
            Vector3 line = lineEnd - lineStart;
            Vector3 circleToLineStart = circleCenter - lineStart;
            
            float lineLength = line.magnitude;
            Vector3 lineDirection = line / lineLength;
            
            // Project circleToLineStart onto line
            float projection = Vector3.Dot(circleToLineStart, lineDirection);
            
            // Calculate closest point on line
            Vector3 closestPoint;
            
            if (projection <= 0)
            {
                // Closest point is lineStart
                closestPoint = lineStart;
            }
            else if (projection >= lineLength)
            {
                // Closest point is lineEnd
                closestPoint = lineEnd;
            }
            else
            {
                // Closest point is on the line segment
                closestPoint = lineStart + lineDirection * projection;
            }
            
            // Check if closest point is within the circle radius
            float distance = Vector3.Distance(circleCenter, closestPoint);
            
            // Draw debug visualization if enabled
            if (debugCollisions)
            {
                // Draw line
                Debug.DrawLine(lineStart, lineEnd, Color.yellow, 2f);
                // Draw closest point
                Debug.DrawLine(circleCenter, closestPoint, Color.red, 2f);
                // Draw circle radius at collision point
                if (distance <= circleRadius)
                {
                    DebugDrawCircle(circleCenter, circleRadius, Color.green, 1f);
                }
                else
                {
                    DebugDrawCircle(circleCenter, circleRadius, Color.red, 1f);
                }
            }
            
            return distance <= circleRadius;
        }
        
        /// <summary>
        /// Helper method to draw a circle in the scene view for debugging
        /// </summary>
        private void DebugDrawCircle(Vector3 center, float radius, Color color, float duration)
        {
            if (!debugCollisions) return;
            
            int segments = 20;
            float angle = 0f;
            float angleStep = 360f / segments;
            
            Vector3 previousPoint = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle) * radius, Mathf.Sin(Mathf.Deg2Rad * angle) * radius, 0);
            
            for (int i = 0; i < segments + 1; i++)
            {
                angle += angleStep;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle) * radius, Mathf.Sin(Mathf.Deg2Rad * angle) * radius, 0);
                Debug.DrawLine(previousPoint, nextPoint, color, duration);
                previousPoint = nextPoint;
            }
        }

        /// <summary>
        /// Fades out the line with animation
        /// </summary>
        private void FadeOutLine()
        {
            if (lineRenderer != null)
            {
                // Create a sequence for line fade out
                Sequence fadeSequence = DOTween.Sequence();
                
                // Get material for temporary color change
                Material tempMaterial = lineRenderer.material;
                Color originalColor = tempMaterial.color;
                
                // Fade out line
                fadeSequence.Append(DOTween.To(() => tempMaterial.color, 
                    x => tempMaterial.color = x, 
                    new Color(originalColor.r, originalColor.g, originalColor.b, 0f), 
                    fadeOutDuration)
                    .SetEase(fadeOutEase));
                
                // Reset after fade out
                fadeSequence.OnComplete(() => {
                    lineRenderer.positionCount = 0;
                    linePoints.Clear();
                    tempMaterial.color = originalColor;
                });
            }
        }
    }
} 