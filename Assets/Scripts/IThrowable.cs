using UnityEngine;

public interface IThrowable
{
    void Throw(Vector2 force);
    GameObject GetGameObject();
}
