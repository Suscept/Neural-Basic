using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public string hiddenLayers;

    public float[] inputs;
    public float[] outputs;

    public float score;

    public Network network;

    public void RunNetwork()
    {
        // Set input neurons
        for (int i = 0; i < inputs.Length; i++)
        {
            network.layers[0].neurons[i].value = inputs[i];
        }

        // Skip input layer since values are already set
        for (int l = 1; l < network.layers.Length; l++)
        {
            for (int n = 0; n < network.layers[l].neurons.Length; n++)
            {
                float sum = network.layers[l].neurons[n].bias;
                for (int w = 0; w < network.layers[l].neurons[n].weights.Length; w++)
                {
                    sum += network.layers[l].neurons[n].weights[w] * network.layers[l - 1].neurons[w].value;
                }
                network.layers[l].neurons[n].value = ActivationFunction(sum);
            }
        }

        for (int o = 0; o < outputs.Length; o++)
        {
            outputs[o] = network.layers[network.layers.Length - 1].neurons[o].value;
        }
    }

    public float ActivationFunction(float value)
    {
        // Standard sigmoid curve function
        return (2 / (1 + Mathf.Pow(2.71828f, -value))) - 1;
    }

    public void Mutate(float amount)
    {
        for (int l = 1; l < network.layers.Length; l++)
        {
            for (int n = 0; n < network.layers[l].neurons.Length; n++)
            {
                for (int w = 0; w < network.layers[l].neurons[n].weights.Length; w++)
                {
                    float mutation = Random.Range(-amount, amount);
                    
                    network.layers[l].neurons[n].weights[w] += mutation;
                }
            }
        }
    }

    public static Network MutateNetwork(Network network, float mutationRange, int mutuateAmount)
    {
        for (int i = 0; i < mutuateAmount; i++)
        {
            int layerIndex = Random.Range(1, network.layers.Length-1);
            int neuronIndex = Random.Range(0, network.layers[layerIndex].neurons.Length-1);
            int weightIndex = Random.Range(0, network.layers[layerIndex].neurons[neuronIndex].weights.Length-1);

            float mutation = Random.Range(-mutationRange, mutationRange);

            network.layers[layerIndex].neurons[neuronIndex].weights[weightIndex] += mutation;
        }

        return network;
    }

    private static Neuron[] GetLayerNeurons(int layer, int[] hiddenLayers, int input, int output, int networkSize)
    {
        if (layer == 0)
            return new Neuron[input];
        if (layer == networkSize - 1)
            return new Neuron[output];

        return new Neuron[hiddenLayers[layer-1]];
    }

    public static Network NewNetwork(int inputSize, int outputSize, string layers, float weightRange)
    {
        string[] layerString = layers.Split(',');
        int[] desiredLayers = new int[layerString.Length];

        // Parse layer string
        for (int i = 0; i < layerString.Length; i++)
        {
            desiredLayers[i] = int.Parse(layerString[i]);
        }

        Network net = new Network
        {
            layers = new Layer[desiredLayers.Length + 2]
        };

        for (int layer = 0; layer < net.layers.Length; layer++)
        {
            net.layers[layer] = new Layer();

            // Add neurons to layer
            net.layers[layer].neurons = GetLayerNeurons(layer, desiredLayers, inputSize, outputSize, net.layers.Length);

            for (int neuron = 0; neuron < net.layers[layer].neurons.Length; neuron++)
            {
                net.layers[layer].neurons[neuron] = new Neuron();
                if (layer > 0)
                {
                    net.layers[layer].neurons[neuron].weights = new float[net.layers[layer - 1].neurons.Length];

                    for (int weight = 0; weight < net.layers[layer].neurons[neuron].weights.Length; weight++)
                    {
                        net.layers[layer].neurons[neuron].weights[weight] = layer > 0 ? Random.Range(-weightRange, weightRange) : 1;
                    }
                }
            }
        }

        return net;
    }

    public static string NetworkToJson(Network network)
    {
        return JsonUtility.ToJson(network);
    }

    public static Network JsonToNetwork(string json)
    {
        return (Network)JsonUtility.FromJson(json, typeof(Network));
    }

    [System.Serializable]
    public class Network
    {
        public Layer[] layers;
    }

    [System.Serializable]
    public class Layer
    {
        public Neuron[] neurons;
    }

    [System.Serializable]
    public class Neuron
    {
        public float value;
        public float[] weights;

        public float bias = 0;
    }
}
