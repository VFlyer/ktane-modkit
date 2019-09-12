using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class LEDDirections : Puzzle
{
    List<int> input = new List<int>();
    List<int>[] stages = new List<int>[3];
    int currentStage = 0;
    
    public LEDDirections(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving LED Directions. Alphanumeric keys present are: {1}. LEDs are: {2}.", moduleId, info.alphabet.Join(", "),  info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
    
        for(int i = 0; i < stages.Length; i++)
        {
            CalcStage(i);
            Debug.LogFormat("[The Modkit #{0}] Stage {1} presses are {2}.", moduleId, i + 1, stages[i].Select(x => ComponentInfo.DIRNAMES[x]).Join(", "));
        }
    }

    public override void OnAlphabetPress(int alphabet)
    {
        if(module.IsAnimating())
            return;

        module.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.alphabet[alphabet].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed alphanumeric key {1} when component selection was [ {2} ] instead of [ {3} ].", moduleId, alphabet + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        module.StartSolve();

        if(alphabet < currentStage)
            return;
            
        if(alphabet > currentStage)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed alphanumeric key {1} on stage {2}.", moduleId, alphabet + 1, currentStage + 1);
            module.GetComponent<KMBombModule>().HandleStrike();
            input.Clear();
            return;
        }

        if(input.SequenceEqual(stages[currentStage]))
        {
		    Debug.LogFormat("[The Modkit #{0}] Received correct input [ {1} ] on stage {2}.", moduleId, input.Select(x => ComponentInfo.DIRNAMES[x]).Join(", "), currentStage + 1);
            module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[0];
            currentStage++;
            input.Clear();
            if(currentStage == 3)
            {
		        Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Received input [ {1} ] on stage {2}.", moduleId, input.Select(x => ComponentInfo.DIRNAMES[x]).Join(", "), currentStage + 1);
            input.Clear();
        }
    }

    public override void OnArrowPress(int arrow)
    {
        if(module.IsAnimating())
            return;

        module.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.arrows[arrow].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed {1} arrow when component selection was [ {2} ] instead of [ {3} ].", moduleId, ComponentInfo.DIRNAMES[arrow], module.GetOnComponents(), module.GetTargetComponents());
            module.GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        module.StartSolve();
        input.Add(arrow);
    }

    void CalcStage(int n)
    {
        stages[n] = new List<int>();

        switch(info.LED[n])
        {
            case 0:
            {
                if(info.alphabet[n][1] == '5')
                    stages[n].Add(ComponentInfo.UP);
                if(module.bomb.GetSerialNumberLetters().Contains(info.alphabet[n][0]))
                    stages[n].Add(ComponentInfo.RIGHT);
                if(module.bomb.IsPortPresent(Port.DVI))
                    stages[n].Add(ComponentInfo.UP);
                if(info.LED.Distinct().Count() == 3)
                    stages[n].Add(ComponentInfo.DOWN);
                if(stages[n].Count == 0)
                    stages[n].Add(ComponentInfo.LEFT);

                break;
            }
            case 1:
            {
                if("STAGE".Contains(info.alphabet[n][0]))
                    stages[n].Add(ComponentInfo.RIGHT);
                if(info.arrows[ComponentInfo.RIGHT] != ComponentInfo.GREEN)
                    stages[n].Add(ComponentInfo.UP);
                if(module.bomb.IsIndicatorPresent(Indicator.BOB))
                    stages[n].Add(ComponentInfo.DOWN);
                if(info.alphabet[n][1] - '0' < 6)
                    stages[n].Add(ComponentInfo.LEFT);
                if(stages[n].Count == 0)
                    stages[n].Add(ComponentInfo.RIGHT);

                break;
            }
            case 2:
            {
                if(module.bomb.GetPortPlates().Any((x) => x.Length == 0))
                    stages[n].Add(ComponentInfo.DOWN);
                if(info.arrows[ComponentInfo.LEFT] == ComponentInfo.RED || info.arrows[ComponentInfo.LEFT] == ComponentInfo.BLUE)
                    stages[n].Add(ComponentInfo.UP);
                if("019".Contains(info.alphabet[n][1]))
                    stages[n].Add(ComponentInfo.LEFT);
                if(info.alphabet[n][0] > 'H' && info.alphabet[n][0] < 'P')
                    stages[n].Add(ComponentInfo.RIGHT);
                if(stages[n].Count == 0)
                    stages[n].Add(ComponentInfo.UP);

                break;
            }
            case 3:
            {
                if(module.bomb.GetBatteryCount() >= 4)
                    stages[n].Add(ComponentInfo.LEFT);
                if("2357".Contains(info.alphabet[n][1]))
                    stages[n].Add(ComponentInfo.DOWN);
                if(info.arrows[ComponentInfo.UP] == ComponentInfo.BLUE || info.arrows[ComponentInfo.DOWN] == ComponentInfo.BLUE)
                    stages[n].Add(ComponentInfo.DOWN);
                if("AEIOU".Contains(info.alphabet[n][0]))
                    stages[n].Add(ComponentInfo.RIGHT);
                if(stages[n].Count == 0)
                    stages[n].Add(ComponentInfo.UP);

                break;
            }
            case 4:
            {
                if(info.LED.Distinct().Count() < 3)
                    stages[n].Add(ComponentInfo.LEFT);
                if(module.bomb.GetSerialNumberNumbers().Count() >= 4)
                    stages[n].Add(ComponentInfo.LEFT);
                if(info.alphabet[n][1] - '0' % 2 == 0)
                    stages[n].Add(ComponentInfo.DOWN);
                if(info.alphabet[n][0] < 'L')
                    stages[n].Add(ComponentInfo.RIGHT);
                if(stages[n].Count == 0)
                    stages[n].Add(ComponentInfo.UP);

                break;
            }
            case 5:
            {
                if(module.bomb.GetIndicators().Count() == 0)
                    stages[n].Add(ComponentInfo.LEFT);
                if("LETTER".Contains(info.alphabet[n][0]))
                    stages[n].Add(ComponentInfo.RIGHT);
                if(module.bomb.GetSerialNumberNumbers().Contains(info.alphabet[n][1] - '0'))
                    stages[n].Add(ComponentInfo.DOWN);
                if(info.AreArrowsAdjacent(ComponentInfo.RED, ComponentInfo.YELLOW))
                    stages[n].Add(ComponentInfo.RIGHT);
                if(stages[n].Count == 0)
                    stages[n].Add(ComponentInfo.UP);

                break;
            }
        }
    }
}