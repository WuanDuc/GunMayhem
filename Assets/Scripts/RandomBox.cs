using Photon.Pun;
using UnityEngine;

public class RandomBox : MonoBehaviour
{
    [SerializeField]
    private GameObject[] gunPrefabs;
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }
    // Update is called once per frame
    void Update()
    {

    }
    public GameObject GetRamdomGun()
    {
        int randomIndex = Random.Range(0, gunPrefabs.Length);
        return gunPrefabs[randomIndex];
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("DestroyBox", RpcTarget.AllBuffered);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    public void DestroyBox()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
