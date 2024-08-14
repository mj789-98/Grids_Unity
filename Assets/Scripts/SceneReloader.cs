using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for using Button component

public class SceneReloader : MonoBehaviour
{
    public Button reloadButton; // Reference to the UI button

    void Start()
    {
        // Make sure the button is assigned in the Inspector
        if (reloadButton != null)
        {
            // Add a listener to the button's onClick event
            reloadButton.onClick.AddListener(ReloadScene);
        }
        else
        {
            Debug.LogError("Reload Button not assigned in the Inspector!");
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}