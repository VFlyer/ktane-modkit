using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Puzzle
{
    protected Modkit module;
    protected int moduleId;
    protected ComponentInfo info;
    protected bool vanilla;

    public Puzzle(Modkit module, int moduleId, ComponentInfo info, bool vanilla = false)
    {
        this.module = module;
        this.moduleId = moduleId;
        this.info = info;
        this.vanilla = vanilla;
    }
    public virtual void BruteForceTest()
    {

    }
    public virtual void OnWireCut(int wire)
    {
        if(module.IsAnimating())
            return;

        module.audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, module.transform);
		module.CutWire(wire);

        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Wire {1} was cut when the component selection was [ {2} ] instead of [ {3} ].", moduleId, wire + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            module.RegenWires();
            return;
        }

        module.StartSolve();
    }

    public virtual void OnSymbolPress(int symbol)
    {
        if(module.IsAnimating())
            return;

        module.audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.symbols[symbol].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Symbol {1} was pressed when the component selection was [ {2} ] instead of [ {3} ].", moduleId, symbol + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }

        module.StartSolve();
    }

    public virtual void OnAlphabetPress(int alphabet)
    {
        if(module.IsAnimating())
            return;

        module.audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.alphabet[alphabet].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Alphanumeric key {1} was pressed when the component selection was [ {2} ] instead of [ {3} ].", moduleId, alphabet + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }

        module.StartSolve();
    }

    public virtual void OnArrowPress(int arrow)
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
    }

    public virtual void OnUtilityPress()
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

        if(vanilla)
        {
            Debug.LogFormat("[The Modkit #{0}] Pressed the ❖ button when no components were valid. Module solved.", moduleId);
            module.Solve();
        }

        module.StartSolve();
    }
    public IEnumerator HandleArrowDelayFlash()
    {
        yield return null;
        for (int i = 0; i < 4; i++)
        {
            module.arrows[i].transform.Find("light").gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 4; i++)
        {
            module.arrows[i].transform.Find("light").gameObject.SetActive(false);
        }
    }

    public IEnumerator HandleArrowDelayFlashSingle(int num)
    {
        if (num < 0 || num >= 4) yield break;
        yield return null;
        module.arrows[num].transform.Find("light").gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        module.arrows[num].transform.Find("light").gameObject.SetActive(false);
    }
    public IEnumerator currentFlashing;
}