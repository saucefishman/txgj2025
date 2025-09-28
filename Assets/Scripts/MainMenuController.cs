using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private static string START_BUTTON_NAME = "startButton";
    private static string QUIT_BUTTON_NAME = "quitButton";
    
    private static string GAME_SCENE_NAME = "Game";
        
    void Awake()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var startButton = root.Q<Button>(START_BUTTON_NAME);
        startButton.clicked += () => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(GAME_SCENE_NAME);
        };
        var quitButton = root.Q<Button>(QUIT_BUTTON_NAME);
        quitButton.clicked += () => {
            Application.Quit();
        };
    }
}
