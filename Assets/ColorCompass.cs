using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class ColorCompass : Puzzle
{
    int[][] sequences = new int[][] {
        new int[] { ComponentInfo.LEFT + 10, ComponentInfo.YELLOW, ComponentInfo.RIGHT + 10, ComponentInfo.UP + 10 },
        new int[] { ComponentInfo.LEFT + 10, ComponentInfo.RED, ComponentInfo.DOWN + 10, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.GREEN, ComponentInfo.DOWN + 10, ComponentInfo.GREEN, ComponentInfo.RED },
        new int[] { ComponentInfo.RIGHT + 10, ComponentInfo.UP + 10, ComponentInfo.DOWN + 10, ComponentInfo.GREEN },
        new int[] { ComponentInfo.LEFT + 10, ComponentInfo.BLUE, ComponentInfo.DOWN + 10, ComponentInfo.UP + 10 },
        new int[] { ComponentInfo.DOWN + 10, ComponentInfo.UP + 10, ComponentInfo.UP + 10, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.DOWN + 10, ComponentInfo.DOWN + 10, ComponentInfo.YELLOW, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.GREEN, ComponentInfo.DOWN + 10, ComponentInfo.YELLOW, ComponentInfo.GREEN },
        new int[] { ComponentInfo.RED, ComponentInfo.RED, ComponentInfo.UP + 10, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.UP + 10, ComponentInfo.DOWN + 10, ComponentInfo.BLUE, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.DOWN + 10, ComponentInfo.RIGHT + 10, ComponentInfo.UP + 10, ComponentInfo.UP + 10 },
        new int[] { ComponentInfo.UP + 10, ComponentInfo.YELLOW, ComponentInfo.YELLOW, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.DOWN + 10, ComponentInfo.LEFT + 10, ComponentInfo.RIGHT + 10, ComponentInfo.UP + 10 },
        new int[] { ComponentInfo.RIGHT + 10, ComponentInfo.DOWN + 10, ComponentInfo.DOWN + 10, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.RIGHT + 10, ComponentInfo.LEFT + 10, ComponentInfo.BLUE, ComponentInfo.GREEN },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.UP + 10, ComponentInfo.GREEN, ComponentInfo.RIGHT + 10 },
        new int[] { ComponentInfo.LEFT + 10, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.RIGHT + 10, ComponentInfo.DOWN + 10, ComponentInfo.RED, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.RIGHT + 10, ComponentInfo.LEFT + 10, ComponentInfo.RIGHT + 10, ComponentInfo.RIGHT + 10 },
        new int[] { ComponentInfo.UP + 10, ComponentInfo.RIGHT + 10, ComponentInfo.DOWN + 10, ComponentInfo.BLUE },
        new int[] { ComponentInfo.UP + 10, ComponentInfo.UP + 10, ComponentInfo.GREEN, ComponentInfo.GREEN },
        new int[] { ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.BLUE, ComponentInfo.RED },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.DOWN + 10, ComponentInfo.RIGHT + 10, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.UP + 10, ComponentInfo.BLUE },
        new int[] { ComponentInfo.LEFT + 10, ComponentInfo.DOWN + 10, ComponentInfo.RED, ComponentInfo.GREEN },
        new int[] { ComponentInfo.GREEN, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.GREEN, ComponentInfo.UP + 10, ComponentInfo.RIGHT + 10, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.DOWN + 10, ComponentInfo.UP + 10, ComponentInfo.DOWN + 10, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.DOWN + 10, ComponentInfo.RED, ComponentInfo.GREEN, ComponentInfo.UP + 10 },
        new int[] { ComponentInfo.UP + 10, ComponentInfo.DOWN + 10, ComponentInfo.DOWN + 10, ComponentInfo.UP + 10 },
        new int[] { ComponentInfo.DOWN + 10, ComponentInfo.RED, ComponentInfo.DOWN + 10, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.UP + 10, ComponentInfo.LEFT + 10, ComponentInfo.YELLOW, ComponentInfo.GREEN },
        new int[] { ComponentInfo.RIGHT + 10, ComponentInfo.UP + 10, ComponentInfo.UP + 10, ComponentInfo.GREEN },
        new int[] { ComponentInfo.RED, ComponentInfo.DOWN + 10, ComponentInfo.RED, ComponentInfo.UP + 10 },
        new int[] { ComponentInfo.BLUE, ComponentInfo.DOWN + 10, ComponentInfo.RIGHT + 10, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.RED, ComponentInfo.RIGHT + 10, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.DOWN + 10, ComponentInfo.UP + 10, ComponentInfo.UP + 10, ComponentInfo.RIGHT + 10 },
        new int[] { ComponentInfo.DOWN + 10, ComponentInfo.RIGHT + 10, ComponentInfo.RED, ComponentInfo.DOWN + 10 },
        new int[] { ComponentInfo.UP + 10, ComponentInfo.UP + 10, ComponentInfo.GREEN, ComponentInfo.GREEN },
        new int[] { ComponentInfo.LEFT + 10, ComponentInfo.GREEN, ComponentInfo.UP + 10, ComponentInfo.UP + 10 },
        new int[] { ComponentInfo.BLUE, ComponentInfo.UP + 10, ComponentInfo.BLUE, ComponentInfo.UP + 10 },
    };

    List<int> LED;
    List<int> off;
    int stage = 0;
    int press = 0;
    int index;

    public ColorCompass(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Color Compass. Arrows are [Up: {1}, Right: {2}, Down: {3}, Left: {4}].", moduleId, ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.UP]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.RIGHT]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.DOWN]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.LEFT]]);
        LED = info.LED.ToList();
        off = new int[] {0, 1, 2}.OrderBy(x => rnd.Range(0, 1000)).ToList();

        Debug.LogFormat("[The Modkit #{0}]] Different LED colors are [ {1} ].", moduleId, info.LED.Distinct().Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
    
        index = CalcIndex();

        Debug.LogFormat("[The Modkit #{0}]] Input sequence is [ {1} ].", moduleId, GetSequenceString());
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
            module.GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        module.StartSolve();

        if(sequences[index][press] == arrow + 10 || sequences[index][press] == info.arrows[arrow])
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed {1} arrow.", moduleId, ComponentInfo.DIRNAMES[arrow]);
            press++;
            if(press == 4)
            {
                info.LED[off[stage]] = -1;
			    module.LED[off[stage]].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[6];
                LED = info.LED.ToList();
                
                stage++;

                if(stage == 3)
                {
                    Debug.LogFormat("[The Modkit #{0}]] Sequence is correct. Module solved.", moduleId);
                    module.Solve();
                }
                else
                {
                    index = CalcIndex();
                    Debug.LogFormat("[The Modkit #{0}]] Sequence is correct. Different LED colors are [ {1} ].", moduleId, info.LED.Distinct().Where(x => x != -1).Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
                    Debug.LogFormat("[The Modkit #{0}]] Input sequence is [ {1} ].", moduleId, GetSequenceString());
                    press = 0;
                }
            }
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed {1} arrow.", moduleId, ComponentInfo.DIRNAMES[arrow]);
            press = 0;
            module.GetComponent<KMBombModule>().HandleStrike();
        }
    }

    int CalcIndex()
    {
        if(LED.Distinct().Where(x => x != -1).Count() == 3)
        {
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.BLUE))    
                return 0;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.YELLOW))    
                return 1;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.ORANGE))    
                return 2;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.PURPLE))    
                return 3;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.YELLOW))    
                return 4;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.ORANGE))    
                return 5;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.PURPLE))    
                return 6;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.YELLOW) && LED.Contains(ComponentInfo.ORANGE))    
                return 7;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.YELLOW) && LED.Contains(ComponentInfo.PURPLE))    
                return 8;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.ORANGE) && LED.Contains(ComponentInfo.PURPLE))    
                return 9;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.YELLOW))    
                return 10;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.ORANGE))    
                return 11;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.PURPLE))    
                return 12;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.YELLOW) && LED.Contains(ComponentInfo.ORANGE))    
                return 13;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.YELLOW) && LED.Contains(ComponentInfo.PURPLE))    
                return 14;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.ORANGE) && LED.Contains(ComponentInfo.PURPLE))    
                return 15;
            if(LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.YELLOW) && LED.Contains(ComponentInfo.ORANGE))    
                return 16;
            if(LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.YELLOW) && LED.Contains(ComponentInfo.PURPLE))    
                return 17;
            if(LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.ORANGE) && LED.Contains(ComponentInfo.PURPLE))    
                return 18;
            if(LED.Contains(ComponentInfo.YELLOW) && LED.Contains(ComponentInfo.ORANGE) && LED.Contains(ComponentInfo.PURPLE))    
                return 19;
        }
        else if(LED.Distinct().Where(x => x != -1).Count() == 2)
        {
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.GREEN))    
                return 20;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.BLUE))    
                return 21;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.YELLOW))    
                return 22;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.ORANGE))    
                return 23;
            if(LED.Contains(ComponentInfo.RED) && LED.Contains(ComponentInfo.PURPLE))    
                return 24;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.BLUE))    
                return 25;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.YELLOW))    
                return 26;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.ORANGE))    
                return 27;
            if(LED.Contains(ComponentInfo.GREEN) && LED.Contains(ComponentInfo.PURPLE))    
                return 28;
            if(LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.YELLOW))    
                return 29;
            if(LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.ORANGE))    
                return 30;
            if(LED.Contains(ComponentInfo.BLUE) && LED.Contains(ComponentInfo.PURPLE))    
                return 31;
            if(LED.Contains(ComponentInfo.YELLOW) && LED.Contains(ComponentInfo.ORANGE))    
                return 32;
            if(LED.Contains(ComponentInfo.YELLOW) && LED.Contains(ComponentInfo.PURPLE))    
                return 33;
            if(LED.Contains(ComponentInfo.ORANGE) && LED.Contains(ComponentInfo.PURPLE))    
                return 34;
        }
        else
        {
            if(LED.Contains(ComponentInfo.RED))    
                return 35;
            if(LED.Contains(ComponentInfo.GREEN))    
                return 36;
            if(LED.Contains(ComponentInfo.BLUE))    
                return 37;
            if(LED.Contains(ComponentInfo.YELLOW))    
                return 38;
            if(LED.Contains(ComponentInfo.ORANGE))    
                return 39;
            if(LED.Contains(ComponentInfo.PURPLE))    
                return 40;
        }

        return -1;
    }

    String GetSequenceString()
    {
        List<string> names = new List<string>();

        for(int i = 0; i < sequences[index].Length; i++)
            if(sequences[index][i] >= 10)
                names.Add(ComponentInfo.DIRNAMES[sequences[index][i] - 10]);
            else
                names.Add(ComponentInfo.COLORNAMES[sequences[index][i]]);

        return names.Join(", ");
    }
}