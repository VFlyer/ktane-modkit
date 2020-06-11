using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class IndexedWires : Puzzle
{
    int[] colors = new int[3];
    List<int> cuts = new List<int>();
    int currentKey = 0;

    public IndexedWires(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Indexed Wires. Alphanumeric keys present: {1}.", moduleId, info.alphabet.Join(", "));
        SolveKeys();
        Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());
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

        cuts.Add(wire);
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

        if(alphabet < currentKey)
            return;

        if(alphabet != currentKey)
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly pressed alphanumeric key {1} when alphanumeric key {2} was expected.", moduleId, alphabet + 1, currentKey + 1);
            module.CauseStrike();
            module.RegenWires();
            Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());
            cuts = new List<int>();
            return;
        }
        bool isAllCorrect = true;
        for(int i = 0; i < info.wires.Length; i++)
        {
            int color1 = info.wires[i] / 10;
            int color2 = info.wires[i] % 10;

            if(cuts.Contains(i) && color1 != colors[currentKey] && color2 != colors[currentKey])
            {
                Debug.LogFormat("[The Modkit #{0}] Wire {2} was cut when it shoudn't have been.", moduleId, alphabet + 1, i + 1);
                isAllCorrect = false;
            }
            else if(!cuts.Contains(i) && (color1 == colors[currentKey] || color2 == colors[currentKey]))
            {
                Debug.LogFormat("[The Modkit #{0}] Wire {2} was not cut when it shoud have been.", moduleId, alphabet + 1, i + 1);
                isAllCorrect = false;
            }
        }

        if (!isAllCorrect)
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Upon pressing alphanumeric key {1}, not all wires were handled correctly!", moduleId, alphabet + 1);
            module.CauseStrike();
            module.RegenWires();
            Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());
            cuts = new List<int>();
            return;
        }


        Debug.LogFormat("[The Modkit #{0}] Correctly pressed alphanumeric key {1} with the wires handled correctly.", moduleId, alphabet + 1);
        module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
        currentKey++;

        if(currentKey == 3)
        {
            Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
            module.Solve();
            return;
        }

        module.RegenWires();
        Debug.LogFormat("[The Modkit #{0}] Wires present: {1}.", moduleId, info.GetWireNames());
        cuts = new List<int>();
    }

    void SolveKeys()
    {
        for(int i = 0; i < info.alphabet.Length; i++)
        {
            if(info.alphabet[i][0] < 'N')
            {
                if(new char[] { 'A', 'E', 'I', 'O', 'U'}.Contains(info.alphabet[i][0]))
                {
                    if((info.alphabet[i][1] - '0') % 2 == 0)
                        colors[i] = ComponentInfo.YELLOW;
                    else
                        colors[i] = ComponentInfo.RED;
                }
                else
                {
                    if(module.bomb.GetSerialNumberNumbers().Contains(info.alphabet[i][1] - '0'))
                        colors[i] = ComponentInfo.GREEN;
                    else
                        colors[i] = ComponentInfo.BLUE;
                }
            }
            else
            {
                if(module.bomb.GetSerialNumberLetters().Contains(info.alphabet[i][0]))
                {
                    if(new int[] { 2, 3, 5, 7 }.Contains(info.alphabet[i][1] - '0'))
                        colors[i] = ComponentInfo.ORANGE;
                    else
                        colors[i] = ComponentInfo.PURPLE;
                }
                else
                {
                    if(info.alphabet[i][1] - '0' > 4)
                        colors[i] = ComponentInfo.PURPLE;
                    else
                        colors[i] = ComponentInfo.RED;
                }
            }

            Debug.LogFormat("[The Modkit #{0}] Key {1} color is {2}.", moduleId, i + 1, ComponentInfo.COLORNAMES[colors[i]]);
        }
    }
}