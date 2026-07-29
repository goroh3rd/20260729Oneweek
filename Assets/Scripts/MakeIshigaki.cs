using UnityEngine;

public class MakeIshigaki : MonoBehaviour
{
    [SerializeField] private TimeKeeper _timeKeeper;
    private void Update()
    {
        if (_timeKeeper.RockPhase)
        {
            ActivateAllChildren();
        }
        else
        {
            DeactivateAllChildren();
        }
    }
    private void ActivateAllChildren()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf == false)
            {
                child.gameObject.SetActive(true);
            }
        }
    }
    private void DeactivateAllChildren()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf == true)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
