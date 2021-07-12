using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class WireInstructions : Puzzle
{
    List<int> toCut = new List<int>();
    int cut = 0;
    bool hasCalcLEDs = false;
    public WireInstructions(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Wire Instructions. LEDs: {1}.", moduleId, info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
        CalcSoltuion();
    }

    public override void OnWireCut(int wire)
    {
        if(module.IsAnimating())
            return;

        module.audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, module.transform);
		module.CutWire(wire);

        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Wire {1} was cut when the component selection was [ {2} ] instead of [ {3} ].", moduleId, wire + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            module.RegenWires();
            CalcSoltuion();
            return;
        }

        module.StartSolve();

        if(!toCut.Contains(wire))
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly cut wire {1}.", moduleId, wire + 1);
            module.CauseStrike();
            return;
        }

        Debug.LogFormat("[The Modkit #{0}] Correctly cut wire {1}.", moduleId, wire + 1);
        cut++;
        if(cut == toCut.Count)
        {
            Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
            module.Solve();
        }
    }

    public override void OnUtilityPress()
    {
        if(module.IsAnimating())
            return;

        module.audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.utilityBtn.AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! The ❖ button was pressed when the component selection was [ {1} ] instead of [ {2} ].", moduleId, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }
       
        module.StartSolve();

        if(toCut.Count == 0)
        {
            Debug.LogFormat("[The Modkit #{0}] Correctly pressed the ❖ button with no valid wires. Module solved.", moduleId);
            module.Solve();
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! At least one wire was valid when the ❖ button was pressed!", moduleId);
            module.CauseStrike();
        }
    }

    void CalcSoltuion()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());
        var toLog = "Cut ";
        // First LED
        switch(info.LED[0])
        {
            case 0:
            {
                    toLog += "all odd numbered wires";
                    toCut.Add(0);
                    toCut.Add(2);
                    toCut.Add(4);
                    break;
            }
            case 1:
            {
                    toLog += "wires 1 and 2";
                    toCut.Add(0);
                    toCut.Add(1);
                    break;
            }
            case 2:
            {
                    toLog += "all wires with multiple colors";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 != info.wires[i] % 10)
                            toCut.Add(i);
                    }
                    break;
            }
            case 3:
            {
                    toLog += "all wires with a blue coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.BLUE || info.wires[i] % 10 == ComponentInfo.BLUE)
                            toCut.Add(i);
                    }
                    break;
            }
            case 4:
            {
                    toLog += "all wires with a purple coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.PURPLE || info.wires[i] % 10 == ComponentInfo.PURPLE)
                            toCut.Add(i);
                    }
                    break;
            }
            case 5:
            {
                    toLog += "all wires adjacent to wires with a red coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.RED || info.wires[i] % 10 == ComponentInfo.RED)
                        {
                            if(i > 0)
                                toCut.Add(i - 1);
                            if(i < 4)
                                toCut.Add(i + 1);
                        }
                    }
                    break;
            }
        }
        // Second LED
        toLog += ", and ";
        switch (info.LED[1])
        {
            case 0:
            {
                    toLog += "wire 3";
                    toCut.Add(2);
                    break;
            }
            case 1:
            {
                    toLog += "all wires with a green coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.GREEN || info.wires[i] % 10 == ComponentInfo.GREEN)
                            toCut.Add(i);
                    }
                    break;
            }
            case 2:
            {
                    toLog += "even numbered wires";
                    toCut.Add(1);
                    toCut.Add(3);
                    break;
            }
            case 3:
            {
                    toLog += "all wires adjacent to wires with a purple coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.PURPLE || info.wires[i] % 10 == ComponentInfo.PURPLE)
                        {
                            if(i > 0)
                                toCut.Add(i - 1);
                            if(i < 4)
                                toCut.Add(i + 1);
                        }
                    }
                    break;
            }
            case 4:
            {
                    toLog += "all wires with a yellow coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.YELLOW || info.wires[i] % 10 == ComponentInfo.YELLOW)
                            toCut.Add(i);
                    }
                    break;
            }
            case 5:
            {
                    toLog += "all wires adjacent to wires with an orange coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.ORANGE || info.wires[i] % 10 == ComponentInfo.ORANGE)
                        {
                            if(i > 0)
                                toCut.Add(i - 1);
                            if(i < 4)
                                toCut.Add(i + 1);
                        }
                    }
                    break;
            }
        }
        toCut = toCut.Distinct().ToList();
        toLog += ", but do not cut ";
        // Third LED
        switch (info.LED[2])
        {
            case 0:
            {
                    toLog += "wires colored with a single color";
                    for(int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == info.wires[i] % 10)
                            toCut.Remove(i);
                    }
                    break;
            }
            case 1:
            {
                    toLog += "wires with a white coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.WHITE || info.wires[i] % 10 == ComponentInfo.WHITE)
                            toCut.Remove(i);
                    }
                    break;
            }
            case 2:
            {
                    toLog += "wires 4 and 5";
                    toCut.Remove(3);
                    toCut.Remove(4);
                    break;
            }
            case 3:
            {
                    toLog += "wires adjacent to wires with blue coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.BLUE || info.wires[i] % 10 == ComponentInfo.BLUE)
                        {
                            if(i > 0)
                                toCut.Remove(i - 1);
                            if(i < 4)
                                toCut.Remove(i + 1);
                        }
                    }
                    break;
            }
            case 4:
            {
                    toLog += "wires with an orange coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.ORANGE || info.wires[i] % 10 == ComponentInfo.ORANGE)
                            toCut.Remove(i);
                    }
                    break;
            }
            case 5:
            {
                    toLog += "wires with a red coloring";
                    for (int i = 0; i < info.wires.Length; i++)
                    {
                        if(info.wires[i] / 10 == ComponentInfo.RED || info.wires[i] % 10 == ComponentInfo.RED)
                            toCut.Remove(i);
                    }
                    break;
            }
        }

        toCut.Sort();
        if (!hasCalcLEDs)
        {
            Debug.LogFormat("[The Modkit #{0}] Full instruction from LEDs: {1}.", moduleId, toLog);
        }
        hasCalcLEDs = true;
        Debug.LogFormat("[The Modkit #{0}] Wires that need to be cut: [ {1} ].", moduleId, toCut.Any() ? toCut.Select(x => x + 1).Join(", ") : "none");
    }
}