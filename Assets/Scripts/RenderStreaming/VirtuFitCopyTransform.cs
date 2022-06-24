using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtuFitCopyTransform : MonoBehaviour
{
    [SerializeField] private Transform origin;

    void Update()
    {
        transform.position = origin.position;
        transform.rotation = origin.rotation;
    }

    public void SetOrigin(Transform value)
    {
        origin = value;
    }
}
