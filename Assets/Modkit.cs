using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Modkit : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	public GameObject[] doors;
	public GameObject[] components;
	public KMSelectable[] selectBtns;
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

	public Mesh wireWhole;
	public Mesh wireCut;

	string[] componentNames = new string[] { "WIRES", "SYMBOLS", "ALPHABET", "LED", "ARROWS" };
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

	void Awake()
	{
		moduleId = moduleIdCounter++;
        selectBtns[0].OnInteract += delegate () { ChangeDisplayComponent(selectBtns[0], -1); return false; };
        selectBtns[1].OnInteract += delegate () { ToggleComponent(); return false; };
        selectBtns[2].OnInteract += delegate () { ChangeDisplayComponent(selectBtns[1], 1); return false; };
	}

	void Start () 
	{
		SetUpComponents();
		CalcComponents();
		AssignHandlers();
		SetSelectables(0, false);
		SetSelectables(1, false);
		SetSelectables(2, false);
		SetSelectables(4, false);
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

	void CalcComponents()
	{
		Port[] columns = { Port.Serial, Port.Parallel, Port.DVI, Port.PS2, Port.StereoRCA, Port.RJ45 };

		int col = 0;
		int val = bomb.GetPortCount(Port.Serial);

		for(int i = 0; i < columns.Length; i++)	
			if(bomb.GetPortCount(columns[i]) > val)
			{
				col = i;
				val = bomb.GetPortCount(columns[i]);
			}	

        Debug.LogFormat("[The Modkit #{0}] Using column {1}.", moduleId, col + 1);
        
		string[][] passwords = {
			new string[] { "CRY2", "HAM8", "TED6", "GIN3", "FLU4" },
			new string[] { "CAP1", "MUD0", "KIT9", "FLY5", "HER7" },
			new string[] { "HUT0", "RED3", "PAC8", "MIX2", "SKY9" },
			new string[] { "REV1", "SHY7", "DIM4", "TUG6", "LAW5" },
			new string[] { "RIB8", "MAN1", "SPY5", "GEL0", "CUT7" },
			new string[] { "SIX6", "FRY2", "HUB9", "LEG3", "JAW4" }
		};

		for(int i = 0; i < passwords[col].Length; i++)
			for(int j = 0; j < passwords[col][i].Length; j++)
				if(bomb.GetSerialNumber().Contains(passwords[col][i][j]))
					targetComponents[i] = true;

		Debug.LogFormat("[The Modkit #{0}] Required components are [ {1} ].", moduleId, componentNames.Where(x => targetComponents[Array.IndexOf(componentNames, x)]).Join(", "));
	}

	void AssignHandlers()
	{
		if(targetComponents.SequenceEqual(new[] {true, false, false, false, false})) p = new ColorfulWires(this, moduleId, info);
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

		wires[0].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnWireCut(0); return false; };
		wires[1].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnWireCut(1); return false; };
		wires[2].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnWireCut(2); return false; };
		wires[3].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnWireCut(3); return false; };
		wires[4].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnWireCut(4); return false; };
	
		symbols[0].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnSymbolPress(0); return false; };
		symbols[1].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnSymbolPress(1); return false; };
		symbols[2].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnSymbolPress(2); return false; };

		alphabet[0].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnAlphabetPress(0); return false; };
		alphabet[1].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnAlphabetPress(1); return false; };
		alphabet[2].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnAlphabetPress(2); return false; };
	
		arrows[0].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnArrowPress(0); return false; };
		arrows[1].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnArrowPress(1); return false; };
		arrows[2].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnArrowPress(2); return false; };
		arrows[3].GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnArrowPress(3); return false; };

		utilityBtn.GetComponentInChildren<KMSelectable>().OnInteract += delegate { p.OnUtilityPress(); return false; };
	}

	void ChangeDisplayComponent(KMSelectable btn, int delta)
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        btn.AddInteractionPunch(0.5f);

		if(moduleSolved)
			return;

		currentComponent += delta;
		
		if(currentComponent < 0)
			currentComponent = componentNames.Length - 1;
		if(currentComponent >= componentNames.Length)
			currentComponent = 0;

		displayText.text = componentNames[currentComponent];
		if(onComponents[currentComponent])
			displayText.color = new Color(0, 1, 0);
		else
			displayText.color = new Color(1, 0, 0);
	}

	void ToggleComponent()
	{
		if(animating)
            return;

		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        selectBtns[2].AddInteractionPunch(0.5f);

		if(moduleSolved || solving)
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
		return componentNames.Where(x => onComponents[Array.IndexOf(componentNames, x)]).Join(", ");
	}

	public string GetTargetComponents()
	{
		return componentNames.Where(x => targetComponents[Array.IndexOf(componentNames, x)]).Join(", ");
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
	}

	public void Solve()
	{
		GetComponent<KMBombModule>().HandlePass();
		moduleSolved = true;
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

	public IEnumerator ShowComponent(int n)
	{
		animating = true;

		SetSelectables(n, true);

        GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);

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

        GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);

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
}
