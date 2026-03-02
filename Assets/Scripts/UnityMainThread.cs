using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThread : MonoBehaviour
{
    static readonly Queue<Action> actions = new Queue<Action>();

    public static void Execute(Action action)
    {
        lock(actions)
        {
            actions.Enqueue(action);
        }
    }

    void Update()
    {
        while (actions.Count > 0)
        {
            actions.Dequeue().Invoke();
        }
    }
}