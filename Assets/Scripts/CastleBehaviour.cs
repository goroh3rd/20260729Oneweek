using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CastleBehaviour : MonoBehaviour, IThrowable, IGeneratable
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _childSpriteRenderer;
    [SerializeField] private List<Sprite> _castleSprites;
    [SerializeField] private PhysicsMaterial2D _castlePhysicsMaterial;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private AudioSource _audioSource;
    private PolygonCollider2D _polygonCollider;
    public CastlePart Part { get; private set; } = CastlePart.Under;
    public enum CastlePart
    {
        Under = 0,
        Top = 1
    }

    public void Init(int part)
    {
        Part = (CastlePart)part;

        // Set SpriteRenderer properties
        _spriteRenderer.sprite = _castleSprites[part];
        _childSpriteRenderer.sprite = _spriteRenderer.sprite;

        GameObject castleChild = transform.GetChild(0).gameObject;
        _polygonCollider = this.gameObject.AddComponent<PolygonCollider2D>();

        // Physics material and mass assignment
        _polygonCollider.sharedMaterial = _castlePhysicsMaterial;

        // Set the castle to kinematic initially
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _polygonCollider.enabled = false;
    }
    // private void Update()
    // {
    //     if (!IsOnScene())
    //     {
    //         Destroy();
    //     }
    // }
    // void FixedUpdate()
    // {
    //     _rb.angularVelocity *= 0.95f;
    // }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // this._rb.linearVelocity = Vector2.zero;
            // this._rb.angularVelocity = 0f;
            // _rb.bodyType = RigidbodyType2D.Kinematic;
            if (this.gameObject.tag != "Ground") _audioSource.Play();
            this.tag = "Ground";
        }
    }

    public void Throw(Vector2 force)
    {
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _polygonCollider.enabled = true;
        _rb.AddForce(force, ForceMode2D.Impulse);
    }
    public GameObject GetGameObject() => gameObject;
    public Collider2D GetCollider() => _polygonCollider;
    public Rigidbody2D GetRigidbody() => _rb;

    private bool IsOnScene()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        return screenPoint.y >= 0; // 高い位置にあるものは不問とする
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
