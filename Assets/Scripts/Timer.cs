using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private float timeRemaining;
    private float baseTime;
    private bool justFinished;
    private bool finished = false;

    private static List<Timer> timers = new List<Timer>();

    public Timer(float time, bool addToRegistry=true)
    {
        baseTime = time;
        timeRemaining = 0.0f;
        finished = true;
        if (addToRegistry)
            timers.Add(this);
    }

    public float getTimeRemaining()
    {
        return timeRemaining;
    }

    public void setTimeRemaining(float newTime)
    {
        timeRemaining = newTime;
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
     * Decrement remaining time by Time.deltaTime and set finished if time runs out. This is the only method
     * that will set finished and justFinished to be true.
     */
    /// <summary>
    /// Decrement remaining time by Time.deltaTime and set finished if time runs out. This is the only method
    /// that will set finished and justFinished to be true.
    /// </summary>
    /// 
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

    /// <summary>
    /// Returns true if the timer has finished (time remaining is 0 or less).
    /// </summary>
    /// <returns>Returns true if the timer has finished (time remaining is 0 or less).</returns>
    public bool isFinished()
    {
        return timeRemaining <= 0.0f;
    }

     /// <summary>
     ///  Returns true if the timer has just finished on the most recent tick.
     /// </summary>
     /// <returns>If the timer has just finished on the most recent tick.</returns>
    public bool hasJustFinished()
    {
        return justFinished;
    }

    public override string ToString()
    {
        return "Timer(timeRemaining=" + timeRemaining + ", baseTime=" + baseTime + ", finished=" + finished +
               ", justFinished=" + justFinished + ")";
    }

    /// <summary>
    /// Call this method from a MonoBehaviour's Update() method to tick all registered timers.
    /// </summary>
    public static void tickRegistered()
    {
        foreach (var timer in timers)
        {
            timer.tick();
        }
    }
}