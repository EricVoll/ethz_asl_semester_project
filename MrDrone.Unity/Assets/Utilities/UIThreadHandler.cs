using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIThreadHandler
{
    List<Action> actionsToExecuteOnMainThread = new List<Action>();
    public void ReportUpdate()
    {
        lock (actionsToExecuteOnMainThread)
        {
            if (actionsToExecuteOnMainThread.Count > 0)
            {
                for (int i = actionsToExecuteOnMainThread.Count - 1; i >= 0; i--)
                {
                    actionsToExecuteOnMainThread[i]();
                }
            }
            actionsToExecuteOnMainThread.Clear();
        }
    }

    /// <summary>
    /// Queues an action which will be executed on the main thread using the Update method in the same order as they are inserted here.
    /// First come first serve
    /// </summary>
    /// <param name="action"></param>
    public void ExecuteOnMainThread(Action action)
    {
        actionsToExecuteOnMainThread.Insert(0, action);
    }
}
