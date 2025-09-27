using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockBackHandler : MonoBehaviour
{
    Rigidbody2D rg;
    public bool isKnocking;
    
    // NEW: Add PhotonView reference for network operations
    private PhotonView photonView;

    private void Awake()
    {
        rg = GetComponent<Rigidbody2D>();
        // NEW: Get PhotonView component
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void ApplyKnockBack(Vector2 direction, float force)
    {
        // NEW: Only apply knockback if this is our player or we're the Master Client
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Applying knockback to player: " + (photonView.Owner?.NickName ?? "Unknown"));
            KnockBack(direction, force);
        }
        else
        {
            Debug.Log("Ignoring knockback - not our player and not Master Client");
        }
    }

    public void KnockBack(Vector2 direction, float force)
    {
        // OLD CODE - keep as is, but add debug info
        Vector2 impulse = direction.normalized * force;
        rg.AddForce(impulse, ForceMode2D.Impulse);
        isKnocking = true;
        
        // NEW: Add debug information
        Debug.Log($"KnockBack applied: Direction={direction}, Force={force}, Player={photonView.Owner?.NickName}");
        
        StartCoroutine(WaitForKnockBack());
    }

    IEnumerator WaitForKnockBack()
    {
        yield return new WaitForSeconds(1f); 
        isKnocking = false;
        
        // NEW: Debug when knockback ends
        Debug.Log($"Knockback ended for player: {photonView.Owner?.NickName}");
    }

    // NEW: Method for Master Client to apply knockback to multiple players (for explosions)
    [PunRPC]
    public void ApplyExplosionKnockBack(Vector2 direction, float force, Vector3 explosionCenter)
    {
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            // Calculate distance-based force reduction
            float distance = Vector3.Distance(transform.position, explosionCenter);
            float adjustedForce = force / (1 + distance * 0.5f); // Reduce force with distance
            
            Debug.Log($"Explosion knockback: Distance={distance}, AdjustedForce={adjustedForce}");
            KnockBack(direction, adjustedForce);
        }
    }
}
