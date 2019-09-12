using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class WireSignaling : Puzzle
{
    List<int> toCut;
    int cutCount;

    public WireSignaling(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Wire Signaling. Symbols present are: {1}. Arrows are [Up: {2}, Right: {3}, Down: {4}, Left: {5}].", moduleId, info.GetSymbols(), ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.UP]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.RIGHT]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.DOWN]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.LEFT]]);

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
            module.GetComponent<KMBombModule>().HandleStrike();
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
            module.GetComponent<KMBombModule>().HandleStrike();
        }
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());
    
        toCut = new List<int>();
        cutCount = 0;

        for(int i = 0; i < 11; i++)
        {
            if(!CheckCondition(i))
                continue;
            
            CalcWires(i);

            if(toCut.Count != 0)
            {
                Debug.LogFormat("[The Modkit #{0}] Applying rule {1}.", moduleId, i + 1);
                Debug.LogFormat("[The Modkit #{0}] Wires that need to be cut are {1}.", moduleId, toCut.Select(x => x + 1).Join(", "));
                return;
            }
            else
            {
                Debug.LogFormat("[The Modkit #{0}] Skipping rule {1} since there are no applicable wires.", moduleId, i + 1);
            }
        }
    }

    bool CheckCondition(int i)
    {
        switch(i)
        {
            case 0: return (info.symbols.Contains(8) || info.symbols.Contains(28)) && info.arrows[ComponentInfo.UP] == ComponentInfo.YELLOW;
            case 1: return info.symbols.Contains(6) && (info.arrows[ComponentInfo.LEFT] == ComponentInfo.GREEN || info.arrows[ComponentInfo.DOWN] == ComponentInfo.GREEN);
            case 2: return (info.symbols.Contains(7) && info.symbols.Contains(22)) && info.arrows[ComponentInfo.RIGHT] != ComponentInfo.RED;
            case 3: return (info.symbols.Contains(10) || info.symbols.Contains(30) || info.symbols.Contains(24)) && info.arrows[ComponentInfo.DOWN] == ComponentInfo.BLUE;
            case 4: return info.symbols.Contains(9) && info.AreArrowsAdjacent(ComponentInfo.RED, ComponentInfo.GREEN);
            case 5: return (!(info.symbols.Contains(3) || info.symbols.Contains(21))) && !info.AreArrowsAdjacent(ComponentInfo.GREEN, ComponentInfo.YELLOW);
            case 6: return (info.symbols.Contains(23) && info.symbols.Contains(15)) && info.arrows[ComponentInfo.DOWN] != ComponentInfo.RED;
            case 7: return (info.symbols.Contains(0) || info.symbols.Contains(12)) && (info.arrows[ComponentInfo.RIGHT] == ComponentInfo.GREEN || info.arrows[ComponentInfo.RIGHT] == ComponentInfo.RED) && info.arrows[ComponentInfo.DOWN] == ComponentInfo.YELLOW;
            case 8: return info.symbols.Contains(13);
            case 9: return (info.symbols.Contains(1) || info.symbols.Contains(26) || info.symbols.Contains(20)) && info.arrows[ComponentInfo.LEFT] == ComponentInfo.BLUE;
            default: return true;
        }
    }

    void CalcWires(int n)
    {
        switch(n)
        {
            case 0:
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;
                    if(color1 == ComponentInfo.WHITE || color2 == ComponentInfo.WHITE || color1 == ComponentInfo.GREEN || color2 == ComponentInfo.GREEN)
                        toCut.Add(i);
                }
                break;
            }
            case 1:
            {
                toCut.Add(2);
                toCut.Add(4);
                break;
            }
            case 2:
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;
                    if(color1 != ComponentInfo.BLUE && color2 != ComponentInfo.BLUE)
                        toCut.Add(i);
                }
                break;
            }
            case 3:
            {
                toCut.Add(1);
                toCut.Add(2);
                toCut.Add(4);
                break;
            }
            case 4:
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;
                    if(color1 != color2)
                        toCut.Add(i);
                }
                break;
            }
            case 5:
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;
                    if(color1 == ComponentInfo.ORANGE || color2 == ComponentInfo.ORANGE)
                        toCut.Add(i);
                }
                break;
            }
            case 6:
            {
                toCut.Add(0);
                toCut.Add(1);
                toCut.Add(2);
                break;
            }
            case 7:
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;
                    if(color1 == ComponentInfo.PURPLE || color2 == ComponentInfo.PURPLE)
                        if(i != 0 && !toCut.Contains(i - 1))
                            toCut.Add(i - 1);
                        if(i != 4 && !toCut.Contains(i + 1))
                            toCut.Add(i + 1);
                }
                break;
            }
            case 8:
            {
                toCut.Add(1);
                toCut.Add(3);
                break;
            }
            case 9:
            {
                if(!info.wires.Contains(ComponentInfo.RED) && !info.wires.Contains(ComponentInfo.YELLOW))
                    break;

                bool foundRed = false;
                int lastYellow = -1;

                for(int i = 0; i < info.wires.Length; i++)
                {
                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;
                    if((color1 == ComponentInfo.RED || color2 == ComponentInfo.RED) && !foundRed)
                    {
                        foundRed = true;
                        toCut.Add(i);
                    }
                    if(color1 == ComponentInfo.YELLOW || color2 == ComponentInfo.YELLOW)
                        lastYellow = i;
                }

                toCut.Add(lastYellow);
                break;
            }
            default:
            {
                toCut.Add(1);
                toCut.Add(3);
                break;
            }
        }
    }
}