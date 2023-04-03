using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MothRelay : MonoBehaviour
{
    public float speed = 1;

    public float maxRPM;
    public float wheelDiameter;
    public float axelLength;

    public float rewardDist = 5;

    public float leftCum;
    public float rightCum;

    public AI ai;

    public Transform target;

    public Rigidbody rb;

    public float driveAmount;

    private Vector2 prevOut;

    public float startDist;

    private float bestDist;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Lamp").transform;
        rb = GetComponent<Rigidbody>();

        float dist = Vector3.Distance(transform.position, target.position);
        startDist = dist;
        bestDist = dist;

        ai.inputs[0] = dist;
        ai.inputs[1] = dist;
    }

    // Update is called once per frame
    void Update()
    {
        float targetDist = Vector3.Distance(transform.position, target.position);

        ai.inputs[1] = ai.inputs[0];
        ai.inputs[0] = targetDist;

        ai.inputs[2] = ParseOut(ai.outputs[0]);
        ai.inputs[3] = ParseOut(ai.outputs[1]);

        ai.FeedForward();

        //ai.score = -Mathf.Pow(Vector3.Distance(transform.position, target.position), 2) * 0.03f;
        //ai.score += -targetDist * Trainer.trainingDeltatime;

        Drive(ParseOut(ai.outputs[0]), ParseOut(ai.outputs[1]));

        targetDist = Vector3.Distance(transform.position, target.position);

        if (targetDist < bestDist)
            bestDist = targetDist;

        targetDist = Mathf.Min(bestDist, targetDist);
        ai.score = 1/(targetDist / startDist);
    }

    public void Drive(float leftPower, float rightPower)
    {
        if (leftPower != 0 || rightPower != 0)
            driveAmount += Trainer.trainingDeltatime;

        leftCum = leftPower;
        rightCum = rightPower;

        float lRPM = leftPower * maxRPM;
        float rRPM = rightPower * maxRPM;

        float lTurns = lRPM / 60 / Trainer.trainingFramerate;
        float rTurns = rRPM / 60 / Trainer.trainingFramerate;

        float innerTurns = lTurns;
        float outerTurns = rTurns;

        if (leftPower == rightPower)
        {
            float forwardDist = Mathf.PI * wheelDiameter * innerTurns;
            transform.position += transform.forward * forwardDist;
            return;
        }

        int turnAround = -1;

        if (Mathf.Abs(leftPower) > Mathf.Abs(rightPower))
        {
            innerTurns = rTurns;
            outerTurns = lTurns;
            turnAround = 1;
        }

        float originDist = innerTurns * axelLength / (outerTurns - innerTurns);

        float rotAngle;
        if (originDist == 0)
        {
            rotAngle = (180 * outerTurns * wheelDiameter) / axelLength;
        }
        else
            rotAngle = innerTurns * 180 * wheelDiameter / originDist;

        Vector3 rotatePoint = transform.position + transform.right * turnAround + (originDist * transform.right * turnAround);

        transform.RotateAround(rotatePoint, Vector3.up, rotAngle * turnAround);
    }

    private int ParseOut(float unparsed)
    {
        if (unparsed > 0.5f)
            return 1;
        if (unparsed < -0.5f)
            return -1;

        return 0;
    }

    private float Reward(float dist)
    {
        return -Mathf.Log(dist + 0.1f);
    }
}
