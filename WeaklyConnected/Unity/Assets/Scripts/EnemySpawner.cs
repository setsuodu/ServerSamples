using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public float spawnRate = 1.5f;
    public Vector2 spawnArea = new Vector2(8f, 1f);

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnRate);
        }
    }

    void SpawnEnemy()
    {
        int idx = Random.Range(0, enemyPrefabs.Length);
        float x = Random.Range(-spawnArea.x, spawnArea.x);
        Vector3 pos = new Vector3(x, 6f, 0);
        Instantiate(enemyPrefabs[idx], pos, Quaternion.identity);
    }
}