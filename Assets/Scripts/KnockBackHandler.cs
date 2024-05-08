using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockBackHandler : MonoBehaviour
{
    Rigidbody2D rg;

    private void Awake()
    {
        rg = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void KnockBack(Vector2 direction, float force)
    {
        Vector2 impulse = direction.normalized * force;
        rg.AddForce(impulse, ForceMode2D.Impulse);
    }
}
