using UnityEngine;

namespace PoEClone2D.Camera
{
    public class BasicCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset = Vector2.zero;

        [Header("Settings")]
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = true;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 newPosition = transform.position;

            if (followX)
                newPosition.x = target.position.x + offset.x;

            if (followY)
                newPosition.y = target.position.y + offset.y;

            transform.position = newPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}