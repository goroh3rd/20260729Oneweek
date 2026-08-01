using UnityEngine;

public class ParabolaDrower : MonoBehaviour
{
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _pointCount = 40;
    [SerializeField] private float _timeStep = 0.05f;

    public void DrawTrajectory(Vector2 startPos, float angle, float speed, float gravityScale)
    {
        _line.positionCount = _pointCount;

        float rad = angle * Mathf.Deg2Rad;

        Vector2 velocity = new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        ) * speed;

        float g = Mathf.Abs(Physics2D.gravity.y) * gravityScale;

        for (int i = 0; i < _pointCount; i++)
        {
            float t = i * _timeStep;

            Vector2 pos = startPos + new Vector2(
                velocity.x * t,
                velocity.y * t - 0.5f * g * t * t
            );

            _line.SetPosition(i, pos);
        }
    }
    public void ClearTrajectory()
    {
        _line.positionCount = 0;
    }
}
