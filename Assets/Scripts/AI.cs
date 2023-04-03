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

    #region Fancy math

    public void FeedForward()
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
                network.layers[l].neurons[n].value = ActivationFunction(GetSumOfNeuron(l, n));
        }

        for (int o = 0; o < outputs.Length; o++)
        {
            outputs[o] = network.layers[network.layers.Length - 1].neurons[o].value;
        }
    }

    public void SetAllNeuronDeltas(float[] expectedOutput)
    {
        for (int l = network.layers.Length - 1; l > 0; l--)
        {
            for (int n = 0; n < network.layers[l].neurons.Length; n++)
            {
                float expectedValue = n < expectedOutput.Length ? expectedOutput[n] : 0;
                network.layers[l].neurons[n].delta = GetNeuronDelta(l, n, expectedValue);
            }
        }
    }

    public float GetNeuronDelta(int layer, int neuron, float expectedValue)
    {
        float derivitiveSum = SigmoidDerivative(GetSumOfNeuron(layer, neuron));
        if (layer == network.layers.Length - 1) // If neuron is on output layer
        {
            float error = network.layers[layer].neurons[neuron].value - expectedValue;
            return -error * derivitiveSum;
        }

        float sumOfWeight = 0;
        for (int n = 0; n < network.layers[layer + 1].neurons.Length; n++)
        {
            sumOfWeight += network.layers[layer + 1].neurons[n].weights[neuron];
        }
        return derivitiveSum * sumOfWeight;
    }

    public float[] GetSumsOfLayer(int layer)
    {
        float[] sums = new float[network.layers[layer].neurons.Length];
        for (int n = 0; n < network.layers[layer].neurons.Length; n++)
        {
            sums[n] = GetSumOfNeuron(layer, n);
        }
        return sums;
    }

    /// <summary>
    /// Gets the sum of the values of all inputted neurons
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="neuron"></param>
    /// <returns></returns>
    public float GetSumOfNeuron(int layer, int neuron)
    {
        float sum = 0;
        int weightsInNeuron = network.layers[layer].neurons[neuron].weights.Length;
        for (int w = 0; w < weightsInNeuron; w++)
        {
            float thisWeight = network.layers[layer].neurons[neuron].weights[w];
            if (w == weightsInNeuron - 1) // Last weight is bias weight
            {
                sum += thisWeight; // Bias = 1 so it will just be the weight
                continue;
            }
            sum += sum += thisWeight * network.layers[layer - 1].neurons[w].value;
        }

        return sum;
    }

    public float GetAverageError(float[] expectedOutput)
    {
        float[] errors = CalculateError(expectedOutput);

        float errorSum = 0;
        for (int i = 0; i < errors.Length; i++)
            errorSum += errors[i];

        return errorSum / errors.Length;
    }

    public float GetSumError(float[] expectedOutput)
    {
        float[] errors = CalculateError(expectedOutput);

        float errorSum = 0;
        for (int i = 0; i < errors.Length; i++)
            errorSum += errors[i];

        return errorSum;
    }

    public float[] CalculateError(float[] expectedOutput)
    {
        if (expectedOutput.Length < outputs.Length)
            Debug.LogError("Expected output too small"); 

        float[] errors = new float[outputs.Length];

        for (int i = 0; i < errors.Length; i++)
        {
            errors[i] = expectedOutput[i] - outputs[i];
            errors[i] *= errors[i];
        }

        return errors;
    }

    #endregion

    #region Genetic

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
                    int weightsForNeuron = net.layers[layer - 1].neurons.Length + 1;
                    net.layers[layer].neurons[neuron].weights = new float[weightsForNeuron];

                    for (int weight = 0; weight < net.layers[layer].neurons[neuron].weights.Length; weight++)
                    {
                        net.layers[layer].neurons[neuron].weights[weight] = Random.Range(-weightRange, weightRange);
                    }
                }
            }
        }

        return net;
    }

    #endregion

    #region Utilities

    public float Exp(float value)
    {
        return Mathf.Pow(2.71828f, value);
    }

    public float ActivationFunction(float value)
    {
        // Standard sigmoid curve function
        return (2 / (1 + Exp(-value))) - 1;
    }

    public float SigmoidDerivative(float value)
    {
        float expVal = Exp(-value);
        float a = 1 + expVal;
        return expVal / (a * a);
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
            int layerIndex = Random.Range(1, network.layers.Length - 1);
            int neuronIndex = Random.Range(0, network.layers[layerIndex].neurons.Length - 1);
            int weightIndex = Random.Range(0, network.layers[layerIndex].neurons[neuronIndex].weights.Length - 1);

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

        return new Neuron[hiddenLayers[layer - 1]];
    }

    public static string NetworkToJson(Network network)
    {
        return JsonUtility.ToJson(network);
    }

    public static Network JsonToNetwork(string json)
    {
        return (Network)JsonUtility.FromJson(json, typeof(Network));
    }

    #endregion

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
        public float delta;
        public float value;
        public float[] weights;

        public float bias = 0;
    }
}
