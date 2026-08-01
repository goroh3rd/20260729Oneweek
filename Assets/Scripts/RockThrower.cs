using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class RockThrower : MonoBehaviour
{
    [SerializeField] private float _prepareTime;
    // [SerializeField] private float _throwInterval;
    // [SerializeField] private int _throwCount;
    [SerializeField] private float _throwForce;
    [SerializeField] private RockBehaviour _rockPrefab;
    [SerializeField] private CastleBehaviour _castlePrefab;
    [SerializeField] private List<Sprite> _rockSprites;
    [SerializeField] private ParabolaDrower _parabolaDrower;
    [SerializeField] private AudioSource _throwSound;
    [SerializeField] private TimeKeeper _timeKeeper;
    [SerializeField] private ScoreShower _scoreShower;
    private IThrowable _currentThrowable;
    private Coroutine _throwCoroutine;
    public List<IThrowable> Throwables { get; private set; } = new List<IThrowable>();
    private int _castleCount = 0;
    public void Start()
    {
        Throwables.Clear();
        _currentThrowable = GenerateRock();
        _throwCoroutine = StartCoroutine(StartThrow(_prepareTime));

    }
    private void Update()
    {
        if (_currentThrowable != null && _parabolaDrower != null && (_timeKeeper.RockPhase || _timeKeeper.CastlePhase))
        {
            float angle = GetAngleToMouse();
            _parabolaDrower.DrawTrajectory(_currentThrowable.GetGameObject().transform.position, angle, _throwForce, _currentThrowable.GetGameObject().GetComponent<Rigidbody2D>().gravityScale);
        }
        if (_timeKeeper.CastlePhase && _castleCount < 2 && _currentThrowable is RockBehaviour)
        {
            _currentThrowable = ChangeCastlePart();
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
            if (_timeKeeper.RockPhase && _currentThrowable != null)
            {
                Vector2 force = new Vector2(Mathf.Cos(GetAngleToMouse() * Mathf.Deg2Rad), Mathf.Sin(GetAngleToMouse() * Mathf.Deg2Rad)) * _throwForce;
                _throwSound.Play();
                IThrowable thrown = ThrowRock(force);
                Throwables.Add(thrown);
            }
            else if (_timeKeeper.CastlePhase && _currentThrowable != null && _castleCount < 3)
            {
                Vector2 force = new Vector2(Mathf.Cos(GetAngleToMouse() * Mathf.Deg2Rad), Mathf.Sin(GetAngleToMouse() * Mathf.Deg2Rad)) * _throwForce;
                _throwSound.Play();
                IThrowable thrown = ThrowCastle(force);
                Throwables.Add(thrown);
            }
            else if (_timeKeeper.CastlePhase && _currentThrowable != null && _castleCount >= 3)
            {
                // Do nothing, no more castles to throw
                Debug.Log("No more castles to throw.");
            }
        }
    }
    public IThrowable ChangeCastlePart()
    {
        Destroy(_currentThrowable.GetGameObject());
        _currentThrowable = null;
        CastleBehaviour castle = GenerateCastle();
        return castle;
    }
    private RockBehaviour GenerateRock()
    {
        RockBehaviour rock = Instantiate(_rockPrefab, transform.position, Quaternion.identity);
        rock.Init();
        return rock;
    }
    private CastleBehaviour GenerateCastle()
    {
        CastleBehaviour castle = Instantiate(_castlePrefab, transform.position, Quaternion.identity);
        castle.Init(_castleCount);
        _castleCount++;
        return castle;
    }
    private IThrowable ThrowRock(Vector2 force)
    {
        if (_currentThrowable != null)
        {
            _currentThrowable.Throw(force);
            if (_timeKeeper.RockPhase)
            {
                _currentThrowable = null;
                _currentThrowable = GenerateRock();
            }
        }
        else
        {
            Debug.LogWarning("No current throwable to throw.");
        }
        return _currentThrowable;
    }
    private IThrowable ThrowCastle(Vector2 force)
    {
        if (_currentThrowable != null)
        {
            _currentThrowable.Throw(force);
            if (_timeKeeper.CastlePhase && _castleCount < 2)
            {
                _currentThrowable = null;
                _currentThrowable = GenerateCastle();
            }
            else
            {
                _currentThrowable = null;
                _timeKeeper.EndCastlePhase();
                _parabolaDrower.ClearTrajectory();
                StartCoroutine(StartMeasureHeight(6f));
            }
        }
        else
        {
            Debug.LogWarning("No current throwable to throw.");
        }
        return _currentThrowable;
    }
    private IEnumerator StartMeasureHeight(float delay)
    {
        HeightMeasure heightMeasure = new HeightMeasure();
        CastleBehaviour castleTop = heightMeasure.FindCastleTop(Throwables) as CastleBehaviour;
        Collider2D castleTopCollider = castleTop.GetCollider();
        Rigidbody2D castleTopRigidbody = castleTop.GetRigidbody();

        // yield return new WaitUntil(() => castleTopRigidbody.IsSleeping());
        float elapsedTime = 0f;

        while (elapsedTime < delay)
        {
            if (castleTopRigidbody.IsSleeping() || Mathf.Approximately(castleTopRigidbody.linearVelocity.magnitude, 0f))
            {
                Debug.Log("Castle top has settled.");
                break; // Exit the loop if the Rigidbody is sleeping
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        float maxHeight = heightMeasure.MeasureMaxHeight(Throwables);
        Debug.Log("Max Height: " + Mathf.Max(maxHeight + 5f, 0f));
        _scoreShower.ShowScore(Mathf.Max(maxHeight + 5f, 0f));
    }
    private float GetAngleToMouse()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mousePosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;
    }
}

public class HeightMeasure
{
    public float MeasureMaxHeight(List<IThrowable> throwable)
    {
        IThrowable castleTop = FindCastleTop(throwable);
        if (castleTop != null)
        {
            Collider2D castleTopCollider = castleTop.GetCollider();
            float maxHeight = castleTopCollider.bounds.max.y;
            return maxHeight;
        }
        return float.MinValue;
    }
    public IThrowable FindCastleTop(List<IThrowable> throwables)
    {
        foreach (IThrowable throwable in throwables)
        {
            if (throwable is CastleBehaviour castleBehaviour)
            {
                if (castleBehaviour.Part == CastleBehaviour.CastlePart.Top)
                {
                    return castleBehaviour;
                }
            }
        }
        Debug.LogWarning("No castle top found in the list of throwables.");
        return null;
    }
}