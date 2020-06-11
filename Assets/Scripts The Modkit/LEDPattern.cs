using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class LEDPattern : Puzzle
{
    int[][][] maps = new int[][][]
    {
        new int[][] {
            new int[] {ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.RED},
            new int[] {ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.RED},
            new int[] {ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.GREEN},
        },
        new int[][] {
            new int[] {ComponentInfo.RED, ComponentInfo.RED, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.BLUE},
            new int[] {ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.PURPLE, ComponentInfo.BLUE},
            new int[] {ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.BLUE},
        },
        new int[][] {
            new int[] {ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.RED},
            new int[] {ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.YELLOW},
            new int[] {ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.RED, ComponentInfo.PURPLE},
        },
        new int[][] {
            new int[] {ComponentInfo.PURPLE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.ORANGE},
            new int[] {ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.YELLOW},
            new int[] {ComponentInfo.PURPLE, ComponentInfo.PURPLE, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.YELLOW},
        },
        new int[][] {
            new int[] {ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.YELLOW, ComponentInfo.YELLOW, ComponentInfo.GREEN},
            new int[] {ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.ORANGE},
            new int[] {ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.GREEN},
        },
        new int[][] {
            new int[] {ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.BLUE},
            new int[] {ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.RED},
            new int[] {ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.GREEN},
        },
        new int[][] {
            new int[] {ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.GREEN},
            new int[] {ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.GREEN, ComponentInfo.GREEN},
            new int[] {ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.GREEN},
        },
        new int[][] {
            new int[] {ComponentInfo.GREEN, ComponentInfo.ORANGE, ComponentInfo.RED, ComponentInfo.PURPLE, ComponentInfo.RED},
            new int[] {ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.ORANGE, ComponentInfo.ORANGE},
            new int[] {ComponentInfo.GREEN, ComponentInfo.PURPLE, ComponentInfo.YELLOW, ComponentInfo.PURPLE, ComponentInfo.RED},
        },
        new int[][] {
            new int[] {ComponentInfo.PURPLE, ComponentInfo.PURPLE, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.BLUE},
            new int[] {ComponentInfo.BLUE, ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.RED, ComponentInfo.RED},
            new int[] {ComponentInfo.ORANGE, ComponentInfo.BLUE, ComponentInfo.ORANGE, ComponentInfo.GREEN, ComponentInfo.BLUE},
        },
        new int[][] {
            new int[] {ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.YELLOW, ComponentInfo.BLUE, ComponentInfo.BLUE},
            new int[] {ComponentInfo.PURPLE, ComponentInfo.BLUE, ComponentInfo.PURPLE, ComponentInfo.RED, ComponentInfo.GREEN},
            new int[] {ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.ORANGE, ComponentInfo.YELLOW, ComponentInfo.ORANGE},
        },
    };

    List<int> matches = new List<int>();

    public LEDPattern(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving LED Pattern. LEDs: {1}.", moduleId, info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));

        for(int i = 0; i < maps.Length; i++)
        {
            for(int j = 0; j < maps[i][0].Length - 2; j++)
            {
                if( (info.LED[0] == maps[i][0][j] && info.LED[1] == maps[i][0][j+1] && info.LED[2] == maps[i][0][j+2]) ||
                    (info.LED[0] == maps[i][0][j+2] && info.LED[1] == maps[i][0][j+1] && info.LED[2] == maps[i][0][j]) ||
                    (info.LED[0] == maps[i][1][j] && info.LED[1] == maps[i][1][j+1] && info.LED[2] == maps[i][1][j+2]) ||
                    (info.LED[0] == maps[i][1][j+2] && info.LED[1] == maps[i][1][j+1] && info.LED[2] == maps[i][1][j]) ||
                    (info.LED[0] == maps[i][2][j] && info.LED[1] == maps[i][2][j+1] && info.LED[2] == maps[i][2][j+2]) ||
                    (info.LED[0] == maps[i][2][j+2] && info.LED[1] == maps[i][2][j+1] && info.LED[2] == maps[i][2][j]) ||
                    (info.LED[0] == maps[i][0][j] && info.LED[1] == maps[i][1][j] && info.LED[2] == maps[i][2][j]) ||
                    (info.LED[0] == maps[i][2][j] && info.LED[1] == maps[i][1][j] && info.LED[2] == maps[i][0][j]) )
                {
                    matches.Add(i);
                    break;
                }
            }
            for(int j = maps[i][0].Length - 2; j < maps[i][0].Length; j++)
            {
                if( (info.LED[0] == maps[i][0][j] && info.LED[1] == maps[i][1][j] && info.LED[2] == maps[i][2][j]) ||
                    (info.LED[0] == maps[i][2][j] && info.LED[1] == maps[i][1][j] && info.LED[2] == maps[i][0][j]) )
                {
                    matches.Add(i);
                    break;
                }
            }
        }

        Debug.LogFormat("[The Modkit #{0}] Corresponding grids are: {1}.", moduleId, matches.Join(", "));
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

        int digit = ((int) module.bomb.GetTime()) % 10;

        if(matches.Contains(digit))
        {
            Debug.LogFormat("[The Modkit #{0}] Correctly pressed the ❖ button when the last seconds digit on the countdown timer was {1}. Module solved.", moduleId, digit);
            module.Solve();
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed the ❖ button when the last seconds digit on the countdown timer was {1}.", moduleId, digit);
            module.CauseStrike();
        }
        
    }
}