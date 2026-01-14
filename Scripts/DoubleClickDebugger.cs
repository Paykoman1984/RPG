using UnityEngine;

namespace PoEClone2D
{
    public class DoubleClickDebugger : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"Left mouse clicked at: {Time.time}");
            }

            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log($"Right mouse clicked at: {Time.time}");
            }
        }
    }
}