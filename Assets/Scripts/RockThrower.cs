using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class RockThrower : MonoBehaviour
{
    [SerializeField] private float throwForce;
    [SerializeField] private RockBehaviour rockPrefab;
    [SerializeField] private List<Sprite> rockSprites;
    private RockBehaviour currentRock;
    public void Start()
    {
        currentRock = GenerateRock();
    }
    public void Throw(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 force = new Vector2(Mathf.Cos(GetAngleToMouse() * Mathf.Deg2Rad), Mathf.Sin(GetAngleToMouse() * Mathf.Deg2Rad)) * throwForce;
            ThrowRock(force);
        }
    }
    private RockBehaviour GenerateRock()
    {
        RockBehaviour rock = Instantiate(rockPrefab, transform.position, Quaternion.identity);
        rock.Init();
        return rock;
    }
    private void ThrowRock(Vector2 force)
    {
        if (currentRock != null)
        {
            currentRock.Throw(force);
            currentRock = GenerateRock();
        }
    }
    private float GetAngleToMouse()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mousePosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;
    }
}
