using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class PerspectiveSymbols : Puzzle
{
    int currentRow = -1;
    int currentCol = -1;
    int startingRow = -1;
    int startingCol = -1;

    List<int> pressed = new List<int>();

    int[][][] mazeDir = new int[][][] {
        new int [][] {
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.DOWN },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.LEFT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
        },
        new int [][] {
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
        },
        new int [][] {
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.UP },
        },
        new int [][] {
            new int[] { ComponentInfo.RIGHT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
        },
        new int [][] {
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.UP },
            new int[] { ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
        },
        new int [][] {
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.UP },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
        },
        new int [][] {
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.DOWN, ComponentInfo.UP },
            new int[] { ComponentInfo.DOWN, ComponentInfo.UP },
            new int[] { ComponentInfo.DOWN, ComponentInfo.UP },
            new int[] { ComponentInfo.DOWN, ComponentInfo.UP },
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
        },
        new int [][] {
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT, ComponentInfo.LEFT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.UP },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
        },
        new int [][] {
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
        },
        new int [][] {
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP },
        },
    };

    int[][][] mazeSym = new int[][][] {
        new int[][] {
            new int[] { 17 },
            new int[] { 17 },
            new int[] { 1 },
            new int[] { },
            new int[] { },
            new int[] { 15, 7 },
            new int[] { },
            new int[] { 8, 27, 26, 19 },
            new int[] { 18 },
            new int[] { 18 },
        },
        new int[][] {
            new int[] { },
            new int[] { 17 },
            new int[] { 1, 4, 12, 5, 23 },
            new int[] { },
            new int[] { 2, 8 },
            new int[] { 15, 7, 2, 8 },
            new int[] { 2, 8 },
            new int[] { 2, 8, 27, 26, 19 },
            new int[] { 3, 16 },
            new int[] { 18 },
        },
        new int[][] {
            new int[] { },
            new int[] { },
            new int[] { 1, 4, 12, 5, 23 },
            new int[] { 15, 27 },
            new int[] { 15, 27 },
            new int[] { 15, 27, 7 },
            new int[] { 15, 27 },
            new int[] { 8, 27, 26, 19, 15 },
            new int[] { 3, 16 },
            new int[] { 3, 18 },
        },
        new int[][] {
            new int[] { 11, 4 },
            new int[] { 11, 4 },
            new int[] { 1, 4, 12, 5, 23, 11 },
            new int[] { 11, 4 },
            new int[] { },
            new int[] { 15, 7, 28, 26 },
            new int[] { 28, 26 },
            new int[] { 28, 26, 8, 27, 19 },
            new int[] { 3, 16, 28, 26 },
            new int[] { 28, 26, 21 },
        },
        new int[][] {
            new int[] { 12, 9, 29 },
            new int[] { 12 },
            new int[] { 1, 4, 12, 5, 23 },
            new int[] { 6 },
            new int[] { 6, 24 },
            new int[] { 15, 7, 6 },
            new int[] { },
            new int[] { 8, 27, 26, 19 },
            new int[] { 3, 16 },
            new int[] { 21 },
        },
        new int[][] {
            new int[] { 9, 29 },
            new int[] { 20 },
            new int[] { 1, 4, 12, 5, 23, 20 },
            new int[] { 25 },
            new int[] { 6, 24, 25 },
            new int[] { 15, 7 },
            new int[] { 13 },
            new int[] { 8, 27, 26, 19, 13 },
            new int[] { 3, 16 },
            new int[] { 21 },
        },
        new int[][] {
            new int[] { 9, 29 },
            new int[] { 29 },
            new int[] { 1, 4, 12, 5, 23 },
            new int[] { 25 },
            new int[] { 6, 24 },
            new int[] { 15, 7 },
            new int[] { 13 },
            new int[] { 8, 27, 26, 19 },
            new int[] { 3, 16 },
            new int[] { 21 },
        },
        new int[][] {
            new int[] { 5, 14 },
            new int[] { 5 },
            new int[] { 1, 4, 12, 5, 23 },
            new int[] { 25 },
            new int[] { 6, 24, 7 },
            new int[] { 15, 7 },
            new int[] { 30 },
            new int[] { 8, 27, 26, 19, 16 },
            new int[] { 3, 16 },
            new int[] { 21 },
        },
        new int[][] {
            new int[] { 14 },
            new int[] { 22 },
            new int[] { 1, 4, 12, 5, 23 },
            new int[] { 25, 10 },
            new int[] { 10 },
            new int[] { 15, 7 },
            new int[] { 30 },
            new int[] { 8, 27, 26, 19 },
            new int[] { },
            new int[] { 21 },
        },
        new int[][] {
            new int[] { 14, 22 },
            new int[] { 14, 22 },
            new int[] { 1, 4, 12, 5, 23 },
            new int[] { 25 },
            new int[] { 10 },
            new int[] { 15, 7 },
            new int[] { 30, 0 },
            new int[] { 8, 27, 26, 19, 30, 0 },
            new int[] { 30, 0 },
            new int[] { 21 },
        },
    };
    
    public PerspectiveSymbols(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Perspective Symbols. Symbols present: {1}.", moduleId, info.GetSymbols());

        startingRow = module.bomb.GetSerialNumberNumbers().ToArray()[0];
        startingCol = module.bomb.GetSerialNumberNumbers().ToArray()[1];
        currentRow = startingRow;
        currentCol = startingCol;

        Debug.LogFormat("[The Modkit #{0}] Starting position: row={1}, column={2}.", moduleId, startingRow, startingCol);
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

        if(mazeSym[currentRow][currentCol].Contains(info.symbols[symbol]))
        {
		    Debug.LogFormat("[The Modkit #{0}] Correctly pressed symbol {1} at row={2}, column={3}.", moduleId, symbol + 1, currentRow, currentCol);
            pressed.Add(symbol);
            module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
            if(pressed.Count == 3)
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed symbol {1} at row={2}, column={3}.", moduleId, symbol + 1, currentRow, currentCol);
            module.CauseStrike();
        }
    }

    public override void OnArrowPress(int arrow)
    {
        if(module.IsAnimating())
            return;

        module.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.arrows[arrow].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed {1} arrow when component selection was [ {2} ] instead of [ {3} ].", moduleId, ComponentInfo.DIRNAMES[arrow], module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        if(!mazeDir[currentRow][currentCol].Contains(arrow))
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! You can't go {1} at row={2}, column={3}.", moduleId, ComponentInfo.DIRNAMES[arrow], currentRow, currentCol);
            if (currentFlashing != null)
                module.StopCoroutine(currentFlashing);
            currentFlashing = HandleArrowDelayFlashSingle(arrow);
            module.StartCoroutine(currentFlashing);
            module.CauseStrike();
            return;
        }

        switch(arrow)
        {
            case 0: currentRow--; break;
            case 1: currentRow++; break;
            case 2: currentCol++; break;
            case 3: currentCol--; break;
        }

        Debug.LogFormat("[The Modkit #{0}] Went {1} to row={2}, column={3}.", moduleId, ComponentInfo.DIRNAMES[arrow], currentRow, currentCol);
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

        if (currentFlashing != null)
            module.StopCoroutine(currentFlashing);
        currentFlashing = HandleArrowDelayFlash();
        module.StartCoroutine(currentFlashing);
        currentRow = startingRow;
        currentCol = startingCol;
        Debug.LogFormat("[The Modkit #{0}] Pressed the ❖ button. Position reset to row={1}, column={2}.", moduleId, currentRow, currentCol);
    }
}