using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueInterface : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI interactHintText;
    public List<string> dialogueSequence;
    public AbilityToGive abilityToGive;
    
    private int currentIndex = 0;
    private bool inDialogue = false;
    private PlayerController pc;

    public void Awake()
    {
        dialogueText.enabled = false;
        interactHintText.enabled = false;
    }

    public void startDialogue(PlayerController pc)
    {
        this.pc = pc;
        dialogueText.enabled = true;
        interactHintText.enabled = false;
        currentIndex = 0;
        if (dialogueSequence.Count > 0)
        {
            dialogueText.text = dialogueSequence[currentIndex];
        }
        inDialogue = true;
    }
    
    public bool advanceDialogue()
    {
        if (!dialogueText.enabled) return false;
        
        currentIndex++;
        if (currentIndex >= dialogueSequence.Count)
        {
            endDialogue();
            return false;
        }

        dialogueText.text = dialogueSequence[currentIndex];
        return true;
    }
    
    public void endDialogue()
    {
        dialogueText.enabled = false;
        interactHintText.enabled = true;
        inDialogue = false;
        if (abilityToGive == AbilityToGive.Dash)
        {
            pc?.giveDashAbility();
        }
    }
    
    public bool isInDialogue()
    {
        return inDialogue;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            interactHintText.enabled = true; 
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            interactHintText.enabled = false; 
        }
    }
    
}
