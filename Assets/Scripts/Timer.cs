using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Timer
{
    private float timeRemaining;
    private float baseTime;
    private bool justFinished;
    private bool finished = false;

    private static List<Timer> timers = new List<Timer>();
    
    public Timer(float time)
    {
        baseTime = time;
        timeRemaining = 0.0f;
        timers.Add(this);
    }

    /**
     * Set the timer to its base time.
     */
    public void restart()
    {
        timeRemaining = baseTime;
        finished = false;
        justFinished = false;
    }

    /**
     * Decrement remaining time by Time.deltaTime.
     */
    public void tick()
    {
        var nextTime = timeRemaining - Time.deltaTime;
        if (nextTime < 0)
        {
            justFinished = !finished;
            finished = true;
            timeRemaining = 0.0f;
        }
        else
        {
            timeRemaining = nextTime;
        }
    }

    /**
     * Interrupts the timer, setting the time remaining to 0.
     * isFinished() will return true after this is called.
     */
    public void interrupt()
    {
        timeRemaining = 0.0f;
    }
    
    /**
     * Returns true if the timer has finished counting down.
     */
    public bool isFinished()
    {
        return timeRemaining <= 0.0f;
    }

    /**
     * Returns true if the timer has just finished this frame.
     */
    public bool hasJustFinished()
    {
        return justFinished;
    }

    public override string ToString()
    {
        return "Timer(timeRemaining=" + timeRemaining + ", baseTime=" + baseTime + ", finished=" + finished + ", justFinished=" + justFinished + ")";
    }

    public static void tickAll()
    {
        foreach (var timer in timers)
        {
            timer.tick();
        }
    }
}