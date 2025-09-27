using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Animator),typeof(Rigidbody2D))]
public class Boom : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float force = 10f;
    
    // NEW: PhotonView reference for network operations
    private PhotonView photonView;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        // NEW: Get PhotonView component
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void KnockBack()
    {
        Debug.Log("Explosion at position: " + transform.position);
        
        // NEW: Only Master Client calculates explosion effects
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
        
        // OLD CODE - commented out (direct knockback)
        /*
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                hit.gameObject.GetComponent<KnockBackHandler>().KnockBack(direction, force);
            }
        }
        */
        
        // NEW: Master Client handles explosion knockback
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                PhotonView targetPlayer = hit.GetComponent<PhotonView>();
                if (targetPlayer != null)
                {
                    Vector2 direction = (hit.transform.position - transform.position).normalized;
                    
                    // Calculate distance-based force
                    float distance = Vector2.Distance(transform.position, hit.transform.position);
                    float adjustedForce = force / (1 + distance * 0.3f); // Reduce force with distance
                    
                    Debug.Log($"Explosion affecting {targetPlayer.Owner?.NickName}: Distance={distance}, Force={adjustedForce}");
                    
                    // Send explosion knockback to the target player
                    targetPlayer.RPC("ApplyExplosionKnockBack", targetPlayer.Owner, 
                        direction, adjustedForce, transform.position);
                }
            }
        }
    }

    public void Destroy()
    {
        Debug.Log("Destroying boom object");
        
        // NEW: Network-aware destruction
        if (PhotonNetwork.IsConnected)
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                // Request destruction from owner
                photonView.RPC("RequestDestroyBoom", photonView.Owner);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // NEW: RPC method for boom destruction
    [PunRPC]
    void RequestDestroyBoom()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    // FIXED: Use correct Gizmos method for drawing circle in editor
    private void OnDrawGizmosSelected()
    {
        // Visualize explosion radius in editor
        Gizmos.color = Color.red;
        
        // Draw wire sphere instead of wire circle (which doesn't exist)
        Gizmos.DrawWireSphere(transform.position, radius);
        
        // Alternative: Draw multiple line segments to form a circle
        /*
        int segments = 32;
        float angle = 0f;
        Vector3 lastPoint = Vector3.zero;
        Vector3 thisPoint = Vector3.zero;
        
        for (int i = 0; i < segments + 1; i++)
        {
            thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius + transform.position.x;
            thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius + transform.position.y;
            thisPoint.z = transform.position.z;
            
            if (i > 0)
            {
                Gizmos.DrawLine(lastPoint, thisPoint);
            }
            
            lastPoint = thisPoint;
            angle += 360f / segments;
        }
        */
    }
}
