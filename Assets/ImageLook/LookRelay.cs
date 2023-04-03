using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookRelay : MonoBehaviour
{
    public Texture2D[] mogus;
    public bool[] isMogus;

    public bool runOnStart = true;

    public float[] isMogusOutput;
    public float[] isNotMogusOutput;

    // Start is called before the first frame update
    void Start()
    {
        if (!runOnStart)
            return;
        AI ai = GetComponent<AI>();
        ai.network = AI.NewNetwork(ai.inputs.Length, ai.outputs.Length, ai.hiddenLayers, 5);

        float overallError = 0;
        for (int i = 0; i < mogus.Length; i++)
        {
            // Set input
            Texture2D tex = mogus[i];
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    ai.inputs[(y * 5) + x] = tex.GetPixel(x, y).grayscale;
                }
            }

            ai.FeedForward();

            float[] expectedOutput = isMogus[i] ? isMogusOutput : isNotMogusOutput;

            float mogusError = ai.GetSumError(expectedOutput);
            overallError += mogusError;

            

            Debug.Log("Error for mogus " + i + ": " + mogusError + " Output: "+ai.outputs[0]+"  " + ai.outputs[1]);
        }

        overallError /= ai.outputs.Length * mogus.Length;

        Debug.Log("OverallError: "+overallError);
    }

    public void RunRelay()
    {
        AI ai = GetComponent<AI>();
        for (int i = 0; i < mogus.Length; i++)
        {
            Texture2D tex = mogus[i];
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    ai.inputs[(y * 5) + x] = tex.GetPixel(x, y).grayscale;
                }
            }
        }
    }
}
