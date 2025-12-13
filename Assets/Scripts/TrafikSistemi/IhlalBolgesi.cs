using UnityEngine;
using System;


public class IhlalBolgesi : MonoBehaviour
{/*
    public TrafikIsigi bagliTrafikIsigi; // Hangi ýþýðý denetliyoruz?

    // Arabanýn etiketi (Tag) mutlaka "Player" olmalý.
    void OnTriggerEnter(Collider other)
    {
        // 1. OYUNCU ÝSE CEZA KES (Eski Kodun)
        if (other.CompareTag("Player"))
        {
            KontrolEt();
        }

        // 2. YAPAY ZEKA ÝSE DURDUR (Yeni Kod)
        if (other.CompareTag("AI_Araba"))
        {
            AICar yapayZeka = other.GetComponent<AICar>();
            if (yapayZeka != null)
            {
                // Eðer ýþýk Kýrmýzý veya Sarý ise DUR emri ver
                if (bagliTrafikIsigi.suankiDurum != TrafikIsigi.IsikDurumu.Yesil)
                {
                    yapayZeka.TrafikIsigiDurumu(true); // DUR
                }
            }
        }
    }

    // OnTriggerStay: Kutu içinde beklerken ýþýk yeþile dönerse?
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("AI_Araba"))
        {
            AICar yapayZeka = other.GetComponent<AICar>();
            if (yapayZeka != null)
            {
                // Iþýk Yeþile döndüyse GEÇ emri ver
                if (bagliTrafikIsigi.suankiDurum == TrafikIsigi.IsikDurumu.Yesil)
                {
                    yapayZeka.TrafikIsigiDurumu(false); // DEVAM ET
                }
                // Iþýk Kýrmýzýya döndüyse DUR emri ver (Sonradan kýrmýzý yandýysa)
                else
                {
                    yapayZeka.TrafikIsigiDurumu(true); // DUR
                }
            }
        }
    }

    // OnTriggerExit: Kutudan çýkýnca (Emin olmak için)
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("AI_Araba"))
        {
            AICar yapayZeka = other.GetComponent<AICar>();
            if (yapayZeka != null)
            {
                yapayZeka.TrafikIsigiDurumu(false); // Özgürsün
            }
        }
    }

    void KontrolEt()
    {
        if (bagliTrafikIsigi.suankiDurum == TrafikIsigi.IsikDurumu.Kirmizi)
        {
            // ESKÝ KOD: Debug.Log("CEZA! Kýrmýzý Iþýk Ýhlali!");

            // YENÝ KOD: Patron'a þikayet et
            GameManager.Instance.CezaVer(20, "Kýrmýzý Iþýk Ýhlali");

            // Ceza sürekli artmasýn diye bu objeyi geçici olarak kapatabiliriz
            // ya da bir 'bool cezaKesildi' deðiþkeni ekleyebiliriz.
            // Þimdilik basit tutuyoruz.
        }
        else
        {
            Debug.Log("Güvenli geçiþ.");
        }
    }*/
}