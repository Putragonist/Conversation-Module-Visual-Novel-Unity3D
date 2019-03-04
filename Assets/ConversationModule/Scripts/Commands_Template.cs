using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conversation_Module
{
    [CreateAssetMenu(menuName = "Command/Commands_Template")]
    public class Commands_Template : Commands
    {
        /// <summary>
        /// command as key, is retrieve value as value
        /// </summary>
        public Dictionary<string, bool> command = new Dictionary<string, bool>();

        public Commands_Template()
        {
            Initiate();
        }

        public override void Initiate()
        {
            command.Clear();
            command.Add(@"\[PauseAllText=(.*)\]", true);
            command.Add("[PauseAllText]", false);
        }

        public override Dictionary<string, bool> GetCommands()
        {            
            return command;
        }

        public override void DoCommand(string command, string value)
        {
            if (command == @"\[PauseAllText=(.*)\]")
            {
                float result = 0;
                float.TryParse(value, out result);
                TimeManager.PauseAll(result);

                Debug.Log("Command " + command + " is triggered and Value is " + value);
            }
        }

        public override void DoCommand(string c)
        {

            if (c == "[PauseAllText]")
            {
                TimeManager.PauseAll();
                Debug.Log("Command " + command + " is triggered");
            }
        }

       
    }
}