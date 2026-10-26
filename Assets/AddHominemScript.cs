using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class AddHominemScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> buttons;
    public TextMesh[] displays;

    private readonly int[] digits = {0, 8, 4, 9, 1, 5, 0, 7, 0, 3, 6, 2, 0, 2, 6, 3, 0, 7, 0, 5, 1, 9, 4, 8, 0};
    private List<int> values = new List<int>() {};
    private List<char> movement = new List<char>() {};
    private List<int> visited = new List<int>() {};
    private int num;
    private int stage;
    private int correct;
    private int fake;
    private int hl;
    private int startupbutton;
    private string[] input = new string[4] { "-", "-", "-", "-" };
    private string[] inputbuttons = new string[4] { "-", "-", "-", "-" };
    private string[] answer = new string[4];
    private int[] ans = new int[4];
    private bool startup;
    private bool tp;
    private string tpstring;
    private int attempts = 1;

    private List<int> v = new List<int> {};
    private int previousq;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        tp = TwitchPlaysActive;
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnHighlight = delegate () { if(!startup && stage > 0 && stage <= 15) displays[b + 3].text = new string[] { "\u2191", "\u2192", "\u2193", "\u2190" }[b]; hl = b + 1; };
            button.OnHighlightEnded = delegate () { if (!startup && stage > 0 && stage <= 15) displays[b + 3].text = ans[b].ToString(); hl = 0; };
            button.OnInteract = delegate ()
            {
                if (!moduleSolved && !startup)
                {
                    button.AddInteractionPunch(0.5f);
                    if (stage == 0) {
                        startupbutton = b;
                        Debug.LogFormat("[Add Hominem #{0}] Pressed {1} to initiate.", moduleID, "URDL"[b]);
                        StartCoroutine("StartUp");
                    }
                    else if (stage > 15)
                    {
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                        for (int i = 0; i < 3; i++) {
                            input[i] = input[i + 1];
                            inputbuttons[i] = inputbuttons[i + 1];
                        }
                        input[3] = "#";
                        inputbuttons[3] = "URDL"[b].ToString();
                        displays[1].text = string.Join("", input);
                    }
                    else if (b == correct)
                    {
                        stage++;
                        Audio.PlaySoundAtTransform("Right", button.transform);
                        Debug.LogFormat("[Add Hominem #{0}] Pressed {1}.", moduleID, "URDL"[b]);
                        values.Add(ans[correct]);
                        switch (b)
                        {
                            case 0: movement.Add('U'); break;
                            case 1: movement.Add('R'); break;
                            case 2: movement.Add('D'); break;
                            case 3: movement.Add('L'); break;
                        }
                        if (stage <= 15)
                            Prompt();
                        else
                        {
                            displays[2].text = "";
                            int row = 2;
                            int col = 2;
                            for (int i = 0; i < 15; i++) {
                                if (i > 0 && movement[i - 1] == movement[i]) {
                                    movement[i] = "URDL"[(Array.IndexOf("URDL".ToCharArray(), movement[i]) + 2) % 4];
                                }
                                switch (movement[i]) {
                                    case 'U':
                                        row--;
                                        if (row == -1) {
                                            row = 4;
                                        }
                                        break;
                                    case 'R':
                                        col++;
                                        col %= 5;
                                        break;
                                    case 'D':
                                        row++;
                                        row %= 5;
                                        break;
                                    case 'L':
                                        col--;
                                        if (col == -1) {
                                            col = 4;
                                        }
                                        break;
                                }
                                if (visited.Contains(row * 5 + col)) {
                                    num += values[i];
                                    Debug.LogFormat("[Add Hominem #{0}] Because the space has been visited before, moving {1} results in the value {2}.", moduleID, movement[i], values[i]);
                                } else {
                                    num += Math.Abs(values[i] - digits[row * 5 + col]);
                                    Debug.LogFormat("[Add Hominem #{0}] Because the space has not been visited before, moving {1} results in the value {2}.", moduleID, movement[i], Math.Abs(values[i] - digits[row * 5 + col]));
                                }
                                visited.Add(row * 5 + col);
                            }
                            displays[1].text = "----";
                            int[] digs = new int[4] { num / 64, (num / 16) % 4, (num / 4) % 4, num % 4 };
                            for (int i = 0; i < 4; i++) {
                                displays[i + 3].text = "#";
                                answer[i] = ("URDL"[(4 - (digs[i] + startupbutton) % 4) % 4]).ToString();
                            }
                            Debug.LogFormat("[Add Hominem #{0}] The sum of the answers is {1}.", moduleID, num);
                            Debug.LogFormat("[Add Hominem #{0}] {1} in base 4 is {2}.", moduleID, num, string.Join("", digs.Select(x => x.ToString()).ToArray()));
                            Debug.LogFormat("[Add Hominem #{0}] The buttons' values start at {1} and go counterclockwise in ascending order.", moduleID, "URDL"[(startupbutton + 2) % 4]);
                            Debug.LogFormat("[Add Hominem #{0}] The passcode is {1}.", moduleID, answer.Join(""));
                        }
                    }
                    else
                        Strike();
                }
                return false;
            };
        }
    }

    private IEnumerator StartUp()
    {
        startup = true;
        stage = 1;
        float time = 0;
		input = new string[4] { "-", "-", "-", "-" };
        v = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        Audio.PlaySoundAtTransform("Startup", transform);
        Debug.LogFormat("[Add Hominem #{0}] Starting Up...", moduleID);
        while(time < 1f)
        {
            time += Time.deltaTime;
            int t = (int)Mathf.Lerp(0, 200, time);
            displays[0].text = (t < 10 ? "0" : "") + t.ToString();
            yield return null;
        }
        Prompt();
        startup = false;
        for(int i = 200; i > 0; i--)
        {
            displays[0].text = (i < 10 ? "0" : "") + i.ToString();
            yield return new WaitForSeconds(1);
        }
        displays[0].text = "00";
        if(stage < 15)
        {
            Debug.LogFormat("[Add Hominem #{0}] Out of Time.", moduleID);
            Strike();
        }
        else if (inputbuttons.SequenceEqual(answer))
        {
            Debug.LogFormat("[Add Hominem #{0}] Submitted {1}. Correct!", moduleID, string.Join("", inputbuttons));
            module.HandlePass();
            moduleSolved = true;
            displays[1].text = "\u2713";
            displays[2].text = attempts > 4 ? "F--" : new string[] { "F++" , "F+", "F", "F-"}[attempts - 1];
            for (int i = 3; i < 7; i++)
                displays[i].text = "";
            Audio.PlaySoundAtTransform("Good", transform);
        }
        else
        {
            Debug.LogFormat("[Add Hominem #{0}] Submitted {1}. Incorrect.", moduleID, string.Join("", inputbuttons));
            Strike();
        }
    }

    private void Prompt()
    {
        correct = Rnd.Range(0, 4);
        int q = 0;
        if (stage > 1) {
            do {
                fake = Rnd.Range(0, 4);
            } while (fake == correct);
            switch (previousq) {
                case 0: q = Rnd.Range(1, 4); break;
                case 1: q = Rnd.Range(0, 2); q *= 3; break;
                case 2: q = Rnd.Range(0, 2); q *= 3; break;
                case 3: q = Rnd.Range(0, 3); break;
            }
        } else {
            q = Rnd.Range(0, 4);
        }
        List<int> a = new List<int> { };
        int X = 0;
        switch (q)
        {
            case 0:
                int b;
                do {
                    a = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Shuffle();
                    b = Rnd.Range(0, 2);
                    a = a.Where(x => x % 2 == 1 - b).ToList();
                } while (!v.Except(a).Any());
                displays[2].text = b == 0 ? "EVEN" : "ODD";
                if (stage == 1) {
                    for (int i = 0; i < 4; i++)
                        if (i == correct)
                            ans[i] = b == 0 ? Rnd.Range(1, 5) * 2 : (Rnd.Range(1, 6) * 2) - 1;
                        else
                            ans[i] = a[i];
                } else {
                    for (int i = 0; i < 4; i++)
                    if (i == fake)
                        do {
                            ans[i] = b == 0 ? Rnd.Range(1, 5) * 2 : (Rnd.Range(1, 6) * 2) - 1;
                        } while (!v.Contains(ans[i]));
                    else if (i == correct)
                        do {
                            ans[i] = b == 0 ? Rnd.Range(1, 5) * 2 : (Rnd.Range(1, 6) * 2) - 1;
                        } while (v.Contains(ans[i]));
                    else
                        ans[i] = a[i];
                }
                if (b == 0)
                    v = new List<int> {2, 4, 6, 8};
                else
                    v = new List<int> {1, 3, 5, 7, 9};
                break;
            case 1:
                do {
                    a = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Shuffle();
                    X = Rnd.Range(5, 9);
                    a = a.Where(x => x < X).ToList();
                } while (!v.Except(a).Any());
                displays[2].text = "≥";
                displays[2].text += X.ToString();
                if (stage == 1) {
                    for (int i = 0; i < 4; i++)
                        if (i == correct)
                            ans[i] = Rnd.Range(X, 10);
                        else
                            ans[i] = a[i];
                } else {
                    for (int i = 0; i < 4; i++)
                    if (i == fake)
                        do {
                            ans[i] = Rnd.Range(X, 10);
                        } while (!v.Contains(ans[i]));
                    else if (i == correct)
                        do {
                            ans[i] = Rnd.Range(X, 10);
                        } while (v.Contains(ans[i]));
                    else
                        ans[i] = a[i];
                }
                v = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
                v.RemoveAll(x => x < X);
                break;
            case 2:
                do {
                    a = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Shuffle();
                    X = Rnd.Range(3, 6);
                    a = a.Where(x => x > X).ToList();
                } while (!v.Except(a).Any());
                displays[2].text = "≤";
                displays[2].text += X.ToString();
                if (stage == 1) {
                    for (int i = 0; i < 4; i++)
                        if (i == correct)
                            ans[i] = Rnd.Range(1, X + 1);
                        else
                            ans[i] = a[i];
                } else {
                    for (int i = 0; i < 4; i++)
                    if (i == fake)
                        do {
                            ans[i] = Rnd.Range(1, X + 1);
                        } while (!v.Contains(ans[i]));
                    else if (i == correct)
                        do {
                            ans[i] = Rnd.Range(1, X + 1);
                        } while (v.Contains(ans[i]));
                    else
                        ans[i] = a[i];
                }
                v = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
                v.RemoveAll(x => x > X);
                break;
            case 3:
                List<int> l;
                do {
                    a = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Shuffle();
                    X = Rnd.Range(3, 6);
                    l = new List<int> {};
                    switch (X) {
                        case 3: l = new List<int> { 1, 2, 6 }; break;
                        case 4: l = new List<int> { 4, 5, 9 }; break;
                        case 5: l = new List<int> { 3, 7, 8 }; break;
                    }
                    a = a.Where(x => !l.Contains(x)).ToList();
                } while (!v.Except(a).Any());
                displays[2].text = X.ToString();
                displays[2].text += "L";
                if (stage == 1) {
                    for (int i = 0; i < 4; i++)
                        if (i == correct)
                            ans[i] = l[Rnd.Range(0, 2)];
                        else
                            ans[i] = a[i];
                } else {
                    List<int> fv = l.Where(x => v.Contains(x)).ToList();
                    List<int> cv = l.Where(x => !v.Contains(x)).ToList();
                    for (int i = 0; i < 4; i++)
                    if (i == fake)
                        ans[i] = fv[Rnd.Range(0, fv.Count)];
                    else if (i == correct)
                        ans[i] = cv[Rnd.Range(0, cv.Count)];
                    else
                        ans[i] = a[i];
                }
                v = l;
                break;
        }
        previousq = q;
        displays[1].text = ((stage > 9 ? "" : "0") + stage.ToString()) + "/15";
        for (int i = 0; i < 4; i++)
            if(i != hl - 1)
                displays[i + 3].text = ans[i].ToString();
        if(tp)
            tpstring = string.Format("The prompt is \"{0}\". {1}.", displays[2].text, string.Join(", ", Enumerable.Range(0, 4).Select(x => "URDL"[x] + "=" + ans[x]).ToArray()));
        Debug.LogFormat("[Add Hominem #{0}] Question {1}: The prompt is \"{2}\" and the possible answers are {3}.", moduleID, stage, displays[2].text, string.Join(", ", Enumerable.Range(0, 4).Select(x => "URDL"[x] + "=" + ans[x]).ToArray()));
    }

    private void Strike()
    {
        module.HandleStrike();
        StopCoroutine("StartUp");
        displays[0].text = "00";
        for (int i = 1; i < 7; i++)
            displays[i].text = "";
        stage = 0;
        num = 0;
        values = new List<int>() {};
        movement = new List<char>() {};
        visited = new List<int>() {};
        attempts++;
    }

    bool TwitchPlaysActive;

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} U/R/D/L [Presses directional button. Presses can be chained when entering the passcode.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant().Replace(" ", "");
        if(command.Any(x => !"URDL".Contains(x.ToString())))
        {
            yield return "sendtochaterror!f Only U, R, D, and L are valid commands.";
            yield break;
        }
        if(command.Length > 1 && stage <= 15)
        {
            yield return "sendtochaterror!f Only one answer to a prompt may be sent at a time.";
            yield break;
        }
        if(stage > 15)
        {
            int[] p = command.Select(x => "URDL".IndexOf(x.ToString())).ToArray();
            for(int i = 0; i < p.Length; i++)
            {
                yield return null;
                buttons[p[i]].OnInteract();
            }
        }
        else
        {
            yield return null;
            buttons["URDL".IndexOf(command)].OnInteract();
            while (startup)
                yield return null;
            yield return null;
            if(stage > 0 && stage <= 15)
                yield return "sendtochat!f " + tpstring;
        }
    }
}
