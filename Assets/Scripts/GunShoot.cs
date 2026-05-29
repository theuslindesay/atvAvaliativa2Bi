using UnityEngine;
using TMPro;
using System.Collections;

public class GunShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Camera playerCamera;
    public TextMeshProUGUI ammoText;

    public float bulletSpeed = 25f;
    public float fireRate = 0.2f;

    public int magazineSize = 10;
    public int currentAmmo = 10;
    public int reserveAmmo = 40;
    public float reloadTime = 1.5f;

    private float nextFireTime;
    private bool isReloading = false;

    void Start()
    {
        UpdateAmmoUI();
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
            return;
        }

        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || playerCamera == null) return;

        currentAmmo--;
        UpdateAmmoUI();

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            targetPoint = hit.point;
        else
            targetPoint = ray.origin + ray.direction * 100f;

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;

        GameObject bulletInstance = Instantiate(
            bulletPrefab,
            firePoint.position + shootDirection * 0.15f,
            Quaternion.LookRotation(shootDirection)
        );

        Rigidbody rb = bulletInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = shootDirection * bulletSpeed;
        }
    }

    void StartReload()
    {
        if (isReloading) return;
        if (currentAmmo == magazineSize) return;
        if (reserveAmmo <= 0) return;

        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;

        if (ammoText != null)
            ammoText.text = "Recarregando...";

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = "Ammo: " + currentAmmo + " / " + reserveAmmo;
    }
}