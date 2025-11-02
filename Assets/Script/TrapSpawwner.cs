using System.Collections;
using UnityEngine;

public class TrapSpawner : MonoBehaviour
{
    [Header("Blade Settings")]
    public GameObject bladePrefab;     
    public Transform spawnPoint;        
    public float rotationSpeed = 180f;  
    public float lifeTime = 5f;         
    public float respawnDelay = 0f;     

    private GameObject spawnedBlade;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }


    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            
            spawnedBlade = Instantiate(bladePrefab, spawnPoint.position, Quaternion.identity);

           
            BladeSpin spin = spawnedBlade.GetComponent<BladeSpin>();
            if (spin != null)
                spin.rotationSpeed = rotationSpeed;

            
            yield return new WaitForSeconds(lifeTime);

            
            if (spawnedBlade != null)
                Destroy(spawnedBlade);

            
            yield return new WaitForSeconds(respawnDelay);
        }
    }
}
