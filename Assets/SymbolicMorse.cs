using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

class SymbolicMorse : Puzzle
{
    readonly String alphabet = "abcdefghijklmnopqrstuvwxyz";
	readonly String[] morseTable = { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };

    string[] words = { "air", "art", "bat", "car", "day", "eel", "fin", "fix", "gym", "hat", "ink", "jam", "key", "leg", "lip", "map", "not", "orb", "pet", "pub", "run", "set", "sob", "sun", "tar", "ten", "toy", "use", "vat", "war", "web", "win", "xis", "yes", "zip", "zoo" };
    int[][] pressOrder = new int[][] {
        new int[] { 13, 29, 0, 1, 2 },
        new int[] { 26, 5, 2, 0, 1 },
        new int[] { 3, 15, 0, 1, 2 },
        new int[] { 21, 23, 1, 0, 2 },
        new int[] { 0, 4, 2, 1, 0 },
        new int[] { 1, 30, 1, 0, 2 },
        new int[] { 18, 19, 1, 2, 0 },
        new int[] { 9, 14, 2, 1, 0 },
        new int[] { 10, 25, 1, 0, 2 },
        new int[] { 27, 7, 2, 0, 1 },
        new int[] { 11, 22, 1, 2, 0 },
        new int[] { 16, 24, 2, 0, 1 },
        new int[] { 28, 20, 1, 2, 0 },
        new int[] { 8, 12, 2, 1, 0 },
        new int[] { 17, 2, 1, 0, 2 },
        new int[] { 6, 29, 1, 0, 2 },
        new int[] { 8, 4, 0, 1, 2 },
        new int[] { 6, 13, 0, 2, 1 },
        new int[] { 1, 2, 0, 1, 2 },
        new int[] { 15, 17, 0, 2, 1 },
        new int[] { 14, 12, 2, 1, 0 },
        new int[] { 9, 27, 1, 0, 2 },
        new int[] { 7, 25, 2, 1, 0 },
        new int[] { 24, 28, 2, 1, 0 },
        new int[] { 11, 30, 2, 1, 0 },
        new int[] { 5, 26, 0, 1, 2 },
        new int[] { 21, 23, 2, 1, 0 },
        new int[] { 16, 26, 0, 2, 1 },
        new int[] { 26, 18, 2, 1, 0 },
        new int[] { 3, 0, 0, 2, 1 },
        new int[] { 22, 19, 0, 2, 1 },
        new int[] { 20, 10, 2, 1, 0 },
        new int[] { 28, 16, 1, 0, 2 },
        new int[] { 22, 27, 0, 1, 2 },
        new int[] { 29, 30, 1, 0, 2 },
        new int[] { 18, 0, 0, 1, 2 }
    };

    int word;
    List<int> presses;
    int nextPress;

    bool flashing = false;

    public SymbolicMorse(Modkit module, int moduleId, ComponentInfo info) : base(module, moduleId, info)
    {
        Debug.LogFormat("[The Modkit #{0}] Solving Symbolic Morse. Symbols present are: {1}.", moduleId, info.GetSymbols());

        CalcSolution();
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
            module.GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        module.StartSolve();

        if(presses.IndexOf(symbol) < nextPress || !flashing)
            return;

        if(presses[nextPress] == symbol)
        {
		    Debug.LogFormat("[The Modkit #{0}] Pressed symbol {1}.", moduleId, symbol + 1);
            nextPress++;
            module.symbols[symbol].transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[1];
            if(nextPress == 3)
            {
                Debug.LogFormat("[The Modkit #{0}] Module solved.", moduleId);
                foreach(GameObject led in module.LED)
                    led.transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[6];
                module.StopAllCoroutines();
                module.Solve();
            }
        }
        else
        {
		    Debug.LogFormat("[The Modkit #{0}] Strike! Pressed symbol {1}. Resetting.", moduleId, symbol + 1);
            module.GetComponent<KMBombModule>().HandleStrike();

            foreach(GameObject s in module.symbols)
               s.transform.Find("Key_TL").Find("LED").GetComponentInChildren<Renderer>().material = module.keyLightMats[6];
            module.StopAllCoroutines();
            CalcSolution();
            StartFlashes();
        }
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
            module.GetComponent<KMBombModule>().HandleStrike();
            return;
        }
       
        module.StartSolve();

        if(!flashing)
        {
            flashing = true;
            StartFlashes();
        }
    }

    void CalcSolution()
    {
        presses = new List<int>();
        nextPress = 0;

        word = rnd.Range(0, words.Length);
        Debug.LogFormat("[The Modkit #{0}] Flashed word is \"{1}\".", moduleId, words[word]);

        for(int i = 0; i < info.symbols.Length; i++)
            if(pressOrder[word][0] == info.symbols[i])
                presses.Add(i);
        for(int i = 0; i < info.symbols.Length; i++)
            if(pressOrder[word][1] == info.symbols[i])
                presses.Add(i);   
        for(int i = 2; i < pressOrder[word].Length; i++)
            if(!presses.Contains(pressOrder[word][i]))
                presses.Add(pressOrder[word][i]);  

        Debug.LogFormat("[The Modkit #{0}] Key press order is {1}.", moduleId, presses.Select(x => x + 1).Join(", "));
    }

    void StartFlashes()
	{
		module.StartCoroutine(FlashLight(0));
		module.StartCoroutine(FlashLight(1));
		module.StartCoroutine(FlashLight(2));
	}

	IEnumerator FlashLight(int n)
	{
		String character = morseTable[Array.IndexOf(alphabet.ToCharArray(), words[word][n])];

        module.LED[n].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[6];
        yield return new WaitForSeconds(1f);

		while(true)
		{
			for(int i = 0; i < character.Length; i++)
			{
			    module.LED[n].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[info.LED[n]];
				if(character[i] == '-')
					yield return new WaitForSeconds(0.6f);
				else
					yield return new WaitForSeconds(0.2f);
				
			    module.LED[n].transform.Find("light").GetComponentInChildren<Renderer>().material = module.LEDMats[6];
				yield return new WaitForSeconds(0.2f);
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
}