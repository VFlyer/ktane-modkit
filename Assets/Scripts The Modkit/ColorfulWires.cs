using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class ColorfulWires : Puzzle
{
    int[] indexes = { 0, 0, 1, 1, 2, 3, 4, 5, 6, 6 };
    int[] colors = { ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.GREEN };
    
    int index;
    int direction;
    List<int> wiresToCut;
    int validCuts;

    public ColorfulWires(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        index = indexes[module.bomb.GetSerialNumberNumbers().ToArray()[0]];
        direction = module.bomb.GetSerialNumberNumbers().Count() % 2 == 0 ? 1 : -1;

        Debug.LogFormat("[The Modkit #{0}] Solving Colorful Wires. Starting color is {1} and direction is {2}.", moduleId, ComponentInfo.COLORNAMES[colors[index]], direction == 1 ? "clockwise" : "counter-clockwise");
    
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

        if(wiresToCut.Contains(wire))
        {
		    Debug.LogFormat("[The Modkit #{0}] Cut wire {1}.", moduleId, wire + 1);
            validCuts++;

            if(validCuts == wiresToCut.Count())
            {
		        Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Cut wire {1}.", moduleId, wire + 1);
            module.CauseStrike();
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

        if(wiresToCut.Count == 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Pressed the ❖ button when no wires were valid. Module solved.", moduleId);
            module.Solve();
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Pressed the ❖ button when at least one wire was valid.", moduleId);
            module.CauseStrike();
        }
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());

        validCuts = 0;
        wiresToCut = new List<int>();

        for(int i = 0; i < info.wires.Length; i++)
        {
            int color1 = info.wires[i] / 10;
            int color2 = info.wires[i] % 10;

            int[] checkColorIndexes = { (index + (i * 3 * direction)), (index + ((i * 3 + 1) * direction)), (index + ((i * 3 + 2) * direction)) };
            for(int j = 0; j < checkColorIndexes.Length; j++)
                while( checkColorIndexes[j] < 0 ) 
                    checkColorIndexes[j] += colors.Length;

            if(color1 == colors[checkColorIndexes[0] % colors.Length] ||
               color1 == colors[checkColorIndexes[1] % colors.Length] ||
               color1 == colors[checkColorIndexes[2] % colors.Length] ||
               color2 == colors[checkColorIndexes[0] % colors.Length] ||
               color2 == colors[checkColorIndexes[1] % colors.Length] ||
               color2 == colors[checkColorIndexes[2] % colors.Length])
                wiresToCut.Add(i);
        }

        Debug.LogFormat("[The Modkit #{0}] Wires that need to be cut are {1}.", moduleId, wiresToCut.Select(x => x + 1).Join(", "));
    }
}