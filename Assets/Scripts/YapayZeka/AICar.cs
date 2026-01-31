using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICar : MonoBehaviour
{
    [Header("Settings")]
    public float safeDistance = 8f; // Slightly reduced to prevent over-cautious stops
    public float stopDistance = 3f;  // Reduced to allow closer queuing
    public float carSpeed = 30f;     // Speed in km/h (approx)
    public float acceleration = 5f;
    public float deceleration = 10f;
    
    [Header("Detection")]
    public LayerMask obstacleMask;   // Layers to detect (Cars, Obstacles)
    public string[] tags = { "Car", "Player", "Obstacle" };
    public float raycastOffset = 0.5f; // Height offset for raycast
    public float sideRayOffset = 0.8f; // Width offset for side rays

    [Header("Navigation")]
    public GameObject currentTrafficRoute;
    public GameObject nextWaypoint;
    public int currentWapointNumber;

    private NavMeshAgent _carNavmesh;
    private bool isiktaDuruyor = false;
    
    // Deadlock handling
    private float stoppedTimer = 0f;
    private float deadlockThreshold = 2.0f; // Seconds before trying to break deadlock
    private float ignoreObstaclesUntil = 0f;

    private void Start()
    {
        _carNavmesh = GetComponent<NavMeshAgent>();
        _carNavmesh.speed = carSpeed;
        _carNavmesh.acceleration = acceleration;
        
        // Randomize avoidance priority to help NavMeshAgent resolve conflicts naturally
        _carNavmesh.avoidancePriority = Random.Range(30, 70);
        
        if (obstacleMask == 0)
        {
            obstacleMask = LayerMask.GetMask("Default", "Car", "Player"); 
        }
    }

    private void Update()
    {
        if (isiktaDuruyor)
        {
            Stop();
            return;
        }

        // Deadlock Recovery: If we are ignoring obstacles, just move
        if (Time.time < ignoreObstaclesUntil)
        {
            Move();
            return;
        }

        Collider obstacleCollider;
        if (DetectObstacle(out float distanceToObstacle, out obstacleCollider))
        {
            // Obstacle detected
            if (distanceToObstacle < stopDistance)
            {
                Stop();
                
                // Check for Deadlock
                if (_carNavmesh.velocity.sqrMagnitude < 0.1f)
                {
                    stoppedTimer += Time.deltaTime;
                    if (stoppedTimer > deadlockThreshold)
                    {
                        ResolveDeadlock(obstacleCollider);
                    }
                }
            }
            else
            {
                // Slow down proportionally
                float factor = (distanceToObstacle - stopDistance) / (safeDistance - stopDistance);
                SlowDown(factor);
                stoppedTimer = 0f;
            }
        }
        else
        {
            // No obstacle, resume speed
            Move();
            stoppedTimer = 0f;
        }
    }

    void ResolveDeadlock(Collider obstacle)
    {
        if (obstacle == null) return;

        // Eğer ışıkta duruyorsak deadlock çözmeye çalışma, çünkü durmamız gerekiyor.
        if (isiktaDuruyor) return;

        AICar otherCar = obstacle.GetComponentInParent<AICar>();
        if (otherCar != null)
        {
            // Eğer diğer araç ışıkta duruyorsa, biz de beklemeliyiz (Onu geçmeye çalışma)
            if (otherCar.isiktaDuruyor)
            {
                stoppedTimer = 0f; // Timer'ı sıfırla ki sürekli deadlock denemesin
                return;
            }

            // If the other car is also stopped (or moving very slowly)
            if (otherCar.IsStopped())
            {
                // Deterministic priority: Higher InstanceID goes first
                if (this.GetInstanceID() > otherCar.GetInstanceID())
                {
                    // I have priority, ignore obstacles for a bit to clear the intersection
                    ignoreObstaclesUntil = Time.time + 3.0f;
                    stoppedTimer = 0f;
                    Debug.Log($"Deadlock resolved: {name} taking priority over {otherCar.name}");
                }
                else
                {
                    // I wait, reset timer to check again later
                    stoppedTimer = 0f; 
                }
            }
        }
        else
        {
            // Obstacle is not an AI Car (maybe Player or static object)
            // If it's static, we might be stuck forever, but let's not force move into walls.
            // If it's Player, we wait.
        }
    }

    public bool IsStopped()
    {
        return _carNavmesh.velocity.sqrMagnitude < 0.1f;
    }

    bool DetectObstacle(out float minDistance, out Collider obstacle)
    {
        minDistance = float.MaxValue;
        obstacle = null;
        bool detected = false;

        Vector3 origin = transform.position + Vector3.up * raycastOffset;
        Vector3 direction = transform.forward;

        // Center Ray
        if (CastRay(origin, direction, out float d1, out Collider c1))
        {
            if (d1 < minDistance) { minDistance = d1; obstacle = c1; }
            detected = true;
        }

        // Right Ray
        if (CastRay(origin + transform.right * sideRayOffset, direction, out float d2, out Collider c2))
        {
            if (d2 < minDistance) { minDistance = d2; obstacle = c2; }
            detected = true;
        }

        // Left Ray
        if (CastRay(origin - transform.right * sideRayOffset, direction, out float d3, out Collider c3))
        {
            if (d3 < minDistance) { minDistance = d3; obstacle = c3; }
            detected = true;
        }

        return detected;
    }

    bool CastRay(Vector3 origin, Vector3 direction, out float distance, out Collider hitCollider)
    {
        distance = float.MaxValue;
        hitCollider = null;
        RaycastHit hit;
        
        Debug.DrawRay(origin, direction * safeDistance, Color.yellow);

        // Use QueryTriggerInteraction.Ignore to avoid hitting triggers (like traffic zones)
        if (Physics.Raycast(origin, direction, out hit, safeDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            // 1. Self Detection Check
            if (hit.collider.transform.root == transform.root) return false;

            // 2. Tag Check
            bool tagMatch = false;
            if (tags.Length == 0) tagMatch = true;
            else
            {
                foreach (string t in tags)
                {
                    if (hit.transform.CompareTag(t))
                    {
                        tagMatch = true;
                        break;
                    }
                }
            }

            if (tagMatch)
            {
                distance = hit.distance;
                hitCollider = hit.collider;
                Debug.DrawLine(origin, hit.point, Color.red);
                return true;
            }
        }
        return false;
    }

    void Stop()
    {
        _carNavmesh.isStopped = true;
        _carNavmesh.velocity = Vector3.zero;
    }

    void SlowDown(float factor)
    {
        _carNavmesh.isStopped = false;
        _carNavmesh.speed = Mathf.Lerp(0, carSpeed, factor);
    }

    void Move()
    {
        if (nextWaypoint == null && currentTrafficRoute == null)
        {
            Stop();
            return;
        }

        _carNavmesh.isStopped = false;
        _carNavmesh.speed = Mathf.MoveTowards(_carNavmesh.speed, carSpeed, acceleration * Time.deltaTime);

        Vector3 targetPos = Vector3.zero;
        bool hasTarget = false;

        if (currentWapointNumber > 0)
        {
            if (currentTrafficRoute != null)
            {
                targetPos = currentTrafficRoute.transform.GetChild(currentWapointNumber - 1).position;
                hasTarget = true;
            }
        }
        else if (nextWaypoint != null)
        {
            targetPos = nextWaypoint.transform.position;
            hasTarget = true;
        }

        if (hasTarget)
        {
            _carNavmesh.SetDestination(targetPos);
            
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist <= 3f)
            {
                if (currentWapointNumber > 0)
                    currentWapointNumber -= 1;
            }
        }
    }

    public void TrafikIsigiDurumu(bool durmali)
    {
        isiktaDuruyor = durmali;
        if (durmali) Stop();
    }
}
