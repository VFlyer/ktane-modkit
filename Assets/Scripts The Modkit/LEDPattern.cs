using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

class LEDPattern : Puzzle
{
    int[][][] maps = new int[][][]
    {
        // Grid ID 0
        new int[][] {
            new int[] {ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.RED},
            new int[] {ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.RED},
            new int[] {ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.GREEN},
        },
        // Grid ID 1
        new int[][] {
            new int[] {ComponentInfo.RED, ComponentInfo.RED, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.BLUE},
            new int[] {ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.PURPLE, ComponentInfo.BLUE},
            new int[] {ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.BLUE},
        },
        // Grid ID 2
        new int[][] {
            new int[] {ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.RED},
            new int[] {ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.YELLOW},
            new int[] {ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.PURPLE},
        },
        // Grid ID 3
        new int[][] {
            new int[] {ComponentInfo.PURPLE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.ORANGE},
            new int[] {ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.YELLOW},
            new int[] {ComponentInfo.PURPLE, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.YELLOW},
        },
        // Grid ID 4
        new int[][] {
            new int[] {ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.YELLOW, ComponentInfo.YELLOW, ComponentInfo.GREEN},
            new int[] {ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.ORANGE},
            new int[] {ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.GREEN},
        },
        // Grid ID 5
        new int[][] {
            new int[] {ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.BLUE},
            new int[] {ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.RED},
            new int[] {ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.GREEN},
        },
        // Grid ID 6
        new int[][] {
            new int[] {ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.GREEN},
            new int[] {ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.GREEN},
            new int[] {ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.GREEN},
        },
        // Grid ID 7
        new int[][] {
            new int[] {ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.RED},
            new int[] {ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.ORANGE},
            new int[] {ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.RED},
        },
        // Grid ID 8
        new int[][] {
            new int[] {ComponentInfo.PURPLE, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.BLUE},
            new int[] {ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.RED, ComponentInfo.RED},
            new int[] {ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.BLUE},
        },
        // Grid ID 9
        new int[][] {
            new int[] {ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.BLUE},
            new int[] {ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.GREEN},
            new int[] {ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.ORANGE},
        },
    };

    List<int> matches = new List<int>();

    private List<int> GetMatchingGrids(int[] ledCombination)
    {
        var currentMatches = new List<int>();
        for (int i = 0; i < maps.Length; i++)
        {
            var curMap = maps[i];
            var foundMatch = false;
            for (var x = 0; x < curMap.Length; x++)
            {
                for (var y = 0; y < curMap[x].Length; y++)
                {
                    var curHorizSequence = new[] { curMap[x][y], curMap[x][(y + 1) % curMap[x].Length], curMap[x][(y + 2) % curMap[x].Length] };
                    var curVertSequence = new[] { curMap[x][y], curMap[(x + 1) % curMap.Length][y], curMap[(x + 2) % curMap.Length][y] };
                    if (curHorizSequence.SequenceEqual(ledCombination) || curHorizSequence.SequenceEqual(ledCombination.Reverse()) ||
                        curVertSequence.SequenceEqual(ledCombination) || curVertSequence.SequenceEqual(ledCombination.Reverse()))
                        foundMatch = true;
                }
            }
            if (foundMatch)
                currentMatches.Add(i);
        }
        return currentMatches;
    }

    public override void BruteForceTest()
    {
        var allCombinationsCounts = 0;
        var workingPatterns = new List<int[]>();
        var failingPatterns = new List<int[]>();
        int[] combinationLeds = new int[3];
        var pointerPos = 0;
        var maxValue = 5;
        var stopLoop = false;
        while (!stopLoop)
        {
            if (GetMatchingGrids(combinationLeds).Any())
                workingPatterns.Add(combinationLeds.ToArray());
            else
                failingPatterns.Add(combinationLeds.ToArray());
            allCombinationsCounts++;
            while (combinationLeds[pointerPos] + 1 > maxValue)
            {
                combinationLeds[pointerPos] = 0;
                pointerPos++;
                if (pointerPos >= combinationLeds.Length)
                {
                    stopLoop = true;
                    break;
                }
            }
            if (pointerPos < combinationLeds.Length)
                combinationLeds[pointerPos]++;
            pointerPos = 0;
        }
        Debug.LogFormat("Usable pattern count: {0} / {1}", workingPatterns.Count(), allCombinationsCounts);
        Debug.LogFormat("Unusable patterns: [{0}]", failingPatterns.Select(a => a.Select(b => ComponentInfo.COLORNAMES[b]).Join(",")).Join("];["));
    }

    public LEDPattern(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving LED Pattern. LEDs: {1}.", moduleId, info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
        matches = GetMatchingGrids(info.LED);
        Debug.LogFormat("[The Modkit #{0}] Corresponding grids that match this LED combination: {1}", moduleId, matches.Join(", "));
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
		    Debug.LogFormat("[The Modkit #{0}] Strike! The ❖ button was pressed when the component selection was [ {1} ] instead of [ {2} ].", moduleId, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        int digit = ((int) module.bomb.GetTime()) % 10;

        if (matches.Contains(digit))
        {
            Debug.LogFormat("[The Modkit #{0}] Correctly pressed the ❖ button when the last seconds digit on the countdown timer was {1}. Module solved.", moduleId, digit);
            module.Solve();
        }
        else if (matches.Any())
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed the ❖ button when the last seconds digit on the countdown timer was {1}.", moduleId, digit);
            module.CauseStrike();
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] There were no grids that contain that LED combination. Activating failsafe and disarming module.", moduleId, digit);
            module.Solve();
        }
        
    }
}