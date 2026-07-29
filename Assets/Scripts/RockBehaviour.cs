using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class RockBehaviour : MonoBehaviour, IThrowable, IGeneratable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> rockSprites;
    private Rigidbody2D rb;
    private PolygonCollider2D polygonCollider;
    public void Init()
    {
        GameObject rockChild = transform.GetChild(0).gameObject;
        spriteRenderer.sprite = rockSprites[Random.Range(0, rockSprites.Count)];
        rockChild.GetComponent<SpriteRenderer>().sprite = spriteRenderer.sprite;
        polygonCollider = this.gameObject.AddComponent<PolygonCollider2D>();
        rb = this.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        polygonCollider.enabled = false;
    }

    public void Throw(Vector2 force)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        polygonCollider.enabled = true;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public GameObject Generate()
    {
        return gameObject;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
