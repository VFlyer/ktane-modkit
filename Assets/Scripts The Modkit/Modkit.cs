﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using KeepCoding;
using System.Text.RegularExpressions;

public class Modkit : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio audioSelf;
	public KMBombModule moduleSelf;

	public GameObject[] doors = new GameObject[5];
	public GameObject[] components = new GameObject[5];
	public KMSelectable[] selectBtns = new KMSelectable[3];
	public KMSelectable utilityBtn;
	public TextMesh displayText;

	public Material[] wireMats;
	public Material[] symbolMats;
	public Material[] LEDMats;
	public Material[] arrowMats;
	public Material[] keyLightMats;

	public GameObject[] wires;
	public GameObject[] symbols;
	public GameObject[] alphabet;
	public GameObject[] LED;
	public GameObject[] arrows;
	public Transform arrowsBase;
	


	public Mesh wireWhole;
	public Mesh wireCut;
    List<int> listWiresCut = new List<int>();

	readonly string[] componentNames = new string[] { "WIRES", "SYMBOLS", "ALPHABET", "LED", "ARROWS" };
	bool[] onComponents = new bool[] { false, false, false, false, false };
	bool[] targetComponents = new bool[] { false, false, false, false, false };
	int currentComponent = 0;

	ComponentInfo info;
	Puzzle p;

	//Logging
	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved = false;
    private bool animating = false;
    private bool solving = false;

	private bool hasStruck = false; // TP Handling, send a strike handling if the module struck. To prevent excessive inputs.

	// Use these for debugging individual puzzles.
	private bool forceComponents, forceByModuleID = false;
	private bool[] componentsForced;
	public bool enableBruteTest = false;
	ModkitSettings modConfig = new ModkitSettings();

	string[][] passwords = {
			new string[] { "CRY2", "HAM8", "TED6", "GIN3", "FLU4" },
			new string[] { "CAP1", "MUD0", "KIT9", "FLY5", "HER7" },
			new string[] { "HUT0", "RED3", "PAC8", "MIX2", "SKY9" },
			new string[] { "REV1", "SHY7", "DIM4", "TUG6", "LAW5" },
			new string[] { "RIB8", "MAN1", "SPY5", "GEL0", "CUT7" },
			new string[] { "SIX6", "FRY2", "HUB9", "LEG3", "JAW4" }
		};

	void Awake()
	{
		moduleId = moduleIdCounter++;
        selectBtns[0].OnInteract += delegate () {
			ChangeDisplayComponent(selectBtns[0], -1);
			return false; };
        selectBtns[1].OnInteract += delegate () {
			ToggleComponent();
			return false; };
        selectBtns[2].OnInteract += delegate () {
			ChangeDisplayComponent(selectBtns[2], 1);
			return false; };

		try
		{
			ModConfig<ModkitSettings> modkitJSON = new ModConfig<ModkitSettings>("ModkitSettings");
			modConfig = modkitJSON.Settings;

			forceComponents = modConfig.EnforceComponents;
			forceByModuleID = modConfig.EnforceByModID;

			componentsForced = new bool[] { modConfig.EnforceWires, modConfig.EnforceSymbols, modConfig.EnforceAlphabet, modConfig.EnforceLEDs, modConfig.EnforceArrows };
		}
		catch
		{
			Debug.LogErrorFormat("[The Modkit #{0}] The settings do not work as intended! Using default settings (do not force required components).", moduleId);
			forceComponents = false;
			forceByModuleID = false;
			componentsForced = new bool[] { false, false, false, false, false };
		}

	}

	void Start () 
	{
		SetUpComponents();
		TryOverrideSettings();

		if (forceComponents) // Check if the components need to be forced on.
		{
			ForceComponents();
			displayText.text = "DISABLED";
		}
		else
		{
			CalcComponents();
			displayText.text = componentNames[currentComponent];
		}
		AssignHandlers();
        for (int x = 0; x < 5; x++)
        {
            SetSelectables(x, forceComponents ? targetComponents[x] : false);
			onComponents[x] = forceComponents && targetComponents[x];
        }
		if (forceComponents)
			StartCoroutine(PlayEnforceAnim());
	}
	void TryOverrideSettings()
    {
        try
        {
			var missionID = Application.isEditor ? "freeplay" : Game.Mission.ID ?? "unknown";
			var overwriteSuccessful = false;
			Debug.LogFormat("<The Modkit #{0}> Mission ID: {1}", moduleId, missionID);
			switch (missionID)
            {
				case "freeplay":
				case "custom":
					Debug.LogFormat("<The Modkit #{0}> MISSION DETECTED AS FREEPLAY. NOT OVERWRITING SETTINGS.", moduleId);
					return;
				case "mod_theBombsBlanMade_deafsHell":
					Debug.LogFormat("<The Modkit #{0}> \"Deaf's Hell\" from \"The Bombs Blan Made\" detected.", moduleId);
					forceComponents = true;
					forceByModuleID = true;
					overwriteSuccessful = true;
					break;
            }
			if (overwriteSuccessful) {
				Debug.LogFormat("<The Modkit #{0}> OVERWRITE SUCCESSFUL BY MISSION ID.", moduleId);
				return;
			}
			/*
			var regexMatchOverrideDescription = Regex.Match(Game.Mission.Description ?? "", @"\[ModkitOverride\]\sEnforce(ModID|((Wires?|Symbols?|Alphabet|LED|Arrows?),)+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant );
			if (regexMatchOverrideDescription.Success)
            {

            }
			else
            {*/
				Debug.LogFormat("<The Modkit #{0}> PREVENTING COMPONENTS FROM BEING OVERRIDDEN.", moduleId);
				forceComponents = false;
            //}
        }
		catch (Exception error)
        {
			Debug.LogWarningFormat("<The Modkit #{0}> Override does not work as intended! ", moduleId);
			Debug.LogException(error);
			Debug.LogWarningFormat("<The Modkit #{0}> Using default settings.", moduleId);
			forceComponents = false;
		}
    }

	public void CauseStrike() // Cause a strike on The Modkit
	{
		moduleSelf.HandleStrike();
		hasStruck = true;
	}
	void SetUpComponents()
	{
		info = new ComponentInfo();

		for(int i = 0; i < wires.Length; i++)
		{
			int color1 = info.wires[i] / 10;
			int color2 = info.wires[i] % 10;

			if(color1 > color2)
			{
				color1 = color2;
				color2 = info.wires[i] / 10;
			}

			if(color1 != color2)
				wires[i].transform.GetComponentInChildren<Renderer>().material = wireMats.Where(x => x.name == ComponentInfo.COLORNAMES[color1] + "_" + ComponentInfo.COLORNAMES[color2]).ToArray()[0];
			else
				wires[i].transform.GetComponentInChildren<Renderer>().material = wireMats.Where(x => x.name == ComponentInfo.COLORNAMES[color1]).ToArray()[0];
		}

		for(int i = 0; i < symbols.Length; i++)
		{
			symbols[i].transform.Find("symbol").GetComponentInChildren<Renderer>().material = symbolMats[info.symbols[i]];
		}

		for(int i = 0; i < alphabet.Length; i++)
		{
			alphabet[i].transform.Find("ButtonText").GetComponentInChildren<TextMesh>().text = info.alphabet[i];
		}

		for(int i = 0; i < LED.Length; i++)
		{
			LED[i].transform.Find("light").GetComponentInChildren<Renderer>().material = LEDMats[info.LED[i]];
		}

		for(int i = 0; i < arrows.Length; i++)
		{
			arrows[i].GetComponentInChildren<Renderer>().material = arrowMats[info.arrows[i]];
			float scalar = transform.lossyScale.x;
    		arrows[i].transform.Find("light").GetComponentInChildren<Light>().range *= scalar;
    		arrows[i].transform.Find("light").GetComponentInChildren<Light>().color = ComponentInfo.LIGHTCOLORS[info.arrows[i]];
		}
	}
	void ForceComponents()
	{
		Debug.LogFormat("[The Modkit #{0}] The calculation procedure for The Modkit has been overridden.", moduleId);
		var curModID = moduleId * 1;
		if (forceByModuleID)
		{
			Debug.LogFormat("[The Modkit #{0}] Enforcing components viva module ID.", moduleId);
			for (int x = 0; x < componentsForced.Length; x++)
			{
				targetComponents[x] = curModID % 2 == 1;
				curModID /= 2;
			}
		}
		else
		{
			Debug.LogFormat("[The Modkit #{0}] Enforcing specific components.", moduleId);
			for (int x = 0; x < componentsForced.Length; x++)
				targetComponents[x] = componentsForced[x];
		}
		Debug.LogFormat("[The Modkit #{0}] Enforced components: [ {1} ].", moduleId, componentNames.Any(x => targetComponents[Array.IndexOf(componentNames, x)]) ? componentNames.Where(x => targetComponents[Array.IndexOf(componentNames, x)]).Join(", ") : "none");
	}
	void CalcComponents()
	{
		Port[] columns = { Port.Serial, Port.Parallel, Port.DVI, Port.PS2, Port.StereoRCA, Port.RJ45 };
        string[] colName = { "Serial", "Parallel", "DVI", "PS2", "Stereo RCA", "RJ-45", };
		int col = 0;
		int val = bomb.GetPortCount(Port.Serial);

		for(int i = 0; i < columns.Length; i++)	
			if(bomb.GetPortCount(columns[i]) > val)
			{
				col = i;
				val = bomb.GetPortCount(columns[i]);
			}	

        Debug.LogFormat("[The Modkit #{0}] The {1} column is the leftmost column with the most amount of ports.", moduleId, colName[col]);

		for(int i = 0; i < passwords[col].Length; i++)
			for(int j = 0; j < passwords[col][i].Length; j++)
				if(bomb.GetSerialNumber().Contains(passwords[col][i][j]))
					targetComponents[i] = true;

		Debug.LogFormat("[The Modkit #{0}] Required components: [ {1} ].", moduleId, targetComponents.Any(x => x) ? componentNames.Where(x => targetComponents[Array.IndexOf(componentNames, x)]).Join(", ") : "none");
	}

	void AssignHandlers()
	{
		Debug.LogFormat("[The Modkit #{0}] --------------------------------------------------", moduleId);
		var value = Enumerable.Range(0, 5).Where(a => targetComponents[a]).Sum(a => 1 << a);
		Debug.LogFormat("<The Modkit #{0}> Val: {1}", moduleId, value);
		
		switch (value)
        {
			default:
			case 0: p = new Puzzle(this, moduleId, info, true); break;
			case 1: p = new ColorfulWires(this, moduleId, info); break;
			case 2: p = new AdjacentSymbols(this, moduleId, info); break;
			case 3: p = new RunicWires(this, moduleId, info); break;
			case 4: p = new EdgeworkKeys(this, moduleId, info); break;
			case 5: p = new IndexedWires(this, moduleId, info); break;
			case 6: p = new EncryptedKeypad(this, moduleId, info); break;
			case 7: p = new SequenceCut(this, moduleId, info); break;
			case 8: p = new LEDPattern(this, moduleId, info); break;
			case 9: p = new WireInstructions(this, moduleId, info); break;
			case 10: p = new SymbolicMorse(this, moduleId, info); break;
			case 11: p = new HierarchicalWires(this, moduleId, info); break;
			case 12: p = new SemaphoreKeys(this, moduleId, info); break;
			case 13: p = new PowerGrid(this, moduleId, info); break;
			case 14: p = new KeyScore(this, moduleId, info); break;
			case 15: p = new TheThirdWire(this, moduleId, info); break;
			case 16: p = new SimonShifts(this, moduleId, info); break;
			case 17: p = new WireMaze(this, moduleId, info); break;
			case 18: p = new PerspectiveSymbols(this, moduleId, info); break;
			case 19: p = new WireSignaling(this, moduleId, info); break;
			case 20: p = new AlphanumericOrder(this, moduleId, info); break;
			case 21: p = new CruelWireSequence(this, moduleId, info); break;
			case 22: p = new LyingKeys(this, moduleId, info); break;
			case 23: p = new TheLastInLine(this, moduleId, info); break;
			case 24: p = new ColorCompass(this, moduleId, info); break;
			case 25: p = new BlinkingWires(this, moduleId, info); break;
			case 26: p = new ColorOffset(this, moduleId, info); break;
			case 27: p = new ColorDominance(this, moduleId, info); break;
			case 28: p = new LEDDirections(this, moduleId, info); break;
			case 29: p = new PreciseWires(this, moduleId, info); break;
			case 30: p = new GatedMaze(this, moduleId, info); break;
			case 31: p = new ParanormalWires(this, moduleId, info); break;
		}
		/*
		if (targetComponents.SequenceEqual(new[] {true, false, false, false, false})) p = new ColorfulWires(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, true, false, false, false})) p = new AdjacentSymbols(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, false, true, false, false})) p = new EdgeworkKeys(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, false, false, true, false})) p = new LEDPattern(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, false, false, false, true})) p = new SimonShifts(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, true, false, false, false})) p = new RunicWires(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, false, true, false, false})) p = new IndexedWires(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, false, false, true, false})) p = new WireInstructions(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, false, false, false, true})) p = new WireMaze(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, true, true, false, false})) p = new EncryptedKeypad(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, true, false, true, false})) p = new SymbolicMorse(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, true, false, false, true})) p = new PerspectiveSymbols(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, false, true, true, false})) p = new SemaphoreKeys(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, false, true, false, true})) p = new AlphanumericOrder(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, false, false, true, true})) p = new ColorCompass(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, true, true, false, false})) p = new SequenceCut(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, true, false, true, false})) p = new HierarchicalWires(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, true, false, false, true})) p = new WireSignaling(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, false, true, true, false})) p = new PowerGrid(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, false, true, false, true})) p = new CruelWireSequence(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, false, false, true, true})) p = new BlinkingWires(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, true, true, true, false})) p = new KeyScore(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, true, true, false, true})) p = new LyingKeys(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, true, false, true, true})) p = new ColorOffset(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, false, true, true, true})) p = new LEDDirections(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, true, true, true, false})) p = new TheThirdWire(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, true, true, false, true})) p = new TheLastInLine(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, true, false, true, true})) p = new ColorDominance(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, false, true, true, true})) p = new PreciseWires(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {false, true, true, true, true})) p = new GatedMaze(this, moduleId, info);
		else if(targetComponents.SequenceEqual(new[] {true, true, true, true, true})) p = new ParanormalWires(this, moduleId, info);
		else p = new Puzzle(this, moduleId, info, true);	
		*/
        for (int x = 0; x < wires.Length; x++)
        {
            int y = x;
            wires[x].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnWireCut(y); return false; };
        }
        for (int x = 0; x < symbols.Length; x++)
        {
            int y = x;
            symbols[x].GetComponentInChildren<KMSelectable>().OnInteract += delegate {
				StartCoroutine(AnimateButtonPress(symbols[y].transform, Vector3.down * 0.005f));
				p.OnSymbolPress(y);
				return false;
			};
        }

        for (int x = 0; x < alphabet.Length; x++)
        {
            int y = x;
            alphabet[x].GetComponentInChildren<KMSelectable>().OnInteract += delegate {
				StartCoroutine(AnimateButtonPress(alphabet[y].transform, Vector3.down * 0.005f));
				p.OnAlphabetPress(y);
				return false;
			};
        }
        for (int x = 0; x < arrows.Length; x++)
        {
            int y = x;
            arrows[x].GetComponentInChildren<KMSelectable>().OnInteract += delegate {
				StartCoroutine(AnimateButtonPress(arrowsBase.transform, Vector3.down * 0.002f));
				StartCoroutine(AnimateButtonRotationPress(arrowsBase.transform, new[] { Vector3.right , Vector3.left , Vector3.back , Vector3.forward }.ElementAt(y) * 5));
				p.OnArrowPress(y);
				return false; };
        }
		
		utilityBtn.OnInteract += delegate {
			StartCoroutine(AnimateButtonPress(utilityBtn.transform, Vector3.down * 0.005f));
			p.OnUtilityPress();
			return false;
		};
		if (enableBruteTest)
			p.BruteForceTest();
	}

	void ChangeDisplayComponent(KMSelectable btn, int delta)
	{
		audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn.transform);
        btn.AddInteractionPunch(0.5f);
		StartCoroutine(AnimateButtonPress(btn.transform, Vector3.down * 0.005f));
		if (moduleSolved || forceComponents)
			return;

		currentComponent += delta;
		
		if(currentComponent < 0)
			currentComponent += componentNames.Length;
		if(currentComponent >= componentNames.Length)
			currentComponent -= componentNames.Length;

		displayText.text = componentNames[currentComponent];
		displayText.color = onComponents[currentComponent] ? Color.green : Color.red;
	}

	void ToggleComponent()
	{

		audioSelf.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, selectBtns[1].transform);
        selectBtns[1].AddInteractionPunch(0.5f);
		StartCoroutine(AnimateButtonPress(selectBtns[1].transform, Vector3.down * 0.005f));

		if (moduleSolved || solving || forceComponents || animating)
			return;

		onComponents[currentComponent] = !onComponents[currentComponent];
		if(onComponents[currentComponent])
			displayText.color = new Color(0, 1, 0);
		else
			displayText.color = new Color(1, 0, 0);

		if(onComponents[currentComponent])
			StartCoroutine(ShowComponent(currentComponent));
		else
			StartCoroutine(HideComponent(currentComponent));
	}

	public void RegenWires()
	{
		info.RegenWires();
        listWiresCut.Clear();
		StartCoroutine(RegenWiresAnim());
	}

	public bool CheckValidComponents()
	{
		for(int i = 0; i < onComponents.Length; i++)
			if(onComponents[i] != targetComponents[i])
				return false;
			
		return true;
	}

	public string GetOnComponents()
	{
        return onComponents.Any(a => a)
			? componentNames.Where(x => onComponents[Array.IndexOf(componentNames, x)]).Join(", ")
			: "none";
    }

	public string GetTargetComponents()
	{
        return targetComponents.Any(a => a)
            ? componentNames.Where(x => targetComponents[Array.IndexOf(componentNames, x)]).Join(", ")
            : "none";
    }

	public bool IsAnimating()
	{
		return animating;
	}

	public bool IsSolved()
	{
		return moduleSolved;
	}

	public void StartSolve()
	{
		solving = true;
	}

	public void CutWire(int wire)
	{
		wires[wire].transform.Find("hl").gameObject.SetActive(false);
		wires[wire].GetComponent<MeshFilter>().mesh = wireCut;
        listWiresCut.Add(wire);
	}
    IEnumerator PlaySolveAnim()
    {
        bool[] leftHalfActive = { onComponents[1], onComponents[0] };
        bool[] rightHalfActive = { onComponents[2], onComponents[3], onComponents[4] };
        for (int x = 0; x < onComponents.Length; x++)
        {
            if (x == 2 && ((leftHalfActive[0] && rightHalfActive[0]) || (leftHalfActive[1] && (rightHalfActive[1] || rightHalfActive[2]))))
                yield return new WaitForSeconds(1f);
            if (onComponents[x])
                StartCoroutine(HideComponent(x));
        }
        yield return true;
    }
	IEnumerator PlayEnforceAnim()
	{
		bool[] leftHalfActive = { onComponents[1], onComponents[0] };
		bool[] rightHalfActive = { onComponents[2], onComponents[3], onComponents[4] };
		for (int x = 0; x < onComponents.Length; x++)
		{
			if (x == 2 && ((leftHalfActive[0] && rightHalfActive[0]) || (leftHalfActive[1] && (rightHalfActive[1] || rightHalfActive[2]))))
				yield return new WaitForSeconds(1f);
			if (onComponents[x])
				StartCoroutine(ShowComponent(x));
		}
		yield return true;
	}
	public void Solve() // Puts The Modkit into a disarmed state.
	{
		moduleSelf.HandlePass();
		moduleSolved = true;
        StartCoroutine(PlaySolveAnim());
	}

	public void SetSelectables(int n, bool enable)
	{
		switch(n)
		{
			case 0: 
			{
				foreach(GameObject wire in wires)
					wire.SetActive(enable);
				break;
			}
			case 1: 
			{
				foreach(GameObject symbol in symbols)
					symbol.SetActive(enable);
				break;
			}
			case 2: 
			{
				foreach(GameObject alpha in alphabet)
					alpha.SetActive(enable);
				break;
			}
			case 4: 
			{
				foreach(GameObject arrow in arrows)
					arrow.SetActive(enable);
				break;
			}
		}
	}

	public IEnumerator AnimateButtonPress(Transform affectedObject, Vector3 offset)
    {
        for (int x = 0; x < 5; x++)
        {
			affectedObject.localPosition += offset / 5;
			yield return new WaitForSeconds(0.01f);
        }
        for (int x = 0; x < 5; x++)
        {
			affectedObject.localPosition -= offset / 5;
			yield return new WaitForSeconds(0.01f);
        }
    }

	public IEnumerator AnimateButtonRotationPress(Transform affectedObject, Vector3 angle)
    {
        for (int x = 0; x < 5; x++)
        {
			affectedObject.localEulerAngles += angle / 5;
			yield return new WaitForSeconds(0.01f);
        }
        for (int x = 0; x < 5; x++)
        {
			affectedObject.localEulerAngles -= angle / 5;
			yield return new WaitForSeconds(0.01f);
        }
    }

	public IEnumerator ShowComponent(int n)
	{
		animating = true;

		SetSelectables(n, true);

		audioSelf.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);

		doors[n].transform.localPosition += new Vector3(0, -0.001f, 0);

		for(int i = 0; i < 10; i++)
		{
			doors[n].transform.localPosition += new Vector3( (n < 2 ? 0.008f : -0.008f), 0,0);
			yield return new WaitForSeconds(0.025f);
		}

		doors[n].SetActive(false);

		for(int i = 0; i < 10; i++)
		{
			components[n].transform.localPosition += new Vector3(0, 0.00121f, 0);
            yield return new WaitForSeconds(0.05f);
		}

		animating = false;
	}

	public IEnumerator HideComponent(int n)
	{
		animating = true;

		audioSelf.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);

		for(int i = 0; i < 10; i++)
		{
			components[n].transform.localPosition += new Vector3(0, -0.00121f, 0);
            yield return new WaitForSeconds(0.05f);
		}

		doors[n].SetActive(true);

		for(int i = 0; i < 10; i++)
		{
			doors[n].transform.localPosition += new Vector3( (n < 2 ? -0.008f : 0.008f), 0,0);
			yield return new WaitForSeconds(0.025f);
		}

		doors[n].transform.localPosition += new Vector3(0, 0.001f, 0);

		SetSelectables(n, false);

		animating = false;
	}

	public IEnumerator RegenWiresAnim()
	{
		yield return HideComponent(0);

		for(int i = 0; i < wires.Length; i++)
		{
			int color1 = info.wires[i] / 10;
			int color2 = info.wires[i] % 10;

			if(color1 > color2)
			{
				color1 = color2;
				color2 = info.wires[i] / 10;
			}

			if(color1 != color2)
				wires[i].transform.GetComponentInChildren<Renderer>().material = wireMats.Where(x => x.name == ComponentInfo.COLORNAMES[color1] + "_" + ComponentInfo.COLORNAMES[color2]).ToArray()[0];
			else
				wires[i].transform.GetComponentInChildren<Renderer>().material = wireMats.Where(x => x.name == ComponentInfo.COLORNAMES[color1]).ToArray()[0];
		
			wires[i].transform.Find("hl").gameObject.SetActive(true);
			wires[i].GetComponent<MeshFilter>().mesh = wireWhole;
		}

		yield return ShowComponent(0);
	}
	// Mod Settings
	public class ModkitSettings
	{
		public bool EnforceComponents = false;
		public bool EnforceByModID = false;
		public bool EnforceWires = false;
		public bool EnforceSymbols = false;
		public bool EnforceAlphabet = false;
		public bool EnforceLEDs = false;
		public bool EnforceArrows = false;
	}
	static readonly Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
	  {
			new Dictionary<string, object>
			{
				{ "Filename", "ModkitSettings.json" },
				{ "Name", "The Modkit Settings" },
				{ "Listings", new List<Dictionary<string, object>>{
					new Dictionary<string, object>
					{
						{ "Key", "EnforceComponents" },
						{ "Text", "Enforce Components" },
						{ "Description", "Enforce specific components to be required on the module instead of edgework." },
					},
					new Dictionary<string, object>{
						{ "Key", "EnforceByModID" },
						{ "Text", "Enforce Based on ModID" },
						{ "Description", "Enforce specific components based off of module ID, the one used for logging.\nOverrides Enforce Wires, LED, Arrows, Symbols, Alphabet." },
					},
					new Dictionary<string, object> {
						{ "Key", "EnforceWires" },
						{ "Text", "Enforce Wires" },
						{ "Description", "Enforce Wires to be required on The Modkit." },
					},
					new Dictionary<string, object> {
						{ "Key", "EnforceSymbols" },
						{ "Text", "Enforce Symbols" },
						{ "Description", "Enforce Symbols to be required on The Modkit." },
					},
					new Dictionary<string, object> {
						{ "Key", "EnforceAlphabet" },
						{ "Text", "Enforce Alphabet" },
						{ "Description", "Enforce Alphabet to be required on The Modkit." },
					},
					new Dictionary<string, object> {
						{ "Key", "EnforceLEDs" },
						{ "Text", "Enforce LEDs" },
						{ "Description", "Enforce LEDs to be required on The Modkit." },
					},
					new Dictionary<string, object> {
						{ "Key", "EnforceArrows" },
						{ "Text", "Enforce Arrows" },
						{ "Description", "Enforce Arrows to be required on The Modkit." },
					},
				} }
			}
	  };
	//Twitch Plays Handling
	/*    private bool isValid(string[] s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				s[i] = s[i].Replace(" ", "");
				s[i] = s[i].ToLower();
				if (!s[i].StartsWith("cutwire") && !s[i].StartsWith("press") && !s[i].StartsWith("select"))
				{
					return false;
				}
			}
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i].StartsWith("cutwire") && !s[i].Equals("cutwire"))
				{
					for (int j = 7; j < s.Length; j++)
					{
						if (j % 2 == 0)
						{
							if (!s[i].ElementAt(j).Equals(","))
							{
								return false;
							}
						}
						else
						{
							if (!s[i].ElementAt(j).Equals("1") && !s[i].ElementAt(j).Equals("2") && !s[i].ElementAt(j).Equals("3") && !s[i].ElementAt(j).Equals("4") && !s[i].ElementAt(j).Equals("5"))
							{
								return false;
							}
						}
					}
				}
				else if (s[i].StartsWith("select") && !s[i].Equals("select"))
				{
					string subs = s[i].Substring(6, s[i].Length - 6);
					string[] param = subs.Split(',');
					for (int k = 0; k < param.Length; k++)
					{
						if (!param[k].Equals("arrows") && !param[k].Equals("led") && !param[k].Equals("alphabet") && !param[k].Equals("symbols") && !param[k].Equals("wires"))
						{
							return false;
						}
					}
				}
				else if (s[i].StartsWith("press") && !s[i].Equals("press"))
				{
					if (s[i].Equals("pressbigdiamondat0") || s[i].Equals("pressbigdiamondat1") || s[i].Equals("pressbigdiamondat2") || s[i].Equals("pressbigdiamondat3") || s[i].Equals("pressbigdiamondat4") || s[i].Equals("pressbigdiamondat5") || s[i].Equals("pressbigdiamondat6") || s[i].Equals("pressbigdiamondat7") || s[i].Equals("pressbigdiamondat8") || s[i].Equals("pressbigdiamondat9"))
					{
						return true;
					}
					string subs = s[i].Substring(5, s[i].Length - 5);
					string[] param = subs.Split(',');
					for (int k = 0; k < param.Length; k++)
					{
						if (!param[k].Equals("bigdiamond") && !param[k].Equals("up") && !param[k].Equals("right") && !param[k].Equals("down") && !param[k].Equals("left") && !param[k].Equals("symbol1") && !param[k].Equals("symbol2") && !param[k].Equals("symbol3") && !param[k].Equals("alpha1") && !param[k].Equals("alpha2") && !param[k].Equals("alpha3"))
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	*/
#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Select the specified component(s) with \"!{0} select led,arrows\" (In this example, select 'led' and 'arrows'). To cut the specified wire(s): \"!{0} cut wire 1,4\" (in this example wires 1 & 4) \n"+
		"Press the specified buttons with \"!{0} press right,alpha2,symbol1,bigdiamond\" (In this example, the right arrow, 2nd alphabet, 1st symbol, and big diamond) Press '❖' based on the last seconds digit with: \"!{0} press bigdiamond at <#>\" Interaction commands may be chained with a semicolon (\";\", I.E \"!{0} select wires; cut wire 5\"). Combine presses/wire cuts with a comma (\",\")";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
		command = command.ToLower();
        string[] parameters = command.Split(';');
		List<KMSelectable> combinedModkitPresses = new List<KMSelectable>();
		List<string> inputTypes = new List<string>();
		List<string> timingType = new List<string>();
		for (int x = 0; x < parameters.Length; x++)
        {
            parameters[x] = parameters[x].Trim();
			if (parameters[x].RegexMatch(@"^select (leds?|arrows?|alphabet|wires?|symbols?)(, ?(leds?|arrows?|alphabet|wires?|symbols?))*$"))
			{
				if (forceComponents)
				{
					yield return "sendtochaterror This module has enforced components that you must use to disarm the module. Selecting components is disabled because of this.";
					yield break;
				}

				string intereptedString = parameters[x].Substring(7);
				string[] possibleComponents = intereptedString.Split(',');
				for (int idx = 0; idx < possibleComponents.Length; idx++)
				{
					timingType.Add("untilx");
					switch (possibleComponents[idx].Trim())
					{
						case "leds":
						case "led":
							inputTypes.Add("selectled");
							break;
						case "arrows":
						case "arrow":
							inputTypes.Add("selectarrows");
							break;
						case "alphabet":
							inputTypes.Add("selectalphabet");
							break;
						case "wires":
						case "wire":
							inputTypes.Add("selectwires");
							break;
						case "symbols":
						case "symbol":
							inputTypes.Add("selectsymbols");
							break;
						default:
							yield return "sendtochaterror Given component \"" + possibleComponents[idx] + "\" is not on the list of possible components!";
							yield break;
					}
				}
			}
			else if (parameters[x].RegexMatch(@"^cut ?wires?\s\d(, ?\d)*$"))
			{
				string intereptedString = parameters[x].Replace("cut wire", "").Replace("cutwire", "").Replace("cut wires", "").Replace("cutwires", "").Trim();
				string[] possibleWires = intereptedString.Split(',');
				for (int idx = 0; idx < possibleWires.Length; idx++)
				{
					timingType.Add("any");
					string allowedWires = "12345";
					if (allowedWires.Contains(possibleWires[idx].Trim()))
					{
						inputTypes.Add("wire" + possibleWires[idx].Trim());
					}
					else
					{
						yield return "sendtochaterror Given wire \""+possibleWires[idx]+"\" is not on the list of possible wires!";
						yield break;
					}
				}
			}
			else if (parameters[x].RegexMatch(@"^press (big\s?diamond|❖) ?(at|on|) ?\d$"))
			{
				string timingDigit = parameters[x].Substring(parameters[x].Length - 1);
				inputTypes.Add("bigdiamond");
				timingType.Add("timing _"+timingDigit);
			}
			else if (parameters[x].RegexMatch(@"^press (big ?diamond|❖|symbol ?\d|alpha(bet)? ?\d|l(eft)?|r(ight)?|u(p)?|d(own)?)(, ?(big ?diamond|❖|symbol ?\d|alpha(bet)? ?\d|l(eft)?|r(ight)?|u(p)?|d(own)?))*$"))
			{
				string intereptedString = parameters[x].Substring(6);
				string[] possiblePresses = intereptedString.Split(',');

				for (int idx = 0; idx < possiblePresses.Length; idx++)
				{
					possiblePresses[idx] = possiblePresses[idx].Trim();
					timingType.Add("any");
					switch (possiblePresses[idx])
					{
						case @"big diamond":
						case @"bigdiamond":
						case @"❖":
							inputTypes.Add("bigdiamond");
							break;
						case "alphabet1":
						case "alpha1":
						case "alpha 1":
						case "alphabet 1":
							inputTypes.Add("alpha1");
							break;
						case "alphabet2":
						case "alpha2":
						case "alpha 2":
						case "alphabet 2":
							inputTypes.Add("alpha2");
							break;
						case "alphabet3":
						case "alpha3":
						case "alpha 3":
						case "alphabet 3":
							inputTypes.Add("alpha3");
							break;
						case "symbol1":
						case "symbol 1":
							inputTypes.Add("symbol1");
							break;
						case "symbol2":
						case "symbol 2":
							inputTypes.Add("symbol2");
							break;
						case "symbol3":
						case "symbol 3":
							inputTypes.Add("symbol3");
							break;
						case "left":
						case "l":
							inputTypes.Add("arrowL");
							break;
						case "right":
						case "r":
							inputTypes.Add("arrowR");
							break;
						case "up":
						case "u":
							inputTypes.Add("arrowU");
							break;
						case "down":
						case "d":
							inputTypes.Add("arrowD");
							break;

						default:
							yield return "sendtochaterror I don't know what button \"" + possiblePresses[idx] + "\" is supposed to relate to. Please check your command again for any typos/mistakes.";
							yield break;
					}
				}
			}
			else
			{
				yield return "sendtochaterror The command portion \"" +parameters[x]+ "\" is not valid, check for typos.";
				yield break;
			}
		}
		for (int x = 0; x < inputTypes.Count; x++)
		{
			inputTypes[x] = inputTypes[x].Trim();
			if (inputTypes[x].RegexMatch(@"^wire\d$"))
			{
				switch (inputTypes[x])
				{
					case "wire5":
						combinedModkitPresses.Add(wires[4].GetComponentInChildren<KMSelectable>());
						break;
					case "wire4":
						combinedModkitPresses.Add(wires[3].GetComponentInChildren<KMSelectable>());
						break;
					case "wire3":
						combinedModkitPresses.Add(wires[2].GetComponentInChildren<KMSelectable>());
						break;
					case "wire2":
						combinedModkitPresses.Add(wires[1].GetComponentInChildren<KMSelectable>());
						break;
					case "wire1":
						combinedModkitPresses.Add(wires[0].GetComponentInChildren<KMSelectable>());
						break;

				}
				continue;
			}
			else if (inputTypes[x].RegexMatch(@"^bigdiamond$"))
			{
				combinedModkitPresses.Add(utilityBtn);
				continue;
			}
			else if (inputTypes[x].RegexMatch(@"^alpha\d$"))
			{
				switch (inputTypes[x])
				{
					case "alpha3":
						combinedModkitPresses.Add(alphabet[2].GetComponentInChildren<KMSelectable>());
						break;
					case "alpha2":
						combinedModkitPresses.Add(alphabet[1].GetComponentInChildren<KMSelectable>());
						break;
					case "alpha1":
						combinedModkitPresses.Add(alphabet[0].GetComponentInChildren<KMSelectable>());
						break;
				}
				continue;
			}
			else if (inputTypes[x].RegexMatch(@"^symbol\d$"))
			{
				switch (inputTypes[x])
				{
					case "symbol3":
						combinedModkitPresses.Add(symbols[2].GetComponentInChildren<KMSelectable>());
						break;
					case "symbol2":
						combinedModkitPresses.Add(symbols[1].GetComponentInChildren<KMSelectable>());
						break;
					case "symbol1":
						combinedModkitPresses.Add(symbols[0].GetComponentInChildren<KMSelectable>());
						break;
				}
				continue;
			}
			else if (inputTypes[x].RegexMatch(@"^arrow(L|R|U|D)$"))
			{
				switch (inputTypes[x].Substring(5))
				{
					case "L":
						combinedModkitPresses.Add(arrows[3].GetComponentInChildren<KMSelectable>());
						break;
					case "R":
						combinedModkitPresses.Add(arrows[2].GetComponentInChildren<KMSelectable>());
						break;
					case "U":
						combinedModkitPresses.Add(arrows[0].GetComponentInChildren<KMSelectable>());
						break;
					case "D":
						combinedModkitPresses.Add(arrows[1].GetComponentInChildren<KMSelectable>());
						break;
				}
				continue;
			}
			else if (inputTypes[x].StartsWith("select"))
			{
				combinedModkitPresses.Add(selectBtns[1]);
				timingType[x] = timingType[x].Replace("untilx", "until"+ inputTypes[x].Substring(6));
				continue;
			}
			yield return "sendtochaterror \"" + inputTypes[x] + "\" does not relate to a selectable on the module. Halting command procedure.";
			yield break;
		}
		hasStruck = false;
		for (int curPressIdx = 0; curPressIdx < new int[] { combinedModkitPresses.Count, inputTypes.Count, timingType.Count }.Min() && !hasStruck;curPressIdx++)
		{
			if (hasStruck || moduleSolved) yield break;
			do
				yield return "trycancel A delayed interaction has been canceled.";
			while (animating);
			if (timingType[curPressIdx].StartsWith("timing"))
			{
				string intereptedValue = timingType[curPressIdx].Substring(7);
				int processedValue;
				if (intereptedValue.RegexMatch(@"^_\d$"))
				{
					processedValue = int.Parse(intereptedValue.Substring(1,1));
					do
						yield return "trycancel A timed interaction has been canceled.";
					while ((int)bomb.GetTime() % 10 != processedValue);
				}
			}
			else if (timingType[curPressIdx].StartsWith("until"))
			{
				int goalIdx = Array.IndexOf(componentNames, timingType[curPressIdx].Substring(5).ToUpper());
				if (goalIdx == -1)
				{
					yield return "sendtochaterror \"" + timingType[curPressIdx] + "\" gave an unknown target. The rest of the commands have been voided.";
					yield break;
				}
				//Debug.LogFormat("Goal component to highlight: {0}",componentNames[goalIdx]);
				//Debug.LogFormat("Current component highlighted: {0}", componentNames[currentComponent]);
				int[] rangeValues = { Math.Abs(currentComponent + 5 - goalIdx), Math.Abs(currentComponent - goalIdx) };
				if (currentComponent != goalIdx)
				{
					
					if (1 == Array.IndexOf(rangeValues, rangeValues.Min()))
					{
						bool leftOrRight = currentComponent < goalIdx;
						for (int x = 0; x < rangeValues[1]; x++)
						{
							yield return null;
							selectBtns[leftOrRight ? 2 : 0].OnInteract();
							yield return new WaitForSeconds(0.1f);
						}
					}
					else if (0 == Array.IndexOf(rangeValues, rangeValues.Min()))
					{
						bool leftOrRight = currentComponent < goalIdx;
						for (int x = 0; x < rangeValues[0]; x++)
						{
							yield return null;
							selectBtns[leftOrRight ? 0 : 2].OnInteract();
							yield return new WaitForSeconds(0.1f);
						}
					}
				}
			}
			else if (timingType[curPressIdx] != "any")
			{
				yield return "sendtochaterror \"" + timingType[curPressIdx] + "\" is an unknown timing procedure. The rest of the commands have been voided.";
				yield break;
			}
			yield return null;
			KMSelectable currentSelected = combinedModkitPresses[curPressIdx];
			if (currentSelected.Highlight.gameObject.activeInHierarchy && currentSelected.gameObject.activeInHierarchy)
			{
				currentSelected.OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			else
			{
				string[] allInputs = { "wire1", "wire2", "wire3", "wire4", "wire5", "alpha1", "alpha2", "alpha3", "symbol1", "symbol2", "symbol3", "arrowU", "arrowD", "arrowL", "arrowR" };
				string[] TPErrorLog = { "Wire 1", "Wire 2", "Wire 3", "Wire 4", "Wire 5", "Alphabet Key 1", "Alphabet Key 2", "Alphabet Key 3", "Symbol Key 1", "Symbol Key 2", "Symbol Key 3", "Arrow Up", "Arrow Down", "Arrow Left", "Arrow Right" };
				int idx = Array.IndexOf(allInputs, inputTypes[curPressIdx]);

				yield return "sendtochaterror " + (idx >= 0 && idx<TPErrorLog.Length ? TPErrorLog[idx] : "An unknown object") + " is currently not selectable when this command was invoked! The rest of the commands have been voided.";
				yield break;
			}
		}

		yield break;
    }
}
