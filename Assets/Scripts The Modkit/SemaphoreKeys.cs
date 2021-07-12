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
        Debug.LogFormat("[The Modkit #{0}] Solving Semaphore Keys. Alphanumeric keys present: {1}. LEDs: {2}.", moduleId, info.alphabet.Join(", "),  info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
    
        CalcSolution();

        Debug.LogFormat("[The Modkit #{0}] Keys that can be pressed: [ {1} ].", moduleId, presses.Select(x => x + 1).Join(", "));
    }

    public override void OnAlphabetPress(int alphabet)
    {
        if(module.IsAnimating())
            return;

        module.audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.alphabet[alphabet].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Alphanumeric key {1} was pressed when the component selection was [ {2} ] instead of [ {3} ].", moduleId, alphabet + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        if(presses.Contains(alphabet))
        {
		    Debug.LogFormat("[The Modkit #{0}] Correctly pressed alphanumeric key {1}. Module solved.", moduleId, alphabet + 1);
            module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
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
        int[] initialIdxList = { 0, 1, 2 };
        List<char> alphabetOrderings = info.alphabet.Select(a => a[0]).OrderBy(a => a).ToList();
        List<int> idxList = initialIdxList.OrderBy(x => info.alphabet.Select(a => a[0]).ToArray()[x]).ToList();

        Debug.LogFormat("[The Modkit #{0}] Sorted Alphabet Keys: {1}", moduleId, alphabetOrderings.Join());
        Debug.LogFormat("[The Modkit #{0}] Key Indexes sorted by alphabet: {1}", moduleId, idxList.Select(a => a + 1).Join());

        for (int i = 0; i < info.alphabet.Length; i++)
            if(module.bomb.GetSerialNumber().Contains(info.alphabet[i][0]) && module.bomb.GetSerialNumber().Contains(info.alphabet[i][1]))
                presses.Add(i);

        if(presses.Any())
        {
            Debug.LogFormat("[The Modkit #{0}] At least 1 of the keys has a letter and digit in the serial number.", moduleId);
            return;
        }

        for(int i = 0; i < info.alphabet.Length; i++)
            if(new[] {2, 3, 5, 7}.Contains(info.alphabet[i][1] - '0') && info.LED.Contains(ComponentInfo.GREEN))
                presses.Add(i);

        if(presses.Any())
        {
            Debug.LogFormat("[The Modkit #{0}] A green LED is on and one of the keys' number is prime.", moduleId);
            return;
        }

        if(info.LED.Distinct().Count() == 1)
        {
            presses.Add(idxList[0]);
            Debug.LogFormat("[The Modkit #{0}] All 3 LEDs have the same color.", moduleId);
            return;
        }

        int dr = module.bomb.GetSerialNumberNumbers().Sum();
        while(dr > 9)
        {
            int sum = 0;
            while(dr > 9)
            {
                sum += dr / 10;
                dr %= 10;
            }
            dr += sum;
        }

        for(int i = 0; i < info.alphabet.Length; i++)
            if(info.alphabet[i][1] - '0' == dr)
                presses.Add(i);

        if(presses.Any())
        {
            Debug.LogFormat("[The Modkit #{0}] One of the keys is the digital root of the sum of the serial number digits.", moduleId);
            return;
        }

        if (info.LED.Contains(ComponentInfo.YELLOW))
            for(int i = 0; i < info.alphabet.Length; i++)
                if(new[] {'A', 'E', 'I', 'O', 'U'}.Contains(info.alphabet[i][0]))
                    presses.Add(i);

        if(presses.Any())
        {
            Debug.LogFormat("[The Modkit #{0}] There is a yellow LED on and at least one of the keys' letter is a vowel.", moduleId);
            return;
        }

        for(int i = 0; i < info.alphabet.Length; i++)
            if(info.alphabet[i][1] - '0' == info.LED.ToList().Where(x => x == ComponentInfo.ORANGE).Count() )
                presses.Add(i);

        if(presses.Any())
        {
            Debug.LogFormat("[The Modkit #{0}] One of the keys' digit is equal to the number of orange LEDs.", moduleId);
            return;
        }

        

        /*      // Old Handling, this DOES NOT ACCOUNT FOR senarios such as (Z,X,Y)
                if (Math.Abs(info.alphabet[0][0] - info.alphabet[1][0]) == 1)
                {
                    if(Math.Abs(info.alphabet[0][0] - info.alphabet[2][0]) == 1 || Math.Abs(info.alphabet[1][0] - info.alphabet[2][0]) == 1)
                    {
                        presses.Add(Array.IndexOf(info.alphabet.Select(x => x[0]).ToArray(), info.alphabet.Select(x => x[0]).Max()));
                    }
                    else
                    {
                        if(info.alphabet[0][0] < info.alphabet[1][0])
                            presses.Add(1);
                        else
                            presses.Add(0);
                    }
                }
                else if(Math.Abs(info.alphabet[1][0] - info.alphabet[2][0]) == 1)
                {
                    if(info.alphabet[1][0] < info.alphabet[2][0])
                            presses.Add(2);
                        else
                            presses.Add(1);
                }
                else if(Math.Abs(info.alphabet[0][0] - info.alphabet[2][0]) == 1)
                {
                    if(info.alphabet[0][0] < info.alphabet[2][0])
                            presses.Add(2);
                        else
                            presses.Add(0);
                }
        */
        int lastIDx = -1;
        for (int x = 1; x < 3; x++)
        {
            if (Math.Abs(alphabetOrderings[x] - alphabetOrderings[x - 1]) == 1)
            {
                lastIDx = alphabetOrderings[x] < alphabetOrderings[x - 1] ? x - 1 : x;
            }
        }
        if (lastIDx >= 0 && idxList.Count() > lastIDx)
        {
            presses.Add(idxList[lastIDx]);
        }
        if(presses.Any())
        {
            Debug.LogFormat("[The Modkit #{0}] Two or more keys have consecutive letters.", moduleId);
            return;
        }
        if (info.LED.Distinct().Count() < 3)
            for(int i = 0; i < info.alphabet.Length; i++)
                if(new[] {'P', 'R', 'E', 'S'}.Contains(info.alphabet[i][0]))
                    presses.Add(i);

        if(presses.Count != 0)
        {
            Debug.LogFormat("[The Modkit #{0}] 2 or more LEDs have the same color and one of the keys' letter is in the word \"PRESS\".", moduleId);
            return;
        }

        presses.Add(1);
        Debug.LogFormat("[The Modkit #{0}] The last otherwise condition applies.", moduleId);
    }
}