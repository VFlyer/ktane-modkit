using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class SemaphoreKeys : Puzzle
{
    List<int> presses = new List<int>();
    
    public SemaphoreKeys(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Semaphore Keys. Alphanumeric keys present are: {1}. LEDs are: {2}.", moduleId, info.alphabet.Join(", "),  info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
    
        CalcSolution();

        Debug.LogFormat("[The Modkit #{0}] Keys that can be pressed: [ {1} ].", moduleId, presses.Select(x => x + 1).Join(", "));
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
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        if(presses.Contains(alphabet))
        {
		    Debug.LogFormat("[The Modkit #{0}] Correctly pressed alphanumeric key {1}. Module solved.", moduleId, alphabet + 1);
            module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[0];
            module.Solve();
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed alphanumeric key {1}.", moduleId, alphabet + 1);
            module.CauseStrike();
        }
    }

    void CalcSolution()
    {
        for(int i = 0; i < info.alphabet.Length; i++)
            if(module.bomb.GetSerialNumber().Contains(info.alphabet[i][0]) && module.bomb.GetSerialNumber().Contains(info.alphabet[i][1]))
                presses.Add(i);

        if(presses.Count != 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Rule 1 applies.", moduleId);
            return;
        }

        for(int i = 0; i < info.alphabet.Length; i++)
            if(new[] {2, 3, 5, 7}.Contains(info.alphabet[i][1] - '0') && info.LED.Contains(ComponentInfo.GREEN))
                presses.Add(i);

        if(presses.Count != 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Rule 2 applies.", moduleId);
            return;
        }

        if(info.LED.Distinct().Count() == 1)
        {
            int val = 1000;
            int key = -1;
            for(int i = 0; i < info.alphabet.Length; i++)
                if(info.alphabet[i][0] < val)
                {
                    val = info.alphabet[i][0];
                    key = 0;
                }

            presses.Add(key);
        }

        if(presses.Count != 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Rule 3 applies.", moduleId);
            return;
        }

        int dr = module.bomb.GetSerialNumberNumbers().Sum();
        while(dr > 9)
        {
            int sum = 0;
            while(dr > 9)
            {
                sum += dr / 10;
                dr = dr % 10;
            }
            dr += sum;
        }

        for(int i = 0; i < info.alphabet.Length; i++)
            if(info.alphabet[i][1] - '0' == dr)
                presses.Add(i);

        if(presses.Count != 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Rule 4 applies.", moduleId);
            return;
        }

        for(int i = 0; i < info.alphabet.Length; i++)
            if(new[] {'A', 'E', 'I', 'O', 'U'}.Contains(info.alphabet[i][0]) && info.LED.Contains(ComponentInfo.YELLOW))
                presses.Add(i);

        if(presses.Count != 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Rule 5 applies.", moduleId);
            return;
        }

        for(int i = 0; i < info.alphabet.Length; i++)
            if(info.alphabet[i][1] - '0' == info.LED.ToList().Where(x => x == ComponentInfo.ORANGE).Count() )
                presses.Add(i);

        if(presses.Count != 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Rule 6 applies.", moduleId);
            return;
        }

        if(Math.Abs(info.alphabet[0][0] - info.alphabet[1][0]) == 1)
        {
            if(Math.Abs(info.alphabet[0][0] - info.alphabet[2][0]) == 1 || Math.Abs(info.alphabet[1][0] - info.alphabet[2][0]) == 1)
            {
                presses.Add(Array.IndexOf(info.alphabet.Select(x => x[0]).ToArray(), info.alphabet.Select(x => x[0]).Min()));
            }
            else
            {
                if(info.alphabet[0][0] < info.alphabet[1][0])
                    presses.Add(0);
                else
                    presses.Add(1);
            }
        }
        else if(Math.Abs(info.alphabet[1][0] - info.alphabet[2][0]) == 1)
        {
            if(info.alphabet[1][0] < info.alphabet[2][0])
                    presses.Add(1);
                else
                    presses.Add(2);
        }
        else if(Math.Abs(info.alphabet[0][0] - info.alphabet[2][0]) == 1)
        {
            if(info.alphabet[0][0] < info.alphabet[2][0])
                    presses.Add(0);
                else
                    presses.Add(2);
        }

        if(presses.Count != 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Rule 7 applies.", moduleId);
            return;
        }

        for(int i = 0; i < info.alphabet.Length; i++)
            if(new[] {'P', 'R', 'E', 'S'}.Contains(info.alphabet[i][0]) && info.LED.Distinct().Count() < 3)
                presses.Add(i);

        if(presses.Count != 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Rule 8 applies.", moduleId);
            return;
        }

        presses.Add(1);
        Debug.LogFormat("[The Modkit #{0}] Rule 9 applies.", moduleId);
    }
}