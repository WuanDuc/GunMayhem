using Photon.Pun;
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
    //[PunRPC]
    //public void ApplyKnockBack(Vector2 direction, float force)
    //{
    //    Debug.Log("Applying knockback: " + direction + " with force: " + force);
    //    Vector2 knockbackDirection = direction.normalized * force;
    //    rg.AddForce(knockbackDirection, ForceMode2D.Impulse);
    //}
    public void KnockBack(Vector2 direction, float force)
    {
        Vector2 impulse = direction.normalized * force;
        rg.AddForce(impulse, ForceMode2D.Impulse);
    }
}
