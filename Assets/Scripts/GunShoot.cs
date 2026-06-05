using UnityEngine;
using TMPro;

public class GunShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    public int ammo = 10;
    public TextMeshProUGUI ammoText;

    void Start()
    {
        UpdateAmmoUI();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && ammo > 0)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        ammo--;
        UpdateAmmoUI();

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * bulletSpeed;
        }
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = "Munição: " + ammo;
        }
    }
}