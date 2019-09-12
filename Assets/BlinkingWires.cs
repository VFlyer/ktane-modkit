using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class BlinkingWires : Puzzle
{
    int cut;
    int[] targetLED = new int[3];

    Coroutine blink = null;
    bool flag = false;

    int currentLED = 0;

    public BlinkingWires(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Blinking Wires.", moduleId);
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
            module.GetComponent<KMBombModule>().HandleStrike();
            module.RegenWires();
            CalcSolution();
            return;
        }

        module.StartSolve();

        if(!info.LED.SequenceEqual(targetLED))
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Expected LEDs to be [ {1} ], but they were [ {2} ] instead.", moduleId, info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "), targetLED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
            module.GetComponent<KMBombModule>().HandleStrike();
            module.RegenWires();
            CalcSolution();
            return;
        }

        if(wire != cut)
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Wire {1} was cut when wire {2} was expected.", moduleId, wire + 1, cut + 1);
            module.GetComponent<KMBombModule>().HandleStrike();
            module.RegenWires();
            CalcSolution();
            return;
        }

        Debug.LogFormat("[The Modkit #{0}] Cut wire {1}. Module solved.", moduleId, wire + 1);
        module.Solve();

        if(blink != null)
            module.StopCoroutine(blink);

        for(int i = 0; i < module.LED.Length; i++)
		{
			module.LED[i].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[i]];
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
            module.GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        module.StartSolve();

        switch(arrow)
        {
            case 0:
            {
                info.LED[currentLED]++;
                if(info.LED[currentLED] == 6)
                    info.LED[currentLED] = 0;
			    module.LED[currentLED].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[currentLED]];
                break;
            }
            case 1:
            {
                info.LED[currentLED]--;
                if(info.LED[currentLED] == -1)
                    info.LED[currentLED] = 5;
			    module.LED[currentLED].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[currentLED]];
                break;
            }
            case 2:
            {
                for(int i = 0; i < module.LED.Length; i++)
                {
                    module.LED[i].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[i]];
                }
                if(currentLED != 2)
                    currentLED++;
                    break;
            }
            case 3:
            {
                for(int i = 0; i < module.LED.Length; i++)
                {
                    module.LED[i].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[i]];
                }
                if(currentLED != 0)
                    currentLED--;
                    break;
            }
        }
        flag = true;

        if(blink == null)
            blink = module.StartCoroutine(Blink());
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());

        cut = -1;

        CalcRule(1);
        CalcRule(2);
        CalcRule(3);

        Debug.LogFormat("[The Modkit #{0}] LED target colors are: {1}.", moduleId, targetLED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
        Debug.LogFormat("[The Modkit #{0}] Wire to be cut is wire {1}.", moduleId, cut + 1);
    }

    void CalcRule(int n)
    {
        int color1;
        int color2;
        int cnt;
        int last;
        int first;

        switch(n)
        {
            case 1:
            {
                cnt = 0;
                last = -1;
                for(int i = 0; i < info.wires.Length; i++)
                {
                    color1 = info.wires[i] / 10;
                    color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.ORANGE || color2 == ComponentInfo.ORANGE)
                    {
                        cnt++;
                        last = i;
                    }
                }

                if(cnt >= 3)
                {
                    targetLED[0] = ComponentInfo.ORANGE;
                    if(cut == -1)
                        cut = last;
                    return;
                }

                color1 = info.wires[3] / 10;
                color2 = info.wires[3] % 10;

                if(color1 == ComponentInfo.RED || color2 == ComponentInfo.RED)
                {
                    targetLED[0] = ComponentInfo.RED;
                    return;
                }

                bool bluePresent = false;

                for(int i = 0; i < info.wires.Length; i++)
                {
                    color1 = info.wires[i] / 10;
                    color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.BLUE || color2 == ComponentInfo.BLUE)
                    {
                        bluePresent = true;
                        break;
                    }
                }

                if(!bluePresent)
                {
                    targetLED[0] = ComponentInfo.BLUE;
                    if(cut == -1)
                        cut = 0;
                    return;
                }

                bool purplePresent = false;
                bool greenPresent = false;
                bool whitePresent = false;
                first = -1;

                for(int i = 0; i < info.wires.Length; i++)
                {
                    color1 = info.wires[i] / 10;
                    color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.PURPLE || color2 == ComponentInfo.PURPLE)
                    {
                        purplePresent = true;
                        if(first == -1)
                            first = i;
                    }
                    if(color1 == ComponentInfo.GREEN || color2 == ComponentInfo.GREEN)
                    {
                        greenPresent = true;
                    }
                    if(color1 == ComponentInfo.WHITE || color2 == ComponentInfo.WHITE)
                    {
                        whitePresent = true;
                    }
                }

                if(purplePresent && greenPresent && whitePresent)
                {
                    targetLED[0] = ComponentInfo.PURPLE;
                    if(cut == -1)
                        cut = first;
                    return;
                }

                cnt = 0;

                for(int i = 0; i < info.wires.Length; i++)
                {
                    color1 = info.wires[i] / 10;
                    color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.YELLOW || color2 == ComponentInfo.YELLOW)
                        cnt++;
                }

                if(cnt == 2)
                {
                    targetLED[0] = ComponentInfo.YELLOW;
                    return;
                }

                targetLED[0] = ComponentInfo.GREEN;
                return;
            }
            case 2:
            {
                color1 = info.wires[0] / 10;
                color2 = info.wires[0] % 10;
                int color3 = info.wires[4] / 10;
                int color4 = info.wires[4] % 10;

                cnt = 0;
                first = -1;

                if((color1 == ComponentInfo.GREEN || color2 == ComponentInfo.GREEN) && (color3 == ComponentInfo.GREEN || color4 == ComponentInfo.GREEN))
                {
                    for(int i = 0; i < info.wires.Length; i++)
                    {
                        color1 = info.wires[i] / 10;
                        color2 = info.wires[i] % 10;

                        if(color1 == ComponentInfo.GREEN || color2 == ComponentInfo.GREEN)
                        {
                            cnt++;
                            if(cnt == 2)
                            {
                                first = i;
                                break;
                            }
                        }
                    }

                    targetLED[1] = ComponentInfo.GREEN;
                    if(cut == -1)
                        cut = first;
                    return;
                }

                cnt = 0;

                for(int i = 0; i < info.wires.Length; i++)
                {
                    color1 = info.wires[i] / 10;
                    color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.YELLOW || color2 == ComponentInfo.YELLOW)
                        cnt++;
                    else
                        first = i;
                }

                if(cnt == 4)
                {
                    targetLED[1] = ComponentInfo.YELLOW;
                    if(cut == -1)
                        cut = first;
                    return;
                }

                bool lastPurple = false;

                for(int i = 0; i < info.wires.Length; i++)
                {
                    color1 = info.wires[i] / 10;
                    color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.PURPLE || color2 == ComponentInfo.PURPLE)
                    {
                        if(lastPurple)
                        {
                            targetLED[1] = ComponentInfo.PURPLE;
                            if(cut == -1)
                                cut = 1;
                            return;
                        }
                        else
                        {
                            lastPurple = true;
                        }
                    }
                    else
                    {
                        lastPurple = false;
                    }
                }

                cnt = 0;

                for(int i = 0; i < info.wires.Length; i++)
                {
                    color1 = info.wires[i] / 10;
                    color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.RED || color2 == ComponentInfo.RED)
                        cnt++;
                }

                if(cnt <= 1)
                {
                    targetLED[1] = ComponentInfo.RED;
                    return;
                }

                color1 = info.wires[1] / 10;
                color2 = info.wires[1] % 10;
                color3 = info.wires[2] / 10;
                color4 = info.wires[2] % 10;

                if((color1 == ComponentInfo.ORANGE || color2 == ComponentInfo.ORANGE) && (color3 == ComponentInfo.ORANGE || color4 == ComponentInfo.ORANGE))
                {
                    targetLED[1] = ComponentInfo.ORANGE;
                    if(cut == -1)
                        cut = 4;
                    return;
                }

                targetLED[1] = ComponentInfo.BLUE;
                return;
            }
            case 3:
            {
                bool bluePresent = false;
                bool whitePresent = false;

                first = -1;

                for(int i = 0; i < info.wires.Length; i++)
                {
                    color1 = info.wires[i] / 10;
                    color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.BLUE || color2 == ComponentInfo.BLUE)
                    {
                        bluePresent = true;
                        if(first == -1)
                            first = i;
                    }
                    if(color1 == ComponentInfo.WHITE || color2 == ComponentInfo.WHITE)
                    {
                        whitePresent = true;
                    }
                }

                if(bluePresent && !whitePresent)
                {
                    targetLED[2] = ComponentInfo.BLUE;
                    if(cut == -1)
                        cut = first;
                    return;
                }

                color1 = info.wires[2] / 10;
                color2 = info.wires[2] % 10;
                int color3 = info.wires[4] / 10;
                int color4 = info.wires[4] % 10;

                cnt = 0;
                first = -1;
                last = -1;

                if((color1 != ComponentInfo.GREEN && color2 != ComponentInfo.GREEN) && (color3 != ComponentInfo.GREEN && color4 != ComponentInfo.GREEN))
                {
                    for(int i = 0; i < info.wires.Length; i++)
                    {
                        color1 = info.wires[i] / 10;
                        color2 = info.wires[i] % 10;

                        if(color1 != ComponentInfo.GREEN && color2 != ComponentInfo.GREEN)
                        {
                            last = i;
                        }
                    }

                    targetLED[2] = ComponentInfo.GREEN;
                    if(cut == -1)
                        cut = last;
                    return;
                }

                color1 = info.wires[1] / 10;
                color2 = info.wires[1] % 10;
                color3 = info.wires[3] / 10;
                color4 = info.wires[3] % 10;

                if((color1 != ComponentInfo.YELLOW && color2 != ComponentInfo.YELLOW) && (color3 != ComponentInfo.YELLOW && color4 != ComponentInfo.YELLOW))
                {
                    targetLED[2] = ComponentInfo.YELLOW;
                    if(cut == -1)
                        cut = 3;
                    return;
                }

                bool found = true;
                last = -1;

                for(int i = 0; i < info.wires.Length; i++)
                {
                    color1 = info.wires[i] / 10;
                    color2 = info.wires[i] % 10;

                    if(color1 != color2)
                    {
                        if(color1 != ComponentInfo.RED && color2 != ComponentInfo.RED)
                        {
                            last = i;
                        }
                        else
                        {
                            found = false;
                            break;
                        }
                    }
                }

                if(found && last != -1)
                {
                    targetLED[2] = ComponentInfo.RED;
                    if(cut == -1)
                        cut = last;
                    return;
                }

                color1 = info.wires[1] / 10;
                color2 = info.wires[1] % 10;
                color3 = info.wires[3] / 10;
                color4 = info.wires[3] % 10;

                if((color1 == ComponentInfo.PURPLE || color2 == ComponentInfo.PURPLE) && color3 != ComponentInfo.PURPLE && color4 != ComponentInfo.PURPLE)
                {
                    targetLED[2] = ComponentInfo.PURPLE;
                    if(cut == -1)
                        cut = 1;
                    return;
                }
                else if((color3 == ComponentInfo.PURPLE || color4 == ComponentInfo.PURPLE) && color1 != ComponentInfo.PURPLE && color2 != ComponentInfo.PURPLE)
                {
                    targetLED[2] = ComponentInfo.PURPLE;
                    if(cut == -1)
                        cut = 3;
                    return;
                }

                targetLED[2] = ComponentInfo.ORANGE;
                if(cut == -1)
                        cut = 2;
                return;
            }
        }
    }

    IEnumerator Blink()
    {
        while(true)
        {
            module.LED[currentLED].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[currentLED]];

            yield return new WaitForSeconds(0.5f);

            while(flag)
            {
                flag = false;
                yield return new WaitForSeconds(0.5f);
            }

            for(int i = 0; i < module.LED.Length; i++)
            {
                module.LED[i].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[i]];
            }

            module.LED[currentLED].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[6];

            yield return new WaitForSeconds(0.5f);
        }
    }
}