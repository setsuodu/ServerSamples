using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;
    public float lifetime = 2f;

    private void OnEnable()
    {
        Invoke(nameof(Deactivate), lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.TakeDamage(damage);
            gameObject.SetActive(false);
        }
    }

    void Deactivate() => gameObject.SetActive(false);

    private void OnDisable() => CancelInvoke();
}