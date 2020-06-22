using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class AlphanumericOrder : Puzzle
{
    readonly string alphabetical = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    readonly string keyboard = "QWERTYUIOPASDFGHJKLZXCVBNM";
    readonly string homotopical = "ADOPQRBCEFGHIJKLMNSTUVWXZY";

    readonly string numerical = "0123456789";
    readonly string evensFirst = "0246813579";
    readonly string oddsFirst = "1357902468";

    string order;

    bool light = false;

    List<int> onArrows = new List<int>();
    List<int> presses = new List<int>();
    List<int> pressed = new List<int>();

    public AlphanumericOrder(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Alphanumeric Order. Alphanumeric keys present: {1}.", moduleId, info.alphabet.Join(", "));

        for(int i = 0; i < 4; i++)
            if(rnd.Range(0, 2) == 1)
                onArrows.Add(i);

        Debug.LogFormat("[The Modkit #{0}] Lighten up arrows: [ {1} ].", moduleId, !onArrows.Any() ? "none" : onArrows.Select(x => ComponentInfo.COLORNAMES[x]).Join(", "));

        CalcOrder();

        for(int i = 0; i < order.Length; i++)
            for(int j = 0; j < info.alphabet.Length; j++)
                if(info.alphabet[j][0] == order[i] || info.alphabet[j][1] == order[i])
                    presses.Add(j);

        Debug.LogFormat("[The Modkit #{0}] Button press order: [ {1} ].", moduleId, presses.Select(x => x + 1).Join(", "));
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

        if(pressed.Contains(alphabet))
            return;

        pressed.Add(alphabet);
        Debug.LogFormat("[The Modkit #{0}] Pressed alphanumeric key {1}.", moduleId, alphabet + 1);
        module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[3];

        if(pressed.Count == 3)
        {
            for(int i = 0; i < presses.Count; i++)
                if(pressed[i] != presses[i])
                {
                    Debug.LogFormat("[The Modkit #{0}] Strike! Incorrect input received: [ {1} ].", moduleId, pressed.Select(x => x + 1).Join(", "));
                    module.CauseStrike();
                    pressed.Clear();
                    foreach(GameObject k in module.alphabet)
                        k.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
                    return;
                }

            Debug.LogFormat("[The Modkit #{0}] Correct input received: [ {1} ]. Module solved.", moduleId, pressed.Select(x => x + 1).Join(", "));
            foreach (GameObject k in module.alphabet)
                k.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
            for (int i = 0; i < info.arrows.Length; i++)
            {
                module.arrows[i].transform.Find("light").gameObject.SetActive(false);
            }
            module.Solve();
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

        if(!light)
        {
            light = true;
            module.StartCoroutine(HandleArrowFlash());
        }
    }

    public IEnumerator HandleArrowFlash()
    {
        int[] flashOrder = rnd.Range(0, 2) == 0 ? new int[] { ComponentInfo.UP, ComponentInfo.RIGHT, ComponentInfo.DOWN, ComponentInfo.LEFT } : new int[] { ComponentInfo.UP, ComponentInfo.LEFT, ComponentInfo.DOWN, ComponentInfo.RIGHT };
        for (int i = 0; i < info.arrows.Length; i++)
        {

            for (int x = 0; x < info.arrows.Length; x++)
            {
                module.arrows[x].transform.Find("light").gameObject.SetActive(flashOrder[i] == x);
            }
            yield return new WaitForSeconds(0.2f);
        }
        for (int i = 0; i < info.arrows.Length; i++)
        {
            module.arrows[i].transform.Find("light").gameObject.SetActive(onArrows.Contains(info.arrows[i]));
        }
        yield return null;
    }

    void CalcOrder()
    {
        int diagram = (onArrows.Contains(ComponentInfo.RED) ? 1000 : 0) +
                      (onArrows.Contains(ComponentInfo.GREEN) ? 100 : 0) +
                      (onArrows.Contains(ComponentInfo.BLUE) ? 10 : 0) +
                      (onArrows.Contains(ComponentInfo.YELLOW) ? 1 : 0);

        switch(diagram)
        {
            case 1111:
            case 0:
            {
                char[] temp = evensFirst.ToCharArray();
                Array.Reverse(temp);
                order = new string(temp);
                Debug.LogFormat("[The Modkit #{0}] Order is Reverse Evens First.", moduleId);
                break;
            }
            case 1011:
            case 1:
            {
                order = alphabetical;
                Debug.LogFormat("[The Modkit #{0}] Order is Alphabetical.", moduleId);
                break;
            }
            case 10:
            {
                char[] temp = numerical.ToCharArray();
                Array.Reverse(temp);
                order = new string(temp);
                Debug.LogFormat("[The Modkit #{0}] Order is Reverse Numerical.", moduleId);
                break;
            }
            case 1100:
            case 100:
            {
                order = keyboard;
                Debug.LogFormat("[The Modkit #{0}] Order is Keyboard.", moduleId);
                break;
            }
            case 1010:
            case 1000:
            {
                order = numerical;
                Debug.LogFormat("[The Modkit #{0}] Order is Numerical.", moduleId);
                break;
            }
            case 1001:
            {
                order = evensFirst;
                Debug.LogFormat("[The Modkit #{0}] Order is Evens First.", moduleId);
                break;
            }
            case 110:
            {
                order = homotopical;
                Debug.LogFormat("[The Modkit #{0}] Order is Homotopical.", moduleId);
                break;
            }
            case 101:
            {
                order = oddsFirst;
                Debug.LogFormat("[The Modkit #{0}] Order is Odds First.", moduleId);
                break;
            }
            case 11:
            {
                char[] temp = alphabetical.ToCharArray();
                Array.Reverse(temp);
                order = new string(temp);
                Debug.LogFormat("[The Modkit #{0}] Order is Reverse Alphabetical.", moduleId);
                break;
            }
            case 1110:
            {
                char[] temp = homotopical.ToCharArray();
                Array.Reverse(temp);
                order = new string(temp);
                Debug.LogFormat("[The Modkit #{0}] Order is Reverse Homotopical.", moduleId);
                break;
            }
            case 1101:
            {
                char[] temp = keyboard.ToCharArray();
                Array.Reverse(temp);
                order = new string(temp);
                Debug.LogFormat("[The Modkit #{0}] Order is Reverse Keyboard.", moduleId);
                break;
            }
            case 111:
            {
                char[] temp = oddsFirst.ToCharArray();
                Array.Reverse(temp);
                order = new string(temp);
                Debug.LogFormat("[The Modkit #{0}] Order is Reverse Odds First.", moduleId);
                break;
            }
        }
    }
}