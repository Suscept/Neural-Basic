using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookRelay : MonoBehaviour
{
    public Texture2D[] mogus;
    public bool[] isMogus;

    // Start is called before the first frame update
    void Start()
    {
        AI ai = GetComponent<AI>();

        int correct = 0;
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
            ai.RunNetwork();
            if (ai.outputs[0] > ai.outputs[1] == isMogus[i])
            {
                ai.score += Mathf.Abs(ai.outputs[0]) - Mathf.Abs(ai.outputs[1]);
                correct++;
                //ai.score += 1;
            }
        }

        //Debug.Log(correct + "/8");
        Trainer.instance.MarkAgentComplete();
    }
}
