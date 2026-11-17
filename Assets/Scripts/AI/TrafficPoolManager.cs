using System.Collections.Generic;
using UnityEngine;

namespace CarGame.AI
{
    /// <summary>
    /// High-performance mobile object pool manager for AI traffic cars.
    /// Recycles cars ahead of the player and despawns cars behind.
    /// </summary>
    public class TrafficPoolManager : MonoBehaviour
    {
        public static TrafficPoolManager Instance { get; private set; }

        [Header("Traffic Car Prefabs / Templates")]
        [SerializeField] private GameObject[] trafficCarPrefabs;
        [SerializeField] private int maxActiveCars = 10;

        [Header("Waypoints & Spawning")]
        [SerializeField] private TrafficWaypoint[] spawnWaypoints;
        [SerializeField] private float spawnForwardDistance = 120f;
        [SerializeField] private float despawnRearDistance = 60f;

        [Header("Player Target")]
        [SerializeField] private Transform playerTarget;

        private List<AITrafficCar> carPool = new List<AITrafficCar>();
        private float checkTimer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (playerTarget == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null) playerTarget = player.transform;
            }
        }

        private void Update()
        {
            if (playerTarget == null) return;

            checkTimer += Time.deltaTime;
            if (checkTimer >= 1.5f)
            {
                checkTimer = 0f;
                RecycleDistantCars();
            }
        }

        private void RecycleDistantCars()
        {
            for (int i = 0; i < carPool.Count; i++)
            {
                AITrafficCar car = carPool[i];
                if (car == null || !car.gameObject.activeSelf) continue;

                float distToPlayer = Vector3.Dot(car.transform.position - playerTarget.position, playerTarget.forward);

                // Despawn if fallen far behind player
                if (distToPlayer < -despawnRearDistance)
                {
                    car.gameObject.SetActive(false);
                }
            }
        }

        public void RegisterTrafficCar(AITrafficCar car)
        {
            if (!carPool.Contains(car))
            {
                carPool.Add(car);
            }
        }
    }
}
