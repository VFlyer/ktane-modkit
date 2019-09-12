using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;

class EdgeworkGenerator 
{
    // static readonly int parallel = 0;
    // static readonly int serial = 1;
    // static readonly int dvi = 2;
    // static readonly int ps2 = 3;
    // static readonly int rj = 4;
    // static readonly int rca = 5;

    List<int> batteries;
    List<string> indicators;
    List<bool> indicatorStatus;
    List<List<int>> portPlates;
    int portTotal;
    string sn;

    string[] letters = "QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray().Select(x => x.ToString()).ToArray();
    string[] numbers = "1234567890".ToCharArray().Select(x => x.ToString()).ToArray();

    int[] components = Enumerable.Range(0, 11112).Select(x => 0).ToArray();

    public EdgeworkGenerator()
    {
        
    }

    public void Run(int n)
    {
        for(int i = 0; i < n; i++)
        {
            Generate();
            CalcComponents();
        }

        Debug.Log("Component 0: " + components[0] / (float) n * 100 + "%");
        Debug.Log("Component 1: " + components[1] / (float) n * 100 + "%");
        Debug.Log("Component 2: " + components[10] / (float) n * 100 + "%");
        Debug.Log("Component 3: " + components[11] / (float) n * 100 + "%");
        Debug.Log("Component 4: " + components[100] / (float) n * 100 + "%");
        Debug.Log("Component 5: " + components[101] / (float) n * 100 + "%");
        Debug.Log("Component 6: " + components[110] / (float) n * 100 + "%");
        Debug.Log("Component 7: " + components[111] / (float) n * 100 + "%");
        Debug.Log("Component 8: " + components[1000] / (float) n * 100 + "%");
        Debug.Log("Component 9: " + components[1001] / (float) n * 100 + "%");
        Debug.Log("Component 10: " + components[1010] / (float) n * 100 + "%");
        Debug.Log("Component 11: " + components[1011] / (float) n * 100 + "%");
        Debug.Log("Component 12: " + components[1100] / (float) n * 100 + "%");
        Debug.Log("Component 13: " + components[1101] / (float) n * 100 + "%");
        Debug.Log("Component 14: " + components[1110] / (float) n * 100 + "%");
        Debug.Log("Component 15: " + components[1111] / (float) n * 100 + "%");
        Debug.Log("Component 16: " + components[10000] / (float) n * 100 + "%");
        Debug.Log("Component 17: " + components[10001] / (float) n * 100 + "%");
        Debug.Log("Component 18: " + components[10010] / (float) n * 100 + "%");
        Debug.Log("Component 19: " + components[10011] / (float) n * 100 + "%");
        Debug.Log("Component 20: " + components[10100] / (float) n * 100 + "%");
        Debug.Log("Component 21: " + components[10101] / (float) n * 100 + "%");
        Debug.Log("Component 22: " + components[10110] / (float) n * 100 + "%");
        Debug.Log("Component 23: " + components[10111] / (float) n * 100 + "%");
        Debug.Log("Component 24: " + components[11000] / (float) n * 100 + "%");
        Debug.Log("Component 25: " + components[11001] / (float) n * 100 + "%");
        Debug.Log("Component 26: " + components[11010] / (float) n * 100 + "%");
        Debug.Log("Component 27: " + components[11011] / (float) n * 100 + "%");
        Debug.Log("Component 28: " + components[11100] / (float) n * 100 + "%");
        Debug.Log("Component 29: " + components[11101] / (float) n * 100 + "%");
        Debug.Log("Component 30: " + components[11110] / (float) n * 100 + "%");
        Debug.Log("Component 31: " + components[11111] / (float) n * 100 + "%");

        Debug.Log("Delta: " + (components.Max() - components.Where(x => x != 0).Min()) );
    }

    void Generate()
    {
        batteries = new List<int>();
        indicators = new List<string>();
        indicatorStatus = new List<bool>();
        portPlates = new List<List<int>>();

        int numberLetters = rnd.Range(2, 5);

        List<string> snProv = new List<string>();

        for(int i = 0; i < numberLetters; i++)
            snProv.Add(letters[rnd.Range(0, letters.Length)]);
        for(int i = 0; i < 6 -numberLetters; i++)
            snProv.Add(numbers[rnd.Range(0, numbers.Length)]);

        snProv.OrderBy(x => rnd.Range(0, 1000));

        sn = "";

        foreach(string s in snProv)
            sn += s;

        int indIndex = 0;
        string[] inds = new string[] { "SND", "IND", "MSA", "NSA", "FRQ", "TRN", "BOB", "SIG", "FRK", "CLR", "CAR" }.OrderBy(x => rnd.Range(0, 1000)).ToArray();

        portTotal = 0;

        for(int i = 0; i < 5; i++)
        {
            switch(rnd.Range(0, 3))
            {
                case 0:
                {
                    batteries.Add(rnd.Range(1, 3));
                    break;
                }
                case 1:
                {
                    indicators.Add(inds[indIndex]);
                    indIndex++;
                    indicatorStatus.Add(rnd.Range(0, 2) == 0);
                    break;
                }
                case 2:
                {
                    List<bool> ports = new List<bool>();

                    for(int j = 0; j < 6; j++)
                        ports.Add(rnd.Range(0, 2) == 0);

                    int plateType = rnd.Range(0, 2);

                    List<int> plate = new List<int>();

                    if(plateType == 0)
                    {
                        for(int j = 0; j < 2; j++)
                            if(ports[j])
                            {
                                plate.Add(j);
                                portTotal++;
                            }
                    }
                    else
                    {
                        for(int j = 2; j < 6; j++)
                            if(ports[j])
                            {
                                plate.Add(j);
                                portTotal++;
                            }
                    }

                    break;
                }
            }
        }
    }

    void CalcComponents()
    {
        int val = 0;

        // if(sn.Contains('A') || sn.Contains('E') || sn.Contains('I') || sn.Contains('O') || sn.Contains('U') || batteries.Sum() >= 6 || (indicators.Contains("SND") && indicatorStatus[indicators.IndexOf("SND")]))
        //     val += 1;
        // if(batteries.Sum() == 0 || portPlates.Exists(x => x.Exists(y => y == rca) ) || indicators.Count <= 1)
        //     val += 10;
        // if((batteries.Exists(x => x == 2) && batteries.Exists(x => x == 1)) || portTotal == 0 || (indicators.Contains("NSA") && indicatorStatus[indicators.IndexOf("NSA")]))
        //     val += 100;
        // if(batteries.Count >= 2 || portPlates.Exists(x => x.Exists(y => y == dvi)) || indicatorStatus.Where(x => x).Count() >= 3)
        //     val += 1000;
        // if(numbers.ToList().Contains(sn[0].ToString()) || portTotal >= 3 || (indicators.Contains("FRQ") && indicatorStatus[indicators.IndexOf("FRQ")]))
        //     val += 10000;

        if(sn.Contains('V') || sn.Contains('D') || sn.Contains('E') || sn.Contains('0'))
            val += 1;
        if(sn.Contains('H') || sn.Contains('I') || sn.Contains('J') || sn.Contains('1'))
            val += 10;
        if(sn.Contains('M') || sn.Contains('N') || sn.Contains('O') || sn.Contains('2'))
            val += 100;
        if(sn.Contains('R') || sn.Contains('S') || sn.Contains('T') || sn.Contains('3'))
            val += 1000;
        if(sn.Contains('W') || sn.Contains('X') || sn.Contains('Y') || sn.Contains('4'))
            val += 10000;

        components[val]++;
    }
}