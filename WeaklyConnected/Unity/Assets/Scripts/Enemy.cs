using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 2;
    public int scoreValue = 10;
    public float speed = 3f;
    public GameObject explosionPrefab;

    private void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        if (transform.position.y < -6f) Destroy(gameObject);
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0) Die();
    }

    void Die()
    {
        GameManager.Instance.AddScore(scoreValue);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}