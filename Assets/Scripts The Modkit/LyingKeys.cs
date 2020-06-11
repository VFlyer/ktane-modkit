using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class LyingKeys : Puzzle
{
    bool[][] patterns = new bool[][]
    {
        new bool [] { false, false, true, true, false, false },
        new bool [] { true, false, false, true, true, false },
        new bool [] { false, true, false, true, true, true },
        new bool [] { true, true, false, false, false, false },
        new bool [] { false, false, false, false, false, true },
        new bool [] { false, true, true, false, true, false },
        new bool [] { true, true, true, true, false, true },
        new bool [] { true, false, true, false, true, true },
    };

    int[] order = Enumerable.Range(0, 6).OrderBy(x => rnd.Range(0, 1000)).ToArray();
    int[] selectedSeq = Enumerable.Range(0, 8).OrderBy(x => rnd.Range(0, 1000)).ToArray();
    int currentSeq = -1;

    public LyingKeys(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Lying Keys. Symbols present: {1}. Alphanumeric keys present: {2}.", moduleId, info.GetSymbols(), info.alphabet.Join(", "));

        for(int i = 0; i < 5; i++)
        {
            patterns[selectedSeq[i]][order[i]] = !patterns[selectedSeq[i]][order[i]];
            Debug.LogFormat("[The Modkit #{0}] Sequence {1}: [ {2} ].", moduleId, i + 1, patterns[selectedSeq[i]].Select(x => x ? "ON" : "OFF").Join(", "));
        }

        Debug.LogFormat("[The Modkit #{0}] Correct key: {1}.", moduleId, order[5] > 2 ? "alphanumeric key " + (order[5] - 2) : "symbol key " + (order[5] + 1));
    }

    public override void OnSymbolPress(int symbol)
    {
        if(module.IsAnimating())
            return;

        module.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.symbols[symbol].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed symbol {1} when component selection was [ {2} ] instead of [ {3} ].", moduleId, symbol + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }

        if(symbol == order[5])
        {
		    Debug.LogFormat("[The Modkit #{0}] Correctly pressed symbol key {1}. Module solved.", moduleId, symbol + 1);
            module.Solve();
            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            foreach(GameObject a in module.alphabet)
               a.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed symbol key {1}.", moduleId, symbol + 1);
            module.CauseStrike();
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
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        if(alphabet + 3 == order[5])
        {
		    Debug.LogFormat("[The Modkit #{0}] Correctly pressed alphanumeric key {1}. Module solved.", moduleId, alphabet + 1);
            module.Solve();
            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            foreach(GameObject a in module.alphabet)
               a.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed alphanumeric key {1}.", moduleId, alphabet + 1);
            module.CauseStrike();
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
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        if(currentSeq == -1)
            return;

        if(arrow == ComponentInfo.UP)
        {
            currentSeq++;
            if(currentSeq == 5)
                currentSeq = 0;
        }
        else if(arrow == ComponentInfo.DOWN)
        {
            currentSeq--;
            if(currentSeq == -1)
                currentSeq = 4;
        }
        else
            return;

        for(int i = 0; i < patterns[selectedSeq[currentSeq]].Length; i++)
        {
            if(i < 3)
                if(patterns[selectedSeq[currentSeq]][i])
                    module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
                else
                    module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            else
                if(patterns[selectedSeq[currentSeq]][i])
                    module.alphabet[i - 3].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
                else
                    module.alphabet[i - 3].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
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
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        if(currentSeq == -1)
        {
            currentSeq = 0;
            for(int i = 0; i < patterns[selectedSeq[currentSeq]].Length; i++)
            {
                if(i < 3)
                    if(patterns[selectedSeq[currentSeq]][i])
                        module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
                    else
                        module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
                else
                    if(patterns[selectedSeq[currentSeq]][i])
                        module.alphabet[i - 3].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
                    else
                        module.alphabet[i - 3].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            }
        }
    }
}