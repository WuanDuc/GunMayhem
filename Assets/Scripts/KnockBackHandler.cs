using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockBackHandler : MonoBehaviour
{
    Rigidbody2D rg;
    public bool isKnocking;

    private void Awake()
    {
        rg = GetComponent<Rigidbody2D>();
    }
    [PunRPC]
    public void ApplyKnockBack(Vector2 direction, float force)
    {
        KnockBack(direction, force);
    }
    public void KnockBack(Vector2 direction, float force)
    {
        Vector2 impulse = direction.normalized * force;
        rg.AddForce(impulse, ForceMode2D.Impulse);
        isKnocking = true;
        StartCoroutine(WaitForKnockBack());
    }
    IEnumerator WaitForKnockBack()
    {
        yield return new WaitForSeconds(1f); 
        isKnocking = false;
    }    
}
