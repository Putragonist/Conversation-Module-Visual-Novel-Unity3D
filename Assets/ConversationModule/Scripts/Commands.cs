using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conversation_Module
{
    public abstract class Commands : ScriptableObject
    {
        
        public abstract Dictionary<string, bool> GetCommands();
        public abstract void Initiate();
        public abstract void DoCommand(string command);
        public abstract void DoCommand(string command, string value);
    }
}