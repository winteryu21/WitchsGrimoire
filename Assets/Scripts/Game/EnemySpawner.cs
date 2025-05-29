using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRadius = 10f;

    public void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = GetRandomEdgePosition();
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
    }

    Vector2 GetRandomEdgePosition()
    {
        float x = Random.value < 0.5f ? -spawnRadius : spawnRadius;
        float y = Random.Range(-spawnRadius, spawnRadius);

        if (Random.value < 0.5f)
        {
            float temp = x;
            x = Random.Range(-spawnRadius, spawnRadius);
            y = Random.value < 0.5f ? -spawnRadius : spawnRadius;
        }

        return new Vector2(x, y);
    }
}
