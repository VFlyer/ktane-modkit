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
    List<int> wiresCut;

    public WireSignaling(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Wire Signaling. Symbols present: {1}. Arrows: [Up: {2}, Right: {3}, Down: {4}, Left: {5}].", moduleId, info.GetSymbols(), ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.UP]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.RIGHT]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.DOWN]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.LEFT]]);

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
		    Debug.LogFormat("[The Modkit #{0}] Strike! Wire {1} was cut when component selection was [ {2} ] instead of [ {3} ].", moduleId, wire + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            module.RegenWires();
            CalcSolution();
            return;
        }

        module.StartSolve();

        if(toCut.Contains(wire))
        {
            Debug.LogFormat("[The Modkit #{0}] Correctly cutted wire {1}.", moduleId, wire + 1);
            wiresCut.Add(wire);
            if(wiresCut.Distinct().Count() == toCut.Distinct().Count())
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly cutted wire {1}.", moduleId, wire + 1);
            module.CauseStrike();
        }
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());
    
        toCut = new List<int>();
        wiresCut = new List<int>();

        for (int i = 0; i < 11; i++)
        {
            if(!CheckCondition(i))
                continue;
            Debug.LogFormat("[The Modkit #{0}] Row {1} returned true for both requirements.", moduleId, i + 1);
            CalcWires(i);
            if(toCut.Count != 0)
            {
                
                Debug.LogFormat("[The Modkit #{0}] Wires that need to be cut: {1}.", moduleId, toCut.Select(x => x + 1).Join(", "));
                return;
            }
            else
            {
                Debug.LogFormat("[The Modkit #{0}] Row {1} returned no applicable wires to cut. Skipping to the next row.", moduleId, i + 1);
            }
        }
    }

    bool CheckCondition(int i)
    {
        switch(i)
        {
            case 0: return (info.symbols.Contains(8) || info.symbols.Contains(28)) && info.arrows[ComponentInfo.UP] == ComponentInfo.YELLOW;
                // hookn or weird nose present and the up arrow is yellow 
            case 1: return info.symbols.Contains(6) && (info.arrows[ComponentInfo.LEFT] == ComponentInfo.GREEN || info.arrows[ComponentInfo.DOWN] == ComponentInfo.GREEN);
                // squidknife present and the left/down arrrow is green
            case 2: return (info.symbols.Contains(7) && info.symbols.Contains(22)) && info.arrows[ComponentInfo.RIGHT] != ComponentInfo.RED;
                // pumpkin and left c present and right arrow is not red
            case 3: return (info.symbols.Contains(10) || info.symbols.Contains(30) || info.symbols.Contains(24)) && info.arrows[ComponentInfo.DOWN] == ComponentInfo.BLUE;
                // six, bt, or tripod present and down arrow is blue
            case 4: return info.symbols.Contains(9) && info.AreArrowsAdjacent(ComponentInfo.RED, ComponentInfo.GREEN);
                // teepee present and red arrow adjacent to green arrow
            case 5: return (!(info.symbols.Contains(3) || info.symbols.Contains(21))) && !info.AreArrowsAdjacent(ComponentInfo.GREEN, ComponentInfo.YELLOW);
                // No smiley face and right c and green arrow opposite to yellow arrow
            case 6: return (info.symbols.Contains(23) && info.symbols.Contains(15)) && info.arrows[ComponentInfo.RIGHT] != ComponentInfo.RED;
                // pitchfork and euro present and the up/down/left arrow is red
            case 7: return (info.symbols.Contains(0) || info.symbols.Contains(12)) && (info.arrows[ComponentInfo.RIGHT] == ComponentInfo.GREEN || info.arrows[ComponentInfo.RIGHT] == ComponentInfo.RED) && info.arrows[ComponentInfo.DOWN] == ComponentInfo.YELLOW;
                // Copyright or at present and (right arrow is green/red and down arrow is yellow)
            case 8: return info.symbols.Contains(13);
                // ae present and right arrow is (any color)
            case 9: return (info.symbols.Contains(1) || info.symbols.Contains(26) || info.symbols.Contains(20)) && info.arrows[ComponentInfo.LEFT] == ComponentInfo.BLUE;
                // filled star, tracks, or paragraph present and left arrow is blue.
            default: return true;
                //The last row.
        }
    }

    void CalcWires(int n)
    {
        switch(n)
        {
            case 0:
            {// Cut wires with green or white coloring, or both.
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
            {// Cut the 3rd and 5th wire.
                toCut.Add(2); // Positions are 0-indexed.
                toCut.Add(4);
                break;
            }
            case 2:
            {// Cut wires that are not blue.
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
            {// Cut wires whose position makes a prime number.
                toCut.Add(1); // Positions are 0-indexed.
                toCut.Add(2);
                toCut.Add(4);
                break;
            }
            case 4:
            {// Cut stripped wires. Stripped wires are denoted by 2 numbers being different.
                for (int i = 0; i < info.wires.Length; i++)
                {
                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;
                    if(color1 != color2)
                        toCut.Add(i);
                }
                break;
            }
            case 5:
            {// Cut wires that contain orange coloring.
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
            {// Cut wires 1, 2, 3.
                toCut.Add(0); // Positions are 0-indexed.
                toCut.Add(1);
                toCut.Add(2);
                break;
            }
            case 7:
            {// Cut wires adjacent to purple wires.
                for(int i = 0; i < info.wires.Length; i++)
                {
                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;
                    if(color1 == ComponentInfo.PURPLE || color2 == ComponentInfo.PURPLE)
                        if(i > 0 && !toCut.Contains(i - 1))
                            toCut.Add(i - 1);
                        if(i < 4 && !toCut.Contains(i + 1))
                            toCut.Add(i + 1);
                }
                break;
            }
            case 8:
            {// Cut odd numbered wires.
                toCut.Add(0); // Positions are 0-indexed.
                toCut.Add(2);
                toCut.Add(4);
                break;
            }
            case 9:
            {// Cut the first red and last yellow wire.
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
            {// Cut wires 2 and 4.
                toCut.Add(1); // Positions are 0-indexed.
                toCut.Add(3);
                break;
            }
        }
        toCut = toCut.Distinct().ToList(); // Remove duplicate wires to cut and assign them as wires to cut.
    }
}