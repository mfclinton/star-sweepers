using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public List<string> tagsToSpawn;
    public Vector2 timeRange;
    public Vector2 numRange;
    private BoxCollider2D boxCollider;
    
    [SerializeField] private float shipMoveDeltaSpawnRequirement = 1f;
    private float lastShipXPos = 0f;
    private Ship ship;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        ship = FindObjectOfType<Ship>();
        lastShipXPos = ship.transform.position.x;
    }

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(timeRange.x, timeRange.y));

            // Don't spawn if ship hasn't moved enough
            Debug.Log($"SHIPPP DIST {ship.transform.position.x - lastShipXPos}");
            if (ship.transform.position.x - lastShipXPos < shipMoveDeltaSpawnRequirement)
                continue;

            lastShipXPos = ship.transform.position.x;

            int numToSpawn = Mathf.RoundToInt(Random.Range(numRange.x, numRange.y));
            string tagToSpawn = tagsToSpawn[Random.Range(0, tagsToSpawn.Count)];

            for (int i = 0; i < numToSpawn; i++)
            {
                Vector3 position = new Vector3(
                    Random.Range(boxCollider.bounds.min.x, boxCollider.bounds.max.x),
                    Random.Range(boxCollider.bounds.min.y, boxCollider.bounds.max.y),
                    0
                );

                GameObject obj = ObjectPooler.Instance.SpawnFromPool(tagToSpawn, position, Quaternion.identity);
                HandleSpecialRules(tagToSpawn, obj);
            }
        }
    }

    private void HandleSpecialRules(string tag, GameObject obj)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if(tag == "CometHazard")
        {
            rb.velocity = Vector2.left * Random.Range(2.5f, 7f);
        }
        else if (rb != null)
        {
            // Give it a random velocity and ang velocity
            rb.velocity = Random.insideUnitCircle * Random.Range(.1f, .35f);
            rb.angularVelocity = Random.Range(-20f, 20f);
        }
    }
}
