using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class EdgeworkKeys : Puzzle
{
    int[] letterVals = new int[3];

    List<int> presses = new List<int>();
    List<int> pressed = new List<int>();

    public EdgeworkKeys(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Edgework Keys. Alphanumeric keys present are: {1}.", moduleId, info.alphabet.Join(", "));

        GetLetterVals();
        CalcPresses();
    }

    void GetLetterVals()
    {
        for(int i = 0; i < letterVals.Length; i++)
        {
            switch(info.alphabet[i][0])
            {
                case 'A': letterVals[i] = module.bomb.GetSerialNumberNumbers().ToArray()[1]; break;
                case 'B': letterVals[i] = module.bomb.GetPortPlates().Where((x) => x.Length == 0).Count(); break;
                case 'C': letterVals[i] = module.bomb.GetPortCount(Port.PS2); break;
                case 'D': letterVals[i] = info.alphabet[1][1] - '0'; break;
                case 'E': letterVals[i] = module.bomb.GetOnIndicators().Count(); break;
                case 'F': letterVals[i] = info.alphabet[0][1] - '0'; break;
                case 'G': letterVals[i] = module.bomb.GetPortPlates().Count(); break;
                case 'H': letterVals[i] = module.bomb.GetBatteryCount(Battery.AA); break;
                case 'I': letterVals[i] = module.bomb.GetSerialNumberNumbers().ToArray()[module.bomb.GetSerialNumberNumbers().ToArray().Count() - 2]; break;
                case 'J': letterVals[i] = module.bomb.GetBatteryCount(); break;
                case 'K': letterVals[i] = module.bomb.GetPortCount(); break;
                case 'L': letterVals[i] = module.bomb.GetIndicators().Count(); break;
                case 'M': letterVals[i] = module.bomb.GetPortCount(Port.DVI); break;
                case 'N': letterVals[i] = module.bomb.GetBatteryCount(Battery.D); break;
                case 'O': letterVals[i] = module.bomb.GetSerialNumberNumbers().ToArray()[0]; break;
                case 'P': letterVals[i] = module.bomb.GetSerialNumberNumbers().ToArray()[module.bomb.GetSerialNumberNumbers().ToArray().Count() - 1]; break;
                case 'Q': letterVals[i] = module.bomb.GetPortCount(Port.Parallel); break;
                case 'R': letterVals[i] = module.bomb.GetPortCount(Port.StereoRCA); break;
                case 'S': letterVals[i] = module.bomb.GetBatteryHolderCount(); break;
                case 'T': letterVals[i] = module.bomb.GetPortCount(Port.RJ45); break;
                case 'U': letterVals[i] = module.bomb.GetOffIndicators().Count(); break;
                case 'V': letterVals[i] = module.bomb.GetPorts().Distinct().Count(); break;
                case 'W': letterVals[i] = info.alphabet[2][1] - '0'; break;
                case 'X': letterVals[i] = module.bomb.GetSerialNumberNumbers().Count(); break;
                case 'Y': letterVals[i] = module.bomb.GetPortCount(Port.Serial); break;
                case 'Z': letterVals[i] = module.bomb.GetSerialNumberLetters().Count(); break;
            }

            Debug.LogFormat("[The Modkit #{0}] Value for {1}: {2}.", moduleId, info.alphabet[i][0], letterVals[i]);
        }
    }

    void CalcPresses()
    {
        for(int i = 0; i < letterVals.Length; i++)
        {
            if((info.alphabet[i][1] - '0') >= letterVals[i])
                presses.Add(i);
        }

        Debug.LogFormat("[The Modkit #{0}] Keys that need to be pressed: {1}.", moduleId, presses.Select(x => x + 1).Join(", "));
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

        if(pressed.Contains(alphabet))
            return;

        if(presses.Contains(alphabet))
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed alphanumeric key {1}.", moduleId, alphabet + 1);
            pressed.Add(alphabet);
            module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[0];
            if(pressed.Count() == presses.Count())
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed alphanumeric key {1}.", moduleId, alphabet + 1);
            module.GetComponent<KMBombModule>().HandleStrike();
        }
    }

    public override void OnUtilityPress()
    {
        if(module.IsAnimating())
            return;

        module.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.utilityBtn.GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed the ❖ button when component selection was [ {1} ] instead of [ {2} ].", moduleId, module.GetOnComponents(), module.GetTargetComponents());
            module.GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        module.StartSolve();

        if(presses.Count == 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Pressed the ❖ button when no keys were valid. Module solved.", moduleId);
            module.Solve();
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Pressed the ❖ button when at least one key was valid.", moduleId);
            module.GetComponent<KMBombModule>().HandleStrike();
        }
    }
}