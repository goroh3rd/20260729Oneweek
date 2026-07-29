using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimeKeeper : MonoBehaviour
{
    [SerializeField] private float _timeLimit;
    [SerializeField] private float _phaseDuration;
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
                // この辺でタイムアップの音を鳴らす
                DoWithDelay(_phaseDuration, () =>
                {
                    ChangeToCastlePhase();
                });
            }
        }
    }
    private void ChangeToCastlePhase()
    {
        RockPhase = false;
        CastlePhase = true;
    }
    private IEnumerator DoWithDelay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}
