using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class TheThirdWire : Puzzle
{
    List<int> valid = new List<int>();

    char[][] map = new char[][]
    {
        new char[] { 'M', 'G', '5', 'ϫ', '҈', 'J', 'ζ', 'ƛ' },
        new char[] { 'Ϟ', 'R', '0', '¶', '2', 'L', 'Ҩ', 'X' },
        new char[] { 'H', '҂', 'ټ', 'Ѣ', 'U', 'S', 'Ѭ', '★' },
        new char[] { '4', 'Ҋ', 'Ӭ', 'Ͽ', 'Y', 'Ϙ', 'B', '8' },
        new char[] { 'Ѯ', '©', 'E', '☆', 'O', 'Җ', 'W', '3' },
        new char[] { 'T', 'Ѫ', 'Ѽ', 'ψ', 'Ͼ', 'K', 'N', 'I' },
        new char[] { 'Q', 'Ѧ', 'Ϭ', '9', 'V', 'ϗ', 'æ', 'P' },
        new char[] { 'C', '7', 'Ω', '6', 'Z', 'F', 'Ԇ', 'D' },
        new char[] { '¿', 'A', '1', '8', 'U', 'W', 'N', '4' },
        new char[] { 'S', 'D', 'C', 'T', 'A', 'H', 'ϫ', 'ټ' },
    };

    public TheThirdWire(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving The Third Wire. Symbols present are: {1}. Alphanumeric keys present are: {2}. LEDs are: {3}.", moduleId, info.GetSymbols(), info.alphabet.Join(", "), info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
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

        if(valid.Contains(symbol))
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed symbol {1}. Module solved.", moduleId, symbol + 1);
            module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
            module.Solve();
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed symbol {1}.", moduleId, symbol + 1);
            module.CauseStrike();
        }
    }

    public override void OnAlphabetPress(int alphabet)
    {
        if(module.IsAnimating())
            return;

        module.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.alphabet[alphabet].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed alphanumeric key {1} when component selection was [ {2} ] instead of [ {3} ].", moduleId, alphabet + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        if(valid.Contains(alphabet + 3))
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed alphanumeric key {1}. Module solved.", moduleId, alphabet + 1);
            module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[0];
            module.Solve();
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed alphanumeric key {1}.", moduleId, alphabet + 1);
            module.CauseStrike();
        }
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());

        valid.Clear();

        int color1 = info.wires[2] / 10;
        int color2 = info.wires[2] % 10;

        int row = module.bomb.GetSerialNumberNumbers().ToArray()[0];
        int col = -1;

        for(int i = 0; i < info.wires.Length; i++)
        {
            int c1 = info.wires[i] / 10;
            int c2 = info.wires[i] % 10;

            if(color1 == c1 || color2 == c1 || color1 == c2 || color2 == c2)
                col++;
        }

        for(int i = 0; i < info.LED.Length; i++)
            if(color1 == info.LED[i] || color2 == info.LED[i])
                col++;

        Debug.LogFormat("[The Modkit #{0}] {1} wires and LEDs match the colors of the third wire.", moduleId, col);
        Debug.LogFormat("[The Modkit #{0}] Target character is {1}.", moduleId, map[row][col]);

        int dist = 0;
        while(true)
        {
            for(int i = col - dist; i <= col + dist; i++)
            {
                int j = dist - Math.Abs(col - i);
                int coor1 = row + j;
                int coor2 = row - j;

                if(i < 0 || i > 7)
                    continue;

                if(coor1 <= 9)
                {
                    char c = map[coor1][i];
                    if(c >= '0' && c <= '9')
                    {
                        for(int k = 0; k < info.alphabet.Length; k++)
                            if(info.alphabet[k][1] == c && !valid.Contains(k + 3))
                                valid.Add(k + 3);
                    }
                    else if(c >= 'A' && c <= 'Z')
                    {
                        for(int k = 0; k < info.alphabet.Length; k++)
                            if(info.alphabet[k][0] == c && !valid.Contains(k + 3))
                                valid.Add(k + 3);
                    }
                    else
                    {
                        for(int k = 0; k < info.symbols.Length; k++)
                            if(ComponentInfo.SYMBOLCHARS[info.symbols[k]][0] == c && !valid.Contains(k))
                                valid.Add(k);
                    }
                }
                if(coor2 >= 0)
                {
                    char c = map[coor2][i];
                    if(c >= '0' && c <= '9')
                    {
                        for(int k = 0; k < info.alphabet.Length; k++)
                            if(info.alphabet[k][1] == c && !valid.Contains(k + 3))
                                valid.Add(k + 3);
                    }
                    else if(c >= 'A' && c <= 'Z')
                    {
                        for(int k = 0; k < info.alphabet.Length; k++)
                            if(info.alphabet[k][0] == c && !valid.Contains(k + 3))
                                valid.Add(k + 3);
                    }
                    else
                    {
                        for(int k = 0; k < info.symbols.Length; k++)
                            if(ComponentInfo.SYMBOLCHARS[info.symbols[k]][0] == c && !valid.Contains(k))
                                valid.Add(k);
                    }
                }
            }

            if(valid.Count != 0)
            {
                Debug.LogFormat("[The Modkit #{0}] Valid keys are [ {1} ] (distance = {2}).", moduleId, valid.Select(x => x < 3 ? "symbol key " + (x + 1) : "alphanumeric key " + (x - 2)).Join(", "), dist);
                break;
            }

            dist++;
        }
    }
}