# Conversation_Module
Conversation Module

This module created for easy use to create my visual novel. But it doesn't mean it can't be used in other projects.
It contain typing effect derivative from TextMeshPro, which add some tag to change variable of script.

 You can either clone the project or [download package](https://github.com/Putragonist/Conversation_Module/tree/master/Assets/ConversationModule/Package)

Since I make this using Unity 2018.3.0f2, using the same unity version is recommended to avoid any error for compatibility (Which I didn't check).
Please use NET version 4.x since some method using C# 7. To change it, go to ```edit->Project Setting->Player->Other Setting->Runtime Script Version```, and change to version 4.

This project already contain TextMeshPro Resource, so you doesn't need to change it anymore (Except if you use package instead).

Make sure input name in script ```MultipleText.cs``` exist in your ```input``` setting in ```Project Setting``` 

# Tags #
This module using Regex to make a tag which used to get value of tags in string, and applied it in variable you need.
For adding more command, just edit ```Command``` method with tag you want. 

For this project, You can run this tag to change variable:

```[conservant={string}]``` Example ```[conservant=Udin]``` to change name conservant.

```[char_limit={int}]``` Example ```[char_limit=70]``` to limit char in string in one season, and put the rest to other season. 

For example, if you have string which has 150 char length and you limit it 70, it will show the 70 at first season, 70 at second season, and 10 at third season.
This useful if you have a translation which have different char count, which probably the text box can't contain them all.

```[reset_char_limit]``` Reset char limit to what initialized in inspector.

```[char_delay={int}]``` Example ```[char_delay=1000]``` To change delay speed per char of string for typing effect. Since it's delay, the more number, the more slower text will typing. It use millisecond (1s = 1000 ms).

```[PauseAllText={int}]``` Example ```[PauseAllText=5]``` Pause all active text with time (second) as parameter input. (Added 2019/2/27)

```[PauseAllText]``` Pause all active text. (V1.1.0)

```[PauseText={int}]``` Example ```[PauseAllText=5]``` Pause text with time (second) as parameter input. (V1.1.0)

```[PauseText]``` Pause  text. (V1.1.0)

```[reset_char_delay]``` Reset char delay speed to default (The one you fill in inspector)

```[clear]``` Clear text that type before.

```[alwaysClearText]``` After one season of text is done, it will clear the previous text. Use this when you want to change from NVL style to ADV style.

```[neverClearText]``` After one season of text is done, keep previous text. Use this when you want to change from ADV style to NVL

# How To #

How to change from one text to another, you just need to change text in ```TypingEffect```. 

But to make ```[char_limit={int}]``` tag works you need to use method that implemented in ```TypingEffect```:

You need to call ```ChangeText(string text, out bool allTextCompleted)```

```string text``` Full text of text you want to change.

```bool allTextCompleted``` Check if all divided season of text has shown and finished. If it has ```false``` value, the text will change to
next season of text rather change it with new text. So, to access this method from your class, you need to use:

```
public int[] texts; //In case you use multiple text.

private void ChangeText()
{
    if (idx < texts.Count) //Make sure it didn't out of array
    {
        textInBox.ChangeText(texts[idx], out bool indicator); //if it return false, the text will to next season of divided text instead                
        if (indicator)
        {
            this.idx++; //Prepare for next text
        }
     }
     else
     {
        textInBox.ChangeNextText(); //try to change text, if text had been divided by char limit.
     }
 }         
 ```
 ## Add your own command ##
 To add your own command, it have 3 method.
 
 First method : Direct Edit of Command Method in classTypingEffect. Disavantage using this method is if you updating the package it will overwrite your code.
 
 Second Method : Create a class that inherit ```class TypingEffect```. Create an override method ```Command``` with including base method inside. Advantage using this is your code will exist even you update the package.
 
 Third Method : Create your own class command using template ```class Commands_Template``` which is inherit ```class Commands```.
 
# Thought and What Can Be Improved #
For now, I use boolean in MultipleText script to check everything in TypingEffect script. This could be changed using delegates and event, so I can remove Update() function to increase performance.

I'm updating ```text count``` in new thread served by C#. Unity dev said that access and modifying between thread is expensive. So, it probably better if I use Unity Entity system or Job System which are unity served rather running my own thread. I'm still learning about unity job system, and for now, using clasic method still easier for me.

Copyright @ [Putragonist](http://putragonist.com)
