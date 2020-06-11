using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class CruelWireSequence : Puzzle
{
    ComponentInfo[] pannels;

    int[] acc;
    List<int>[] cuts;
    List<int>[] toCut;
    int currentPannel = 0;

    public CruelWireSequence(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Cruel Wire Sequence. Alphanumeric keys present: {1}.", moduleId, info.alphabet.Join(", "));
        
        pannels = new ComponentInfo[] { info, new ComponentInfo(), new ComponentInfo() };

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
            module.CauseStrike();
            module.RegenWires();
            CalcSolution();
            return;
        }

        module.StartSolve();

        cuts[currentPannel].Add(wire);

        if(!toCut[currentPannel].Contains(wire))
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Incorrectly cutted wire {1} of panel {2}.", moduleId, wire + 1, currentPannel + 1);
            module.CauseStrike();
        }
        else
        {
            Debug.LogFormat("[The Modkit #{0}] Correctly cutted wire {1} of panel {2}.", moduleId, wire + 1, currentPannel + 1);
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

        if(arrow == ComponentInfo.UP)
        {
            if(currentPannel > 0)
            {
                currentPannel--;
                module.StartCoroutine(SwitchPannel());
            }
        }
        else if(arrow == ComponentInfo.DOWN)
        {
            if(toCut[currentPannel].Where(x => !cuts[currentPannel].Contains(x)).Count() != 0)
            {
                Debug.LogFormat("[The Modkit #{0}] Strike! Can't leave panel {1} because wires {2} still need to be cut.", moduleId, currentPannel + 1, toCut[currentPannel].Where(x => !cuts[currentPannel].Contains(x)).Select(x => x + 1).Join(", "));
                module.CauseStrike();
                return;
            }

            currentPannel++;

            if(currentPannel == 3)
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
                //module.StartCoroutine(module.HideComponent(0));
            }
            else
                module.StartCoroutine(SwitchPannel());
        }
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] First panel wires: {1}.", moduleId, info.GetWireNames());
        Debug.LogFormat("[The Modkit #{0}] Second panel wires: {1}.", moduleId, pannels[1].GetWireNames());
        Debug.LogFormat("[The Modkit #{0}] Third panel wires: {1}.", moduleId, pannels[2].GetWireNames());

        acc = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        cuts = new List<int>[] { new List<int>(), new List<int>(), new List<int>() };
        toCut = new List<int>[] { new List<int>(), new List<int>(), new List<int>() };

        String s = info.alphabet[0] + info.alphabet[1] + info.alphabet[2];

        for(int i = 0; i < pannels.Length; i++)
            for(int j = 0; j < pannels[i].wires.Length; j++)
            {
                int color1 = pannels[i].wires[j] / 10;
                int color2 = pannels[i].wires[j] % 10;

                if(CheckCut(color1, s) || CheckCut(color2, s))
                    toCut[i].Add(j);
                
                acc[color1]++;
                if(color1 != color2)
                    acc[color2]++;
            }

        Debug.LogFormat("[The Modkit #{0}] First panel wires that need to be cut: [ {1} ].", moduleId, toCut[0].Any() ? toCut[0].Select(x => x + 1).Join(", ") : "none");
        Debug.LogFormat("[The Modkit #{0}] Second panel wires that need to be cut: [ {1} ].", moduleId, toCut[1].Any() ? toCut[1].Select(x => x + 1).Join(", ") : "none");
        Debug.LogFormat("[The Modkit #{0}] Third panel wires that need to be cut: [ {1} ].", moduleId, toCut[2].Any() ? toCut[2].Select(x => x + 1).Join(", ") : "none");
    }

    bool CheckCut(int color, String s)
    {
        switch(color)
        {
            case 0:
            {
                switch(acc[color])
                {
                    case 0:
                    case 8: return s.Contains('D') || s.Contains('8') || s.Contains('9');
                    case 1:
                    case 9: return s.Contains('W') || s.Contains('Z') || s.Contains('0');
                    case 2:
                    case 10: return s.Contains('P') || s.Contains('W') || s.Contains('X');
                    case 3:
                    case 11: return s.Contains('C') || s.Contains('S') || s.Contains('Y');
                    case 4:
                    case 12: return s.Contains('B') || s.Contains('J') || s.Contains('0');
                    case 5:
                    case 13: return s.Contains('F') || s.Contains('O') || s.Contains('4');
                    case 6:
                    case 14: return s.Contains('D') || s.Contains('U') || s.Contains('6');
                    case 17: return s.Contains('L') || s.Contains('O') || s.Contains('0');
                }
                break;
            }
            case 1:
            {
                switch(acc[color])
                {
                    case 0:
                    case 8: return s.Contains('F') || s.Contains('2') || s.Contains('6');
                    case 1:
                    case 9: return s.Contains('A') || s.Contains('E') || s.Contains('G');
                    case 2:
                    case 10: return s.Contains('N') || s.Contains('O') || s.Contains('0');
                    case 3:
                    case 11: return s.Contains('E') || s.Contains('T') || s.Contains('1');
                    case 4:
                    case 12: return s.Contains('K') || s.Contains('R') || s.Contains('S');
                    case 5:
                    case 13: return s.Contains('T') || s.Contains('1') || s.Contains('3');
                    case 6:
                    case 14: return s.Contains('N') || s.Contains('R') || s.Contains('7');
                    case 17: return s.Contains('C') || s.Contains('P') || s.Contains('1');
                }
                break;
            }
            case 2:
            {
                switch(acc[color])
                {
                    case 0:
                    case 8: return s.Contains('H') || s.Contains('K') || s.Contains('Y');
                    case 1:
                    case 9: return s.Contains('L') || s.Contains('N') || s.Contains('5');
                    case 2:
                    case 10: return s.Contains('G') || s.Contains('K') || s.Contains('3');
                    case 3:
                    case 11: return s.Contains('D') || s.Contains('J') || s.Contains('Z');
                    case 4:
                    case 12: return s.Contains('I') || s.Contains('M') || s.Contains('U');
                    case 5:
                    case 13: return s.Contains('A') || s.Contains('V') || s.Contains('W');
                    case 6:
                    case 14: return s.Contains('H') || s.Contains('M') || s.Contains('V');
                    case 17: return s.Contains('K') || s.Contains('4') || s.Contains('8');
                }
                break;
            }
            case 3:
            {
                switch(acc[color])
                {
                    case 0:
                    case 8: return s.Contains('R') || s.Contains('U') || s.Contains('1');
                    case 1:
                    case 9: return s.Contains('X') || s.Contains('3') || s.Contains('7');
                    case 2:
                    case 10: return s.Contains('R') || s.Contains('5') || s.Contains('6');
                    case 3:
                    case 11: return s.Contains('A') || s.Contains('U') || s.Contains('2');
                    case 4:
                    case 12: return s.Contains('2') || s.Contains('6') || s.Contains('8');
                    case 5:
                    case 13: return s.Contains('N') || s.Contains('P') || s.Contains('Z');
                    case 6:
                    case 14: return s.Contains('A') || s.Contains('E') || s.Contains('5');
                    case 17: return s.Contains('F') || s.Contains('Q') || s.Contains('3');
                }
                break;
            }
            case 4:
            {
                switch(acc[color])
                {
                    case 0:
                    case 8: return s.Contains('C') || s.Contains('M') || s.Contains('S');
                    case 1:
                    case 9: return s.Contains('I') || s.Contains('P') || s.Contains('Q');
                    case 2:
                    case 10: return s.Contains('M') || s.Contains('Q') || s.Contains('7');
                    case 3:
                    case 11: return s.Contains('F') || s.Contains('I') || s.Contains('8');
                    case 4:
                    case 12: return s.Contains('C') || s.Contains('D') || s.Contains('Q');
                    case 5:
                    case 13: return s.Contains('E') || s.Contains('G') || s.Contains('H');
                    case 6:
                    case 14: return s.Contains('T') || s.Contains('X') || s.Contains('2');
                    case 17: return s.Contains('B') || s.Contains('J') || s.Contains('W');
                }
                break;
            }
            case 5:
            {
                switch(acc[color])
                {
                    case 0:
                    case 8: return s.Contains('J') || s.Contains('V') || s.Contains('4');
                    case 1:
                    case 9: return s.Contains('B') || s.Contains('O') || s.Contains('T');
                    case 2:
                    case 10: return s.Contains('B') || s.Contains('H') || s.Contains('4');
                    case 3:
                    case 11: return s.Contains('L') || s.Contains('V') || s.Contains('9');
                    case 4:
                    case 12: return s.Contains('L') || s.Contains('X') || s.Contains('5');
                    case 5:
                    case 13: return s.Contains('Y') || s.Contains('7') || s.Contains('9');
                    case 6:
                    case 14: return s.Contains('G') || s.Contains('Y') || s.Contains('9');
                    case 17: return s.Contains('I') || s.Contains('S') || s.Contains('Z');
                }
                break;
            }
        }
        
        return false;
    }

    IEnumerator SwitchPannel()
	{
		yield return module.HideComponent(0);

		for(int i = 0; i < pannels[currentPannel].wires.Length; i++)
		{
			int color1 = pannels[currentPannel].wires[i] / 10;
			int color2 = pannels[currentPannel].wires[i] % 10;

			if(color1 > color2)
			{
				color1 = color2;
				color2 = pannels[currentPannel].wires[i] / 10;
			}

			if(color1 != color2)
				module.wires[i].transform.GetComponentInChildren<Renderer>().material = module.wireMats.Where(x => x.name == ComponentInfo.COLORNAMES[color1] + "_" + ComponentInfo.COLORNAMES[color2]).ToArray()[0];
			else
				module.wires[i].transform.GetComponentInChildren<Renderer>().material = module.wireMats.Where(x => x.name == ComponentInfo.COLORNAMES[color1]).ToArray()[0];
		
            if(cuts[currentPannel].Contains(i))
            {
                module.wires[i].transform.Find("hl").gameObject.SetActive(true);
			    module.wires[i].GetComponent<MeshFilter>().mesh = module.wireCut;
            }
            else
            {
                module.wires[i].transform.Find("hl").gameObject.SetActive(true);
			    module.wires[i].GetComponent<MeshFilter>().mesh = module.wireWhole;
            }
		}

		yield return module.ShowComponent(0);
	}
}