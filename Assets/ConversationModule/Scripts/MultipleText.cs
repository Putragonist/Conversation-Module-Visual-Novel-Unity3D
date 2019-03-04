
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Conversation_Module
{
    public class MultipleText : MonoBehaviour
    {
        [Tooltip("Text in textbox that this text want to show and process")]
        public TypingEffect textInBox;

        public TMPro.TMP_Text nameBox;

        [Tooltip("List of text")]
        public List<string> texts = new List<string>();
        private int idx = 0;
        public bool SkipText = false;
        public float waitTime = 1; //1000ms or 1s       
        private float timeSub = 0;
        public bool auto = false;
        private bool lastUpdate = false;

        //StringBuilder lastName = new StringBuilder();
        string lastConservant = "";

        private void Awake()
        {
            timeSub = Time.time;
            ChangeText(true);
            textInBox.Initiate();
        }

        private void Update()
        {            
            if(nameBox != null && lastConservant != textInBox.conservant)
            {
                nameBox.text = textInBox.conservant;
                lastConservant = textInBox.conservant;
            }

            if (lastUpdate != textInBox.isTextFinished_UpdateSide)
            {
                lastUpdate = textInBox.isTextFinished_UpdateSide;
                timeSub = Time.time;
            } 

            if (SkipText)
            {
                if (Input.GetButtonUp("CompleteText"))
                {
                    SkipText = false;
                    return;
                }
                textInBox.CompleteText();
                ChangeText();
            }

            else if (textInBox.isTextFinished_UpdateSide)
            {
                if (auto && Time.time - timeSub > waitTime)
                {                              
                    ChangeText();
                    timeSub = Time.time;
                }
                else if (Input.GetButtonUp("CompleteText"))
                {                    
                    ChangeText();
                }
            }
            else
            {
                if (Input.GetButtonUp("CompleteText"))
                {
                    auto = false;
                    textInBox.CompleteText();
                }
            }
        }

        //bool indicator;
        private void ChangeText(bool start = false)
        {            
            if (idx < texts.Count)
            {                
                textInBox.ChangeText(texts[idx], out bool indicator, start);
               
                if (indicator)
                {
                    this.idx++;
                }
            } else
            {
                textInBox.ChangeNextText();
            }
        }
    }
}