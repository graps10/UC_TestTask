using Game.CameraControl;
using Game.Level;
using Game.Obstacles;
using Game.Player;
using Game.UI;
using UnityEngine;

namespace Game.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private LevelGenerationSettings levelSettings = new();
        [SerializeField] private BalanceSettings balance = new();

        [Header("Ground")]
        [Tooltip("Extra width beyond the corridor the ground plane extends on each side, in units.")]
        [SerializeField] private float groundWidthMargin = 4f;
        [Tooltip("Extra length before the start and after the door the ground plane extends, in units.")]
        [SerializeField] private float groundLengthMargin = 5f;
        [Tooltip("Ground plane thickness, in units.")]
        [SerializeField] private float groundThickness = 0.2f;

        [Header("Prefabs (assign in the Inspector)")]
        [SerializeField] private PlayerBall playerPrefab;
        [SerializeField] private Obstacle obstaclePrefab;
        [SerializeField] private Door doorPrefab;
        [SerializeField] private Shot shotPrefab;
        [SerializeField] private Transform groundPrefab;

        [Header("Scene references (assign in the Inspector)")]
        [Tooltip("The hand-authored UIManager_Canvas instance already placed in this scene.")]
        [SerializeField] private UIManager uiManager;
        [Tooltip("FollowCamera component on the scene's Main Camera.")]
        [SerializeField] private FollowCamera followCamera;

        private void Awake()
        {
            levelSettings.ObstacleRadius = obstaclePrefab.GetComponent<CapsuleCollider>().radius;

            var layout = LevelGenerator.Generate(levelSettings);

            BuildGround(layout);

            var obstacles = new GameObject("ObstacleField").AddComponent<ObstacleField>();
            obstacles.Initialize(layout, balance, obstaclePrefab);

            var player = Instantiate(playerPrefab, new Vector3(0f, 0f, layout.StartZ), Quaternion.identity);

            var door = Instantiate(doorPrefab, new Vector3(0f, 0f, layout.DoorZ), Quaternion.identity);
            door.Initialize(door.transform.position, player.transform);

            var shotController = new GameObject("ShotController").AddComponent<ShotController>();
            shotController.Initialize(player, obstacles, door.transform, balance, shotPrefab);

            var gameManager = new GameObject("GameManager").AddComponent<GameManager>();
            gameManager.Initialize(layout, balance, player, shotController, door);

            var pathTrail = new GameObject("PathTrail").AddComponent<PathTrail>();
            pathTrail.Initialize(layout.StartZ, player);

            if (followCamera != null)
                followCamera.Initialize(player.transform);

            if (uiManager != null)
                uiManager.Initialize(gameManager, player, balance);
        }

        private void BuildGround(LevelLayout layout)
        {
            if (groundPrefab == null)
                return;

            var ground = Instantiate(groundPrefab, transform);

            float width = layout.CorridorWidth + groundWidthMargin * 2f;
            float length = (layout.DoorZ - layout.StartZ) + groundLengthMargin * 2f;
            float centerZ = (layout.StartZ + layout.DoorZ) * 0.5f;

            ground.localScale = new Vector3(width, groundThickness, length);
            ground.position = new Vector3(0f, -groundThickness * 0.5f, centerZ);
        }
    }
}
