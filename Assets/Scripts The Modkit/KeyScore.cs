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
    string[] combinedList = { "symbol 1", "symbol 2", "symbol 3", "alphabet 1", "alphabet 2", "alphabet 3" };

    int lastScore = -1;
    List<string> allPresses = new List<string>();

    public KeyScore(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Key Score. Symbols present: {1}. Alphanumeric keys present: {2}. LEDs: {3}.", moduleId, info.GetSymbols(), info.alphabet.Join(", "), info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));

        for(int i = 0; i < info.alphabet.Length; i++)
            alphabetScore[i] = (info.alphabet[i][0] - 'A' + 1) * (info.alphabet[i][1] - '0');

        for(int i = 0; i < info.symbols.Length; i++)
            symbolScore[i] = (info.symbols[i] + 1) * (info.LED[i] + 1);

        Debug.LogFormat("[The Modkit #{0}] Symbol keys' scores: [ {1} ].", moduleId, symbolScore.Join(", "));
        Debug.LogFormat("[The Modkit #{0}] Alphanumeric keys' scores: [ {1} ].", moduleId, alphabetScore.Join(", "));
        List<int> combinedScoreList = new List<int>();
        foreach (int one_score in symbolScore)
        {
            combinedScoreList.Add(one_score);
        }
        foreach (int one_score in alphabetScore)
        {
            combinedScoreList.Add(one_score);
        }
        Debug.LogFormat("[The Modkit #{0}] One possible set of key presses to solve this module: [ {1} ].", moduleId, combinedList.OrderBy(a => combinedScoreList[Array.IndexOf(combinedList,a)]).Join(", "));


    }

    public override void OnSymbolPress(int symbol)
    {
        if(module.IsAnimating())
            return;

        module.audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.symbols[symbol].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Symbol {1} was pressed when the component selection was [ {2} ] instead of [ {3} ].", moduleId, symbol + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        if(symbolScore[symbol] < lastScore)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Upon pressing symbol {1}, its score was {2} which was less than the previous score ({3}) from another key.", moduleId, symbol + 1, symbolScore[symbol], lastScore);
            module.CauseStrike();
            lastScore = -1;
            allPresses.Clear();
            foreach (GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            foreach(GameObject a in module.alphabet)
               a.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
        }
        else if (!allPresses.Distinct().Contains("symbol" + (symbol + 1).ToString()))
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed symbol {1}.", moduleId, symbol + 1);
            lastScore = symbolScore[symbol];
            module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
            allPresses.Add("symbol" + (symbol + 1).ToString());
            if(allPresses.Distinct().Count() == 6)
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

        if(alphabetScore[alphabet] < lastScore)
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Upon pressing alphanumeric key {1}, its score was {2} which was less than the previous score ({3}) from another key.", moduleId, alphabet + 1, symbolScore[alphabet], lastScore);
            module.CauseStrike();
            lastScore = -1;
            allPresses.Clear();
            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            foreach(GameObject a in module.alphabet)
               a.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
        }
        else if (!allPresses.Distinct().Contains("alphabet" + (alphabet + 1).ToString()))
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed alphanumeric key {1}.", moduleId, alphabet + 1);
            lastScore = alphabetScore[alphabet];
            module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
            allPresses.Add("alphabet" + (alphabet + 1).ToString());
            if (allPresses.Distinct().Count() == 6)
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