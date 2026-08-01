using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreShower : MonoBehaviour
{
    [SerializeField] private Vector2 _startPos;
    [SerializeField] private float _spacing;
    [SerializeField] private List<UgoUgoScript> _nums;
    [SerializeField] private UgoUgoScript _dot;
    [SerializeField] private GameObject _metre;
    public void ShowScore(float score)
    {
        _metre.SetActive(true);
        string scoreStr = score.ToString("F3"); // 小数点以下3桁まで表示
        int index = 0;
        foreach (char c in scoreStr)
        {
            UgoUgoScript numScript = Instantiate(c == '.' ? _dot : _nums[int.Parse(c.ToString())], transform);
            numScript.transform.localPosition = new Vector2(_startPos.x + index * _spacing, _startPos.y);
            index++;
        }
    }
}
