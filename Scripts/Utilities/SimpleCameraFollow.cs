using UnityEngine;

namespace PoEClone2D.Camera
{
    public class BasicCameraFollow : MonoBehaviour
    {
        [Header("Follow Settings")]
        public Transform target;
        public float smoothSpeed = 5f;
        public Vector3 offset = new Vector3(0, 0, -10f);
        public bool followX = true;
        public bool followY = true;

        [Header("Bounds")]
        public bool useBounds = false;
        public float minX = -10f;
        public float maxX = 10f;
        public float minY = -10f;
        public float maxY = 10f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;

            // Apply axis constraints
            if (!followX) desiredPosition.x = transform.position.x;
            if (!followY) desiredPosition.y = transform.position.y;

            // Apply bounds
            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }

            // Smooth movement
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                // Snap to target immediately
                transform.position = target.position + offset;
            }
        }

        public void SetBounds(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            useBounds = true;
        }

        public void ClearBounds()
        {
            useBounds = false;
        }
    }
}