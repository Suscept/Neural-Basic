using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AI))]
public class Relay : MonoBehaviour
{
    public float speed = 3;

    public Vector2 bounds;

    public float eatDistance = 0.2f;

    public float foodValue = 1;
    public float entityValue = 1;

    private AI ai;

    private GameObject closestFood;
    private GameObject closestActor;

    private Vector3 myPos;

    // Start is called before the first frame update
    void Start()
    {
        ai = GetComponent<AI>();
    }

    // Update is called once per frame
    void Update()
    {
        myPos = transform.position;
        #region Movement
        Vector3 moveVector = new Vector3(ai.outputs[0], 0, ai.outputs[1]);

        myPos += speed * Time.deltaTime * moveVector;
        transform.position = new Vector3(Mathf.Clamp(myPos.x, -bounds.x, bounds.x), 1, Mathf.Clamp(myPos.z, -bounds.y, bounds.y));
        myPos = transform.position;
        #endregion
    }

    public GameObject GetClosestFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        return GetClosest(foods, myPos);
    }

    public GameObject GetClosestActor()
    {
        List<GameObject> others = new List<GameObject>(GameObject.FindGameObjectsWithTag("Entity"));
        others.Remove(gameObject);
        return GetClosest(others.ToArray(), myPos);
    }

    public GameObject GetClosest(GameObject[] objects, Vector3 sourcePosition)
    {
        GameObject closestOther = objects[0];
        float closestDist = float.PositiveInfinity;
        foreach (GameObject obj in objects)
        {
            float distTo = Vector3.Distance(sourcePosition, obj.transform.position);

            if (distTo < closestDist)
            {
                closestOther = obj;
                closestDist = Vector3.Distance(sourcePosition, closestOther.transform.position);
            }
        }
        return closestOther;
    }

    private void LateUpdate()
    {
        #region Eat
        closestActor = GetClosestActor();
        closestFood = GetClosestFood();

        float distTo = Vector3.Distance(myPos, closestActor.transform.position);
        if (distTo <= eatDistance)
        {
            closestActor.SetActive(false);
            ai.score += entityValue;
            closestActor = GetClosestFood();
        }

        distTo = Vector3.Distance(myPos, closestFood.transform.position);
        if (distTo <= eatDistance)
        {
            Destroy(closestFood);
            ai.score += foodValue;
            closestFood = closestActor = GetClosestActor();
        }
        #endregion

        #region Input assignment
        Vector3 otherVector = (closestActor.transform.position - myPos).normalized;
        float otherDist = Vector3.Distance(myPos, closestActor.transform.position);

        Vector3 foodVector = (closestFood.transform.position - myPos).normalized;
        float foodDist = Vector3.Distance(myPos, closestFood.transform.position);

        ai.inputs[0] = otherVector.x;
        ai.inputs[1] = otherVector.z;
        ai.inputs[2] = otherDist;

        ai.inputs[3] = foodVector.x;
        ai.inputs[4] = foodVector.z;
        ai.inputs[5] = foodDist;

        ai.RunNetwork();
        #endregion

        #region Debug
        Debug.DrawRay(myPos, otherVector, Color.red);
        Debug.DrawRay(myPos, foodVector, Color.green);
        #endregion
    }
}
