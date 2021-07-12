using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class SimonShifts : Puzzle
{
    int[] sequence = new int[4];
    int stage = 0;
    bool flashing = false;

    List<int> expected;
    int input = 0;

    Coroutine flash;

    //indexation: color, direction
    int[][] colors = new int[][] {
        new int[] { ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.RED, ComponentInfo.RED, ComponentInfo.BLUE, ComponentInfo.RED },
        new int[] { ComponentInfo.YELLOW, ComponentInfo.GREEN, ComponentInfo.YELLOW, ComponentInfo.YELLOW },
        new int[] { ComponentInfo.GREEN, ComponentInfo.BLUE, ComponentInfo.BLUE, ComponentInfo.BLUE }
    };

    int[][] direction = new int[][] {
        new int[] { ComponentInfo.DOWN, ComponentInfo.LEFT, ComponentInfo.DOWN, ComponentInfo.LEFT },
        new int[] { ComponentInfo.RIGHT, ComponentInfo.UP, ComponentInfo.DOWN, ComponentInfo.LEFT },
        new int[] { ComponentInfo.UP, ComponentInfo.LEFT, ComponentInfo.DOWN, ComponentInfo.RIGHT },
        new int[] { ComponentInfo.UP, ComponentInfo.LEFT, ComponentInfo.LEFT, ComponentInfo.UP }
    };

    public SimonShifts(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Simon Shifts. Arrows: [Up: {1}, Right: {2}, Down: {3}, Left: {4}].", moduleId, ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.UP]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.RIGHT]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.DOWN]], ComponentInfo.COLORNAMES[info.arrows[ComponentInfo.LEFT]]);

        for(int i = 0; i < sequence.Length; i++)
            sequence[i] = rnd.Range(0, 4);
    }

    public override void OnArrowPress(int arrow)
    {
        if(module.IsAnimating())
            return;

        module.audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.arrows[arrow].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! The {1} arrow was pressed when the component selection was [ {2} ] instead of [ {3} ].", moduleId, ComponentInfo.DIRNAMES[arrow], module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }
        
        module.StartSolve();

        if(!flashing)
            return;

        if(flash != null)
            module.StopCoroutine(flash);
        for(int i = 0; i < module.arrows.Length; i++)
            module.arrows[i].transform.Find("light").gameObject.SetActive(false);

        if(stage % 2 == 0)
        {
            if(expected[input] == info.arrows[arrow])
            {
                module.StartCoroutine(FlashButton(arrow));
                input++;
                if(input == expected.Count())
                {
                    stage++;

                    if(stage == 4)
                    {
		                Debug.LogFormat("[The Modkit #{0}] Input is correct. Module solved.", moduleId, ComponentInfo.COLORNAMES[info.arrows[arrow]], stage + 1);
                        module.Solve();
                        return;
                    }

                    expected = new List<int>();
                    input = 0;
                    for(int i = 0; i <= stage; i++)
                        expected.Add(direction[info.arrows[sequence[i]]][sequence[i]]);
                    Debug.LogFormat("[The Modkit #{0}] Input is correct. Flashing: [ {1} ]. next inputs expected: [ {2} ]", moduleId, sequence.Take(stage + 1).Select(x => ComponentInfo.DIRNAMES[x]).Join(", "), expected.Select(x => ComponentInfo.DIRNAMES[x]).Join(", "));
                    flash = module.StartCoroutine(FlashSequence());
                }
            }
            else
            {
		        Debug.LogFormat("[The Modkit #{0}] Strike! Received {1} as input {2}. Resetting input.", moduleId, ComponentInfo.COLORNAMES[info.arrows[arrow]], stage + 1);
                module.CauseStrike();
                input = 0;
                flash = module.StartCoroutine(FlashSequence());
            }
        }
        else
        {
            if(expected[input] == arrow)
            {
                module.StartCoroutine(FlashButton(arrow));
                input++;
                if(input == expected.Count())
                {
                    stage++;

                    if(stage == 4)
                    {
		                Debug.LogFormat("[The Modkit #{0}] Input is correct. Module solved.", moduleId, ComponentInfo.COLORNAMES[info.arrows[arrow]], stage + 1);
                        module.Solve();
                        return;
                    }

                    expected = new List<int>();
                    input = 0;
                    for(int i = 0; i <= stage; i++)
                        expected.Add(colors[info.arrows[sequence[i]]][sequence[i]]);
                    Debug.LogFormat("[The Modkit #{0}] Input is correct. Flashing: [ {1} ]. Next inputs expected: [ {2} ]", moduleId, sequence.Take(stage + 1).Select(x => ComponentInfo.DIRNAMES[x]).Join(", "), expected.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
                    flash = module.StartCoroutine(FlashSequence());
                }
            }
            else
            {
		        Debug.LogFormat("[The Modkit #{0}] Strike! Received {1} as input {2}. Resetting input.", moduleId, ComponentInfo.COLORNAMES[info.arrows[arrow]], stage + 1);
                module.CauseStrike();
                input = 0;
                flash = module.StartCoroutine(FlashSequence());
            }
        }
    }

    public override void OnUtilityPress()
    {
        if(module.IsAnimating())
            return;

        module.audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
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

        if(!flashing)
        {
            Debug.LogFormat("[The Modkit #{0}] Pressed the ❖ button with the correct components. Starting flashes...", moduleId);
            
            expected = new List<int>();

            expected.Add(colors[info.arrows[sequence[0]]][sequence[0]]);
            Debug.LogFormat("[The Modkit #{0}] Flashing: [ {1} ]. Input expected: [ {2} ]", moduleId, sequence.Take(stage + 1).Select(x => ComponentInfo.DIRNAMES[x]).Join(", "), expected.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));
            
            flashing = true;
            flash = module.StartCoroutine(FlashSequence());
        }
    }

    void PlayColorSound(int sound)
    {
        string s = "";
        switch(sound)
        {
            case 0: s = "E"; break;
            case 1: s = "F#"; break;
            case 2: s = "G#"; break;
            case 3: s = "A"; break;
        }

        module.audioSelf.PlaySoundAtTransform(s, module.transform);
    }

    IEnumerator FlashButton(int btn)
    {
        module.arrows[btn].transform.Find("light").gameObject.SetActive(true);
        PlayColorSound(info.arrows[btn]);
        yield return new WaitForSeconds(0.3f);
        module.arrows[btn].transform.Find("light").gameObject.SetActive(false);
    }

    IEnumerator FlashSequence()
    {
        yield return new WaitForSeconds(0.7f);

        while(!module.IsSolved())
        {
            for(int i = 0; i <= stage; i++)
            {
                module.arrows[sequence[i]].transform.Find("light").gameObject.SetActive(true);
                PlayColorSound(info.arrows[sequence[i]]);
                yield return new WaitForSeconds(0.7f);
                module.arrows[sequence[i]].transform.Find("light").gameObject.SetActive(false);
                yield return new WaitForSeconds(0.7f);
            }
            yield return new WaitForSeconds(0.7f);
        }
    }
}