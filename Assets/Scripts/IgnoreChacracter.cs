using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IgnoreChacracter : MonoBehaviour
{
    [SerializeField]
    private int layerIndex = 7; // index of layer to ignore
    void Start()
    {
        Physics2D.IgnoreLayerCollision(layerIndex, layerIndex);
    }

  
}
