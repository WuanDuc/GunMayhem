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
        CheckIfOutOfBounds();
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

    private void CheckIfOutOfBounds()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        if (transform.position.x > screenBounds.x || transform.position.x < -screenBounds.x ||
            transform.position.y > screenBounds.y || transform.position.y < -screenBounds.y)
        {
            Destroy(gameObject);
        }
    }
}
