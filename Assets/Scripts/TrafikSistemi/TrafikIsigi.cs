using System.Collections;
using UnityEngine;

public class TrafikIsigi : MonoBehaviour
{
    public enum IsikDurumu { Kirmizi, Sari, Yesil }
    public IsikDurumu suankiDurum;

    [Header("Modelin Parçaları")]
    public Renderer kirmiziLambaObjesi;
    public Renderer sariLambaObjesi;
    public Renderer yesilLambaObjesi;

    [Header("Fiziksel Engel (Kritik)")]
    public GameObject ihlalBolgesiObjesi; // <-- BUNU EKLEDİK. Inspector'dan buraya o objeyi sürükle.

    // Işık Parlaklığı için renkler
    private Color parlakKirmizi = new Color(1f, 0f, 0f, 1f);
    private Color parlakSari = new Color(1f, 0.92f, 0.016f, 1f);
    private Color parlakYesil = new Color(0f, 1f, 0f, 1f);
    private Color sonukRenk = Color.black;

    // Süreler
    public float kirmiziSure = 5f;
    public float sariSure = 2f;
    public float yesilSure = 5f;

    void Start()
    {
        StartCoroutine(TrafikIsigiDongusu());
    }

    public void DisaridanDurumAta(IsikDurumu yeniDurum)
    {
        StopAllCoroutines();
        IsiklariGuncelle(yeniDurum);
    }

    IEnumerator TrafikIsigiDongusu()
    {
        while (true)
        {
            IsiklariGuncelle(IsikDurumu.Kirmizi);
            yield return new WaitForSeconds(kirmiziSure);

            IsiklariGuncelle(IsikDurumu.Sari); // Kırmızıdan Yeşile geçerken Sarı (Hazırlan)
            yield return new WaitForSeconds(sariSure);

            IsiklariGuncelle(IsikDurumu.Yesil);
            yield return new WaitForSeconds(yesilSure);

            IsiklariGuncelle(IsikDurumu.Sari); // Yeşilden Kırmızıya geçerken Sarı (Durmaya hazırlan)
            yield return new WaitForSeconds(sariSure);
        }
    }

    void IsiklariGuncelle(IsikDurumu durum)
    {
        suankiDurum = durum;

        // 1. Görsel Güncelleme (Renkler)
        if (kirmiziLambaObjesi) kirmiziLambaObjesi.material.color = sonukRenk;
        if (sariLambaObjesi) sariLambaObjesi.material.color = sonukRenk;
        if (yesilLambaObjesi) yesilLambaObjesi.material.color = sonukRenk;

        switch (durum)
        {
            case IsikDurumu.Kirmizi:
                if (kirmiziLambaObjesi) kirmiziLambaObjesi.material.color = parlakKirmizi;
                break;
            case IsikDurumu.Sari:
                if (sariLambaObjesi) sariLambaObjesi.material.color = parlakSari;
                break;
            case IsikDurumu.Yesil:
                if (yesilLambaObjesi) yesilLambaObjesi.material.color = parlakYesil;
                break;
        }

        // 2. FİZİKSEL GÜNCELLEME (İhlal Bölgesi Kontrolü)
        if (ihlalBolgesiObjesi != null)
        {
            // Eğer ışık KIRMIZI veya SARI ise engel AKTİF olsun (Araba çarpsın ve dursun).
            // Sadece YEŞİL olduğunda engel kalksın.
            if (durum == IsikDurumu.Kirmizi || durum == IsikDurumu.Sari)
            {
                ihlalBolgesiObjesi.SetActive(true);
            }
            else // Durum Yeşil ise
            {
                ihlalBolgesiObjesi.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("TrafikIsigi: Ihlal Bolgesi Objesi atanmamış! Arabalar durmaz.");
        }
    }
}