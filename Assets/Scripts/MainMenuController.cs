using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public Texture2D startButtonNormalTexture;
    public Texture2D startButtonPressedTexture;
    public Texture2D quitButtonNormalTexture;
    public Texture2D quitButtonPressedTexture;
    private static string START_BUTTON_NAME = "startButton";
    private static string QUIT_BUTTON_NAME = "quitButton";
    
    private static string GAME_SCENE_NAME = "MyLevel";
        
    void Awake()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var startButton = root.Q<Button>(START_BUTTON_NAME);
        startButton.clicked += () => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(GAME_SCENE_NAME);
        };
        startButton.RegisterCallback<MouseUpEvent>(evt =>
        {
            startButton.style.backgroundImage = startButtonNormalTexture;
        });
        startButton.RegisterCallback<MouseDownEvent>(evt =>
        {
            startButton.style.backgroundImage = startButtonPressedTexture;
        }, TrickleDown.TrickleDown);
        
        var quitButton = root.Q<Button>(QUIT_BUTTON_NAME);
        
        quitButton.RegisterCallback<MouseUpEvent>(evt =>
        {
            quitButton.style.backgroundImage = quitButtonNormalTexture;
        });
        quitButton.RegisterCallback<MouseDownEvent>(evt =>
        {
            quitButton.style.backgroundImage = quitButtonPressedTexture;
        }, TrickleDown.TrickleDown);
        
        quitButton.clicked += () => {
            Application.Quit();
        };
    }
}
