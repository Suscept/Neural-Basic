using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaldRelay : MonoBehaviour
{
    public float speed = 1;

    public AI ai;

    public Transform target;

    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("FunkFood").transform;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        ai.inputs[0] = target.position.x > transform.position.x ? 1 : -1;
        ai.score += Reward(Vector3.Distance(transform.position, target.position)) * Time.deltaTime;
        ai.RunNetwork();
        rb.velocity = Vector3.right * ai.outputs[0];
    }

    private float Reward(float dist)
    {
        return -Mathf.Log(dist + 0.1f);
    }
}
