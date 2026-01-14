using UnityEngine;
using System.Collections.Generic;

namespace PoEClone2D.Testing
{
    public class TestLevelGenerator : MonoBehaviour
    {
        [Header("Room Settings")]
        [SerializeField] private int roomWidth = 20;
        [SerializeField] private int roomHeight = 15;
        [SerializeField] private int numberOfRooms = 5;
        [SerializeField] private float roomSpacing = 25f;

        [Header("Tile Prefabs")]
        [SerializeField] private GameObject floorTilePrefab;
        [SerializeField] private GameObject wallTilePrefab;
        [SerializeField] private GameObject cornerTilePrefab;
        [SerializeField] private GameObject doorTilePrefab;

        [Header("Obstacles & Props")]
        [SerializeField] private GameObject[] obstaclePrefabs;
        [SerializeField] private GameObject[] propPrefabs;
        [SerializeField] private int maxObstaclesPerRoom = 3;
        [SerializeField] private int maxPropsPerRoom = 5;

        [Header("Enemy Spawning")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int minEnemiesPerRoom = 1;
        [SerializeField] private int maxEnemiesPerRoom = 3;
        [SerializeField] private float enemySpawnRadius = 3f;

        [Header("Player Spawn")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        [Header("Lighting")]
        [SerializeField] private GameObject roomLightPrefab;
        [SerializeField] private Color roomLightColor = new Color(1f, 0.95f, 0.8f);
        [SerializeField] private float lightIntensity = 1f;

        [Header("Visuals")]
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Color[] roomColors;

        // Room data
        private List<Room> rooms = new List<Room>();
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private GameObject playerInstance;

        [System.Serializable]
        public class Room
        {
            public Vector2 position;
            public int width;
            public int height;
            public Color color;
            public List<GameObject> tiles = new List<GameObject>();
            public List<GameObject> obstacles = new List<GameObject>();
            public List<GameObject> props = new List<GameObject>();
            public List<GameObject> enemies = new List<GameObject>();
            public bool hasPlayerSpawn = false;
        }

        private void Start()
        {
            GenerateLevel();
        }

        public void GenerateLevel()
        {
            ClearLevel();
            CreateRooms();
            SpawnPlayer();
            Debug.Log($"Test Level Generated: {rooms.Count} rooms created");
        }

        private void CreateRooms()
        {
            for (int roomIndex = 0; roomIndex < numberOfRooms; roomIndex++)
            {
                Vector2 roomPos = new Vector2(
                    (roomIndex % 3) * roomSpacing,
                    Mathf.Floor(roomIndex / 3) * roomSpacing
                );

                Room room = new Room
                {
                    position = roomPos,
                    width = roomWidth,
                    height = roomHeight,
                    color = roomColors.Length > 0 ? roomColors[Random.Range(0, roomColors.Length)] : Color.white
                };

                rooms.Add(room);
                GenerateRoom(room);

                // Connect rooms with corridors (except first room)
                if (roomIndex > 0)
                {
                    ConnectRooms(rooms[roomIndex - 1], room);
                }
            }
        }

        private void GenerateRoom(Room room)
        {
            // Create floor
            for (int x = 0; x < room.width; x++)
            {
                for (int y = 0; y < room.height; y++)
                {
                    Vector3 tilePos = new Vector3(
                        room.position.x + x,
                        room.position.y + y,
                        0
                    );

                    GameObject tile;

                    // Walls
                    if (x == 0 || x == room.width - 1 || y == 0 || y == room.height - 1)
                    {
                        // Corners
                        if ((x == 0 && y == 0) || (x == 0 && y == room.height - 1) ||
                            (x == room.width - 1 && y == 0) || (x == room.width - 1 && y == room.height - 1))
                        {
                            tile = Instantiate(cornerTilePrefab, tilePos, Quaternion.identity, transform);
                        }
                        else
                        {
                            tile = Instantiate(wallTilePrefab, tilePos, Quaternion.identity, transform);

                            // Rotate wall tiles properly
                            if (x == 0) tile.transform.rotation = Quaternion.Euler(0, 0, 90);
                            else if (x == room.width - 1) tile.transform.rotation = Quaternion.Euler(0, 0, -90);
                            else if (y == 0) tile.transform.rotation = Quaternion.Euler(0, 0, 180);
                        }

                        // Set wall material
                        if (wallMaterial != null)
                        {
                            Renderer renderer = tile.GetComponent<Renderer>();
                            if (renderer != null) renderer.material = wallMaterial;
                        }
                    }
                    // Floor
                    else
                    {
                        tile = Instantiate(floorTilePrefab, tilePos, Quaternion.identity, transform);

                        // Set floor material
                        if (floorMaterial != null)
                        {
                            Renderer renderer = tile.GetComponent<Renderer>();
                            if (renderer != null) renderer.material = floorMaterial;
                        }

                        // Random floor color variation
                        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            Color floorColor = room.color * Random.Range(0.8f, 1.2f);
                            floorColor.a = 1f;
                            spriteRenderer.color = floorColor;
                        }
                    }

                    room.tiles.Add(tile);
                }
            }

            // Add obstacles
            SpawnObstacles(room);

            // Add props
            SpawnProps(room);

            // Spawn enemies (not in first room)
            if (rooms.IndexOf(room) > 0)
            {
                SpawnEnemies(room);
            }

            // Add lighting
            Vector3 roomCenter = new Vector3(
                room.position.x + room.width / 2f,
                room.position.y + room.height / 2f,
                -5f
            );

            GameObject lightObj = Instantiate(roomLightPrefab, roomCenter, Quaternion.identity, transform);
            Light lightComp = lightObj.GetComponent<Light>();
            if (lightComp != null)
            {
                lightComp.color = roomLightColor;
                lightComp.intensity = lightIntensity;
                lightComp.range = Mathf.Max(room.width, room.height) * 1.5f;
            }
        }

        private void SpawnObstacles(Room room)
        {
            if (obstaclePrefabs.Length == 0) return;

            int obstacleCount = Random.Range(0, maxObstaclesPerRoom + 1);

            for (int i = 0; i < obstacleCount; i++)
            {
                // Find valid position (not too close to walls)
                Vector3 obstaclePos = new Vector3(
                    room.position.x + Random.Range(2, room.width - 2),
                    room.position.y + Random.Range(2, room.height - 2),
                    0
                );

                GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                GameObject obstacle = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity, transform);

                // Random rotation
                obstacle.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

                // Random scale
                float randomScale = Random.Range(0.8f, 1.2f);
                obstacle.transform.localScale = new Vector3(randomScale, randomScale, 1);

                room.obstacles.Add(obstacle);
            }
        }

        private void SpawnProps(Room room)
        {
            if (propPrefabs.Length == 0) return;

            int propCount = Random.Range(0, maxPropsPerRoom + 1);

            for (int i = 0; i < propCount; i++)
            {
                // Find valid position
                Vector3 propPos = new Vector3(
                    room.position.x + Random.Range(1, room.width - 1),
                    room.position.y + Random.Range(1, room.height - 1),
                    -0.1f  // Slightly behind floor
                );

                GameObject propPrefab = propPrefabs[Random.Range(0, propPrefabs.Length)];
                GameObject prop = Instantiate(propPrefab, propPos, Quaternion.identity, transform);

                // Random rotation
                prop.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

                // Random scale
                float randomScale = Random.Range(0.7f, 1.3f);
                prop.transform.localScale = new Vector3(randomScale, randomScale, 1);

                room.props.Add(prop);
            }
        }

        private void SpawnEnemies(Room room)
        {
            if (enemyPrefab == null) return;

            int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

            for (int i = 0; i < enemyCount; i++)
            {
                // Find valid position away from walls
                Vector3 enemyPos = new Vector3(
                    room.position.x + Random.Range(enemySpawnRadius, room.width - enemySpawnRadius),
                    room.position.y + Random.Range(enemySpawnRadius, room.height - enemySpawnRadius),
                    0
                );

                GameObject enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity, transform);
                spawnedEnemies.Add(enemy);
                room.enemies.Add(enemy);

                Debug.Log($"Enemy spawned in room at position: {enemyPos}");
            }
        }

        private void ConnectRooms(Room roomA, Room roomB)
        {
            Vector2 centerA = new Vector2(
                roomA.position.x + roomA.width / 2f,
                roomA.position.y + roomA.height / 2f
            );

            Vector2 centerB = new Vector2(
                roomB.position.x + roomB.width / 2f,
                roomB.position.y + roomB.height / 2f
            );

            // Create corridor
            Vector2 direction = (centerB - centerA).normalized;
            float distance = Vector2.Distance(centerA, centerB);

            // Create door openings
            CreateDoorOpening(roomA, direction);
            CreateDoorOpening(roomB, -direction);

            // Create corridor tiles
            CreateCorridor(centerA, centerB, direction, distance);
        }

        private void CreateDoorOpening(Room room, Vector2 direction)
        {
            // Find wall position for door
            Vector2 doorPos = new Vector2(
                room.position.x + room.width / 2f,
                room.position.y + room.height / 2f
            );

            // Adjust based on direction
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal connection
                doorPos.x = direction.x > 0 ? room.position.x + room.width - 1 : room.position.x;
                doorPos.y = room.position.y + Mathf.Floor(room.height / 2f);
            }
            else
            {
                // Vertical connection
                doorPos.x = room.position.x + Mathf.Floor(room.width / 2f);
                doorPos.y = direction.y > 0 ? room.position.y + room.height - 1 : room.position.y;
            }

            // Replace wall tile with door tile
            foreach (GameObject tile in room.tiles)
            {
                if (Vector2.Distance(tile.transform.position, doorPos) < 0.5f)
                {
                    Destroy(tile);
                    GameObject door = Instantiate(doorTilePrefab, doorPos, Quaternion.identity, transform);
                    room.tiles.Remove(tile);
                    room.tiles.Add(door);
                    break;
                }
            }
        }

        private void CreateCorridor(Vector2 start, Vector2 end, Vector2 direction, float distance)
        {
            int corridorWidth = 3;
            int steps = Mathf.FloorToInt(distance / 1f);

            for (int i = 0; i < steps; i++)
            {
                Vector2 pos = Vector2.Lerp(start, end, (float)i / steps);

                for (int w = -corridorWidth / 2; w <= corridorWidth / 2; w++)
                {
                    Vector2 corridorPos = pos;

                    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                    {
                        corridorPos.y += w;
                    }
                    else
                    {
                        corridorPos.x += w;
                    }

                    GameObject tile = Instantiate(floorTilePrefab, corridorPos, Quaternion.identity, transform);

                    // Darker color for corridors
                    SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    }
                }
            }
        }

        private void SpawnPlayer()
        {
            if (playerPrefab == null || rooms.Count == 0) return;

            Room firstRoom = rooms[0];
            Vector3 spawnPos = new Vector3(
                firstRoom.position.x + firstRoom.width / 2f,
                firstRoom.position.y + firstRoom.height / 2f,
                0
            );

            if (playerSpawnPoint != null)
            {
                spawnPos = playerSpawnPoint.position;
            }

            playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            firstRoom.hasPlayerSpawn = true;

            Debug.Log($"Player spawned at: {spawnPos}");

            // Setup camera to follow player
            SetupCamera(playerInstance);
        }

        private void SetupCamera(GameObject target)
        {
            // Just position the camera, don't add any follow scripts
            // Let your existing camera system handle it

            UnityEngine.Camera unityCamera = UnityEngine.Camera.main;
            if (unityCamera != null)
            {
                unityCamera.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, -10f);
                Debug.Log("Camera positioned at player start");
            }
        }

        public void ClearLevel()
        {
            // Destroy all spawned objects
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            // Destroy enemies
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }

            // Destroy player
            if (playerInstance != null) Destroy(playerInstance);

            rooms.Clear();
            spawnedEnemies.Clear();
        }

        public void RespawnPlayer()
        {
            if (playerInstance != null)
            {
                Destroy(playerInstance);
            }

            SpawnPlayer();
        }

        public void RespawnEnemies()
        {
            // Clear existing enemies
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }
            spawnedEnemies.Clear();

            // Respawn in all rooms except first
            for (int i = 1; i < rooms.Count; i++)
            {
                rooms[i].enemies.Clear();
                SpawnEnemies(rooms[i]);
            }
        }

        // Debug GUI
        private void OnGUI()
        {
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 200, 300));
            GUILayout.Box("Test Level Controls");

            if (GUILayout.Button("Regenerate Level"))
            {
                GenerateLevel();
            }

            if (GUILayout.Button("Respawn Player"))
            {
                RespawnPlayer();
            }

            if (GUILayout.Button("Respawn Enemies"))
            {
                RespawnEnemies();
            }

            if (GUILayout.Button("Clear Level"))
            {
                ClearLevel();
            }

            GUILayout.Space(10);
            GUILayout.Label($"Rooms: {rooms.Count}");
            GUILayout.Label($"Enemies: {spawnedEnemies.Count}");

            if (playerInstance != null)
            {
                GUILayout.Label($"Player Position: {playerInstance.transform.position}");
            }

            GUILayout.EndArea();
        }

        // Gizmos for debugging
        private void OnDrawGizmosSelected()
        {
            if (rooms == null) return;

            Gizmos.color = Color.green;
            foreach (Room room in rooms)
            {
                Vector3 roomCenter = new Vector3(
                    room.position.x + room.width / 2f,
                    room.position.y + room.height / 2f,
                    0
                );

                Gizmos.DrawWireCube(roomCenter, new Vector3(room.width, room.height, 0));

                // Draw spawn points
                if (room.hasPlayerSpawn)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(roomCenter, 0.5f);
                    Gizmos.color = Color.green;
                }
            }
        }
    }

    // Simple camera follow script
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothSpeed = 5f;
        public Vector3 offset = new Vector3(0, 0, -10f);

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }
}