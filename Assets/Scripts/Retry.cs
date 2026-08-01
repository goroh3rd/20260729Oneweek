using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Retry : MonoBehaviour
{
    public void OnRetryButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
