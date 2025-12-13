using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AICar : MonoBehaviour
{
    public float safeDistance = 2f;
    public float carSpeed = 5f;
    public string[] tags;

    public GameObject currentTrafficRoute;
    public GameObject nextWaypoint;
    public int currentWapointNumber;

    private NavMeshAgent _carNavmesh;

    private bool isiktaDuruyor = false;
    private void Start()
    {
        _carNavmesh = this.gameObject.GetComponent<NavMeshAgent>();
        _carNavmesh.speed = carSpeed;
    }

    private void Update()
    {
        RaycastHit hit;

        // Varsayýlan olarak her karede hareket etmeye çalýþmalýyýz.
        // Durdurucu bir engel çýkarsa aþaðýda fikrimizi deðiþtireceðiz.
        bool engelVar = false;

        // Raycast'i IF içinde kullan. Çarparsa TRUE döner.
        if (Physics.Raycast(transform.position, transform.forward, out hit, safeDistance))
        {
            // 1. Etiket Kontrolü
            for (int i = 0; i < tags.Length; i++)
            {
                if (hit.transform.CompareTag(tags[i]))
                {
                    // 2. Mesafe Kontrolü (Ýstediðin Özellik)
                    // Zaten safeDistance kadar uzaða bakýyoruz ama 
                    // ekstra garantici olmak istersen veya farklý tepki vereceksen:
                    if (hit.distance <= 3.5f) // Örn: 2 metreden yakýnsa
                    {
                        engelVar = true;
                        Stop(); // Çakýl
                        break; // Döngüyü kýr, engeli bulduk.
                    }
                }
            }
        }

        // Eðer engel yoksa hareket etmeye devam et
        if (!engelVar)
        {
            Move();
        }

        // Debug Çizgisi: Sahne ekranýnda ýþýný gör ki ne yaptýðýný anla.
        Debug.DrawRay(transform.position, transform.forward * safeDistance, Color.red);
    }

    void Stop()
    {
        _carNavmesh.speed = 0;
    }

    void Move()
    {
        if (nextWaypoint == null)
        {
            _carNavmesh.speed = 0;
        }

        if (currentWapointNumber > 0)
        {
            if (_carNavmesh.speed == 0)
                _carNavmesh.speed = carSpeed;

            _carNavmesh.SetDestination(currentTrafficRoute.transform.GetChild(currentWapointNumber - 1).transform.position);

        }
        else
        {
            if (nextWaypoint != null)
            {
                if (_carNavmesh.speed == 0)
                    _carNavmesh.speed = carSpeed;
                _carNavmesh.SetDestination(nextWaypoint.transform.position);
            }
        }

        if (currentWapointNumber > 0)
        {
            float distance = Vector3.Distance(transform.position, currentTrafficRoute.transform.GetChild(currentWapointNumber - 1).transform.position);
            if (distance <= 2)
                currentWapointNumber -= 1;
        }
        else
        {
            /*
            if (nextWaypoint != null)
            {
                float distance = Vector3.Distance(transform.position, nextWaypoint.transform.position);
                if (distance <= 1)
                {
                    currentWapointNumber = 2;
                    currentTrafficRoute = nextWaypoint.transform.parent.gameObject;
                }
            }*/
            
        }
    }


    public void TrafikIsigiDurumu(bool durmali) => isiktaDuruyor = durmali;

}
