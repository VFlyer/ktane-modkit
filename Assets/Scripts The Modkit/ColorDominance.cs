using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class ColorDominance : Puzzle
{
    List<int> cut = new List<int>();
    List<int> symbolOff = new List<int>();
    List<int> LEDOff = new List<int>();

    int[] wireTable = { 0, 3, 1, 0, 4, 2 };
    int[] keyTable = { 0, 2, 1, 2, 0, 1 };
    int[] arrowTable = { ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT, ComponentInfo.LEFT, ComponentInfo.UP, ComponentInfo.DOWN };
    int[] digitTable = { 4, 2, 0, 5, 9, 8 };

    int dominantColor = -1;
    int stage = 0;
    List<int> keyColors = new List<int>();

    public ColorDominance(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Color Dominance. Symbols present: {1}. LEDs: {3}.", moduleId, info.GetSymbols(), info.alphabet.Join(", "), info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
        Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());
    }

    public override void OnWireCut(int wire)
    {
        if(module.IsAnimating())
            return;

        module.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, module.transform);
		module.CutWire(wire);

        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Cut wire {1} when component selection was [ {2} ] instead of [ {3} ].", moduleId, wire + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            module.RegenWires();
            return;
        }

        module.StartSolve();

        if(dominantColor == -1)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Wire {1} was cut before pressing the ❖ button.", moduleId, wire + 1);
            module.CauseStrike();
            module.RegenWires();
            Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());
            return;
        }

        if(stage != 0)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Wire {1} was cut on stage {2}.", moduleId, wire + 1, stage + 1);
            module.CauseStrike();
            Reset();
            return;
        }

        if(wire != wireTable[dominantColor])
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Wire {1} was cut on stage {2} when wire {3} was expected.", moduleId, wire + 1, stage + 1, wireTable[dominantColor] + 1);
            module.CauseStrike();
            Reset();
            return;
        }

        Debug.LogFormat("[The Modkit #{0}] Correctly cut wire {1} on stage {2}.", moduleId, wire + 1, stage + 1);
        cut.Add(wire);
        stage++;
        CalcSolution(stage);
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

        if(dominantColor == -1)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Symbol {1} was pressed before pressing the ❖ button.", moduleId, symbol + 1);
            module.CauseStrike();
            return;
        }

        if(stage != 1)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Symbol {1} was pressed on stage {2}.", moduleId, symbol + 1, stage + 1);
            module.CauseStrike();
            Reset();
            return;
        }

        if(symbol != keyTable[dominantColor])
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Symbol {1} was incorrectly pressed on stage {2} when symbol {3} was expected.", moduleId, symbol + 1, stage + 1, keyTable[dominantColor] + 1);
            module.CauseStrike();
            Reset();
            return;
        }

        Debug.LogFormat("[The Modkit #{0}] Correctly pressed symbol {1} on stage {2}.", moduleId, symbol + 1, stage + 1);
        symbolOff.Add(symbol);
        module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
        stage++;
        CalcSolution(stage);
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

        if(dominantColor == -1)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! The {1} arrow was pressed before pressing the ❖ button.", moduleId, ComponentInfo.DIRNAMES[arrow]);
            module.CauseStrike();
            return;
        }

        if(stage != 2)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! The {1} arrow was pressed on stage {2}.", moduleId, ComponentInfo.DIRNAMES[arrow], stage + 1);
            module.CauseStrike();
            Reset();
            return;
        }

        if(arrow != arrowTable[dominantColor])
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed {1} arrow on stage {2} when symbol {3} was expected.", moduleId, ComponentInfo.DIRNAMES[arrow], stage + 1, keyTable[dominantColor] + 1);
            module.CauseStrike();
            Reset();
            return;
        }

        Debug.LogFormat("[The Modkit #{0}] Pressed {1} arrow on stage {2}.", moduleId, ComponentInfo.DIRNAMES[arrow], stage + 1);
        LEDOff.Add(rnd.Range(0, 3));
        Debug.LogFormat("[The Modkit #{0}] LED {1} in now off.", moduleId, LEDOff[0] + 1);
        module.LED[LEDOff[0]].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[6];
        stage++;
        CalcSolution(stage);
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

        if(dominantColor == -1)
        {
            for(int i = 0; i < info.symbols.Length; i++)
            {
                keyColors.Add(rnd.Range(0, 6));
                module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[keyColors[i]];
            }

            Debug.LogFormat("[The Modkit #{0}] Symbol keys' lights: {1}.", moduleId, keyColors.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
            CalcSolution(stage);
            return;
        }

        if(stage != 3)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed the ❖ button on stage {1}.", moduleId, stage + 1);
            module.CauseStrike();
            Reset();
            return;
        }

        var sec = ((int) module.bomb.GetTime()) % 10;

        if(sec != digitTable[dominantColor])
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed the ❖ button on stage {1} when the last digit of the countdown timer was {2}.", moduleId, stage + 1, sec);
            module.CauseStrike();
            Reset();
            return;
        }

        Debug.LogFormat("[The Modkit #{0}] The ❖ button was correctly pressed on stage {1} when the last digit of the countdown timer was {2}. Module solved.", moduleId, stage + 1, sec);
        module.Solve();
        for(int i = 0; i < info.symbols.Length; i++)
            module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
        for(int i = 0; i < info.LED.Length; i++)
			module.LED[i].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[6];

    }

    void CalcSolution(int stage)
    {
        int maxCount = -1;

        for(int i = 0; i < 6; i++)
        {
            int colorCnt = 0;
            
            for(int j = 0; j < info.wires.Length; j++)
            {
                int color1 = info.wires[j] / 10;
                int color2 = info.wires[j] % 10;

                if(!cut.Contains(j) && (color1 == i || color2 == i))
                    colorCnt++;
            }

            for(int j = 0; j < keyColors.Count; j++)
            {
                if(!symbolOff.Contains(j) && keyColors[j] == i)
                    colorCnt++;
            }

            for(int j = 0; j < info.LED.Length; j++)
            {
                if(!LEDOff.Contains(j) && info.LED[j] == i)
                    colorCnt++;
            }

            if(colorCnt > maxCount)
            {
                dominantColor = i;
                maxCount = colorCnt;
            }
        }

        Debug.LogFormat("[The Modkit #{0}] Dominant color for stage {1} is {2}.", moduleId, stage + 1, ComponentInfo.COLORNAMES[dominantColor]);
    }

    void Reset()
    {
        cut = new List<int>();
        symbolOff = new List<int>();
        LEDOff = new List<int>();

        stage = 0;

        module.RegenWires();
        Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());

        for(int i = 0; i < info.symbols.Length; i++)
            module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[keyColors[i]];
        for(int i = 0; i < info.LED.Length; i++)
			module.LED[i].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[i]];

        CalcSolution(stage);
    }
}