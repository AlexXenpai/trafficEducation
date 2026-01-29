using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static Action<int> OnPuanDegisti;
    public static Action<string> OnCezaYendi;

    [Header("Mode Objects")]
    public GameObject playerCar;
    public GameObject pedestrian;
    [Header("XR")]
    public GameObject xrOrigin;


    [Header("UI")]
    public GameObject entryPanel;
    public GameObject cezaUyariText;

    [Header("Game State")]
    public int toplamPuan = 100;
    public bool oyunDevamEdiyor = true;

    // ---------------- CAMERA ----------------
    public enum CameraMode
    {
        Car,
        Pedestrian
    }

    private Camera mainCam;
    // private CameraFollow cameraFollow; // ARTIK KULLANILMIYOR, KameraTakip.cs kullanılıyor
    private TrackedPoseDriver trackedPoseDriver;

    // ---------------- UNITY ----------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        mainCam = Camera.main;
        // cameraFollow = mainCam.GetComponent<CameraFollow>();
        if (mainCam != null) trackedPoseDriver = mainCam.GetComponent<TrackedPoseDriver>();

        Time.timeScale = 0;
        if (entryPanel != null)
            entryPanel.SetActive(true);

        if (playerCar != null) playerCar.SetActive(false);
        if (pedestrian != null) pedestrian.SetActive(false);
    }

    // ---------------- MODE SELECTION ----------------
    public void StartCarMode()
    {
        Time.timeScale = 1;
        if (entryPanel != null) entryPanel.SetActive(false);

        if (playerCar != null) playerCar.SetActive(true);
        if (pedestrian != null) pedestrian.SetActive(false);

        SetCameraMode(CameraMode.Car);
    }

    public void StartPedestrianMode()
    {
        Time.timeScale = 1;
        if (entryPanel != null) entryPanel.SetActive(false);

        if (playerCar != null) playerCar.SetActive(false);
        if (pedestrian != null) pedestrian.SetActive(true);

        SetCameraMode(CameraMode.Pedestrian);
    }

    // ---------------- CAMERA LOGIC ----------------
    void SetCameraMode(CameraMode mode)
    {
        // Kamera kontrolü artık ModSecimi.cs ve KameraTakip.cs üzerinden yapılıyor.
        // Burada sadece TrackedPoseDriver'ı yönetebiliriz gerekirse.
        
        if (trackedPoseDriver != null)
        {
            // Her iki modda da kafa takibi açık olabilir, 
            // ancak araba modunda sadece rotasyon, yaya modunda pozisyon+rotasyon istenebilir.
            // Şimdilik varsayılan olarak açık bırakıyoruz.
            trackedPoseDriver.enabled = true;
        }
    }

    // ---------------- CEZA SISTEMI ----------------
    public void CezaYe(int miktar)
    {
        CezaVer(miktar, "Kural İhlali");
    }

    public void CezaVer(int cezaMiktari, string sebep)
    {
        if (!oyunDevamEdiyor) return;

        toplamPuan -= cezaMiktari;
        if (toplamPuan < 0) toplamPuan = 0;

        Debug.Log($"Ceza: {sebep} | Yeni Puan: {toplamPuan}");

        OnCezaYendi?.Invoke(sebep);
        OnPuanDegisti?.Invoke(toplamPuan);

        if (cezaUyariText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowCezaTextRoutine());
        }

        if (toplamPuan <= 0)
            OyunBitti();
    }

    IEnumerator ShowCezaTextRoutine()
    {
        cezaUyariText.SetActive(true);
        yield return new WaitForSeconds(3f);
        cezaUyariText.SetActive(false);
    }

    void OyunBitti()
    {
        oyunDevamEdiyor = false;
        Debug.Log("OYUN BITTI");

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
