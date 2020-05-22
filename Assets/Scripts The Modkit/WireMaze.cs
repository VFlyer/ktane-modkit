using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class WireMaze : Puzzle
{
    int currentRow = -1;
    int currentCol = -1;
    int startingRow = -1;
    int startingCol = -1;
    int targetRow = -1;
    int targetCol = -1;
    bool wireCut = false;

    int[][] mazeColor = new int[][] {
        new int[] { ComponentInfo.BLUE * 10 + ComponentInfo.PURPLE, ComponentInfo.PURPLE * 10 + ComponentInfo.WHITE, ComponentInfo.PURPLE * 10 + ComponentInfo.PURPLE, ComponentInfo.GREEN * 10 + ComponentInfo.WHITE, ComponentInfo.BLUE * 10 + ComponentInfo.WHITE, ComponentInfo.RED * 10 + ComponentInfo.ORANGE, ComponentInfo.BLUE * 10 + ComponentInfo.ORANGE },
        new int[] { ComponentInfo.RED * 10 + ComponentInfo.YELLOW, ComponentInfo.GREEN * 10 + ComponentInfo.YELLOW, ComponentInfo.BLUE * 10 + ComponentInfo.GREEN, ComponentInfo.WHITE * 10 + ComponentInfo.WHITE, ComponentInfo.RED * 10 + ComponentInfo.GREEN, ComponentInfo.RED * 10 + ComponentInfo.WHITE, ComponentInfo.ORANGE * 10 + ComponentInfo.PURPLE },
        new int[] { ComponentInfo.RED * 10 + ComponentInfo.RED, ComponentInfo.BLUE * 10 + ComponentInfo.YELLOW, ComponentInfo.BLUE * 10 + ComponentInfo.BLUE, ComponentInfo.RED * 10 + ComponentInfo.PURPLE, ComponentInfo.ORANGE * 10 + ComponentInfo.ORANGE, ComponentInfo.YELLOW * 10 + ComponentInfo.YELLOW, ComponentInfo.GREEN * 10 + ComponentInfo.ORANGE },
        new int[] { ComponentInfo.ORANGE * 10 + ComponentInfo.WHITE, ComponentInfo.RED * 10 + ComponentInfo.BLUE, ComponentInfo.GREEN * 10 + ComponentInfo.GREEN, ComponentInfo.GREEN * 10 + ComponentInfo.PURPLE, ComponentInfo.YELLOW * 10 + ComponentInfo.PURPLE, ComponentInfo.YELLOW * 10 + ComponentInfo.ORANGE, ComponentInfo.YELLOW * 10 + ComponentInfo.WHITE }
    };

    int[][][] mazeDir = new int[][][] {
        new int [][] {
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.LEFT }
        },
        new int [][] {
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN }
        },
        new int [][] {
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN },
            new int[] { ComponentInfo.DOWN, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.LEFT },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.DOWN },
            new int[] { ComponentInfo.UP, ComponentInfo.DOWN }
        },
        new int [][] {
            new int[] { ComponentInfo.UP },
            new int[] { ComponentInfo.RIGHT, ComponentInfo.UP },
            new int[] { ComponentInfo.LEFT, ComponentInfo.RIGHT },
            new int[] { ComponentInfo.LEFT },
            new int[] { ComponentInfo.RIGHT },
            new int[] { ComponentInfo.UP, ComponentInfo.RIGHT, ComponentInfo.LEFT },
            new int[] { ComponentInfo.LEFT, ComponentInfo.UP }
        }
    };

    public WireMaze(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Wire Maze. Wires present are {1}.", moduleId, info.GetWireNames());
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
            return;
        }

        module.StartSolve();

        int color1 = info.wires[wire] / 10;
        int color2 = info.wires[wire] % 10;

        if(targetRow == -1)
        {
            for(int i = 0; i < mazeColor.Length; i++)
                for(int j = 0; j < mazeColor[i].Length; j++)
                    if( (mazeColor[i][j] / 10 == color1 && mazeColor[i][j] % 10 == color2) || (mazeColor[i][j] / 10 == color2 && mazeColor[i][j] % 10 == color1) )
                    {
                        targetRow = i;
                        targetCol = j;
                    }
            
		    Debug.LogFormat("[The Modkit #{0}] Cut wire {1}. Destination set to row={2}, column={3}.", moduleId, wire + 1, targetRow + 1, targetCol + 1);
            return;
        }
        if(currentRow == -1)
        {
            for(int i = 0; i < mazeColor.Length; i++)
                for(int j = 0; j < mazeColor[i].Length; j++)
                    if( (mazeColor[i][j] / 10 == color1 && mazeColor[i][j] % 10 == color2) || (mazeColor[i][j] / 10 == color2 && mazeColor[i][j] % 10 == color1) )
                    {
                        currentRow = i;
                        currentCol = j;
                        startingRow = i;
                        startingCol = j;
                    }
            
		    Debug.LogFormat("[The Modkit #{0}] Cut wire {1}. Current position set to row={2}, column={3}.", moduleId, wire + 1, currentRow + 1, currentCol + 1);
            wireCut = true;
            return;
        }


        if( (mazeColor[currentRow][currentCol] / 10 == color1 && mazeColor[currentRow][currentCol] % 10 == color2) || (mazeColor[currentRow][currentCol] / 10 == color2 && mazeColor[currentRow][currentCol] % 10 == color1) )
        {
            if(currentRow == targetRow && currentCol == targetCol)
            {
		        Debug.LogFormat("[The Modkit #{0}] Cut wire {1}. Module solved.", moduleId, wire + 1);
                module.Solve();
                return;
            }
		    Debug.LogFormat("[The Modkit #{0}] Cut wire {1}. You may now move in the maze.", moduleId, wire + 1);
            wireCut = true;
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Cut wire {1}. That does not match your position in the maze.", moduleId, wire + 1);
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

        if(!wireCut)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! You have not cut a wire corresponding to your current position.", moduleId);
            module.CauseStrike();
            return;
        }

        if(!mazeDir[currentRow][currentCol].Contains(arrow))
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! You can't go {1} when you are at row={2}, column={3}.", moduleId, ComponentInfo.DIRNAMES[arrow], currentRow + 1, currentCol + 1);
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

        Debug.LogFormat("[The Modkit #{0}] Went {1} to row={2}, column={3}.", moduleId, ComponentInfo.DIRNAMES[arrow], currentRow + 1, currentCol + 1);
        wireCut = false;
        RegenWires(false);
        Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());
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

        if(currentRow == -1 || targetRow == -1)
            return;

        currentRow = startingRow;
        currentCol = startingCol;
        Debug.LogFormat("[The Modkit #{0}] Pressed the ❖ button. Position reset to row={1}, column={2}.", moduleId, currentRow + 1, currentCol + 1);
        wireCut = true;
        RegenWires(true);
    }

    public void RegenWires(bool cut)
	{
		info.RegenWires();

        bool wireFound = false;
        int correctWire = -1;
        for(int i = 0; i < info.wires.Length; i++)
        {
            int color1 = info.wires[i] / 10;
            int color2 = info.wires[i] % 10;

            if( (mazeColor[currentRow][currentCol] / 10 == color1 && mazeColor[currentRow][currentCol] % 10 == color2) || (mazeColor[currentRow][currentCol] / 10 == color2 && mazeColor[currentRow][currentCol] % 10 == color1) )
            {
                correctWire = i;
                wireFound = true;
                break;
            }
        }
        if(!wireFound)
        {
            correctWire = rnd.Range(0, 5);
            info.wires[correctWire] = mazeColor[currentRow][currentCol];
        }

		module.StartCoroutine(RegenWiresAnim(correctWire, cut));
	}

    IEnumerator RegenWiresAnim(int wire, bool cut)
	{
		yield return module.HideComponent(0);

		for(int i = 0; i < module.wires.Length; i++)
		{
			int color1 = info.wires[i] / 10;
			int color2 = info.wires[i] % 10;

			if(color1 > color2)
			{
				color1 = color2;
				color2 = info.wires[i] / 10;
			}

			if(color1 != color2)
				module.wires[i].transform.GetComponentInChildren<Renderer>().material = module.wireMats.Where(x => x.name == ComponentInfo.COLORNAMES[color1] + "_" + ComponentInfo.COLORNAMES[color2]).ToArray()[0];
			else
				module.wires[i].transform.GetComponentInChildren<Renderer>().material = module.wireMats.Where(x => x.name == ComponentInfo.COLORNAMES[color1]).ToArray()[0];
		
			module.wires[i].transform.Find("hl").gameObject.SetActive(true);
			
            if(cut && i == wire)
                module.wires[i].GetComponent<MeshFilter>().mesh = module.wireCut;
            else
                module.wires[i].GetComponent<MeshFilter>().mesh = module.wireWhole;
		}

		yield return module.ShowComponent(0);
	}
}