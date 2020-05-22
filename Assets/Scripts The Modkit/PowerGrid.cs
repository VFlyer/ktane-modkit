using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class PowerGrid : Puzzle
{
    List<int> toCut;
    int cutCount;

    public PowerGrid(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Power Grid. Alphanumeric keys present are: {1}.", moduleId, info.alphabet.Join(", "));

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

        if(toCut.Contains(wire))
        {
            Debug.LogFormat("[The Modkit #{0}] Cut wire {1}.", moduleId, wire + 1);
            cutCount++;
            if(cutCount == toCut.Count)
            {
                Debug.LogFormat("[The Modkit #{0}] Module solve.", moduleId);
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

        if(toCut.Count == 0)
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
        
        String s = info.alphabet[0] + info.alphabet[1] + info.alphabet[2];
        toCut = new List<int>();

        for(int i = 0; i < info.wires.Length; i++)
        {
            switch(info.wires[i])
            {
                case 0: if(!(info.LED.Contains(ComponentInfo.RED) && (s.Contains('D') || s.Contains('1'))) && !(info.LED.Contains(ComponentInfo.BLUE) && (s.Contains('I') || s.Contains('M')))) toCut.Add(i); break;
                case 11: if(!(info.LED.Contains(ComponentInfo.GREEN) && (s.Contains('O') || s.Contains('4'))) && !(info.LED.Contains(ComponentInfo.BLUE) && (s.Contains('G') || s.Contains('8')))) toCut.Add(i); break;
                case 22: if(!(info.LED.Contains(ComponentInfo.BLUE) && (s.Contains('G') || s.Contains('8'))) && !(info.LED.Contains(ComponentInfo.GREEN) && (s.Contains('S') || s.Contains('2')))) toCut.Add(i); break;
                case 33: if(!(info.LED.Contains(ComponentInfo.GREEN) && (s.Contains('S') || s.Contains('2'))) && !(info.LED.Contains(ComponentInfo.ORANGE) && (s.Contains('H') || s.Contains('E')))) toCut.Add(i); break;
                case 44: if(!(info.LED.Contains(ComponentInfo.GREEN) && (s.Contains('S') || s.Contains('2'))) && !(info.LED.Contains(ComponentInfo.ORANGE) && (s.Contains('B') || s.Contains('C')))) toCut.Add(i); break;
                case 55: if(!(info.LED.Contains(ComponentInfo.ORANGE) && (s.Contains('W') || s.Contains('F'))) && !(info.LED.Contains(ComponentInfo.GREEN) && (s.Contains('Z') || s.Contains('T')))) toCut.Add(i); break;
                case 66: if(!(info.LED.Contains(ComponentInfo.ORANGE) && (s.Contains('W') || s.Contains('F'))) && !(info.LED.Contains(ComponentInfo.YELLOW) && (s.Contains('Y') || s.Contains('7')))) toCut.Add(i); break;
                case 01:
                case 10: if(!(info.LED.Contains(ComponentInfo.ORANGE) && (s.Contains('W') || s.Contains('F')))) toCut.Add(i); break;
                case 02:
                case 20: if(!(info.LED.Contains(ComponentInfo.YELLOW) && (s.Contains('9') || s.Contains('3'))) && !(info.LED.Contains(ComponentInfo.ORANGE) && (s.Contains('B') || s.Contains('C')))) toCut.Add(i); break;
                case 03:
                case 30: if(!(info.LED.Contains(ComponentInfo.YELLOW) && (s.Contains('9') || s.Contains('3'))) && !(info.LED.Contains(ComponentInfo.ORANGE) && (s.Contains('B') || s.Contains('C')))) toCut.Add(i); break;
                case 04:
                case 40: if(!(info.LED.Contains(ComponentInfo.ORANGE) && (s.Contains('H') || s.Contains('E'))) && !(info.LED.Contains(ComponentInfo.RED) && (s.Contains('K') || s.Contains('6')))) toCut.Add(i); break;
                case 05:
                case 50: if(!(info.LED.Contains(ComponentInfo.ORANGE) && (s.Contains('H') || s.Contains('E'))) && !(info.LED.Contains(ComponentInfo.GREEN) && (s.Contains('Z') || s.Contains('T')))) toCut.Add(i); break;
                case 06:
                case 60: if(!(info.LED.Contains(ComponentInfo.BLUE) && (s.Contains('I') || s.Contains('M'))) && !(info.LED.Contains(ComponentInfo.RED) && (s.Contains('K') || s.Contains('6')))) toCut.Add(i); break;
                case 12:
                case 21: if(!(info.LED.Contains(ComponentInfo.GREEN) && (s.Contains('O') || s.Contains('4'))) && !(info.LED.Contains(ComponentInfo.PURPLE) && (s.Contains('J') || s.Contains('N')))) toCut.Add(i); break;
                case 13:
                case 31: if(!(info.LED.Contains(ComponentInfo.RED) && (s.Contains('D') || s.Contains('1'))) && !(info.LED.Contains(ComponentInfo.BLUE) && (s.Contains('I') || s.Contains('M')))) toCut.Add(i); break;
                case 14:
                case 41: if(!(info.LED.Contains(ComponentInfo.PURPLE) && (s.Contains('J') || s.Contains('N'))) && !(info.LED.Contains(ComponentInfo.BLUE) && (s.Contains('R') || s.Contains('U')))) toCut.Add(i); break;
                case 15:
                case 51: if(!(info.LED.Contains(ComponentInfo.BLUE) && (s.Contains('G') || s.Contains('8'))) && !(info.LED.Contains(ComponentInfo.RED) && (s.Contains('P') || s.Contains('L')))) toCut.Add(i); break;
                case 16:
                case 61: if(!(info.LED.Contains(ComponentInfo.BLUE) && (s.Contains('R') || s.Contains('W'))) && !(info.LED.Contains(ComponentInfo.RED) && (s.Contains('P') || s.Contains('L')))) toCut.Add(i); break;
                case 23:
                case 32: if(!(info.LED.Contains(ComponentInfo.RED) && (s.Contains('P') || s.Contains('L'))) && !(info.LED.Contains(ComponentInfo.BLUE) && (s.Contains('R') || s.Contains('U')))) toCut.Add(i); break;
                case 24:
                case 42: if(!(info.LED.Contains(ComponentInfo.PURPLE) && (s.Contains('V') || s.Contains('X'))) && !(info.LED.Contains(ComponentInfo.YELLOW) && (s.Contains('Q') || s.Contains('5')))) toCut.Add(i); break;
                case 25:
                case 52: if(!(info.LED.Contains(ComponentInfo.PURPLE) && (s.Contains('V') || s.Contains('X'))) && !(info.LED.Contains(ComponentInfo.YELLOW) && (s.Contains('Q') || s.Contains('5')))) toCut.Add(i); break;
                case 26:
                case 62: if(!(info.LED.Contains(ComponentInfo.PURPLE) && (s.Contains('V') || s.Contains('X'))) && !(info.LED.Contains(ComponentInfo.YELLOW) && (s.Contains('Q') || s.Contains('5')))) toCut.Add(i); break;
                case 34:
                case 43: if(!(info.LED.Contains(ComponentInfo.YELLOW) && (s.Contains('Y') || s.Contains('7'))) && !(info.LED.Contains(ComponentInfo.PURPLE) && (s.Contains('A') || s.Contains('0')))) toCut.Add(i); break;
                case 35:
                case 53: if(!(info.LED.Contains(ComponentInfo.YELLOW) && (s.Contains('Y') || s.Contains('7'))) && !(info.LED.Contains(ComponentInfo.PURPLE) && (s.Contains('A') || s.Contains('0')))) toCut.Add(i); break;
                case 36:
                case 63: if(!(info.LED.Contains(ComponentInfo.YELLOW) && (s.Contains('9') || s.Contains('3'))) && !(info.LED.Contains(ComponentInfo.PURPLE) && (s.Contains('A') || s.Contains('0')))) toCut.Add(i); break;
                case 45:
                case 54: if(!(info.LED.Contains(ComponentInfo.RED) && (s.Contains('D') || s.Contains('1'))) && !(info.LED.Contains(ComponentInfo.GREEN) && (s.Contains('Z') || s.Contains('T')))) toCut.Add(i); break;
                case 46:
                case 64: if(!(info.LED.Contains(ComponentInfo.RED) && (s.Contains('K') || s.Contains('6'))) && !(info.LED.Contains(ComponentInfo.PURPLE) && (s.Contains('N') || s.Contains('J')))) toCut.Add(i); break;
                case 56:
                case 65: if(!(info.LED.Contains(ComponentInfo.GREEN) && (s.Contains('O') || s.Contains('4')))) toCut.Add(i); break;
            }
        }

        Debug.LogFormat("[The Modkit #{0}] Safe wires are {1}.", moduleId, toCut.Count != 0 ? toCut.Select(x => x + 1).Join(", ") : "none");
    }
}