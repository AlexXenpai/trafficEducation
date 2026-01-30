using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static Action<int> OnPuanDegisti;
    public static Action<string> OnCezaYendi;
    public static Action<float> OnTimerTick;
    public static Action<int, List<string>> OnGameSuccess;

    [Header("Game Settings")]
    public float gameDuration = 60f;
    private float currentTimer;
    public List<string> mistakesLog = new List<string>();

    [Header("Mode Objects")]
    public GameObject playerCar;
    public GameObject pedestrian;
    [Header("XR")]
    public GameObject xrOrigin;


    [Header("UI")]
    public GameObject entryPanel;
    public GameObject cezaUyariText;

    [Header("Game Over UI")]
    public CanvasGroup gameOverGroup;
    public TextMeshProUGUI gameOverText;
    public float gameOverShowSeconds = 2.5f;

    bool gameOverStarted;

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
        currentTimer = gameDuration;
        mistakesLog.Clear();

        if (entryPanel != null)
            entryPanel.SetActive(true);

        if (playerCar != null) playerCar.SetActive(false);
        if (pedestrian != null) pedestrian.SetActive(false);
    }

    private void Update()
    {
        if (oyunDevamEdiyor)
        {
            currentTimer -= Time.deltaTime;
            if (currentTimer < 0) currentTimer = 0;
            
            OnTimerTick?.Invoke(currentTimer);

            if (currentTimer <= 0)
            {
                GameSuccess();
            }
        }
    }

    // ---------------- MODE SELECTION ----------------
    public void StartCarMode()
    {
        Time.timeScale = 1;
        if (entryPanel != null) entryPanel.SetActive(false);

        if (playerCar != null) playerCar.SetActive(true);
        if (pedestrian != null) pedestrian.SetActive(false);

        SetCameraMode(CameraMode.Car);

        UIManager.Instance?.ShowModeInfo(
            "ARABA MODU - DİKKAT\n" +
            "• Binaya çarpma: -10\n" +
            "• Arabaya çarpma: -10\n" +
            "• Yayaya çarpma: -20\n" +
            "• Sinyal vermeden dönüş: -10\n" +
            "• Yol dışına çıkma: -10 (hemen) + 5 saniyede bir tekrar\n" +
            "Kurallara dikkat edin ve puan kaybetmeyin!"
        );
    }

    public void StartPedestrianMode()
    {
        Time.timeScale = 1;
        if (entryPanel != null) entryPanel.SetActive(false);

        if (playerCar != null) playerCar.SetActive(false);
        if (pedestrian != null) pedestrian.SetActive(true);

        SetCameraMode(CameraMode.Pedestrian);

        UIManager.Instance?.ShowModeInfo(
            "YAYA MODU - DİKKAT\n" +
            "• Yaya geçidi dışından karşıya geçme: -20\n" +
            "• Kırmızı ışıkta karşıya geçme: -15\n" +
            "Kurallara dikkat edin ve puan kaybetmeyin!"
        );
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

        mistakesLog.Add($"{sebep}: -{cezaMiktari} Puan");

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
        if (gameOverStarted) return;
        gameOverStarted = true;

        oyunDevamEdiyor = false;
        Debug.Log("OYUN BITTI");

        StartCoroutine(GameOverRoutine());
    }

    void GameSuccess()
    {
        if (gameOverStarted) return;
        gameOverStarted = true;

        oyunDevamEdiyor = false;
        Debug.Log("SIMULASYON BASARIYLA TAMAMLANDI");

        OnGameSuccess?.Invoke(toplamPuan, mistakesLog);
        
        // Stop time
        Time.timeScale = 0;
    }

    IEnumerator GameOverRoutine()
    {
        // Ceza yazısı coroutine'ini iptal etme (StopAllCoroutines) gibi yan etkiler istemiyoruz.
        // Bu yüzden ayrı coroutine.

        // Ekranı göster
        if (gameOverText != null)
            gameOverText.text = "Simülasyon başarısız!\nTekrar deneyin.";

        if (gameOverGroup != null)
        {
            gameOverGroup.alpha = 1f;
            gameOverGroup.interactable = false;
            gameOverGroup.blocksRaycasts = true;
        }

        // Oyunu durdur
        Time.timeScale = 0;

        // TimeScale 0 iken de bekleyebilmek için realtime kullan
        yield return new WaitForSecondsRealtime(gameOverShowSeconds);

        // Başlangıç ekranına dönmek için sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
