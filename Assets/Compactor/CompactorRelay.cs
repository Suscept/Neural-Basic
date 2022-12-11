using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompactorRelay : MonoBehaviour
{
    public Transform head;

    public float maxHeight = 1;

    public float acceleration = 0.5f;

    [Header("Stats")]
    [SerializeField]
    private float speed;
    [SerializeField]
    private float position;
    [SerializeField]
    private float time;

    private AI ai;

    // Start is called before the first frame update
    void Start()
    {
        ai = GetComponent<AI>();
    }

    // Update is called once per frame
    void Update()
    {
        ai.inputs[0] = position / maxHeight;
        ai.inputs[1] = speed;

        ai.RunNetwork();

        float moveDir = Mathf.Sign(ai.outputs[0]);

        if (position == 0)
            moveDir = Mathf.Max(0, moveDir);

        speed += moveDir * acceleration * Trainer.trainingDeltatime;

        position += speed * Trainer.trainingDeltatime;

        position = Mathf.Clamp(position, 0, maxHeight);

        if (position == 0 && speed < 0)
            ai.score += Mathf.Abs(speed);

        if (position == 0 || position == maxHeight)
            speed = 0;

        time += Trainer.trainingDeltatime;

        head.localPosition = new Vector3(0, position, 0);
    }
}
