using System.Collections.Generic;
using UnityEngine;

public class CrosswalkManager : MonoBehaviour
{
    public static CrosswalkManager Instance;

    public List<PedestrianCrossZone> crossZones = new List<PedestrianCrossZone>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Register(PedestrianCrossZone zone)
    {
        if (!crossZones.Contains(zone))
            crossZones.Add(zone);
    }

    public void Unregister(PedestrianCrossZone zone)
    {
        if (crossZones.Contains(zone))
            crossZones.Remove(zone);
    }
}
