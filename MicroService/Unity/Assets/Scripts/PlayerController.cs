using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 8f;
    public Vector2 bounds = new Vector2(8f, 4.5f); // 屏幕边界

    [Header("射击")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    private float nextFireTime;

    private void Update()
    {
        Move();
        Shoot();
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, v, 0).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        // 限制在屏幕内
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -bounds.x, bounds.x);
        pos.y = Mathf.Clamp(pos.y, -bounds.y, bounds.y);
        transform.position = pos;
    }

    void Shoot()
    {
        if (Time.time >= nextFireTime && Input.GetKey(KeyCode.Space))
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            nextFireTime = Time.time + fireRate;
        }
    }
}