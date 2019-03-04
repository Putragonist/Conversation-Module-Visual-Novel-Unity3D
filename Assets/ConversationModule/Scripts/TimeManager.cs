using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager
{
    
    public TimeManager()
    {

    }

    private static bool allUsingTime = false;
    private static bool isAllPause = false;    
    public static bool IsAllPause
    {
        get
        {
            if (allUsingTime && Time.time - pauseAllStartTime > pauseAllTime)
                isAllPause = false;
            return isAllPause;
        }
    }
    private static float pauseAllTime = 0;
    private static float pauseAllStartTime = 0;   
    

    public static void PauseAll(float pauseTime = 0)
    {
        if (pauseTime > 0)
        {
            pauseAllTime = pauseTime;
            pauseAllStartTime = Time.time;
            allUsingTime = true;
        } else
        {
            allUsingTime = false;
        }
        
        isAllPause = true;
    }

    public static void ContinueAll()
    {
        isAllPause = false;
    }
    private bool isPause = false;
    public bool IsPause
    {
        get
        {
            if (usingTime && Time.time - pauseStartTime > pauseTime)
                isPause = false;
            return isPause;
        }
    }
    private float pauseTime = 0;
    private float pauseStartTime = 0;
    private bool usingTime = false;

    public void Pause(float pauseTime = 0)
    {
        if (pauseTime > 0)
        {
            usingTime = true;
            this.pauseTime = pauseTime;
            pauseStartTime = Time.time;
        }
        else
        {
            usingTime = false;
        }
        isPause = true;
    }

    public void Continue()
    {
        isPause = false;
    }
}
