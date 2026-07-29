using UnityEngine;

public class TimeKeeper : MonoBehaviour
{
    [SerializeField] private float _timeLimit;
    public float TimeRemaining { get; private set; }
    public float IsTimeUp => TimeRemaining <= 0f ? 1f : 0f;
    public bool RockPhase { get; private set; } = true;
    public bool CastlePhase { get; private set; } = false;
    public void StartGame()
    {
        if (RockPhase) return;
        TimeRemaining = _timeLimit;
        RockPhase = true;
        CastlePhase = false;
    }
    private void Update()
    {
        if (RockPhase)
        {
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0)
            {
                TimeRemaining = 0;
                RockPhase = false;
                CastlePhase = true;
                // Handle game over logic here
            }
        }
    }
}
