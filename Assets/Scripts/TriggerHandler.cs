using System;
using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    public PlayerController pc;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        pc.AcceptTriggerEnter(other);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        pc.AcceptTriggerExit(other);
    }
}
