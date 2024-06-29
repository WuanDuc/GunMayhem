using System.Collections.Generic;
using UnityEngine;

public enum BulletType
{
    NORMAL,
    SHOTGUN
}
public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    private Vector2 direction;
    public float force = 10f;
    public BulletType type;
    // Update is called once per frame
    void Update()
    {
        switch (type)
        {
            case BulletType.NORMAL:
                CheckIfOutOfBounds();
           
                transform.Translate(speed * Time.deltaTime * direction);
                break;
            case BulletType.SHOTGUN:
                
                
                break;
        }

    }
    public void SetShootDirection(Vector2 direction)
    {
        this.direction = direction.normalized;
        this.direction.y = 0;
    
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<KnockBackHandler>().KnockBack(direction, force);
            Destroy(gameObject);
        }
    }

    private void CheckIfOutOfBounds()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        if (transform.position.x > screenBounds.x || transform.position.x < -screenBounds.x ||
            transform.position.y > screenBounds.y || transform.position.y < -screenBounds.y)
        {
            Destroy(gameObject);
        }
    }
    public void ShotgunKnockBack()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.2f);
        foreach (var collider in hitColliders)
        {
            
                if (collider.CompareTag("Player"))
                {
                    collider.gameObject.GetComponent<KnockBackHandler>().KnockBack(direction, force);
                }
            
        }
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
   
}
