using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class GatedMaze : Puzzle
{
    int[][] maze = new int[][] {
        new int[] { 22, 12, 00, 24, 21, 58, 38, 36 },
        new int[] { 30, 23, 50, 60, 40, 09, 34, 41 },
        new int[] { 27, 32, 05, 19, 28, 07, 03, 62 },
        new int[] { 55, 43, 33, 42, 51, 04, 35, 61 },
        new int[] { 37, 02, 57, 15, 59, 46, 13, 31 },
        new int[] { 29, 16, 39, 52, 06, 01, 63, 14 },
        new int[] { 11, 25, 08, 49, 47, 44, 18, 56 },
        new int[] { 20, 54, 48, 10, 17, 26, 45, 53 }
    };

    int[][][] mazeDir = new int[][][] {
        new int [][] {
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.LEFT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.LEFT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.LEFT }
        },
        new int [][] {
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.LEFT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.UP },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN }
        },
        new int [][] {
            new int[] { ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT }
        },
        new int [][] {
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN }
        },
        new int [][] {
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.UP },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT }
        },
        new int [][] {
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.LEFT }
        },
        new int [][] {
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.DOWN, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT }
        },
        new int [][] {
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.UP },
            new int[] { ComponentInfo.LEFT }
        },
    };

    int[][] specialDir = new int[][]
    {
        new int[] { 30, ComponentInfo.DOWN, ComponentInfo.RED },
        new int[] { 27, ComponentInfo.UP, ComponentInfo.RED },
        new int[] { 00, ComponentInfo.DOWN, ComponentInfo.YELLOW },
        new int[] { 50, ComponentInfo.UP, ComponentInfo.YELLOW },
        new int[] { 28, ComponentInfo.RIGHT, ComponentInfo.BLUE },
        new int[] { 07, ComponentInfo.LEFT, ComponentInfo.BLUE },
        new int[] { 42, ComponentInfo.RIGHT, ComponentInfo.ORANGE },
        new int[] { 51, ComponentInfo.LEFT, ComponentInfo.ORANGE },
        new int[] { 52, ComponentInfo.DOWN, ComponentInfo.GREEN },
        new int[] { 49, ComponentInfo.UP, ComponentInfo.GREEN },
        new int[] { 17, ComponentInfo.RIGHT, ComponentInfo.PURPLE },
        new int[] { 26, ComponentInfo.LEFT, ComponentInfo.PURPLE },
    };

    int[] numberGroup = { 23, 50, 60, 40, 27, 32, 05, 19, 28, 55, 43, 33, 42, 37, 02, 47, 29, 16, 39, 11, 25, 08, 49, 20, 54, 48, 10, 17 };

    int origin;
    int destination;

    int row = -1;
    int col = -1;

    bool solving = false;

    public GatedMaze(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Gated Maze. LEDs are: {1}.", moduleId, info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
    
        destination = module.bomb.GetSerialNumberNumbers().Sum() % 64;
        if(numberGroup.Contains(destination))
            do { origin = rnd.Range(0, 64); } while(numberGroup.Contains(origin));
        else do { origin = rnd.Range(0, 64); } while(!numberGroup.Contains(origin));

        Debug.LogFormat("[The Modkit #{0}] Origin: {1}. Destination: {2}.", moduleId, origin, destination);
    
        for(int i = 0; i < maze.Length; i++)
            for(int j = 0; j < maze[i].Length; j++)
                if(maze[i][j] == origin)
                {
                    row = i;
                    col = j;
                    break;
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

        if(!solving)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! You need to press the ❖ button first.", moduleId);
            module.CauseStrike();
            return;
        }

        if(mazeDir[row][col].Contains(arrow) || Array.Exists(specialDir, x => x[0] == origin && arrow == x[1] && info.LED.Contains(x[2])))
        {
            int provRow = row;
            int provCol = col;

            switch(arrow)
            {
                case 0: provRow = row - 1; break;
                case 1: provRow = row + 1; break;
                case 2: provCol = col + 1; break;
                case 3: provCol = col - 1; break;
            }

		    Debug.LogFormat("[The Modkit #{0}] {1} -> {2}", moduleId, origin, maze[provRow][provCol]);
            row = provRow;
            col = provCol;
            origin = maze[row][col];

            String n = Convert.ToString(origin, 2);
            while(n.Length < 6)
                n = "0" + n;

            for(int i = 0; i < n.Length; i++)
            {
                if(i < 3)
                    if(n[i] == '1')
                        module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
                    else
                        module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
                else
                    if(n[i] == '1')
                        module.alphabet[i - 3].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[0];
                    else
                        module.alphabet[i - 3].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            }

            if(origin == destination)
            {
		        Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Can't go {1} at {2}.", moduleId, ComponentInfo.DIRNAMES[arrow], origin);
            module.CauseStrike();
            return;
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

        if(!solving)
        {
            solving = true;
            String n = Convert.ToString(origin, 2);
            while(n.Length < 6)
                n = "0" + n;

            for(int i = 0; i < n.Length; i++)
            {
                if(i < 3)
                    if(n[i] == '1')
                        module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
                    else
                        module.symbols[i].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
                else
                    if(n[i] == '1')
                        module.alphabet[i - 3].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[0];
                    else
                        module.alphabet[i - 3].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            }
        }
    }
}