using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaldTrainer : MonoBehaviour
{
    public GameObject actor;
    public GameObject food;

    public float spawnRange;

    public int actorAmount;
    public int foodAmount;

    public float generationTime;

    public float mutateAmount;

    public float fastTime = 10;

    private bool isFast;

    private float generationTimer;

    private List<GameObject> actors = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        generationTimer = generationTime;
        Create();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isFast = !isFast;
            float timeSpeed = ((fastTime - 1) * System.Convert.ToInt32(isFast)) + 1;
            Time.timeScale = timeSpeed;
            Debug.Log("Set time to " + timeSpeed);
        }

        generationTimer -= Time.deltaTime;
        if (generationTimer <= 0)
        {
            generationTimer = generationTime;
            InitGeneration();
        }
    }

    public bool RandToggle(float min, float max, float t)
    {
        float chance = min / max * t;
        return false;
    }

    public void Create()
    {
        SpawnActors(actorAmount);
        SpawnFoods(foodAmount);
    }

    public void SpawnFoods(int amount)
    {
        food.transform.position = new Vector3(Random.Range(-spawnRange, spawnRange), 1, 0);
    }

    public void SpawnActors(int amount)
    {
        for (int a = 0; a < amount; a++)
        {
            Vector3 spawnPos = new Vector3(Random.Range(-spawnRange, spawnRange), 1, 0);
            GameObject actorCache = Instantiate(actor, spawnPos, Quaternion.identity);
            //actorCache.GetComponent<AI>().InitNetwork();
            actors.Add(actorCache);
        }
    }

    public void SpawnActors(int amount, AI.Network brain)
    {
        for (int a = 0; a < amount; a++)
        {
            Vector3 spawnPos = new Vector3(Random.Range(-spawnRange, spawnRange), 1, 0);
            GameObject actorCache = Instantiate(actor, spawnPos, Quaternion.identity);
            actorCache.GetComponent<AI>().network = brain;
            actorCache.GetComponent<AI>().Mutate(mutateAmount);
            actors.Add(actorCache);
        }
    }

    public void InitGeneration()
    {
        SpawnFoods(foodAmount);

        // LOCATE THE STRONG
        GameObject best = actors[0];
        float score = actors[0].GetComponent<AI>().score;
        for (int i = 1; i < actors.Count; i++)
        {
            score += actors[i].GetComponent<AI>().score;
            if (actors[i].GetComponent<AI>().score > best.GetComponent<AI>().score)
                best = actors[i];
        }
        score /= actors.Count;
        Debug.Log("Avg score " + score);

        AI.Network bestBrain = best.GetComponent<AI>().network;

        // Clear actors
        foreach (GameObject actor in actors)
        {
            Destroy(actor);
        }
        actors.Clear();

        // ASEXUAL SEEEEEEEEEEEEEEEEX
        SpawnActors(actorAmount, bestBrain);
    }
}
