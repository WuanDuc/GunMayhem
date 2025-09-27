using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletPool : MonoBehaviourPunCallbacks
{
    public static BulletPool Instance;
    
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 50;
    
    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    private List<GameObject> activeBullets = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        GameObject bullet;
        
        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            // Pool hết, tạo mới
            bullet = Instantiate(bulletPrefab);
        }
        
        bullet.SetActive(true);
        activeBullets.Add(bullet);
        return bullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        if (activeBullets.Contains(bullet))
        {
            activeBullets.Remove(bullet);
            bullet.SetActive(false);
            
            // Reset bullet state
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.ResetBullet();
            }
            
            bulletPool.Enqueue(bullet);
        }
    }

    // Cleanup khi game over
    public void ClearAllBullets()
    {
        foreach (GameObject bullet in activeBullets)
        {
            ReturnBullet(bullet);
        }
        activeBullets.Clear();
    }
}