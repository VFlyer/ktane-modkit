using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class ColorOffset : Puzzle
{
    int[][] colorMap = new int[][]
    {
        new int[] { ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.BLUE },
        new int[] { ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.RED },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.RED },
        new int[] { ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.BLUE },
        new int[] { ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.BLUE },
    };

    int[] standardSymbolOffset = { 4, 6, 1, 5, 6, 6, 6, 3, 1, 3, 3, 2, 2, 5, 4, 1, 4, 1, 4, 3, 6, 5, 5, 2, 1, 3, 4, 2, 2, 5, 1 };

    int sol;
    int[] arrowOffset = new int[4];
    int[] symbolOffset = new int[3];

    public ColorOffset(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Color Offset. Symbols present are: {1}. LEDs are: {2}.", moduleId, info.GetSymbols(), info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
    
        for(int i = 0; i < arrowOffset.Length; i++)
            arrowOffset[i] = rnd.Range(1, 7);
        
        Debug.LogFormat("[The Modkit #{0}] Arrow offsets are: Up - {1}, Right - {2}, Down - {3}, Left - {4}.", moduleId, arrowOffset[ComponentInfo.UP], arrowOffset[ComponentInfo.RIGHT], arrowOffset[ComponentInfo.DOWN], arrowOffset[ComponentInfo.LEFT]);
    
        sol = rnd.Range(0, 3);

        for(int i = 0; i < symbolOffset.Length; i++)
        {
            if(i != sol)
                symbolOffset[i] = standardSymbolOffset[info.symbols[i]];
            else
            {
                int val = rnd.Range(1, 7);
                while(val == standardSymbolOffset[info.symbols[i]])
                    val = rnd.Range(1, 7);
                symbolOffset[i] = val;
            }
        }

        Debug.LogFormat("[The Modkit #{0}] Symbol offsets are: [ {1} ].", moduleId, symbolOffset.Join(", "));
        Debug.LogFormat("[The Modkit #{0}] Solution is symbol {1}.", moduleId, sol + 1);
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
            module.GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        module.StartSolve();

        if(symbol == sol)
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed symbol {1}. Module solved.", moduleId, symbol + 1);
            module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
            module.Solve();
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed symbol {1}.", moduleId, symbol + 1);
            module.GetComponent<KMBombModule>().HandleStrike();
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

        for(int i = 0; i < info.LED.Length; i++)
        {
            info.LED[i] = colorMap[info.LED[i]][(arrowOffset[arrow] + symbolOffset[i]) % 6];
			module.LED[i].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[i]];
        }

        Debug.LogFormat("[The Modkit #{0}] Pressed arrow {1}. LEDs are: {2}.", moduleId, ComponentInfo.DIRNAMES[arrow], info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
    }
}