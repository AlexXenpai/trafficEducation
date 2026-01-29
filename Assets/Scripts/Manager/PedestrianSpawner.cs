using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PedestrianSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] yayaPrefabs;
    public int spawnCount = 50;
    public float spawnRadius = 100f;

    [Header("Animation")]
    public RuntimeAnimatorController pedestrianAnimator; // Yaya animator controller

    [Header("Container")]
    public string containerName = "PedestrianContainer";

    private Transform container;

    void Start()
    {
        // Animator Controller'ı otomatik yükle
        if (pedestrianAnimator == null)
        {
            pedestrianAnimator = Resources.Load<RuntimeAnimatorController>("PedestrianAnimator");
            
            // Resources'da yoksa Assets/Animations'dan yükle
            if (pedestrianAnimator == null)
            {
                #if UNITY_EDITOR
                pedestrianAnimator = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    "Assets/Animations/PedestrianAnimator.controller"
                );
                #endif
            }
        }
        
        SpawnPedestrians();
    }

    void SpawnPedestrians()
    {
        if (yayaPrefabs == null || yayaPrefabs.Length == 0)
        {
            Debug.LogError("PedestrianSpawner: Yaya prefabları atanmamış!");
            return;
        }

        GameObject containerObj = GameObject.Find(containerName);
        if (containerObj == null)
            containerObj = new GameObject(containerName);

        container = containerObj.transform;

        int spawnedCount = 0;
        int attempts = 0;
        int maxAttempts = spawnCount * 5;

        while (spawnedCount < spawnCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 5.0f, NavMesh.AllAreas))
            {
                GameObject prefabToSpawn = yayaPrefabs[Random.Range(0, yayaPrefabs.Length)];

                GameObject newPedestrian = Instantiate(prefabToSpawn, hit.position, Quaternion.identity);
                newPedestrian.transform.SetParent(container);

                // Animator Controller ata
                Animator animator = newPedestrian.GetComponent<Animator>();
                if (animator == null)
                    animator = newPedestrian.GetComponentInChildren<Animator>();
                    
                if (animator != null && pedestrianAnimator != null)
                {
                    animator.runtimeAnimatorController = pedestrianAnimator;
                }

                // PedestrianAI
                if (newPedestrian.GetComponent<PedestrianAI>() == null)
                    newPedestrian.AddComponent<PedestrianAI>();

                // NavMeshAgent
                NavMeshAgent agent = newPedestrian.GetComponent<NavMeshAgent>();
                if (agent == null)
                {
                    agent = newPedestrian.AddComponent<NavMeshAgent>();
                    agent.height = 1.8f;
                    agent.radius = 0.3f;
                }

                // Tag
                newPedestrian.tag = "Pedestrian";

                // Collider
                Collider col = newPedestrian.GetComponent<Collider>();
                if (col == null)
                {
                    CapsuleCollider cap = newPedestrian.AddComponent<CapsuleCollider>();
                    cap.center = new Vector3(0, 0.9f, 0);
                    cap.height = 1.8f;
                    cap.radius = 0.3f;
                }

                // Rigidbody (başta kinematic)
                Rigidbody rb = newPedestrian.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = newPedestrian.AddComponent<Rigidbody>();

                rb.isKinematic = true;
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeRotation;

                // Arabaya çarpınca düşme scripti
                if (newPedestrian.GetComponent<PedestrianHitByCar>() == null)
                    newPedestrian.AddComponent<PedestrianHitByCar>();

                spawnedCount++;
            }
        }

        Debug.Log($"PedestrianSpawner: {spawnedCount} adet yaya oluşturuldu.");
    }
}
