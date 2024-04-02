using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaygroundExitUI : MonoBehaviour
{
    [SerializeField] private GameObject exitDialogPanel;
    // Start is called before the first frame update
    void Start()
    {
        exitDialogPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            exitDialogPanel.SetActive(!exitDialogPanel.activeSelf);
        }
        if(exitDialogPanel.activeSelf && Input.GetKeyDown(KeyCode.Y))
        {
            ConfirmExitPlayground();
        }
    }

    public void CancelExitPlayground()
    {
        exitDialogPanel.SetActive(false);
    }

    public void ConfirmExitPlayground()
    {
        Debug.Log("Leave playground");
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadSceneAsync(0);
    }
}
