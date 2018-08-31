using System;
using UnityEngine;

public class ProfileBlock : IDisposable
{
    private string name;
    private float startTime;

    public ProfileBlock (string name)
    {
        this.name = name;
        this.startTime = Time.realtimeSinceStartup;
    }

    public void Dispose ()
    {
        Debug.Log ($"{name}: {Time.realtimeSinceStartup - startTime}");
    }
}