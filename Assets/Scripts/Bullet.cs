using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    private Vector2 direction;
    public float force = 10f;
    // Update is called once per frame
    void Update()
    {
        transform.Translate(speed * Time.deltaTime * direction );
    }
    public void SetShootDirection(Vector2 direction)
    {
        this.direction = direction.normalized;
        this.direction.y = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.GetComponent<KnockBackHandler>().KnockBack(direction,force);
            Destroy(gameObject);
        }
    }
}
