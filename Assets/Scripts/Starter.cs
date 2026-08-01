using UnityEngine;
using UnityEngine.SceneManagement;

public class Starter : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
