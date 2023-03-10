using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Logic of the main menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField]
    MenuButton StartButton;

    [SerializeField]
    MenuButton QuitButton;

    Camera cam;

    void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.None;
        StartButton.OnClick = PlayGame;
        QuitButton.OnClick = Quit;
    }

    void PlayGame()
    {
        string sceneName = "ShapeGrammar";
        SceneManager.LoadScene(sceneName);
    }

    void Quit()
    {
        Application.Quit();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mouseRay = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out var hit))
            {
                var button = hit.collider.GetComponent<MenuButton>();
                if (button != null)
                {
                    button.OnClick();
                }
            }
        }
    }
}
