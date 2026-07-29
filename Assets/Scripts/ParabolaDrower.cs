using UnityEngine;

public class ParabolaDrower : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private int pointCount = 40;
    [SerializeField] private float timeStep = 0.05f;

    public void DrawTrajectory(Vector2 startPos, float angle, float speed, float gravityScale)
    {
        line.positionCount = pointCount;

        float rad = angle * Mathf.Deg2Rad;

        Vector2 velocity = new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        ) * speed;

        float g = Mathf.Abs(Physics2D.gravity.y) * gravityScale;

        for (int i = 0; i < pointCount; i++)
        {
            float t = i * timeStep;

            Vector2 pos = startPos + new Vector2(
                velocity.x * t,
                velocity.y * t - 0.5f * g * t * t
            );

            line.SetPosition(i, pos);
        }
    }
}
