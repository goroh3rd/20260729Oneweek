using UnityEngine;

public interface IThrowable
{
    void Throw(Vector2 force);
    GameObject GetGameObject();
    Collider2D GetCollider();
    Rigidbody2D GetRigidbody();
}
