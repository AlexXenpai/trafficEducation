using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class PedestrianAI : MonoBehaviour
{
    [Header("Settings")]
    public float wanderRadius = 20f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isWaiting = false;
    
    // Animator parametreleri
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsWalkingParam = Animator.StringToHash("IsWalking");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // Agent ayarları (CityPeople için uygun değerler)
        agent.speed = Random.Range(1.5f, 2.5f);
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        
        // İlk hedefe git
        SetNewRandomDestination();
    }

    void Update()
    {
        if (agent == null || !agent.enabled) return;
        
        // Animator entegrasyonu
        UpdateAnimation();

        // Hedefe vardık mı?
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                StartCoroutine(WaitAndMove());
            }
        }
    }
    
    void UpdateAnimation()
    {
        if (animator == null) return;
        
        float speed = agent.velocity.magnitude;
        bool isMoving = speed > 0.1f;
        
        // Speed parametresi (float) - bazı animator'lar bunu kullanır
        if (HasParameter(SpeedParam))
        {
            animator.SetFloat(SpeedParam, speed);
        }
        
        // IsWalking parametresi (bool) - bizim oluşturduğumuz animator bunu kullanır
        if (HasParameter(IsWalkingParam))
        {
            animator.SetBool(IsWalkingParam, isMoving);
        }
    }
    
    bool HasParameter(int paramHash)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == paramHash)
                return true;
        }
        return false;
    }

    IEnumerator WaitAndMove()
    {
        isWaiting = true;
        
        // Rastgele bekleme süresi
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        SetNewRandomDestination();
        isWaiting = false;
    }

    void SetNewRandomDestination()
    {
        if (agent == null || !agent.enabled) return;
        
        Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
        agent.SetDestination(newPos);
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, 10.0f, layermask);

        return navHit.position;
    }
}
