using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class ParanormalWires : Puzzle
{
    int toCut;
    List<int> cut = new List<int>();
    int[] options = { 00, 01, 02, 10, 11, 12, 20, 21, 22, 30, 31, 32, 40, 41, 42, 50, 51, 52, 60, 61, 62, 63, 70, 71, 72, 73 };

    int[] symbolLights = new int[3];
    int[] alphabetLights = new int[3];
    int litCnt = 0;

    public ParanormalWires(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Paranormal Wires. Symbols present are: {1}. Alphanumeric keys present are: {2}. LEDs are: {3}. Arrows are [Up: {4}, Right: {5}, Down: {6}, Left: {7}]", moduleId, info.GetSymbols(), info.alphabet.Join(", "), info.LED.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "), ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.UP]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.RIGHT]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.DOWN]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.LEFT]]);
        Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());
    
        options = options.OrderBy(x => rnd.Range(0, 10000)).ToArray();
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
            Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());
            return;
        }

        module.StartSolve();

        if(cut.Count == 0 || wire == toCut)
        {
            Debug.LogFormat("[The Modkit #{0}] Cut wire {1}.", moduleId, wire + 1);
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Cut wire {1}.", moduleId, wire + 1);
            module.CauseStrike();
        }

        int op = options[cut.Count];
        cut.Add(wire);
        cut = cut.Distinct().ToList();
        if(cut.Count == 5)
        {
            Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
            module.StopAllCoroutines();
            module.Solve();
            return;
        }

        ParanormalActivity(op);
        CalcSolution(op);
    }

    void ParanormalActivity(int op)
    {
        int temp;
        string stemp;

        if(op / 10 == 0)
        {
            temp = info.symbols[op];
            do{ info.symbols[op] = rnd.Range(0, 31); } while(temp == info.symbols[op]);
            module.symbols[op].transform.Find("symbol").GetComponentInChildren<Renderer>().material = module.symbolMats[info.symbols[op]];
            Debug.LogFormat("[The Modkit #{0}] Symbol {1} is now {2}.", moduleId, (op % 10) + 1, ComponentInfo.SYMBOLCHARS[info.symbols[op % 10]]);
        }
        else if (op / 10 == 1)
        {
            symbolLights[op % 10] = rnd.Range(0, 6);
            module.symbols[op % 10].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[symbolLights[op % 10]];
            Debug.LogFormat("[The Modkit #{0}] Symbol {1}'s light is now {2}.", moduleId, (op % 10) + 1, ComponentInfo.COLORNAMES[symbolLights[op % 10]]);
        }
        else if(op / 10 == 2)
        {
            string[] letters = "QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray().Select(x => x.ToString()).OrderBy(x => rnd.Range(0, 1000)).ToArray();
            string[] numbers = "1234567890".ToCharArray().Select(x => x.ToString()).OrderBy(x => rnd.Range(0, 1000)).ToArray();

            stemp = info.alphabet[op % 10];
            do{ 
                info.alphabet[op % 10] = letters[0] + numbers[0]; 
                letters = "QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray().Select(x => x.ToString()).OrderBy(x => rnd.Range(0, 1000)).ToArray();
                numbers = "1234567890".ToCharArray().Select(x => x.ToString()).OrderBy(x => rnd.Range(0, 1000)).ToArray();
            } while(stemp == info.alphabet[op % 10]);
			module.alphabet[op % 10].transform.Find("ButtonText").GetComponentInChildren<TextMesh>().text = info.alphabet[op % 10];
            Debug.LogFormat("[The Modkit #{0}] Alphanumeric key {1} is now {2}.", moduleId, (op % 10) + 1, info.alphabet[op % 10]);
        }
        else if (op / 10 == 3)
        {
            alphabetLights[op % 10] = rnd.Range(0, 6);
            module.alphabet[op % 10].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[alphabetLights[op % 10]];
            Debug.LogFormat("[The Modkit #{0}] Alphanumeric key {1}'s light is now {2}.", moduleId, (op % 10) + 1, ComponentInfo.COLORNAMES[alphabetLights[op % 10]]);
        }
        else if(op / 10 == 4)
        {
            temp = info.LED[op % 10];
            do{ info.LED[op % 10] = rnd.Range(0, 6); } while(temp == info.LED[op % 10]);
			module.LED[op % 10].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[op % 10]];
            Debug.LogFormat("[The Modkit #{0}] LED {1} is now {2}.", moduleId, (op % 10) + 1, ComponentInfo.COLORNAMES[info.LED[op % 10]]);
        }
        else if(op / 10 == 5)
        {
            module.StartCoroutine(FlickerLight(op % 10));
            Debug.LogFormat("[The Modkit #{0}] LED {1} is now flickering.", moduleId, (op % 10) + 1);
        }
        else if (op / 10 == 6)
        {
            temp = info.arrows[op % 10];
            do{ info.arrows[op % 10] = rnd.Range(0, 4); } while(temp == info.arrows[op % 10]);
            module.arrows[op % 10].GetComponentInChildren<Renderer>().material = module.arrowMats[info.arrows[op % 10]];
    		module.arrows[op % 10].transform.Find("light").GetComponentInChildren<Light>().color = ComponentInfo.LIGHTCOLORS[info.arrows[op % 10]];
            Debug.LogFormat("[The Modkit #{0}] {1} arrow is now {2}.", moduleId, ComponentInfo.DIRNAMES[op % 10], ComponentInfo.COLORNAMES[info.arrows[op % 10]]);
        }
        else
        {
            module.arrows[op % 10].transform.Find("light").gameObject.SetActive(true);
            litCnt++;
            Debug.LogFormat("[The Modkit #{0}] {1} arrow now lit.", moduleId, ComponentInfo.DIRNAMES[op % 10]);
        }
    }

    void CalcSolution(int op)
    {
        if(op / 10 == 0)
        {
            if(info.symbols.Distinct().Count() < 3)
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.RED || color2 == ComponentInfo.RED)
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }
                }

                DefaultWire();
            }
            else if(op % 10 == 0)
            {
                int last = -1;
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.WHITE || color2 == ComponentInfo.WHITE)
                        last = i;
                }

                if(last != -1)
                {
                    toCut = last;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
            else
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == color2)
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }

                    DefaultWire();
                }
            }
        }
        else if(op / 10 == 1)
        {
            if(symbolLights[op % 10] == ComponentInfo.GREEN)
            {
                for(int i = 2; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.BLUE || color2 == ComponentInfo.BLUE)
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }
                }

                DefaultWire();
            }
            else if(op % 10 == 1)
            {
                int last = -1;
                int secondToLast = -1;
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.YELLOW || color2 == ComponentInfo.YELLOW)
                    {
                        secondToLast = last;
                        last = i;
                    }    
                }

                if(secondToLast != -1)
                {
                    toCut = secondToLast;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
            else
            {
                int cnt = 0;
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.ORANGE || color2 == ComponentInfo.ORANGE)
                    {
                        cnt++;
                        if(cnt == 3)
                        {
                            toCut = i;
                            Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                            return;
                        }
                    }    
                }

                DefaultWire();
            }
        }
        else if(op / 10 == 2)
        {
            if("AEIOU".Contains(info.alphabet[op % 10]))
            {
                int last = -1;
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 != color2)
                        last = i;
                }

                if(last != -1)
                {
                    toCut = last;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
            else if(module.bomb.GetSerialNumberNumbers().Contains(info.alphabet[op % 10][1] - '0'))
            {
                if(!cut.Contains(3) && (info.wires[3] / 10 == ComponentInfo.GREEN || info.wires[3] % 10 == ComponentInfo.GREEN))
                {
                    toCut = 3;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                if(!cut.Contains(1) && (info.wires[1] / 10 == ComponentInfo.GREEN || info.wires[1] % 10 == ComponentInfo.GREEN))
                {
                    toCut = 1;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
            else
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if((color1 == ComponentInfo.BLUE && color2 == ComponentInfo.PURPLE) || (color2 == ComponentInfo.BLUE && color1 == ComponentInfo.PURPLE))
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }    
                }

                DefaultWire();
            }
        }
        else if(op / 10 == 3)
        {
            if(alphabetLights[op % 10] == ComponentInfo.RED)
            {
                if(!cut.Contains(4) && (info.wires[4] / 10 != ComponentInfo.WHITE && info.wires[4] % 10 != ComponentInfo.WHITE))
                {
                    toCut = 4;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                if(!cut.Contains(2) && (info.wires[2] / 10 != ComponentInfo.WHITE && info.wires[2] % 10 != ComponentInfo.WHITE))
                {
                    toCut = 2;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                if(!cut.Contains(0) && (info.wires[0] / 10 != ComponentInfo.WHITE && info.wires[0] % 10 != ComponentInfo.WHITE))
                {
                    toCut = 0;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
            else if(op % 10 == 2)
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if((color1 != ComponentInfo.RED && color2 != ComponentInfo.RED) && (color2 != ComponentInfo.ORANGE && color1 != ComponentInfo.ORANGE))
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }    
                }

                DefaultWire();
            }
            else
            {
                bool found = false;
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.BLUE || color2 == ComponentInfo.BLUE)
                    {
                        if(found)
                        {
                            toCut = i;
                            Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                            return;
                        }
                        else
                            found = true;
                    }
                }
                        
                DefaultWire();
            }
        }
        else if(op / 10 == 4)
        {
            if(info.LED[op % 10] == ComponentInfo.RED || info.LED[op % 10] == ComponentInfo.YELLOW)
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if((color1 == ComponentInfo.RED && color2 == ComponentInfo.YELLOW) || (color2 == ComponentInfo.RED && color1 == ComponentInfo.YELLOW))
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }    
                }

                DefaultWire();
            }
            else if(info.LED.Distinct().Count() == 1)
            {
                if(!cut.Contains(1))
                {
                    toCut = 1;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                if(!cut.Contains(3))
                {
                    toCut = 3;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
            else
            {
                int last = -1;
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.YELLOW || color2 == ComponentInfo.YELLOW)
                        last = i;
                }

                if(last != -1)
                {
                    toCut = last;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
        }
        else if(op / 10 == 5)
        {
            if(info.LED[op % 10] == ComponentInfo.GREEN)
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if((color1 == ComponentInfo.GREEN || color2 == ComponentInfo.GREEN) && (color1 != ComponentInfo.ORANGE && color2 != ComponentInfo.ORANGE))
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }    
                }

                DefaultWire();
            }
            else if(op % 10 == 2)
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 != ComponentInfo.RED && color2 != ComponentInfo.RED)
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }
                }
                        
                DefaultWire();
            }
            else
            {
                bool found = false;
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.WHITE || color2 == ComponentInfo.WHITE)
                    {
                        if(found)
                        {
                            toCut = i;
                            Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                            return;
                        }
                        else
                            found = true;
                    }
                }
                        
                DefaultWire();
            }
        }
        else if(op / 10 == 6)
        {
            if(info.arrows[op % 10] == ComponentInfo.YELLOW)
            {
                if(!cut.Contains(1) && (info.wires[1] / 10 == ComponentInfo.RED || info.wires[1] % 10 == ComponentInfo.RED))
                {
                    toCut = 1;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                if(!cut.Contains(2) && (info.wires[2] / 10 == ComponentInfo.RED || info.wires[2] % 10 == ComponentInfo.RED))
                {
                    toCut = 2;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                if(!cut.Contains(4) && (info.wires[4] / 10 == ComponentInfo.RED || info.wires[4] % 10 == ComponentInfo.RED))
                {
                    toCut = 4;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
            else if(info.AreArrowsAdjacent(ComponentInfo.BLUE, ComponentInfo.BLUE) || info.AreArrowsAdjacent(ComponentInfo.RED, ComponentInfo.RED) || info.AreArrowsAdjacent(ComponentInfo.GREEN, ComponentInfo.GREEN) || info.AreArrowsAdjacent(ComponentInfo.YELLOW, ComponentInfo.YELLOW))
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == ComponentInfo.PURPLE || color2 == ComponentInfo.PURPLE)
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }
                }

                DefaultWire();
            }
            else
            {
                if(!cut.Contains(1) && (info.wires[1] / 10 == ComponentInfo.RED || info.wires[1] % 10 == ComponentInfo.RED))
                {
                    toCut = 1;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                if(!cut.Contains(4) && (info.wires[4] / 10 == ComponentInfo.RED || info.wires[4] % 10 == ComponentInfo.RED))
                {
                    toCut = 4;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
        }
        else
        {
            if(op % 10 == ComponentInfo.UP)
            {
                int last = -1;
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if(color1 == color2)
                        last = i;
                }

                if(last != -1)
                {
                    toCut = last;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }

                DefaultWire();
            }
            else if(litCnt >= 2)
            {
                for(int i = 0; i < info.wires.Length; i++)
                {
                    if(cut.Contains(i))
                        continue;

                    int color1 = info.wires[i] / 10;
                    int color2 = info.wires[i] % 10;

                    if((color1 == ComponentInfo.BLUE && color2 == ComponentInfo.WHITE) || (color2 == ComponentInfo.BLUE && color1 == ComponentInfo.WHITE))
                    {
                        toCut = i;
                        Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                        return;
                    }    
                }

                DefaultWire();
            }
            else
            {
                if(!cut.Contains(3))
                {
                    toCut = 3;
                    Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                    return;
                }
            }
        }
    }

    void DefaultWire()
    {
        for(int i = 0; i < info.wires.Length; i++)
        {
            if(!cut.Contains(i))
            {
                toCut = i;
                Debug.LogFormat("[The Modkit #{0}] Wire {1} must be cut next.", moduleId, toCut + 1);
                return;
            }
        }
    }

    IEnumerator FlickerLight(int n)
    {
        while(true)
        {
			module.LED[n].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[6];
            yield return new WaitForSeconds(rnd.Range(0.05f, 0.3f));
            module.LED[n].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[n]];
            yield return new WaitForSeconds(rnd.Range(0.05f, 0.3f));
        }
    }
}