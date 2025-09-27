using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class Boom : MonoBehaviourPunCallbacks
{
    private Animator animator;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float force = 10f;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void KnockBack()
    {
        // Chỉ Master Client xử lý boom knockback
        if (!PhotonNetwork.IsMasterClient) return;
        
        Debug.Log(transform.position);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                PhotonView targetPV = hit.GetComponent<PhotonView>();
                if (targetPV != null)
                {
                    // Gửi knockback command cho tất cả clients
                    Vector2 knockDirection = hit.transform.position - transform.position;
                    photonView.RPC("ApplyBoomKnockback", RpcTarget.All, targetPV.ViewID, knockDirection, force);
                }
            }
        }
    }
    
    [PunRPC]
    void ApplyBoomKnockback(int targetViewID, Vector2 direction, float knockForce)
    {
        PhotonView target = PhotonView.Find(targetViewID);
        if (target != null)
        {
            KnockBackHandler knockback = target.GetComponent<KnockBackHandler>();
            if (knockback != null)
            {
                knockback.KnockBack(direction, knockForce);
            }
        }
    }
    
    public void Destroy()
    {
        SoundManager.PlaySound(SoundManager.Sound.BoomExplose);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
