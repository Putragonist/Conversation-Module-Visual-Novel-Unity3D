using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Commands : ScriptableObject
{
    public abstract Dictionary<string, bool> GetCommands();
    public abstract void DoCommand(string command);
    public abstract void DoCommand(string command, string value);
}
