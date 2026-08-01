using UnityEngine;

public class GunzeuSpawner : MonoBehaviour
{
    [SerializeField] private GunzeuBehaviour _gunzeuPrefab;
    [SerializeField] private TimeKeeper _timeKeeper;
    [SerializeField] private Vector2 _spawnPosition;
    [SerializeField] private Vector2 _spawnPositionRandomNess;
    [SerializeField] private Vector2 _moveTo;
    [SerializeField] private float _spawnInterval;
    [SerializeField] private float _spawnIntervalRandomNess;
    private float _elapsedTime = 0f;
    public void SpawnGunzeu(Vector2? spawnPosition = null)
    {
        Vector2 pos = spawnPosition ?? _spawnPosition;
        GunzeuBehaviour gunzeu = Instantiate(_gunzeuPrefab, pos, Quaternion.identity);
        gunzeu.Init(_timeKeeper.TimeRemaining, _moveTo, _timeKeeper);
    }
    private void Update()
    {
        if (_timeKeeper.RockPhase)
        {
            _elapsedTime += Time.deltaTime;
            float interval = _spawnInterval + Random.Range(-_spawnIntervalRandomNess, _spawnIntervalRandomNess);
            if (_elapsedTime >= interval)
            {
                _elapsedTime = 0f;
                Vector2 randomOffset = new Vector2(Random.Range(-_spawnPositionRandomNess.x, _spawnPositionRandomNess.x), Random.Range(-_spawnPositionRandomNess.y, _spawnPositionRandomNess.y));
                Vector2 spawnPos = _spawnPosition + randomOffset;
                SpawnGunzeu(spawnPos);
            }
        }
    }
}
