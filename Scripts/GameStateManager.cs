using UnityEngine;
using UnityEngine.Events;

namespace PoEClone2D
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public UnityEvent<bool> OnInventoryStateChanged = new UnityEvent<bool>();

        private bool isInventoryOpen = false;
        private bool isGamePaused = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("GameStateManager: Initialized");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetInventoryOpen(bool open)
        {
            if (isInventoryOpen != open)
            {
                isInventoryOpen = open;
                OnInventoryStateChanged.Invoke(open);
                Debug.Log($"GameStateManager: Inventory {(open ? "opened" : "closed")}");
            }
        }

        public bool IsInventoryOpen() => isInventoryOpen;
        public bool IsGamePaused() => isGamePaused;

        public void SetGamePaused(bool paused)
        {
            isGamePaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            Debug.Log($"GameStateManager: Game {(paused ? "paused" : "unpaused")}");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}