using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class PreciseWires : Puzzle
{
    int[][] map = new int[][]
    {
        new int[] { ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.WHITE, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.GREEN },
        new int[] { ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.WHITE, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.RED }, 
        new int[] { ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.WHITE, ComponentInfo.PURPLE, ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.BLUE },
        new int[] { ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.WHITE, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.RED },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.WHITE, ComponentInfo.YELLOW, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.RED, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.YELLOW, ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.WHITE },
        new int[] { ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.YELLOW, ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.PURPLE},
        new int[] { ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.WHITE},
        new int[] { ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.WHITE, ComponentInfo.PURPLE, ComponentInfo.WHITE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.ORANGE },
        new int[] { ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.WHITE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.WHITE, ComponentInfo.GREEN }
    };

    int[] keyColors = new int[3];
    List<int>[] cutGroups = new List<int>[10];
    List<int> cut = new List<int>();
    

    public PreciseWires(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Precise Wires. Alphanumeric keys present are: {1}. LEDs are: {2}.", moduleId, info.alphabet.Join(", "), info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
    
        for(int i = 0; i < info.alphabet.Length; i++)
            keyColors[i] = map[info.alphabet[i][1] - '0'][info.alphabet[i][0]-'A'];

        Debug.LogFormat("[The Modkit #{0}] Alphanumeric keys colors are: {1}.", moduleId, keyColors.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
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

        List<int> group = null;
        int acc = 0;

        for(int i = 0; i < cutGroups.Length; i++)
        {
            acc += cutGroups[i].Count();
            if(acc > cut.Count())
            {
                group = cutGroups[i];
                break;
            }
        }

        if(group.Contains(wire))
        {
            Debug.LogFormat("[The Modkit #{0}] Correctly cutted wire {1}.", moduleId, wire + 1);
            if (!cut.Contains(wire))
                cut.Add(wire);
            if(cut.Count() == 5)
            {
		        Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Wire {1} was incorrectly cut.", moduleId, wire + 1);
            module.CauseStrike();
            module.RegenWires();
            CalcSolution();
        }
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());
    
        for(int i = 0; i < cutGroups.Length; i++)
            cutGroups[i] = new List<int>();

        List<int> used = new List<int>();
        List<int> colors = new List<int>();
        cut = new List<int>();

        colors.Add(keyColors[0] * 10 + keyColors[1]);
        colors.Add(keyColors[0] * 10 + keyColors[2]);
        colors.Add(keyColors[1] * 10 + keyColors[0]);
        colors.Add(keyColors[1] * 10 + keyColors[2]);
        colors.Add(keyColors[2] * 10 + keyColors[0]);
        colors.Add(keyColors[2] * 10 + keyColors[1]);

        for(int i = 0; i < info.wires.Length; i++)
            if(!used.Contains(i) && colors.Contains(info.wires[i]))
            {
                used.Add(i);
                cutGroups[0].Add(i);
                Debug.LogFormat("[The Modkit #{0}] Wire {1} has a combination matching the 1st instruction.", moduleId, i + 1);
            }

        colors.Clear();

        colors.Add(info.LED[0] * 10 + info.LED[1]);
        colors.Add(info.LED[0] * 10 + info.LED[2]);
        colors.Add(info.LED[1] * 10 + info.LED[0]);
        colors.Add(info.LED[1] * 10 + info.LED[2]);
        colors.Add(info.LED[2] * 10 + info.LED[0]);
        colors.Add(info.LED[2] * 10 + info.LED[1]);

        for(int i = 0; i < info.wires.Length; i++)
            if(!used.Contains(i) && colors.Contains(info.wires[i]))
            {
                used.Add(i);
                cutGroups[1].Add(i);
                Debug.LogFormat("[The Modkit #{0}] Wire {1} has a combination matching the 2nd instruction.", moduleId, i + 1);
            }

        colors.Clear();

        colors.Add(info.arrows[ComponentInfo.UP] * 10 + info.arrows[ComponentInfo.DOWN]);
        colors.Add(info.arrows[ComponentInfo.DOWN] * 10 + info.arrows[ComponentInfo.UP]);
        colors.Add(info.arrows[ComponentInfo.LEFT] * 10 + info.arrows[ComponentInfo.RIGHT]);
        colors.Add(info.arrows[ComponentInfo.RIGHT] * 10 + info.arrows[ComponentInfo.LEFT]);

        for(int i = 0; i < info.wires.Length; i++)
            if(!used.Contains(i) && colors.Contains(info.wires[i]))
            {
                used.Add(i);
                cutGroups[2].Add(i);
                Debug.LogFormat("[The Modkit #{0}] Wire {1} has a combination matching the 3rd instruction.", moduleId, i + 1);
            }

        colors.Clear();

        colors.Add(info.arrows[ComponentInfo.UP] * 10 + info.arrows[ComponentInfo.RIGHT]);
        colors.Add(info.arrows[ComponentInfo.RIGHT] * 10 + info.arrows[ComponentInfo.UP]);
        colors.Add(info.arrows[ComponentInfo.RIGHT] * 10 + info.arrows[ComponentInfo.DOWN]);
        colors.Add(info.arrows[ComponentInfo.DOWN] * 10 + info.arrows[ComponentInfo.RIGHT]);
        colors.Add(info.arrows[ComponentInfo.DOWN] * 10 + info.arrows[ComponentInfo.LEFT]);
        colors.Add(info.arrows[ComponentInfo.LEFT] * 10 + info.arrows[ComponentInfo.DOWN]);
        colors.Add(info.arrows[ComponentInfo.LEFT] * 10 + info.arrows[ComponentInfo.UP]);
        colors.Add(info.arrows[ComponentInfo.UP] * 10 + info.arrows[ComponentInfo.LEFT]);

        for(int i = 0; i < info.wires.Length; i++)
            if(!used.Contains(i) && colors.Contains(info.wires[i]))
            {
                used.Add(i);
                cutGroups[3].Add(i);
                Debug.LogFormat("[The Modkit #{0}] Wire {1} has a combination matching the 4th instruction.", moduleId, i + 1);
            }

        colors.Clear();

        colors.Add(info.LED[0] * 10 + keyColors[0]);
        colors.Add(keyColors[0] * 10 + info.LED[0]);
        colors.Add(info.LED[1] * 10 + keyColors[1]);
        colors.Add(keyColors[1] * 10 + info.LED[1]);
        colors.Add(info.LED[2] * 10 + keyColors[2]);
        colors.Add(keyColors[2] * 10 + info.LED[2]);

        for(int i = 0; i < info.wires.Length; i++)
            if(!used.Contains(i) && colors.Contains(info.wires[i]))
            {
                used.Add(i);
                cutGroups[4].Add(i);
                Debug.LogFormat("[The Modkit #{0}] Wire {1} has a combination matching the 5th instruction.", moduleId, i + 1);
            }

        for (int i = 0; i < info.wires.Length; i++)
            if (!used.Contains(i))
            {
                cutGroups[i + 5].Add(i);
                Debug.LogFormat("[The Modkit #{0}] Wire {1} is the remaining wire not matching any instructions.", moduleId, i + 1);
            }

        Debug.LogFormat("[The Modkit #{0}] This gives the wire cut groups {1}.", moduleId, cutGroups.Where(x => x.Count() != 0).Select(x => "[" + x.Select(y => y + 1).Join(", ") + "]").Join(""));
    }
}