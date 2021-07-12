using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class EncryptedKeypad : Puzzle
{
    int[][] columns = new int[][] {
        new int[] { 21, 22, 11, 26, 3, 16, 12, 29, 28, 24 },
        new int[] { 9, 19, 15, 8, 1, 23, 6, 20, 5, 13 },
        new int[] { 6, 25, 18, 7, 20, 30, 8, 1, 14, 21 },
        new int[] { 10, 29, 27, 30, 17, 25, 7, 19, 15, 18 },
        new int[] { 4, 14, 12, 13, 24, 22, 0, 2, 11, 16 },
        new int[] { 2, 28, 0, 5, 23, 17, 26, 10, 3, 4 }
    };

    List<int> convertedKeys = new List<int>();
    int[] symbolMatches = { 0, 0, 0, 0, 0, 0 };
    int correctColumn;
    List<int> presses = new List<int>();
    List<int> pressed = new List<int>();

    public EncryptedKeypad(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Encrypted Keypad. Symbols present: {1}. Alphanumeric keys present: {2}.", moduleId, info.GetSymbols(), info.alphabet.Join(", "));

        for(int i = 0; i < info.alphabet.Length; i++)
        {
            int letterVal = info.alphabet[i][0];

            if(letterVal <= 'E')
                letterVal = 0;
            else if (letterVal <= 'J')
                letterVal = 1;
            else if (letterVal <= 'N')
                letterVal = 2;  
            else if (letterVal <= 'R')
                letterVal = 3;
            else if (letterVal <= 'V')
                letterVal = 4;
            else
                letterVal = 5;

            convertedKeys.Add(columns[letterVal][info.alphabet[i][1] - '0']);
        }
        Debug.LogFormat("[The Modkit #{0}] Alphanumeric keys corresponding symbols: {1}.", moduleId, convertedKeys.Select(x => ComponentInfo.SYMBOLCHARS[x]).Join(", "));
    
        for(int i = 0; i < columns.Length; i++)
            for(int j = 0; j < info.symbols.Length; j++)
            {
                if(columns[i].Contains(info.symbols[j]))
                    symbolMatches[i]++;
                if(columns[i].Contains(convertedKeys[j]))
                    symbolMatches[i]++;
            }

        correctColumn = Array.IndexOf(symbolMatches, symbolMatches.Max());
        Debug.LogFormat("[The Modkit #{0}] Using column {1} ({2} matched symbols).", moduleId, correctColumn + 1, symbolMatches.Max());
    
        for(int j = 0; j < info.symbols.Length; j++)
        {
            if(columns[correctColumn].Contains(info.symbols[j]))
                presses.Add(j);
            if(columns[correctColumn].Contains(convertedKeys[j]))
                presses.Add(j + 3);
        }

        presses.Sort();

        Debug.LogFormat("[The Modkit #{0}] The following that need to be pressed: symbol keys [ {1} ] and alphanumeric keys [ {2} ].", moduleId, presses.Where(x => x < 3).Select(x => x + 1).Join(", "), presses.Where(x => x >= 3).Select(x => x - 2).Join(", "));
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

        if(pressed.Contains(symbol))
            return;

        if(presses.Contains(symbol))
        {
		    Debug.LogFormat("[The Modkit #{0}] Correctly pressed symbol {1}.", moduleId, symbol + 1);
            pressed.Add(symbol);
            module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
            if(pressed.Count == presses.Count)
            {
		        Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed symbol {1}.", moduleId, symbol + 1);
            module.CauseStrike();
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

        if(pressed.Contains(alphabet + 3))
            return;

        if(presses.Contains(alphabet + 3))
        {
		    Debug.LogFormat("[The Modkit #{0}] Correctly pressed alphanumeric key {1}.", moduleId, alphabet + 1);
            pressed.Add(alphabet + 3);
            module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
            if(pressed.Count() == presses.Count())
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed alphanumeric key {1}.", moduleId, alphabet + 1);
            module.CauseStrike();
        }
    }
}