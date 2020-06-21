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
    List<int> wiresCut = new List<int>();

    public PowerGrid(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Power Grid. Alphanumeric keys present: {1} Distinct LEDs: {2}", moduleId, info.alphabet.Join(", "), info.LED.Distinct().Select(a => a < 0 || a >= ComponentInfo.COLORNAMES.Length ? "Unknown" : ComponentInfo.COLORNAMES[a]).Join(", "));

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
            Debug.LogFormat("[The Modkit #{0}] Correctly cut wire {1}.", moduleId, wire + 1);
            if (!wiresCut.Contains(wire))
                wiresCut.Add(wire);
            if(wiresCut.Count == toCut.Count)
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly cut wire {1}.", moduleId, wire + 1);
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
            Debug.LogFormat("[The Modkit #{0}] Correctly pressed the ❖ button when no wires were valid. Module solved.", moduleId);
            module.Solve();
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! At least one wire was valid to cut upon pressing the ❖ button!", moduleId);
            module.CauseStrike();
        }
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present: {1}", moduleId, info.GetWireNames());
        
        char[] allChars = (info.alphabet[0] + info.alphabet[1] + info.alphabet[2]).ToUpper().ToCharArray();
        toCut = new List<int>();
        List<int> allWires = info.wires.ToList();
        for(int i = 0; i < info.LED.Length; i++)
        {
            /*
             * Procedure:
             * Instead of checking each individual wires, and checking if each wire has an active power channel,
             * - Create a separate list of all wires that will be modified.
             * - Create a character array for all characters that are present on the module.
             * - For each active LED on the module:
             *  + If at least 1 character is present in the given batch of wires:
             *  + - Remove wires that are not safe on the wires list.
             * - Then for each of the remaining wires:
             *  + Get the index of the given wires and add that into the list of wires to cut.
            */
            switch (info.LED[i])
            {
                case ComponentInfo.RED:
                    {
                        if (allChars.Contains('D') || allChars.Contains('1'))
                        {
                            allWires.Remove(00);
                            allWires.Remove(45);
                            allWires.Remove(54);
                            allWires.Remove(13);
                            allWires.Remove(31);
                        }
                        if (allChars.Contains('K') || allChars.Contains('6'))
                        {
                            allWires.Remove(04);
                            allWires.Remove(40);
                            allWires.Remove(46);
                            allWires.Remove(64);
                            allWires.Remove(06);
                            allWires.Remove(60);
                        }
                        if (allChars.Contains('P') || allChars.Contains('L'))
                        {
                            allWires.Remove(15);
                            allWires.Remove(51);
                            allWires.Remove(16);
                            allWires.Remove(61);
                            allWires.Remove(23);
                            allWires.Remove(32);
                        }
                        break;
                    }
                case ComponentInfo.GREEN:
                    {
                        if (allChars.Contains('Z') || allChars.Contains('T'))
                        {
                            allWires.Remove(05);
                            allWires.Remove(50);
                            allWires.Remove(54);
                            allWires.Remove(45);
                            allWires.Remove(55);
                        }
                        if (allChars.Contains('O') || allChars.Contains('4'))
                        {
                            allWires.Remove(56);
                            allWires.Remove(65);
                            allWires.Remove(12);
                            allWires.Remove(21);
                            allWires.Remove(11);
                        }
                        if (allChars.Contains('S') || allChars.Contains('2'))
                        {
                            allWires.Remove(22);
                            allWires.Remove(44);
                            allWires.Remove(33);
                        }
                        break;
                    }
                case ComponentInfo.BLUE:
                    {
                        if (allChars.Contains('I') || allChars.Contains('M'))
                        {
                            allWires.Remove(31);
                            allWires.Remove(13);
                            allWires.Remove(00);
                            allWires.Remove(06);
                            allWires.Remove(60);
                        }
                        if (allChars.Contains('8') || allChars.Contains('G'))
                        {
                            allWires.Remove(11);
                            allWires.Remove(22);
                            allWires.Remove(15);
                            allWires.Remove(51);

                        }
                        if (allChars.Contains('R') || allChars.Contains('U'))
                        {
                            allWires.Remove(14);
                            allWires.Remove(41);
                            allWires.Remove(16);
                            allWires.Remove(61);
                            allWires.Remove(23);
                            allWires.Remove(32);
                        }
                        break;
                    }
                case ComponentInfo.YELLOW:
                    {
                        if (allChars.Contains('Q') || allChars.Contains('5'))
                        {
                            allWires.Remove(25);
                            allWires.Remove(52);
                            allWires.Remove(24);
                            allWires.Remove(42);
                            allWires.Remove(26);
                            allWires.Remove(62);
                        }
                        if (allChars.Contains('9') || allChars.Contains('3'))
                        {
                            allWires.Remove(03);
                            allWires.Remove(30);
                            allWires.Remove(36);
                            allWires.Remove(63);
                            allWires.Remove(20);
                            allWires.Remove(02);

                        }
                        if (allChars.Contains('7') || allChars.Contains('Y'))
                        {
                            allWires.Remove(66);
                            allWires.Remove(34);
                            allWires.Remove(43);
                            allWires.Remove(35);
                            allWires.Remove(53);
                        }
                        break;
                    }
                case ComponentInfo.ORANGE:
                    {
                        if (allChars.Contains('W') || allChars.Contains('F'))
                        {
                            allWires.Remove(66);
                            allWires.Remove(55);
                            allWires.Remove(10);
                            allWires.Remove(01);
                        }
                        if (allChars.Contains('H') || allChars.Contains('E'))
                        {
                            allWires.Remove(04);
                            allWires.Remove(40);
                            allWires.Remove(05);
                            allWires.Remove(50);
                            allWires.Remove(33);
                        }
                        if (allChars.Contains('B') || allChars.Contains('C'))
                        {
                            allWires.Remove(02);
                            allWires.Remove(20);
                            allWires.Remove(03);
                            allWires.Remove(30);
                            allWires.Remove(44);
                        }
                        break;
                    }
                case ComponentInfo.PURPLE:
                    {
                        if (allChars.Contains('J') || allChars.Contains('N'))
                        {
                            allWires.Remove(14);
                            allWires.Remove(41);
                            allWires.Remove(46);
                            allWires.Remove(64);
                            allWires.Remove(12);
                            allWires.Remove(21);
                        }
                        if (allChars.Contains('0') || allChars.Contains('A'))
                        {
                            allWires.Remove(34);
                            allWires.Remove(43);
                            allWires.Remove(35);
                            allWires.Remove(53);
                            allWires.Remove(36);
                            allWires.Remove(63);
                        }
                        if (allChars.Contains('X') || allChars.Contains('V'))
                        {
                            allWires.Remove(26);
                            allWires.Remove(62);
                            allWires.Remove(24);
                            allWires.Remove(42);
                            allWires.Remove(25);
                            allWires.Remove(52);
                        }
                        break;
                    }
                case ComponentInfo.WHITE:
                default: break;
            }
        }
        foreach (int oneColor in allWires)
        {
            toCut.Add(Array.IndexOf(info.wires, oneColor));
        }
        toCut = toCut.Where(x => x >= 0 && x < 5).ToList(); // Keep items within the range of 0-4
        Debug.LogFormat("[The Modkit #{0}] Safe wires: {1}", moduleId, toCut.Count != 0 ? toCut.Select(x => x + 1).Join(", ") : "none");
    }
}