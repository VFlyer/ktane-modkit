using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class TheLastInLine : Puzzle
{
    Dictionary<int, string[]> rules = new Dictionary<int, string[]>();
    Dictionary<int, int> directions = new Dictionary<int, int>();

    List<int> pressed = new List<int>();
    int arrow = -1;
    string lastPress = null;

    public TheLastInLine(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving The Last In Line. Symbols present are: {1}. Alphanumeric keys present are: {2}.", moduleId, info.GetSymbols(), info.alphabet.Join(", "));
        
        string snLetters = module.bomb.GetSerialNumberLetters().Join("");
        string snDigits = module.bomb.GetSerialNumberNumbers().Join("");

        rules.Add(00, new string[] { "02468", snDigits });
        rules.Add(11, new string[] { snLetters, "13579" });
        rules.Add(22, new string[] { "**", snDigits });
        rules.Add(33, new string[] { snLetters, "02468" });
        rules.Add(44, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", "02468" });
        rules.Add(55, new string[] { "**", "AEIOU" });
        rules.Add(66, new string[] { snDigits, "**" });
        rules.Add(01, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", "13579" });
        rules.Add(10, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", "13579" });
        rules.Add(02, new string[] { "13579", "AEIOU" });
        rules.Add(20, new string[] { "13579", "AEIOU" });
        rules.Add(03, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", "**" });
        rules.Add(30, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", "**" });
        rules.Add(04, new string[] { "AEIOU", snDigits });
        rules.Add(40, new string[] { "AEIOU", snDigits });
        rules.Add(05, new string[] { "**", "02468" });
        rules.Add(50, new string[] { "**", "02468" });
        rules.Add(06, new string[] { "13579", "**" });
        rules.Add(60, new string[] { "13579", "**" });
        rules.Add(12, new string[] { "**", "13579" });
        rules.Add(21, new string[] { "**", "13579" });
        rules.Add(13, new string[] { "02468", "**" });
        rules.Add(31, new string[] { "02468", "**" });
        rules.Add(14, new string[] { "**", snLetters });
        rules.Add(41, new string[] { "**", snLetters });
        rules.Add(15, new string[] { "AEIOU", "13579" });
        rules.Add(51, new string[] { "AEIOU", "13579" });
        rules.Add(16, new string[] { "13579", "02468" });
        rules.Add(61, new string[] { "13579", "02468" });
        rules.Add(23, new string[] { "02468", "AEIOU" });
        rules.Add(32, new string[] { "02468", "AEIOU" });
        rules.Add(24, new string[] { "02468", "13579" });
        rules.Add(42, new string[] { "02468", "13579" });
        rules.Add(25, new string[] { snLetters, "AEIOU" });
        rules.Add(52, new string[] { snLetters, "AEIOU" });
        rules.Add(26, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", snLetters });
        rules.Add(62, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", snLetters });
        rules.Add(34, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", "AEIOU" });
        rules.Add(43, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", "AEIOU" });
        rules.Add(35, new string[] { "13579", snDigits });
        rules.Add(53, new string[] { "13579", snDigits });
        rules.Add(36, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", snDigits });
        rules.Add(63, new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM1234567890", snDigits });
        rules.Add(45, new string[] { "**", "**" });
        rules.Add(54, new string[] { "**", "**" });
        rules.Add(46, new string[] { "**", "QWERTYUIOPASDFGHJKLZXCVBNM1234567890" });
        rules.Add(64, new string[] { "**", "QWERTYUIOPASDFGHJKLZXCVBNM1234567890" });
        rules.Add(56, new string[] { "AEIOU", "02468" });
        rules.Add(65, new string[] { "AEIOU", "02468" });
        
        directions.Add(00, ComponentInfo.DOWN);
        directions.Add(11, ComponentInfo.LEFT);
        directions.Add(22, ComponentInfo.RIGHT);
        directions.Add(33, ComponentInfo.UP);
        directions.Add(44, ComponentInfo.LEFT);
        directions.Add(55, ComponentInfo.DOWN);
        directions.Add(66, ComponentInfo.RIGHT);
        directions.Add(01, ComponentInfo.UP);
        directions.Add(10, ComponentInfo.UP);
        directions.Add(02, ComponentInfo.UP);
        directions.Add(20, ComponentInfo.UP);
        directions.Add(03, ComponentInfo.RIGHT);
        directions.Add(30, ComponentInfo.RIGHT);
        directions.Add(04, ComponentInfo.LEFT);
        directions.Add(40, ComponentInfo.LEFT);
        directions.Add(05, ComponentInfo.DOWN);
        directions.Add(50, ComponentInfo.DOWN);
        directions.Add(06, ComponentInfo.RIGHT);
        directions.Add(60, ComponentInfo.RIGHT);
        directions.Add(12, ComponentInfo.UP);
        directions.Add(21, ComponentInfo.UP);
        directions.Add(13, ComponentInfo.LEFT);
        directions.Add(31, ComponentInfo.LEFT);
        directions.Add(14, ComponentInfo.DOWN);
        directions.Add(41, ComponentInfo.DOWN);
        directions.Add(15, ComponentInfo.DOWN);
        directions.Add(51, ComponentInfo.DOWN);
        directions.Add(16, ComponentInfo.UP);
        directions.Add(61, ComponentInfo.UP);
        directions.Add(23, ComponentInfo.RIGHT);
        directions.Add(32, ComponentInfo.RIGHT);
        directions.Add(24, ComponentInfo.LEFT);
        directions.Add(42, ComponentInfo.LEFT);
        directions.Add(25, ComponentInfo.RIGHT);
        directions.Add(52, ComponentInfo.RIGHT);
        directions.Add(26, ComponentInfo.LEFT);
        directions.Add(62, ComponentInfo.LEFT);
        directions.Add(34, ComponentInfo.UP);
        directions.Add(43, ComponentInfo.UP);
        directions.Add(35, ComponentInfo.DOWN);
        directions.Add(53, ComponentInfo.DOWN);
        directions.Add(36, ComponentInfo.LEFT);
        directions.Add(63, ComponentInfo.LEFT);
        directions.Add(45, ComponentInfo.RIGHT);
        directions.Add(54, ComponentInfo.RIGHT);
        directions.Add(46, ComponentInfo.UP);
        directions.Add(64, ComponentInfo.UP);
        directions.Add(56, ComponentInfo.DOWN);
        directions.Add(65, ComponentInfo.DOWN);

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
            return;
        }

        module.StartSolve();
    }

    public override void OnSymbolPress(int symbol)
    {
        if(module.IsAnimating())
            return;

        module.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, module.transform);
        module.symbols[symbol].GetComponentInChildren<KMSelectable>().AddInteractionPunch(0.5f);
    
        if(module.IsSolved())
            return;

        if(!module.CheckValidComponents())
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed symbol {1} when component selection was [ {2} ] instead of [ {3} ].", moduleId, symbol + 1, module.GetOnComponents(), module.GetTargetComponents());
            module.CauseStrike();
            return;
        }

        module.StartSolve();

        if(pressed.Contains(symbol))
            return;

        if(lastPress == null)
        {
            if(symbol == 0)
            {
		        Debug.LogFormat("[The Modkit #{0}] Pressed symbol {1}.", moduleId, symbol + 1);
                lastPress = "**";
                pressed.Add(0);
                module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
            }
            else
            {
		        Debug.LogFormat("[The Modkit #{0}] Strike! Pressed symbol {1}. Symbol 1 must be pressed first.", moduleId, symbol + 1);
                module.CauseStrike();
            }
            return;
        }

        if(arrow == -1)
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! No arrow was pressed between key presses.", moduleId);
            module.CauseStrike();
            return;
        }

        for(int i = 0; i < info.wires.Length; i++)
        {
            String[] r;
            rules.TryGetValue(info.wires[i], out r);

            if(!r[0].Contains(lastPress[0]) && !r[0].Contains(lastPress[1]))
                continue;
            if(!r[1].Contains('*'))
                continue;

            int a;
            directions.TryGetValue(info.wires[i], out a);

            if(a != arrow)
                continue;

            Debug.LogFormat("[The Modkit #{0}] Successfully went from {1} to {2} using {3}.", moduleId, lastPress == "**" ? "symbol" : lastPress, "symbol", ComponentInfo.DIRNAMES[arrow]);
            pressed.Add(symbol);
            module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
            if(pressed.Count == 6)
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
            arrow = -1;
            lastPress = "**";
            return;
        }

        Debug.LogFormat("[The Modkit #{0}] Strike! No rule allows to move from {1} to {2} using the {3} arrow.", moduleId, lastPress == "**" ? "symbol" : lastPress, "symbol", ComponentInfo.DIRNAMES[arrow]);
        arrow = -1;
        module.CauseStrike();
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

        if(pressed.Contains(alphabet + 3))
            return;

        if(lastPress == null)
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! Pressed alphanumeric key {1}. Symbol 1 must be pressed first.", moduleId, alphabet + 1);
            module.CauseStrike();
            return;
        }

        if(arrow == -1)
        {
            Debug.LogFormat("[The Modkit #{0}] Strike! No arrow was pressed between key presses.", moduleId);
            module.CauseStrike();
            return;
        }

        for(int i = 0; i < info.wires.Length; i++)
        {
            String[] r;
            rules.TryGetValue(info.wires[i], out r);

            if(!r[0].Contains(lastPress[0]) && !r[0].Contains(lastPress[1]))
                continue;
            if(!r[1].Contains(info.alphabet[alphabet][0]) && !r[1].Contains(info.alphabet[alphabet][1]))
                continue;

            int a;
            directions.TryGetValue(info.wires[i], out a);

            if(a != arrow)
                continue;

            Debug.LogFormat("[The Modkit #{0}] Successfully went from {1} to {2} using {3}.", moduleId, lastPress == "**" ? "symbol" : lastPress, info.alphabet[alphabet], ComponentInfo.DIRNAMES[arrow]);
            pressed.Add(alphabet + 3);
            module.alphabet[alphabet].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[0];
            if(pressed.Count == 6)
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                module.Solve();
            }
            arrow = -1;
            lastPress = info.alphabet[alphabet];
            return;
        }

        Debug.LogFormat("[The Modkit #{0}] Strike! No rule allows to move from {1} to {2} using the {3} arrow.", moduleId, lastPress == "**" ? "symbol" : lastPress, info.alphabet[alphabet], ComponentInfo.DIRNAMES[arrow]);
        arrow = -1;
        module.CauseStrike();
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

        if(this.arrow != -1)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed another arrow before pressing a key.", moduleId);
            module.CauseStrike();
            return;
        }

        Debug.LogFormat("[The Modkit #{0}] Pressed the {1} arrow.", moduleId, ComponentInfo.DIRNAMES[arrow]);
        this.arrow = arrow;
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

        if(lastPress == null)
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed the ❖ button when there was at least one valid move.", moduleId);
            module.CauseStrike();
            return;
        }

        for(int i = 0; i < info.wires.Length; i++)
        {
            String[] r;
            rules.TryGetValue(info.wires[i], out r);

            if(!r[0].Contains(lastPress[0]) && !r[0].Contains(lastPress[1]))
                continue;

            if(r[1].Contains('*') && !(pressed.Exists(x => x == 0) && pressed.Exists(x => x == 1) && pressed.Exists(x => x == 0)))
            {
                Debug.LogFormat("[The Modkit #{0}] Strike! Pressed the ❖ button when there was at least one valid move.", moduleId);
                module.CauseStrike();
                return;
            }

            for(int j = 0; j < info.alphabet.Length; j++)
            {
                if(pressed.Contains(j + 3))
                    continue;

                if(r[1].Contains(info.alphabet[j][0]) || r[1].Contains(info.alphabet[j][1]))
                {
                    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed the ❖ button when there was at least one valid move.", moduleId);
                    module.CauseStrike();
                    return;
                }
            }
        }

        while(true)
        {
            info.RegenWires();

            for(int i = 0; i < info.wires.Length; i++)
            {
                String[] r;
                rules.TryGetValue(info.wires[i], out r);

                if(!r[0].Contains(lastPress[0]) && !r[0].Contains(lastPress[1]))
                    continue;

                if(r[1].Contains('*') && !(pressed.Exists(x => x == 0) && pressed.Exists(x => x == 1) && pressed.Exists(x => x == 2)))
                {
                    Debug.LogFormat("[The Modkit #{0}] Pressed the ❖ button. Wires now present are {1}.", moduleId, info.GetWireNames());
                    module.StartCoroutine(module.RegenWiresAnim());
                    return;
                }

                for(int j = 0; j < info.alphabet.Length; j++)
                {
                    if(pressed.Contains(j + 3))
                        continue;

                    if(r[1].Contains(info.alphabet[j][0]) || r[1].Contains(info.alphabet[j][1]))
                    {
                        Debug.LogFormat("[The Modkit #{0}] Pressed the ❖ button. Wires now present are {1}.", moduleId, info.GetWireNames());
                        module.StartCoroutine(module.RegenWiresAnim());
                        return;
                    }
                }
            }
        }
    }

    void CalcSolution()
    {
        Debug.LogFormat("[The Modkit #{0}] Wires present are {1}.", moduleId, info.GetWireNames());
    }
}