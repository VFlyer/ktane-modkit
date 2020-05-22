using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class ComponentInfo
{
    public static readonly int RED = 0;
    public static readonly int GREEN = 1;
    public static readonly int BLUE = 2;
    public static readonly int YELLOW = 3;
    public static readonly int ORANGE = 4;
    public static readonly int PURPLE = 5;
    public static readonly int WHITE = 6;

    public static readonly int UP = 0;
    public static readonly int DOWN = 1;
    public static readonly int RIGHT = 2;
    public static readonly int LEFT = 3;

    public static readonly string[] COLORNAMES = { "Red", "Green", "Blue", "Yellow", "Orange", "Purple", "White" };
    public static readonly string[] DIRNAMES = { "Up", "Down", "Right", "Left" };
    public static readonly string[] SYMBOLCHARS = { "©", "★", "☆", "ټ", "Җ", "Ω", "Ѭ", "Ѽ", "ϗ", "ϫ", "Ϭ", "Ϟ", "Ѧ", "æ", "Ԇ", "Ӭ", "҈", "Ҋ", "Ѯ", "¿", "¶", "Ͼ", "Ͽ", "ψ", "Ѫ", "Ҩ", "҂", "Ϙ", "ζ", "ƛ", "Ѣ" };
    public static readonly Color[] LIGHTCOLORS = { new Color(1, 0, 0), new Color(0, 0.737f, 0), new Color(0.388f, 0.27f, 1), new Color(1, 1, 0) };


    public int[] wires;
    public int[] symbols;
    public string[] alphabet; 
    public int[] LED;
    public int[] arrows;

    public ComponentInfo()
    {
        List<int> prov = new List<int>();
        while(prov.Count < 5)
        {
            int wireColor = rnd.Range(0, 7) * 10 + rnd.Range(0, 7);
            int altcolor = (wireColor % 10) * 10 + (wireColor / 10);
            if(!(prov.Contains(wireColor) || prov.Contains(altcolor)))
                prov.Add(wireColor);
        }        
        wires = prov.ToArray();

        prov.Clear();
        while(prov.Count < 3)
        {
            int symbol = rnd.Range(0, 31);
            if(!prov.Contains(symbol))
                prov.Add(symbol);
        }
        symbols = prov.ToArray();

        string[] letters = "QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray().Select(x => x.ToString()).OrderBy(x => rnd.Range(0, 1000)).ToArray();
        string[] numbers = "1234567890".ToCharArray().Select(x => x.ToString()).OrderBy(x => rnd.Range(0, 1000)).ToArray();
        alphabet = new string[3];
        alphabet[0] = letters[0] + numbers[0];
        alphabet[1] = letters[1] + numbers[1];
        alphabet[2] = letters[2] + numbers[2];

        prov.Clear();
        while(prov.Count < 3)
        {
            int color = rnd.Range(0, 6);
            prov.Add(color);
        }
        LED = prov.ToArray();

        arrows = new int[] { RED, GREEN, BLUE, YELLOW }.OrderBy(x => rnd.Range(0, 1000)).ToArray();
    }

    public void RegenWires()
    {
        List<int> prov = new List<int>();
        while(prov.Count < 5)
        {
            int wireColor = rnd.Range(0, 7) * 10 + rnd.Range(0, 7);
            int altcolor = (wireColor % 10) * 10 + (wireColor / 10);
            if(!(prov.Contains(wireColor) || prov.Contains(altcolor)))
                prov.Add(wireColor);
        }        
        wires = prov.ToArray();
    }

    public string GetWireNames()
    {
        List<string> names = new List<string>();

        for(int i = 0; i < wires.Length; i++)
        {
            int color1 = wires[i] / 10;
            int color2 = wires[i] % 10;

            if(color1 == color2)
                names.Add(COLORNAMES[color1]);
            else
                names.Add(COLORNAMES[color1] + "/" + COLORNAMES[color2]);
        }

        return names.Join(", ");
    }

    public string GetSymbols()
    {
        List<string> sym = new List<string>();

        for(int i = 0; i < symbols.Length; i++)
            sym.Add(SYMBOLCHARS[symbols[i]]);

        return sym.Join(", ");
    }

    public bool AreArrowsAdjacent(int color1, int color2)
    {
        return (arrows[UP] == color1 && arrows[RIGHT] == color2) ||
                (arrows[RIGHT] == color1 && arrows[UP] == color2) ||
                (arrows[RIGHT] == color1 && arrows[DOWN] == color2) ||
                (arrows[DOWN] == color1 && arrows[RIGHT] == color2) ||
                (arrows[DOWN] == color1 && arrows[LEFT] == color2) ||
                (arrows[LEFT] == color1 && arrows[DOWN] == color2) ||
                (arrows[UP] == color1 && arrows[LEFT] == color2) ||
                (arrows[LEFT] == color1 && arrows[UP] == color2);
    }
}