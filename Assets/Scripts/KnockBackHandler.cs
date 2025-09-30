using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockBackHandler : MonoBehaviour
{
    Rigidbody2D rg;
    public bool isKnocking;
    
    private PhotonView photonView;

    private void Awake()
    {
        rg = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void ApplyKnockBack(Vector2 direction, float force)
    {
        // FIXED: Only apply knockback if this is OUR player (removed Master Client condition)
        if (photonView.IsMine)
        {
            Debug.Log($"Applying knockback to my player: {photonView.Owner?.NickName}");
            KnockBack(direction, force);
        }
        else
        {
            Debug.Log($"Ignoring knockback RPC - not my player. Owner: {photonView.Owner?.NickName}");
        }
    }

    public void KnockBack(Vector2 direction, float force)
    {
        Vector2 impulse = direction.normalized * force;
        rg.AddForce(impulse, ForceMode2D.Impulse);
        isKnocking = true;
        
        Debug.Log($"KnockBack applied: Direction={direction}, Force={force}, Player={photonView.Owner?.NickName}");
        
        StartCoroutine(WaitForKnockBack());
    }

    IEnumerator WaitForKnockBack()
    {
        yield return new WaitForSeconds(1f); 
        isKnocking = false;
        
        Debug.Log($"Knockback ended for player: {photonView.Owner?.NickName}");
    }

    [PunRPC]
    public void ApplyExplosionKnockBack(Vector2 direction, float force, Vector3 explosionCenter)
    {
        // FIXED: Only apply to own player (removed Master Client condition)
        if (photonView.IsMine)
        {
            // Calculate distance-based force reduction
            float distance = Vector3.Distance(transform.position, explosionCenter);
            float adjustedForce = force / (1 + distance * 0.5f);
            
            Debug.Log($"Applying explosion knockback to my player: Distance={distance}, AdjustedForce={adjustedForce}");
            KnockBack(direction, adjustedForce);
        }
        else
        {
            Debug.Log($"Ignoring explosion knockback RPC - not my player. Owner: {photonView.Owner?.NickName}");
        }
    }
}
