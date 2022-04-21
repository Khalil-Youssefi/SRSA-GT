using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
public class BotsDistances : MonoBehaviour
{
    static List<string> distances = new List<string>();
    public static void save(string Experiment,bool GT)
    {
        System.IO.File.WriteAllLines(Experiment + "_Distances" + GT.ToString() + ".csv", distances);
    }
    private void Update()
    {
        var bots = GameObject.FindGameObjectsWithTag("Robot");
        float t = 0.0f;
        foreach (var b1 in bots)
        {
            foreach (var b2 in bots)
            {
                t += (b1.transform.position - b2.transform.position).magnitude;
            }
        }
        t /= 2;
        distances.Add(t.ToString("F3"));
    }
}
