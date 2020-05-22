using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class KeyScore : Puzzle
{
    int[] symbolScore = new int[3];
    int[] alphabetScore = new int[3];
    string[] combinedList = { "symbol1", "symbol2", "symbol3", "alpha1", "alpha2", "alpha3" };

    int lastScore = -1;
    int cnt = 0; // Note, exploitable.


    public KeyScore(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Key Score. Symbols present are: {1}. Alphanumeric keys present are: {2}. LEDs are: {3}.", moduleId, info.GetSymbols(), info.alphabet.Join(", "), info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));

        for(int i = 0; i < info.alphabet.Length; i++)
            alphabetScore[i] = (info.alphabet[i][0] - 'A' + 1) * (info.alphabet[i][1] - '0');

        for(int i = 0; i < info.symbols.Length; i++)
            symbolScore[i] = (info.symbols[i] + 1) * (info.LED[i] + 1);

        Debug.LogFormat("[The Modkit #{0}] Symbol keys' scores are [ {1} ].", moduleId, symbolScore.Join(", "));
        Debug.LogFormat("[The Modkit #{0}] Alphanumeric keys' scores are [ {1} ].", moduleId, alphabetScore.Join(", "));



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

        module.StartSolve();

        if(symbolScore[symbol] < lastScore)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed symbol {1}. It had less score that the previous press.", moduleId, symbol + 1);
            module.CauseStrike();
            lastScore = -1;
            cnt = 0;
            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            foreach(GameObject a in module.alphabet)
               a.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed symbol {1}.", moduleId, symbol + 1);
            lastScore = symbolScore[symbol];
            module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
            cnt++;
            if(cnt == 6)
            {
                foreach (GameObject s in module.symbols)
                    s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
                foreach (GameObject a in module.alphabet)
                    a.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
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

        if(alphabetScore[alphabet] < lastScore)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed alphanumeric key {1}. It had less score that the previous press.", moduleId, alphabet + 1);
            module.CauseStrike();
            lastScore = -1;
            cnt = 0;
            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            foreach(GameObject a in module.alphabet)
               a.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed alphanumeric key {1}.", moduleId, alphabet + 1);
            lastScore = alphabetScore[alphabet];
            module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
            cnt++;
            if(cnt == 6)
            {
                foreach (GameObject s in module.symbols)
                    s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
                foreach (GameObject a in module.alphabet)
                    a.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }

    }
}