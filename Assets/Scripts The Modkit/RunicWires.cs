using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class RunicWires : Puzzle
{
    int[][] runes = new int[][] {
            new int[] { ComponentInfo.RED, ComponentInfo.ORANGE },
        new int[] { ComponentInfo.BLUE, ComponentInfo.GREEN },
        new int[] { ComponentInfo.BLUE, ComponentInfo.GREEN },
        new int[] { ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.PURPLE },
            new int[] { ComponentInfo.BLUE },
        new int[] { ComponentInfo.GREEN },
        new int[] { ComponentInfo.RED },
        new int[] { ComponentInfo.YELLOW },
        new int[] { ComponentInfo.BLUE, ComponentInfo.GREEN },
            new int[] { ComponentInfo.ORANGE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.PURPLE },
        new int[] { ComponentInfo.BLUE, ComponentInfo.GREEN },
        new int[] { ComponentInfo.PURPLE },
        new int[] { ComponentInfo.PURPLE },
            new int[] { ComponentInfo.RED, ComponentInfo.ORANGE },
        new int[] { ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.RED },
        new int[] { ComponentInfo.PURPLE },
        new int[] { ComponentInfo.RED, ComponentInfo.ORANGE },
            new int[] { ComponentInfo.PURPLE },
        new int[] { ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.BLUE },
        new int[] { ComponentInfo.BLUE, ComponentInfo.GREEN },
            new int[] { ComponentInfo.YELLOW },
        new int[] { ComponentInfo.PURPLE },
        new int[] { ComponentInfo.BLUE, ComponentInfo.ORANGE },
        new int[] { ComponentInfo.PURPLE },
        new int[] { ComponentInfo.PURPLE },
            new int[] { ComponentInfo.PURPLE },
    };

    List<int>[] presses = new List<int>[5]; 
    List<int> pressed = new List<int>();
    List<int> cuts = new List<int>();
    int currentWire = 0;

    public RunicWires(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Runic Wires. Symbols present: {1}.", moduleId, info.GetSymbols());

        for(int i = 0; i < info.symbols.Length; i++)
            Debug.LogFormat("[The Modkit #{0}] Symbol {1} runes: [ {2} ].", moduleId, i + 1, runes[info.symbols[i]].Select(x => GetRuneName(x)).Join(", "));
       
        CalcSolution();
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
            CalcSolution();
            return;
        }

        module.StartSolve();
        cuts.Add(wire);
        
        if(wire != currentWire)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Cut wire {1} when wire {2} was expected.", moduleId, wire + 1, currentWire + 1);
            module.CauseStrike();
            pressed = new List<int>();
            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
        }
        else if(presses[currentWire].Exists(x => !pressed.Contains(x)) || pressed.Exists(x => !presses[currentWire].Contains(x)))
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly cutted wire {1} when pressed symbols were [ {2} ], but expected sybmols [ {3} ].", moduleId, wire + 1, pressed.Any() ? pressed.Select(x => x + 1).Join(", ") : "none", presses[wire].Any() ? presses[wire].Select(x => x + 1).Join(", ") : "none");
            module.CauseStrike();
            pressed = new List<int>();
            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];

            do{currentWire++;}
            while(cuts.Contains(currentWire));

            if(currentWire == 5)
                module.StartCoroutine(DelayedSolve());
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Corrected cutted wire {1} when pressed symbols were [ {2} ].", moduleId, wire + 1, pressed.Any() ? pressed.Select(x => x + 1).Join(", ") : "none");
            pressed = new List<int>();
            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];

            do{currentWire++;}
            while(cuts.Contains(currentWire));

            if(currentWire == 5)
            {
                Debug.LogFormat("[The Modkit #{0}] No more wires. Module solved.", moduleId);
                module.Solve();
            }
        }
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

        if(pressed.Contains(symbol))
            return;

        pressed.Add(symbol);
        module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());

        for(int i = 0; i < info.wires.Length; i++)
        {
            int color1 = info.wires[i] / 10;
            int color2 = info.wires[i] % 10;

            presses[i] = new List<int>();

            for(int j = 0; j < info.symbols.Length; j++)
            {
                if(runes[info.symbols[j]].Contains(color1) || runes[info.symbols[j]].Contains(color2))
                    presses[i].Add(j);
            }

            Debug.LogFormat("[The Modkit #{0}] Symbols that need to be pressed for wire {1}: [ {2} ].", moduleId, i + 1, presses[i].Count == 0 ? "none" : presses[i].Select(x => x + 1).Join(", "));
        }
    }

    String GetRuneName(int rune)
    {
        switch(rune)
        {
            case 0: return "Fire";
            case 1: return "Air";
            case 2: return "Water";
            case 3: return "Light";
            case 4: return "Earth";
            case 5: return "Darkness";
        }

        return "";
    }

    IEnumerator DelayedSolve()
    {
        yield return new WaitForSeconds(1f);
        Debug.LogFormat("[The Modkit #{0}] No more wires. Module solved.", moduleId);
        module.Solve();
    }

}
