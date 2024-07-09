using Photon.Pun;
using System.Collections;
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
            if (photonView.IsMine)
            {
                DestroyBox();
            }
            else
            {
                photonView.RPC("RequestDestroyBox", photonView.Owner, photonView.ViewID);
            }
        }
    }

    private void DestroyBox()
    {
        Debug.Log("Destroying RandomBox owned by: " + photonView.OwnerActorNr);
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    public void RequestDestroyBox(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null && targetView.IsMine)
        {
            targetView.GetComponent<RandomBox>().DestroyBox();
        }
        else
        {
            Debug.LogWarning("Requested PhotonView not found or not owned by this client.");
        }
    }
}
