using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UgoUgoScript : MonoBehaviour
{
    [SerializeField] private float _interval = 0.5f;
    [SerializeField] private List<Sprite> _sprites;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private int index = 0;
    private Coroutine _changeSpriteCoroutine;
    // private void Start()
    // {
    //     StartAnimation();
    // }
    private void OnEnable()
    {
        StartAnimation();
    }
    private void OnDisable()
    {
        StopAnimation();
    }
    public void StopAnimation()
    {
        StopCoroutine(_changeSpriteCoroutine);
    }
    public void StartAnimation()
    {
        _changeSpriteCoroutine = StartCoroutine(ChangeSpriteCoroutine());
    }
    private IEnumerator ChangeSpriteCoroutine()
    {
        while (true)
        {
            _spriteRenderer.sprite = _sprites[index];
            index = (index + 1) % _sprites.Count;
            yield return new WaitForSeconds(_interval);
        }
    }
}
