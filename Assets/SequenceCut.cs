using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class SequenceCut : Puzzle
{
    int[] symbolToRow = { 29, 20, 30, 17, 4, 23, 25, 18, 16, 1, 15, 13, 3, 24, 2, 8, 7, 26, 14, 19, 11, 27, 21, 5, 28, 22, 10, 6, 0, 9, 12 };
    int[][] sequences = new int[][] {
        new int[] { ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.BLUE },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.BLUE },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.BLUE, ComponentInfo.RED },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.WHITE, ComponentInfo.PURPLE },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.BLUE },
        new int[] { ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.YELLOW, ComponentInfo.RED },
        new int[] { ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.PURPLE },
        new int[] { ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.GREEN },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.ORANGE },
        new int[] { ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.WHITE },
        new int[] { ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.BLUE },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.WHITE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.ORANGE },
        new int[] { ComponentInfo.WHITE, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.GREEN },
        new int[] { ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.WHITE, ComponentInfo.YELLOW, ComponentInfo.GREEN },
        new int[] { ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.BLUE },
        new int[] { ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.WHITE, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.BLUE },
        new int[] { ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.YELLOW, ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.PURPLE },
        new int[] { ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.YELLOW, ComponentInfo.WHITE },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.RED, ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.PURPLE },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.PURPLE, ComponentInfo.ORANGE },
        new int[] { ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.WHITE, ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.RED },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.PURPLE },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.WHITE, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.RED, ComponentInfo.WHITE, ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.WHITE, ComponentInfo.GREEN },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.WHITE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.GREEN },
        new int[] { ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.WHITE },
        new int[] { ComponentInfo.WHITE, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.PURPLE },
        new int[] { ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.WHITE },
    };

    int row;
    int[] seq;
    List<int>[] cutGroups = new List<int>[7];
    List<int> used;
    List<int> cut;

    public SequenceCut(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Sequence Cut. Symbols present are: {1}. Alphanumeric keys present are: {2}.", moduleId, info.GetSymbols(), info.alphabet.Join(", "));
        row = info.symbols.Select(x => symbolToRow[x]).Min();
        seq = sequences[row];
        Debug.LogFormat("[The Modkit #{0}] Using sequence {1} - [ {2} ].", moduleId, row + 1, seq.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
        
        CalcKeysSwitches();
        
        Debug.LogFormat("[The Modkit #{0}] New sequence is [ {1} ].", moduleId, seq.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));

        CalcWireCuts();
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
            module.GetComponent<KMBombModule>().HandleStrike();
            module.RegenWires();
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
            Debug.LogFormat("[The Modkit #{0}] Cut wire {1}.", moduleId, wire + 1);
            cut.Add(wire);
            if(cut.Count() == 5)
            {
		        Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Cut wire {1}.", moduleId, wire + 1);
            module.GetComponent<KMBombModule>().HandleStrike();
            module.RegenWires();
            CalcWireCuts();
        }
    }

    void CalcKeysSwitches()
    {
        for(int i = 0; i < info.alphabet.Length; i++)
        {
            int pos1;
            int pos2;

            if(module.bomb.GetSerialNumber().IndexOf(info.alphabet[i][0]) != -1)
                pos1 = module.bomb.GetSerialNumber().IndexOf(info.alphabet[i][0]);
            else
                pos1 = (info.alphabet[i][0] - 'A' + 1) % 7;

            if((info.alphabet[i][1] - '0') >= 1 && (info.alphabet[i][1] - '0') <= 7)
                pos2 = (info.alphabet[i][1] - '0') - 1;
            else if ((info.alphabet[i][1] - '0') == 0)
                pos2 = module.bomb.GetSerialNumber().IndexOf(module.bomb.GetSerialNumberNumbers().ToArray()[0].ToString()[0]);
            else
                pos2 = 6;

            Debug.LogFormat("[The Modkit #{0}] Alphanumeric key {1} switches positions {2} and {3}.", moduleId, i + 1, pos1 + 1, pos2 + 1);

            int temp = seq[pos1];
            seq[pos1] = seq[pos2];
            seq[pos2] = temp;
        }
    }

    void CalcWireCuts()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());

        used = new List<int>();
        cut = new List<int>();
        for(int i = 0; i < cutGroups.Length; i++)
        {
            cutGroups[i] = new List<int>();
            for(int j = 0; j < info.wires.Length; j++)
            {
                int color1 = info.wires[j] / 10;
                int color2 = info.wires[j] % 10;
                if((color1 == seq[i] || color2 == seq[i]) && !used.Contains(j))
                {
                    used.Add(j);
                    cutGroups[i].Add(j);
                }
            }
        }

        Debug.LogFormat("[The Modkit #{0}] Wires cut groups are {1}.", moduleId, cutGroups.Where(x => x.Count() != 0).Select(x => "[" + x.Select(y => y + 1).Join(", ") + "]").Join(""));
    }
}