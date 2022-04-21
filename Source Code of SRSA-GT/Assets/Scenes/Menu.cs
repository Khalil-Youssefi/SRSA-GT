using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();
    }
    public void LoadScene (string sname)
    {
        SceneManager.LoadScene(sname);
    }
    public static string[] cfglines_names;
    public static string[] cfglines_values;
    public void LoadCFG(string cfgfile = "SRSAv2_GT.cfg")
    {
        var lines = File.ReadAllLines(cfgfile);
        cfglines_names = new string[lines.Length];
        cfglines_values = new string[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            cfglines_names[i] = lines[i].Split('=')[0].Trim()+"=";
            cfglines_values[i] = lines[i].Split('=')[1].Trim();
            GameObject.Find("LName (" + i.ToString() + ")").GetComponent<Text>().text = cfglines_names[i];
            GameObject.Find("LValue (" + i.ToString() + ")").GetComponent<InputField>().text = cfglines_values[i];
        }
    }
    public void ResetCFG()
    {
        LoadCFG("standard_SRSAv2_GT.cfg");
        SaveCFG();
    }
    public void SaveCFG()
    {
        var lines = new List<string>();
        for (int i = 0; i < cfglines_names.Length; i++)
        {
            lines.Add(cfglines_names[i] + GameObject.Find("LValue (" + i.ToString() + ")").GetComponent<InputField>().text);
        }
        File.WriteAllLines("SRSAv2_GT.cfg",lines);
    }
    private void Start()
    {
        LoadCFG();
    }
}
