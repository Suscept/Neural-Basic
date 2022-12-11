using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Trainer : MonoBehaviour
{
    public static Trainer instance;
    public const float trainingFramerate = 60;
    public static float trainingDeltatime;

    [Header("Agents")]
    public GameObject agent;
    
    public int agentAmount = 1;

    public bool resetPhysicalAgents = true;

    [Header("Generation")]
    public TrainingMode trainingMode;
    public float generationTime = 10;
    public bool reloadScene = true;
    public bool logFitness = true;
    public float logRate = 0.5f;

    [Header("Mutation")]
    public float mutateAmount = 0.01f;
    public int mutations = 1;
    public float initialWeightRange = 2;

    private bool isFast;

    private float generationTimer;
    private int generationCount;

    private List<GameObject> agents = new List<GameObject>();

    private List<AI.Network> networkCache = new List<AI.Network>();

    private bool loadingScene;

    private int unfinishedAgents;

    private float logTimer;

    private bool newGeneration;

    public List<int> sort1 = new List<int>();
    public List<int> sort2 = new List<int>();

    public enum TrainingMode { timed, waitForAll};
    
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        trainingDeltatime = 1 / trainingFramerate;
    }

    // Start is called before the first frame update
    void Start()
    {
        AI agentAi = agent.GetComponent<AI>();
        for (int i = 0; i < agentAmount; i++)
        {
            SpawnAgent(AI.NewNetwork(agentAi.inputs.Length, agentAi.outputs.Length, agentAi.hiddenLayers, initialWeightRange), true, i);
        }

        generationTimer = generationTime;


        int cum = sort1.Count;
        for (int i = 0; i < cum; i++)
        {
            int bestAi = sort1[0];
            int bestIndex = 0;
            for (int z = 0; z < sort1.Count; z++)
            {
                int fart = sort1[z];
                if (fart > bestAi)
                {
                    bestAi = fart;
                    bestIndex = z;
                }
            }
            sort1.RemoveAt(bestIndex);
            sort2.Add(bestAi);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isFast)
                Application.targetFrameRate = 10000;
            else
                Application.targetFrameRate = 60;

            isFast = !isFast;
        }

        generationTimer -= trainingDeltatime;
        if (generationTimer <= 0 && trainingMode == TrainingMode.timed || newGeneration)
        {
            generationTimer = generationTime;
            NextGeneration();
        }

        logTimer -= Time.deltaTime;
    }

    public GameObject SpawnAgent(AI.Network network, bool spawn, int agentID)
    {
        if (!agent)
            Debug.LogError("No agent prefab!");
        if (network == null)
            Debug.LogError("No template network provided!");

        AI.Network net = NewFromTemplate(network);

        if (spawn)
        {
            GameObject agentCache = Instantiate(agent);
            agentCache.GetComponent<AI>().network = net;

            agents.Add(agentCache);

            return agentCache;
        }

        // Re use existing agent instance for big proformance
        AI agentAi = agents[agentID].GetComponent<AI>();
        agentAi.network = net;
        agentAi.score = 0;
        agents[agentID].SendMessage("Start", SendMessageOptions.DontRequireReceiver);
        return agents[agentID];
    }

    public void NextGeneration()
    {
        List<AI.Network> networks = GetAiSorted(out float bestScore);

        if (logFitness && logTimer <= 0)
            Debug.Log("Fitness: " + bestScore + " Generation: " + generationCount);
        generationCount++;

        // Cull low scoring networks
        for (int i = 0; i < networks.Count; i++)
        {
            if (ShouldCull(i, networks.Count))
                networks.RemoveAt(i);
        }

        // Reset scene
        networkCache = networks;

        if (reloadScene)
        {
            loadingScene = true;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            SceneManager.sceneLoaded += SceneLoaded;

            return;
        }

        if (resetPhysicalAgents)
        {
            foreach (GameObject agent in agents)
                Destroy(agent);

            agents.Clear();
        }

        FreshGeneration();
    }

    public void MarkAgentComplete()
    {
        unfinishedAgents--;
        if (unfinishedAgents <= 0 && trainingMode == TrainingMode.waitForAll)
            newGeneration = true;
    }

    private List<AI.Network> GetAiSorted(out float bestScore)
    {
        List<GameObject> unsorted = new List<GameObject>(agents);
        List<AI.Network> sorted = new List<AI.Network>();

        float highestScore = unsorted[0].GetComponent<AI>().score;
        int unsortedCount = agents.Count;
        // Loop over all unsorted and search for best score
        // Add to sorted list and remove from unsorted list
        // This sorts in ascending order
        for (int i = 0; i < unsortedCount; i++)
        {
            AI bestAi = unsorted[0].GetComponent<AI>();
            int bestIndex = 0;
            for (int z = 0; z < unsorted.Count; z++)
            {
                AI agentAi = unsorted[z].GetComponent<AI>();
                if (agentAi.score > bestAi.score)
                {
                    bestAi = agentAi;
                    bestIndex = z;
                }

                if (agentAi.score > highestScore) // Just for logging fitness
                    highestScore = agentAi.score;
            }
            unsorted.RemoveAt(bestIndex);
            sorted.Add(bestAi.network);
        }
        
        bestScore = highestScore;
        return sorted;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!loadingScene || !reloadScene)
            return;

        loadingScene = false;

        FreshGeneration();
    }

    private void FreshGeneration()
    {
        unfinishedAgents = agentAmount;
        for (int i = 0; i < agentAmount; i++)
        {
            AI.Network templateNetwork = networkCache[Random.Range(0, networkCache.Count)];
            templateNetwork = AI.MutateNetwork(templateNetwork, mutateAmount, mutations);

            if (i < networkCache.Count)
                templateNetwork = networkCache[i];

            SpawnAgent(templateNetwork, resetPhysicalAgents, i);
        }
    }

    private bool ShouldCull(float i, float max)
    {
        float chance = -(i / max) + 1;
        if (Random.value > chance)
            return true;
        return false;
    }

    // Why do this? I will break your spine if you remove it
    private AI.Network NewFromTemplate(AI.Network network)
    {
        AI.Network newNetwork = new AI.Network();

        newNetwork.layers = new AI.Layer[network.layers.Length];
        for (int l = 0; l < network.layers.Length; l++)
        {
            newNetwork.layers[l] = new AI.Layer();

            newNetwork.layers[l].neurons = new AI.Neuron[network.layers[l].neurons.Length];
            for (int n = 0; n < network.layers[l].neurons.Length; n++)
            {
                newNetwork.layers[l].neurons[n] = new AI.Neuron();

                if (l == 0) // Skip input layer
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
