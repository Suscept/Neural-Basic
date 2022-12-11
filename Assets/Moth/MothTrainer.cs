using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MothTrainer : MonoBehaviour
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

        generationTimer -= 1f / 60f;
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
        food.transform.position = new Vector3(Random.Range(-spawnRange, spawnRange), 0.5f, Random.Range(-spawnRange, spawnRange));
    }

    public void SpawnActors(int amount)
    {
        for (int a = 0; a < amount; a++)
        {
            //Vector3 spawnPos = new Vector3(Random.Range(-spawnRange, spawnRange), 0.5f, 0);
            Vector3 spawnPos = Vector3.up * 0.5f;
            GameObject actorCache = Instantiate(actor, spawnPos, Quaternion.Euler(0, Random.Range(-180f, 180f), 0));
            //actorCache.GetComponent<AI>().InitNetwork();
            actors.Add(actorCache);
        }
    }

    public void SpawnActors(int amount, AI.Network brain)
    {
        for (int a = 0; a < amount; a++)
        {
            //Vector3 spawnPos = new Vector3(Random.Range(-spawnRange, spawnRange), 1, 0);
            Vector3 spawnPos = Vector3.up * 0.5f;
            GameObject actorCache = Instantiate(actor, spawnPos, Quaternion.Euler(0,Random.Range(-180f, 180f), 0));
            actorCache.GetComponent<AI>().network = NewFromTemplate(brain);
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

    // Why do this? I will break your spine if you remove it
    private AI.Network NewFromTemplate(AI.Network network)
    {
        AI.Network newNetwork = new AI.Network();

        newNetwork.layers = new AI.Layer[network.layers.Length];
        for(int l = 0; l < network.layers.Length; l++)
        {
            newNetwork.layers[l] = new AI.Layer();

            newNetwork.layers[l].neurons = new AI.Neuron[network.layers[l].neurons.Length];
            for (int n = 0; n < network.layers[l].neurons.Length; n++)
            {
                newNetwork.layers[l].neurons[n] = new AI.Neuron();

                if (l == 0)
                    continue;

                newNetwork.layers[l].neurons[n].weights = new float[network.layers[l].neurons[n].weights.Length];
                for (int w = 0; w < network.layers[l].neurons[n].weights.Length; w++)
                {
                    newNetwork.layers[l].neurons[n].weights[w] = network.layers[l].neurons[n].weights[w];
                }
            }
        }

        return newNetwork;
    }
}
