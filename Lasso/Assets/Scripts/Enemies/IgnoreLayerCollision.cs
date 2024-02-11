using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreLayerCollision : MonoBehaviour
{

    void Update()
    {
        Physics.IgnoreLayerCollision(9, 10, true);
    }
}
