using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 25;

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bala colidiu com: " + collision.gameObject.name);

        ZombieHealth zombie = collision.gameObject.GetComponentInParent<ZombieHealth>();

        if (zombie != null)
        {
            Debug.Log("ZombieHealth encontrado, aplicando dano");
            zombie.TakeDamage(damage);
        }
        else
        {
            Debug.Log("ZombieHealth NÃO encontrado");
        }

        Destroy(gameObject);
    }
}