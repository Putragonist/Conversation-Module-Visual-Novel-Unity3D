/*
MIT License

Copyright (c) 2019 Muhammad Ihsan Diputra (https://github.com/Putragonist)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using System.Text.RegularExpressions;
using System.Text;

namespace Conversation_Module
{
    public class TypingEffect : MonoBehaviour
    {
        private string textInTextMeshProGUI = string.Empty; //string of full text.
        private string lastText = string.Empty; //As comparer it text has changed
        public string conservant = string.Empty; // The conservant who talking
        private Object objLock = new Object(); //lock for thread
        [Tooltip("Text speed delay")]
        public int perCharDelay = 1000; //1000ms or 1 s;
        private int init_perCharDelay = 1000; //save variable of init files
        [Tooltip("Text that shown and processed for typing effect")]
        public string text; //text that want to be showed
        [Tooltip("Limit character shown in one season")]
        public int limitChar = 180;
        private int init_limitChar = 280;
        [Tooltip("Everytime text change. Clear previous text")]
        public bool clearNewText = true;
        private Thread th;
        private Queue qth = new Queue(); // q to process thread
        private TMP_Text m_textMeshPro;
        private TimeManager timeManager;
        public TimeManager TimeManagerTE
        {
            get
            {
                return timeManager;
            }
        }

        int visibleCount = 0;
        int totalVisibleCharacters = 0;

        [HideInInspector]
        /// <summary>
        /// Check if in function Update all text already rendered. Update Side always more come after thread side processing
        /// </summary>
        public bool isTextFinished_UpdateSide = false;

        // Start is called before the first frame update
        void Start()
        {
            if (!initiated)
                Initiate();
        }

        private bool initiated = false;

        private void Awake()
        {
            if (!initiated)
                Initiate();
        }
        private void OnEnable()
        {
            if (!initiated)
                Initiate();
        }

        public void Initiate()
        {
            timeManager = new TimeManager();
            init_perCharDelay = perCharDelay + 0;
            init_limitChar = limitChar + 0;
            m_textMeshPro = GetComponent<TMP_Text>();
            UpdateText();

            th = new Thread(RunText);
            qth.Enqueue(th);
            th.Start();
            initiated = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (timeManager.IsPause || TimeManager.IsAllPause)
                return;

            if (isTextChanged)
            {
                lock (objLock)
                {
                    UpdateText();
                    isTextFinished_UpdateSide = false;
                    m_textMeshPro.maxVisibleCharacters = visibleCount;
                    if (!th.IsAlive)
                    {
                        th = new Thread(RunText);
                        qth.Enqueue(th);
                        th.Start();
                    }
                }
            }
            else
            {
                if (stringq.Count > 0 && totalVisibleCharacters == m_textMeshPro.maxVisibleCharacters)
                {
                    text = stringq.Dequeue();
                    ////Debug.Log("String after dequeue= " + text);
                }
            }


            if ((totalVisibleCharacters != m_textMeshPro.maxVisibleCharacters))
            {
                m_textMeshPro.maxVisibleCharacters = visibleCount;

            }
            else if (isQDone)
            {
                isTextFinished_UpdateSide = true;
            }
        }

        Queue<string> textLimited = new Queue<string>();
        Queue<string> stringq = new Queue<string>();
        bool isQDone = true;
        bool firstInitiated = false;
        StringBuilder tempText = new StringBuilder();
        StringBuilder tempText2 = new StringBuilder();
        string _connectSymbol = ((char)0x2014).ToString(); //Dash symbol to symbolize connection between text
        bool firstSentence = true; //Is this a first sentence when text change
        int longSentence = 0; //how long (char) sentence is.
        /// <summary>
        /// UpdateText. Calling this method after text changed. Inside this method calling other method to
        /// process (Command) in string if it's have any.
        /// </summary>
        private void UpdateText()
        {
            if (isQDone)
            {
                //Devide text with text limit char count
                if (limitChar > 0 && LengthTextWithoutCommand(text) + m_textMeshPro.maxVisibleCharacters
                    > limitChar && textLimited.Count == 0)
                {
                    string[] split = Regex.Split(text, @"(?=\s)");
                    tempText.Clear();
                    tempText2.Clear();
                    tempText.Append(split[0]);
                    tempText2.Append(split[0]);
                    firstSentence = true;
                    for (int i = 1; i < split.Length; i++)
                    {
                        tempText2.Append(split[i]);

                        longSentence = firstSentence ? LengthTextWithoutCommand(tempText2.ToString()) +
                            visibleCount : LengthTextWithoutCommand(tempText2.ToString());
                      //  Debug.Log("Long Sentence = " + longSentence);
                        if (longSentence > limitChar)
                        {
                            if (i < split.Length - 1)
                            {
                                tempText.Append(_connectSymbol);
                            }
                            textLimited.Enqueue(tempText.ToString());
                            tempText.Clear();
                            tempText.Append(split[i]);
                            tempText2.Clear();
                            tempText2.Append(split[i]);
                            firstSentence = false;
                        }
                        else
                        {
                            tempText.Append(split[i]);
                        }

                        if (i == split.Length - 1)
                        {
                            textLimited.Enqueue(tempText.ToString());
                        }
                    }
                    text = textLimited.Dequeue();
                }

                //split text for get the command
                foreach (string s in Regex.Split(text, @"(?=\[)|(?<=\])"))
                {
                    stringq.Enqueue(s);
                }
                text = stringq.Dequeue(); //deque it first to change the text
                isQDone = false; //since it add to que, that mean que is still not done
            }
            text = RunandClearCommand(text); // process regex
            lastText = text; //set last text to know text is not changed

            //Add sentence from previous sentence. If you want to clear it, add command [clear] in string            
            match = Regex.Match(textInTextMeshProGUI, @"([a-z]|[A-Z])$");
            if (match.Success)
            {
                textInTextMeshProGUI += ((match.Value.Length > 0) ? string.Empty : " ") + lastText;
            }
            else
            {
                textInTextMeshProGUI += (textInTextMeshProGUI == string.Empty ? " " : string.Empty) + lastText;
            }

            // 
            m_textMeshPro.text = textInTextMeshProGUI;
            m_textMeshPro.ForceMeshUpdate();
            totalVisibleCharacters = m_textMeshPro.textInfo.characterCount; // Get # of Visible Character in text object                    
            if (!isQDone && stringq.Count == 0)
            {
                isQDone = true;
            }
        }

        Match match;
        System.Object regVal;

        protected virtual string RunandClearCommand(string text)
        {
            return Command(text, out int countChar, false);
        }

        protected virtual int LengthTextWithoutCommand(string text)
        {
            Command(text, out int countChar, true);
            return countChar;
        }

        bool doCommand = false;
        bool success = false;
        public Commands additionCommand;
        public string someVariable = ""; //for texting purpose;

        /// <summary>
        /// run command if it exist in string. and clear commands in string.
        /// </summary>
        /// <param name="text">text with command in it if it has any</param>
        /// <returns>String without any command string</returns>
        protected virtual string Command(string text, out int countChar, bool ignoreCommand)
        {
            doCommand = !ignoreCommand;

            //---------------------------Run Aditional Command-------------------------------------
            if(additionCommand != null)
            {
              //  Debug.Log("Command count = "+additionCommand.GetCommands().Count);
                foreach (KeyValuePair<string,bool> valPair in additionCommand.GetCommands())
                {
                    //if (valPair.Key == string.Empty)
                        //continue;
                    if (valPair.Value) {
                        regVal = GetCommandValue(ref text, @valPair.Key, @"(?<=\=)(.*)(?=\])", out success);
                        
                        if (success)
                        {
                            if(doCommand)
                                additionCommand.DoCommand(valPair.Key, (string)regVal);

                            #region //Editing value inside
                            /*
                            System.Reflection.PropertyInfo propertyInfo = this.GetType().GetProperty(valPair.Value,System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);
                            if (propertyInfo != null)
                            {
                                if (doCommand)
                                {
                                    Debug.Log("This in typing script "+@valPair.Key);
                                    additionCommand.DoCommand(@valPair.Key);
                                }
                                if (propertyInfo.GetType() == typeof(string))
                                    propertyInfo.SetValue(this,(string)regVal);
                                else if(propertyInfo.GetType() == typeof(int))
                                    propertyInfo.SetValue(this, int.Parse((string)regVal));
                                else if (propertyInfo.GetType() == typeof(float))
                                    propertyInfo.SetValue(this, float.Parse((string)regVal));
                                else
                                {
                                    Debug.LogWarning("Type data is not supported. Command will not run");
                                }


                            } 
                            else
                            {
                                if(doCommand)
                                    Debug.Log("This in typing script has key " + @valPair.Key+" and value "+valPair.Value);
                            }*/
                            #endregion

                        }
                    } else if (text.Contains(valPair.Key))
                    {                        
                        if (doCommand)
                            additionCommand.DoCommand(valPair.Key);
                        text = text.Replace(valPair.Key, string.Empty);
                        //Debug.Log("Command doesn't have any value");
                    }
                }
            } else
            {
                //Debug.Log("No Addition Command");
            }

            //-----------------Change Conservant------------------------------------
           // regVal = GetCommandValue(ref text, @"\[conservant=(.*)\]", @"(?>\=)(.*?)(?<\])", out bool success);
            regVal = GetCommandValue(ref text, @"\[conservant=(.*)\]", @"(?<=\=)(.*)(?=\])", out success);
         //   Debug.Log(success);
            if (success && doCommand)
            {
                conservant = (string)regVal;
               // Debug.Log("Conservant is = " + conservant);
            }

            //-----------------Clear Conservant-------------------------------------------------
            if(text.Contains("[clear_conservant]"))
            {
                if(doCommand)
                    conservant = string.Empty;
                text = text.Replace("[clear_conservant]", string.Empty);
            }

           // Debug.Log("Conservant is = " + conservant);
            //---------------Change Text Limit--------------------------------------------
            regVal = GetCommandValue(ref text, @"\[char_limit=\d+\]", @"\d+");
            if(doCommand)
                limitChar = (regVal != null) ? int.Parse((string)regVal) : limitChar;

            //---------------Use initialized value of limitchar from inspector------------
            if (text.Contains("[reset_char_limit]"))
            {
                if(doCommand)
                    limitChar = init_limitChar;
                text = text.Replace("[reset_char_limit]", string.Empty);
            }

            //---------------Change perCharDelay speed------------------------------------

            regVal = GetCommandValue(ref text, @"\[char_delay=\d+\]", @"\d+");
            if(doCommand)
                perCharDelay = (regVal != null) ? int.Parse ((string) regVal) : perCharDelay;

            //---------------Pause text typing effect------------------------------------
            regVal = GetCommandValue(ref text, @"\[PauseText=\d+\]", @"\d+",out success);

            Debug.Log("Pausing for " +(string) regVal + " or " + regVal + " second");
            if (doCommand && success)
            {
                double result = 0;
                double.TryParse((string) regVal, out result);
                timeManager.Pause((float)result);
                Debug.Log("Pausing for "+result+" or "+ (string)regVal +" second");
            }

            if (text.Contains("[PauseText]"))
            {
                if (doCommand)
                {
                    timeManager.Pause();
                    Debug.Log("Pausing");
                }
            }

            //----------------------------------------------------------------------------
            //---------------Clear all visible text---------------------------------------
            if (text.Contains("[clear]"))
            {
                //text = text.Replace("[clear]", string.Empty);
                if(doCommand)
                    ClearText();
            }
            //----------------------------------------------------------------------------
            //------------------Change char delay time to init value----------------------
            if (text.Contains("[reset_char_delay]"))
            {
                if(doCommand)
                    perCharDelay = init_perCharDelay;
                text = text.Replace("[reset_char_delay]", string.Empty);
            }

            //----------------------------------------------------------------------------
            //------------------Always Clear previous Text when text change---------------
            if (text.Contains("[alwaysClearText]"))
            {
                if(doCommand)
                    clearNewText = true;
                text = text.Replace("[alwaysClearText]", string.Empty);
            }

            //----------------------------------------------------------------------------
            //------------------Never Clear previous Text when text change---------------

            if (text.Contains("[neverClearText]"))
            {
                if(doCommand)
                    clearNewText = false;
                text = text.Replace("[neverClearText]", string.Empty);
            }
            //----------------------------------------------------------------------------- 
            string[] tsplit = Regex.Split(text,"[clear]");
            int[] tcsplit = new int[tsplit.Length];
            for(int i=0; i < tsplit.Length; i++)
            {
                tcsplit[i] = tsplit[i].Length;
            }
            if (tsplit.Length > 0)
                countChar = Mathf.Max(tcsplit);
            else
                countChar = text.Length;

            return text;
        }

        /// <summary>
        /// Get the command and return it value
        /// </summary>
        /// <param name="text">the full text that want to be processed</param>
        /// <param name="tagWithRegex">command complete with it regex</param>
        /// <param name="regexVal">val to be returned</param>
        /// <returns>value of regex</returns>
        protected System.Object GetCommandValue(ref string text, string tagWithRegex, string regexVal)
        {
            return GetCommandValue(ref text, tagWithRegex, regexVal, out bool success);            
        }       
        

        /// <summary>
        /// Get the command and return it value
        /// </summary>
        /// <param name="text">the full text that want to be processed</param>
        /// <param name="tagWithRegex">command complete with it regex</param>
        /// <param name="regexVal">val to be returned</param>
        /// <param name="success">Is matching process is success</param>
        /// <returns>value of regex</returns>
        protected System.Object GetCommandValue(ref string text, string tagWithRegex, string regexVal, out bool success)
        {
            success = false;
            match = Regex.Match(text, @tagWithRegex);
            if (match.Success)
            {
                string temp = match.Value;
              //  Debug.Log("Command is"+match.Value);
                match = Regex.Match(temp, @regexVal);
                if (match.Success)
                {
                    success = true;
                    Regex r = new Regex(@tagWithRegex);
                    text = r.Replace(text, string.Empty);
                 //   Debug.Log("Value is " + match.Value);
                    return match.Value;
                }
                else
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            return null;
        }

        /// <summary>
        /// Clear the text
        /// </summary>
        public void ClearText()
        {
            visibleCount = 0;
            textInTextMeshProGUI = string.Empty;
            lastText = string.Empty;
        }

        /// <summary>
        /// Check if text changed
        /// </summary>
        public bool isTextChanged
        {
            get
            {
                if (lastText != text)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Check if all sentense is already finished in thread side. Text more likely not finished when the display still not finished
        /// </summary>
        public bool isTextFinished_ThreadSide
        {
            get
            {
                if (isQDone && visibleCount == totalVisibleCharacters)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Force to complete text
        /// </summary>
        public void CompleteText()
        {
            isTextFinished_UpdateSide = false;
            if(stringq.Count > 0)
            {
                while(stringq.Count > 0) { 
                    text = stringq.Dequeue();
                    //Debug.Log("String after dequeue = " + text);
                    UpdateText();
                    //Debug.Log("Complete Text");
                }
            }
            visibleCount = totalVisibleCharacters;
        }

        /// <summary>
        /// This the main process to give typewriting effect
        /// </summary>
        void RunText()
        {
            while (visibleCount < totalVisibleCharacters)
            {
                lock (objLock)
                {
                    visibleCount += 1;
                }
                Thread.Sleep(perCharDelay);
            }
            qth.Dequeue();
        }


        /// <summary>
        /// Changing text safely
        /// </summary>
        /// <param name="text">change text with this text</param>
        /// <param name="allTextCompleted">Is there still any text in queue. If there still text, change text
        /// with the queue instead and return false (Text will not changed with the input)</param>
        public void ChangeText(string text, out bool allTextCompleted, bool isFirstText = false)
        {
            //firstInitiated = start;
            //isQDone = firstInitiated ? true : isQDone;
            allTextCompleted = false;
            if(textLimited.Count == 0)
            {
                if (clearNewText)
                    ClearText();
                allTextCompleted = true;
                this.text = text;
                isQDone = true;
            } else
            {
                ClearText();
                this.text = textLimited.Dequeue();
            }
        }

        /// <summary>
        /// Force change text to next divided text
        /// </summary>
        public void ChangeNextText()
        {
            if(textLimited.Count > 0)
            {
                ClearText();
                this.text = textLimited.Dequeue();
            }
        }          

        
    }
}