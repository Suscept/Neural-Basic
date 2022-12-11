using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPositioner : MonoBehaviour
{
    public Vector3 positionRange;
    public Vector3 rotationRange;

    private void Awake()
    {
        Vector3 randPos = new Vector3(
            Random.Range(-positionRange.x, positionRange.x),
            Random.Range(-positionRange.y, positionRange.y),
            Random.Range(-positionRange.z, positionRange.z)
            );

        Vector3 randRot = new Vector3(
            Random.Range(-rotationRange.x, rotationRange.x),
            Random.Range(-rotationRange.y, rotationRange.y),
            Random.Range(-rotationRange.z, rotationRange.z)
            );

        transform.position += randPos;
        transform.Rotate(randRot);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
