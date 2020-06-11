using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class AdjacentSymbols : Puzzle
{
    int[][] map = new int[][] {
        new int[] { 23, 26, 4, 17, 20, 12, 15, 27 },
        new int[] { 5, 30, 10, 6, -1, 3, 22, 19 },
        new int[] { 21, 14, 16, 9, 2, 11, 7, 28 },
        new int[] { 0, 1, 18, 13, 25, 29, 24, 8 }
    };

    List<int> presses = new List<int>();
    List<int> pressed = new List<int>();
    int nextPress = 0;

    public AdjacentSymbols(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Adjacent Symbols. Symbols present: {1}.", moduleId, info.GetSymbols());

        string sn = module.bomb.GetSerialNumber();
        string convertedSN = "";

        foreach(char c in sn)
            if(c < 65)
                convertedSN += c;
            else
                convertedSN += c - 64;

        Debug.LogFormat("[The Modkit #{0}] Converted serial number: {1}.", moduleId, convertedSN);
    
        List<int> dir = new List<int>();

        foreach(char c in convertedSN)
            if(c == '1' || c == '5' || c == '9')
                dir.Add(ComponentInfo.UP);
            else if(c == '2' || c == '6' || c == '0')
                dir.Add(ComponentInfo.RIGHT);
            else if(c == '3' || c == '7')
                dir.Add(ComponentInfo.DOWN);
            else if(c == '4' || c == '8')
                dir.Add(ComponentInfo.LEFT);

        Debug.LogFormat("[The Modkit #{0}] Direction sequence: {1}.", moduleId, dir.Select(x => ComponentInfo.DIRNAMES[x]).Join(", "));
        
        List<int> symbolOrder = new List<int>();
        int row = 1;
        int col = 4;
        
        foreach(int d in dir)
        {
            if(d == ComponentInfo.UP)
                row--;
            else if(d == ComponentInfo.DOWN)
                row++;
            else if(d == ComponentInfo.LEFT)
                col--;
            else if(d == ComponentInfo.RIGHT)
                col++;

            if(row < 0)
                row = 3;
            if(row > 3)
                row = 0;
            if(col < 0)
                col = 7;
            if(col > 7)
                col = 0;

            symbolOrder.Add(map[row][col]);
        }

        Debug.LogFormat("[The Modkit #{0}] Symbol order: {1}.", moduleId, symbolOrder.Select(x => x == -1 ? "black" : ComponentInfo.SYMBOLCHARS[x]).Join(", "));

        foreach(int s in symbolOrder)
        {
            if(info.symbols.Contains(s) && !presses.Contains(s))
                presses.Add(s);
        }
        foreach(int s in info.symbols)
        {
            if(!presses.Contains(s))
                presses.Add(s);
        }

        Debug.LogFormat("[The Modkit #{0}] Keys press order: {1}.", moduleId, presses.Select(x => ComponentInfo.SYMBOLCHARS[x]).Join(", "));
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

        if(info.symbols[symbol] == presses[nextPress])
        {
		    Debug.LogFormat("[The Modkit #{0}] Correctly pressed symbol {1}.", moduleId, symbol + 1);
            pressed.Add(symbol);
            nextPress++;
            module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];
            if(nextPress == 3)
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                for (int x = 0; x < 3; x++)
                    module.symbols[x].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
                module.Solve();
            }
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed symbol {1}. Resetting inputs...", moduleId, symbol + 1);
            module.CauseStrike();

            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            pressed = new List<int>();
            nextPress = 0;
        }
    }

}