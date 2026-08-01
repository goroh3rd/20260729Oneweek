using UnityEngine;
using DG.Tweening;

public class GunzeuBehaviour : MonoBehaviour
{
    [SerializeField] private Vector2 _moveTo;
    private float _moveTime;
    private TimeKeeper _timeKeeper;
    [SerializeField] private GameObject _gunzeuChild;
    [SerializeField] private GameObject _spear;
    [SerializeField] private GameObject _flag;
    [SerializeField] private GameObject _jingasa;
    public void Init(float moveTime, Vector2 moveTo, TimeKeeper timeKeeper)
    {
        _timeKeeper = timeKeeper;
        _moveTime = _timeKeeper.TimeLimit;
        _moveTo = moveTo;
        if (Random.value < 0.5f) _spear.SetActive(true);
        else _spear.SetActive(false);
        if (Random.value < 0.5f) _flag.SetActive(true);
        else _flag.SetActive(false);
        if (Random.value < 0.5f) _jingasa.SetActive(true);
        else _jingasa.SetActive(false);
        Move(_moveTime);
    }
    private void Move(float moveTime)
    {
        transform.DOMove(new Vector2(_moveTo.x, this.transform.position.y), moveTime).SetEase(Ease.Linear);
        _gunzeuChild.transform.DOShakePosition(moveTime, 1f, 10, 90f, false, true);
    }
    private void Update()
    {
        if (!_timeKeeper.RockPhase)
        {
            DOTween.KillAll();
        }
    }
}
