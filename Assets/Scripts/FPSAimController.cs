using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class FPSAimController : MonoBehaviour
{
    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public bool lockCursor = true;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 50f;
    public float fireRate = 0.2f;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;
    public float maxShootDistance = 100f;

    [Header("UI")]
    public Image crosshair;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    [Header("Game Settings")]
    public int scorePerHit = 10;
    public float gameDuration = 60f;
    public LayerMask targetLayer;

    private float xRotation = 0f;

    private int currentAmmo;
    private int currentScore;
    private float nextFireTime;
    private bool isReloading = false;
    private float gameTimer;
    private bool isGameActive = true;
    private Camera playerCamera;

    void Start()
    {
        currentAmmo = maxAmmo;
        currentScore = 0;
        gameTimer = gameDuration;

        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        UpdateUI();

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isGameActive) return;

        HandleMouseLook();
        UpdateCrosshairFeedback();

        gameTimer -= Time.deltaTime;
        if (gameTimer <= 0)
        {
            EndGame();
            return;
        }

        if (Input.GetButtonDown("Fire1") && !isReloading && currentAmmo > 0 && Time.time >= nextFireTime)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            lockCursor = false;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            lockCursor = true;
        }

        if (ammoText != null)
        {
            if (isReloading)
                ammoText.text = "RECARREGANDO...";
            else
                ammoText.text = $"Munição: {currentAmmo}/{maxAmmo}\nTempo: {Mathf.CeilToInt(gameTimer)}s";
        }
    }

    void HandleMouseLook()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
        else
            transform.Rotate(Vector3.up * mouseX);
    }

    void UpdateCrosshairFeedback()
    {
        if (crosshair == null || playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxShootDistance, targetLayer))
            crosshair.color = Color.red;
        else
            crosshair.color = Color.white;
    }

    void Shoot()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera não encontrada.");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("Fire Point não foi atribuído.");
            return;
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab não foi atribuído.");
            return;
        }

        nextFireTime = Time.time + fireRate;
        currentAmmo--;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxShootDistance, targetLayer))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(maxShootDistance);

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = shootDirection * bulletSpeed;

        Debug.DrawLine(firePoint.position, targetPoint, Color.red, 1f);

        Destroy(bullet, 5f);

        UpdateUI();

        if (crosshair != null)
            StartCoroutine(FlashCrosshair());
    }

    IEnumerator Reload()
    {
        isReloading = true;
        UpdateUI();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateUI();
    }

    IEnumerator FlashCrosshair()
    {
        if (crosshair == null) yield break;

        Color originalColor = crosshair.color;
        crosshair.color = Color.yellow;
        yield return new WaitForSeconds(0.05f);
        crosshair.color = originalColor;
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Pontos: {currentScore}";
    }

    void EndGame()
    {
        isGameActive = false;

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = $"Game Over!\nPontuação Final: {currentScore}";
        }

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestartGame()
    {
        currentScore = 0;
        currentAmmo = maxAmmo;
        gameTimer = gameDuration;
        isGameActive = true;
        isReloading = false;

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        UpdateUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        lockCursor = true;
        xRotation = 0f;
    }
}