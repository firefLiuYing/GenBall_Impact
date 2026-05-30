using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    /// <summary>
    /// Lightweight MonoBehaviour shell for bullet rendering.
    /// Handles Bezier/Lerp interpolation to converge visual position toward logic position.
    /// Pooled — Recycle() returns to pool rather than Destroy().
    /// </summary>
    public class BulletVisual : MonoBehaviour
    {
        [SerializeField] private Transform modelTransform;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private float defaultPredictDistance = 1000f;

        private bool _flying;
        private float _flyTime;
        private float _predictTime;
        private bool _needBezier;

        private Vector3 _rendererSpawnPoint;
        private Vector3 _rendererControlPoint;
        private Vector3 _rendererTargetPoint;

        private bool _isRecycled;

        /// <summary>
        /// Initialize the visual with spawn and target info.
        /// Computes the Bezier curve that blends visual origin into logic trajectory.
        /// </summary>
        /// <param name="visualOrigin">Where the visual bullet appears (gun muzzle).</param>
        /// <param name="logicTarget">Rough logic endpoint (used for initial curve estimation).</param>
        public void Init(Vector3 visualOrigin, Vector3 logicTarget)
        {
            _isRecycled = false;
            _flying = true;
            _flyTime = 0f;

            _rendererSpawnPoint = visualOrigin;

            // Find the logic source (Camera.main) for Bezier calculation
            Vector3 logicSource = Camera.main != null ? Camera.main.transform.position : visualOrigin;
            Vector3 direction = (logicTarget - logicSource).normalized;

            // Raycast to find actual target point
            if (Physics.Raycast(logicSource, direction, out var hitInfo))
            {
                _rendererTargetPoint = hitInfo.point;
            }
            else
            {
                _rendererTargetPoint = logicSource + direction * defaultPredictDistance;
            }

            _predictTime = Vector3.Distance(_rendererTargetPoint, logicSource) / 50f; // rough estimate at 50 m/s
            if (_predictTime <= 0f) _predictTime = 0.1f;

            // Calculate Bezier control point to blend visual origin into logic trajectory
            Vector3 distanceLine = _rendererTargetPoint - logicSource;
            Vector3 offsetLine = _rendererSpawnPoint - logicSource;
            float alpha = Vector3.Angle(distanceLine, offsetLine);
            float distance = distanceLine.magnitude;
            float offset = offsetLine.magnitude;

            float delta = offset * Mathf.Cos(alpha * Mathf.Deg2Rad);
            distance -= 2f * delta;
            _rendererControlPoint = _rendererSpawnPoint + distance * distanceLine.normalized;
            _needBezier = distance > 0f;

            // Snap model to visual origin
            if (modelTransform != null)
            {
                modelTransform.position = visualOrigin;
                modelTransform.rotation = Quaternion.LookRotation(direction);
            }

            // Clear trail
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.emitting = true;
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Called every frame (Update) to interpolate visual position.
        /// </summary>
        /// <param name="logicPosition">Current logic bullet position.</param>
        /// <param name="progress">Normalized progress (0-1) of bullet lifetime.</param>
        public void UpdateVisual(Vector3 logicPosition, float progress)
        {
            if (!_flying || _isRecycled) return;

            // Respect pause
            var ps = SystemRepository.Instance.GetSystem<IPauseSystem>();
            if (ps != null && ps.IsLogicPaused) return;

            _flyTime += Time.deltaTime;

            if (_flyTime < _predictTime && _needBezier)
            {
                float t = _flyTime / _predictTime;
                if (modelTransform != null)
                {
                    modelTransform.position = Bezier(t, _rendererSpawnPoint, _rendererControlPoint, _rendererTargetPoint);
                }
            }
            else if (_flyTime < _predictTime)
            {
                float t = _flyTime / _predictTime;
                if (modelTransform != null)
                {
                    modelTransform.position = Vector3.Lerp(_rendererSpawnPoint, _rendererTargetPoint, t);
                }
            }
            else
            {
                // Snap to logic position
                if (modelTransform != null)
                {
                    modelTransform.position = logicPosition;
                }
            }
        }

        /// <summary>
        /// Recycle this visual back to the pool.
        /// Stops trail, clears state, deactivates.
        /// </summary>
        public void OnRecycle()
        {
            if (_isRecycled) return;
            _isRecycled = true;

            _flying = false;

            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
                trailRenderer.Clear();
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Called by CPoolManager when this visual is spawned from pool.
        /// </summary>
        public void OnPoolSpawn()
        {
            _isRecycled = false;
            _flying = false;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _flying = false;
        }

        // ======== Bezier ========

        private static Vector3 Bezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
            => (1f - t) * (1f - t) * p0 + 2f * (1f - t) * t * p1 + t * t * p2;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_flying) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_rendererSpawnPoint, _rendererControlPoint);
            Gizmos.DrawLine(_rendererControlPoint, _rendererTargetPoint);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_rendererTargetPoint, 0.2f);
        }
#endif
    }
}
