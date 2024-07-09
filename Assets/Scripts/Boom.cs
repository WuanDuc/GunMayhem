using UnityEngine;
using Photon.Pun;
[RequireComponent(typeof(Animator),typeof(Rigidbody2D))]
public class Boom : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float force = 10f;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void KnockBack()
    {
        Debug.Log(transform.position);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                hit.gameObject.GetComponent<KnockBackHandler>().KnockBack(hit.gameObject.transform.position - transform.position, force);
                //hit.gameObject.GetComponent<PhotonView>().RPC("ApplyKnockBack", RpcTarget.AllBuffered, hit.gameObject.transform.position - transform.position, force);
            }
        }
    
    }
    public void Destroy()
    {
        SoundManager.PlaySound(SoundManager.Sound.BoomExplose);
        Destroy(gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
