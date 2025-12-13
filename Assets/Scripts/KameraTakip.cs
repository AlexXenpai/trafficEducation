using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KameraTakip : MonoBehaviour
{
    public Transform target; // Araba
    public Vector3 offset = new Vector3(0, 5, -8); // Arkadan ve yukarýdan bakýþ
    public float smoothSpeed = 0.125f;

    void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        // Lerp ile yumuþak geçiþ saðla, titremeyi önler
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Arabaya bak ama kafayý kilitleme (LookAt kullanma), 
        // Kullanýcý VR'da kafasýný çevirip etrafa bakabilmeli.
        // Sadece pozisyonu takip et, rotasyonu kullanýcýya býrak veya çok hafif döndür.

        // Basitçe aracýn arkasýna dönmesi için:
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target.forward), smoothSpeed);
    }
}