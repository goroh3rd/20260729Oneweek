using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class RockThrower : MonoBehaviour
{
    [SerializeField] private float _prepareTime;
    [SerializeField] private float _throwInterval;
    [SerializeField] private int _throwCount;
    [SerializeField] private float _throwForce;
    [SerializeField] private RockBehaviour _rockPrefab;
    [SerializeField] private List<Sprite> _rockSprites;
    [SerializeField] private ParabolaDrower _parabolaDrower;
    [SerializeField] private TimeKeeper _timeKeeper;
    private RockBehaviour _currentRock;
    private Coroutine _throwCoroutine;
    public void Start()
    {
        _currentRock = GenerateRock();
        _throwCoroutine = StartCoroutine(StartThrow(_prepareTime));

    }
    private void Update()
    {
        if (_currentRock != null)
        {
            float angle = GetAngleToMouse();
            _parabolaDrower.DrawTrajectory(_currentRock.transform.position, angle, _throwForce, _currentRock.GetComponent<Rigidbody2D>().gravityScale);
        }
    }
    private IEnumerator StartThrow(float delay)
    {
        yield return new WaitForSeconds(delay);
        _timeKeeper.StartGame();
    }
    public void Throw(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_timeKeeper.RockPhase && _currentRock != null)
            {
                Vector2 force = new Vector2(Mathf.Cos(GetAngleToMouse() * Mathf.Deg2Rad), Mathf.Sin(GetAngleToMouse() * Mathf.Deg2Rad)) * _throwForce;
                ThrowRock(force);
            }
            else
            {
            }
        }
    }
    private RockBehaviour GenerateRock()
    {
        RockBehaviour rock = Instantiate(_rockPrefab, transform.position, Quaternion.identity);
        rock.Init();
        return rock;
    }
    private void ThrowRock(Vector2 force)
    {
        if (_currentRock != null)
        {
            _currentRock.Throw(force);
            if (_timeKeeper.RockPhase)
            {
                _currentRock = null;
                _currentRock = GenerateRock();
            }
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
