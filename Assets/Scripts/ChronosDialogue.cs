using UnityEngine;
public class ChronosDialogue : DialogueInterface
{
    public override void endDialogue()
    {
        base.endDialogue();
        print("Loading Title Menu");
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleMenu");
    }
}