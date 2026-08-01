using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class RockBehaviour : MonoBehaviour, IThrowable, IGeneratable
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _childSpriteRenderer;
    [SerializeField] private List<Sprite> _rockSprites;
    [SerializeField] private PhysicsMaterial2D _rockPhysicsMaterial;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private AudioSource _audioSource;
    private PolygonCollider2D _polygonCollider;
    public void Init()
    {
        // Set SpriteRenderer properties
        _spriteRenderer.sprite = _rockSprites[Random.Range(0, _rockSprites.Count)];
        _childSpriteRenderer.sprite = _spriteRenderer.sprite;
        float c = Random.Range(0.1f, 0.5f);
        _childSpriteRenderer.color = new Color(c, c, c);

        GameObject rockChild = transform.GetChild(0).gameObject;
        _polygonCollider = this.gameObject.AddComponent<PolygonCollider2D>();

        // Physics material and mass assignment
        _polygonCollider.sharedMaterial = _rockPhysicsMaterial;

        // Set Transform
        this.transform.localScale = this.transform.localScale * Random.Range(0.8f, 1.2f);
        this.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));


        // Set the rock to kinematic initially
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _polygonCollider.enabled = false;
    }
    private void Update()
    {

        if (!IsOnScene())
        {
            Destroy();
        }
    }
    void FixedUpdate()
    {
        _rb.angularVelocity *= 0.95f;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            this._rb.linearVelocity = Vector2.zero;
            this._rb.angularVelocity = 0f;
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
