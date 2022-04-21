using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
public class SRSA : MonoBehaviour
{
    //Metric 1
    static int loads = 1;
    static int frames_past = 0;
    static float time_past = 0.0f;
    static float NMEC = 0.0f;
    Vector3 NMEC_track;
    //Log
    public static float TotalWalks = 0.0f;
    static float PathLengh = 0.0f;
    uint g = 0;
    static uint N = 0;
    // Configurations
    [Header("# Environment")]
    public string Experiment = "Experiment 1";
    public int Memory_Size = 500;
    public float Max_F = 300.0f;
    public int MaxTime = 5;
    public float Target_R = 10.0f;
    public float FTHR = 3000.0f;
    [Header("# SRSA")]
    public float C1 = 2.0f;
    public float C2 = 4.0f;
    public float T_RR = 0.8f;
    public float MPR = 7.5f;
    public float MD = 7.5f;
    public float MPP1 = 1.7f;
    public float MPP2 = 0.17f;
    public bool Use_Memory = true;
    public bool Use_Moving_Angle = true;
    //----------------------------------------------------------------------------------------------------------------------------//
    // Global Best
    static MemoryCell global_best;
    //----------------------------------------------------------------------------------------------------------------------------//
    // Private Feaures And Properties
    MemoryCell personal_best;
    float D = 0.0f, Di, T = 0.0f, Dc = 0.0f;
    Vector3 force;
    Vector3 Vc;
    List<MemoryCell> memory;
    bool freez = false;
    //----------------------------------------------------------------------------------------------------------------------------//
    // Programming Things (Variables , Structures , etc)
    Rigidbody rg;
    Vector3 old_pos,last_pos;
    static Dictionary<string, MemoryCell> personal_bests = new Dictionary<string, MemoryCell>();
    struct MemoryCell
    {
        public Vector3 pos;
        public float distance_to_target;
    }
    //----------------------------------------------------------------------------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    //-----------------------------------------------------The Algorithm----------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    // These two method initialize the particle.
    // Restar Method can be called many times in some cases.
    [Header("# Use CFG File")]
    public bool Load_CFG = false;
    string record = "";
    void LoadCFG()
    {
        var lines = File.ReadAllLines("SRSA.cfg");
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Split('=')[1].Trim();
            record += lines[i] + ",";
        }
        Experiment = lines[0];
        Memory_Size = int.Parse(lines[1]);
        Max_F = float.Parse(lines[2]);
        MaxTime = int.Parse(lines[3]);
        Target_R = float.Parse(lines[4]);
        FTHR = float.Parse(lines[5]);
        C1 = float.Parse(lines[6]);
        C2 = float.Parse(lines[7]);
        T_RR = float.Parse(lines[8]);
        MPR = float.Parse(lines[9]);
        MD = float.Parse(lines[10]);
        MPP1 = float.Parse(lines[11]);
        MPP2 = float.Parse(lines[12]);
        Use_Memory = lines[13] == "1";
        Use_Moving_Angle = lines[14] == "1";
    }
    void Start()
    {
        N++;
        if(Load_CFG)
            LoadCFG();
        Application.runInBackground = true;
        rg = GetComponent<Rigidbody>();
        old_pos = transform.position;
        last_pos = old_pos;
        NMEC_track = transform.position;
        Restart();
    }

    public void Restart()
    {
        global_best.pos = Vector3.zero;
        global_best.distance_to_target = 10000000;
        memory = new List<MemoryCell>();
        T = 0.0f;
        Di = (Random.value > 0.5) ? -1.0f : 1.0f;
        Vc = new Vector3(Random.value, 0, Random.value);
        Vc = Vc.normalized * Max_F;
        personal_best.pos = transform.position;
        personal_best.distance_to_target = (sc_target.pos - transform.position).magnitude;
        if (!personal_bests.ContainsKey(gameObject.name))
            personal_bests.Add(gameObject.name, personal_best);
    }

    // This Method, adds the rotated force to the particle.
    // After that, it limits the velocity of the particle.
    // Also this method rotates the particle so that it faces its moving direction.
    // Finally if The amount of displacement is too small , current Personal Best gets penalty.
    void AddForceAndLimit()
    {
        rg.AddForce(force);
        if (rg.velocity.magnitude > Max_F)
        {
            rg.velocity = rg.velocity.normalized * Max_F;
        }
        if (rg.velocity != Vector3.zero)
            transform.forward = rg.velocity;
    }

    // Rotates the force by D and calls AddForceAndLimit
    // Then reduces D by T and reduces T by T_h
    void FixedUpdate()
    {
        if (!freez)
        {
            g++;
            if (Use_Moving_Angle)
            {
                float sn = Mathf.Sin(Mathf.Deg2Rad * D);
                float cn = Mathf.Cos(Mathf.Deg2Rad * D);
                force = new Vector3(cn * force.x - sn * force.z, 0.0f, sn * force.x + cn * force.z);
                D -= D * (1.0f - T) * Time.deltaTime;
                T -= T * (1.0f - T_RR) * Time.deltaTime;
                T = (T < 0.0f) ? 0 : T;
            }
            AddForceAndLimit();
            float displacement = (transform.position - last_pos).magnitude;
            TotalWalks += displacement;
            PathLengh += displacement;
            if (displacement < MD)
            {
                UpdateMemory(MPP2);
            }
            last_pos = transform.position;
            ///---------------------------------------
            float current_distance_to_target = (transform.position - sc_target.pos).magnitude;
            if (current_distance_to_target < personal_best.distance_to_target)
            {
                personal_best.pos = transform.position;
                personal_best.distance_to_target = current_distance_to_target;
                UpdateMemory(MPP1);
            }
            //Calculate force for next iteration
            float w1 = (personal_best.distance_to_target < FTHR) ? 1.0f : 0.0f;
            float w2 = (global_best.distance_to_target < FTHR) ? 1.0f : 0.0f;
            if (w1 + w1 == 0.0f)
            {
                // Resets if both P and G got too much penalty.
                //Restart();
            }
            else
            {
                Vector3 Vn = Vc + w1 * Random.value * C1 * (personal_best.pos - transform.position) + w2 * Random.value * C2 * (global_best.pos - transform.position);
                force = (Vn-Vc) / Time.deltaTime;
            }
        }
        else
        {
            rg.velocity = Vector3.zero;
            rg.angularVelocity = Vector3.zero;            
        }
    }

    void LateUpdate()
    {
        //Termination
        if (!freez && (sc_target.pos - transform.position).magnitude < Target_R || Time.realtimeSinceStartup - time_past > MaxTime * 60.0f)
        {
            record = N + "," + record;
            record += Time.time + "," + g + "," + PathLengh + "," + TotalWalks;
            File.AppendAllText(Experiment.Replace("\\","_").Replace("/","_").Replace(":","_") + "_Records.csv", record + "\n");
            freez = true;
            Time.timeScale = 0;
            if (Load_CFG)
                Application.Quit();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (!Use_Moving_Angle)
            return;
        D += Di;
        if (D > 359)
            D = 0;
        else if (D < -359)
            D = 0;
        T = 1.0f;
        Dc += 0.01f * Time.deltaTime;
        if(Dc>=1.0f)
        {
            Dc = 0.0f;
            Di = -Di;
            D = 0;
        }
        if (Random.value < 0.01)
            D = 0.0f;
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (!Use_Moving_Angle)
            return;
        Dc = 0.0f;
    }
    
    void UpdateMemory(float penalty)
    {
        if (Memory_Size > 0)
        {
            //Search in the Memory
            int index = -1;
            var temp1 = float.MaxValue;
            for (int i = 0; i < memory.Count; i++)
            {
                var temp2 = (personal_best.pos - memory[i].pos).magnitude;
                if (temp2 < MPR)
                {
                    if(temp2 < temp1)
                    {
                        temp1 = temp2;
                        index = i;
                    }
                }
            }
            if (index >= 0)
            {
                //Update the Memory
                if (Use_Memory)
                {
                    var cell = memory[index];
                    cell.distance_to_target += cell.distance_to_target * penalty * Time.deltaTime;
                    memory.RemoveAt(index);
                    memory.Add(cell);
                }
                
            }
            else
            {
                //Add to the Memory
                memory.Add(personal_best);
                if (memory.Count > Memory_Size)
                    memory.RemoveAt(0);
            }
            UpdatePersonalBest();
        }
        else
            UpdatePersonalBests();
    }

    // This Method sets the position with maximum fitness as Personal Best.
    private void UpdatePersonalBest()
    {
        int new_best = 0;
        for (int i = 0; i < memory.Count; i++)
        {
            if (memory[i].distance_to_target <= memory[new_best].distance_to_target)
                new_best = i;
        }
        personal_best = memory[new_best];
        UpdatePersonalBests();
    }

    // To find the global best, this Method inserts current personal best of the particle in a list.
    // Then it just pick the best in the set and it's new Global Best.
    private void UpdatePersonalBests()
    {

        if (personal_bests.ContainsKey(gameObject.name))
        {
            personal_bests[gameObject.name] = personal_best;
        }
        else
            personal_bests.Add(gameObject.name, personal_best);
        UpdateGlobalBest();
    }

    // This Method simply finds the fitest one the PersonalBests set, and set it as Global Best
    private void UpdateGlobalBest()
    {
        global_best.pos = Vector3.zero;
        global_best.distance_to_target = 100000;
        foreach (var i in personal_bests)
        {
            if (global_best.distance_to_target > i.Value.distance_to_target)
                global_best = i.Value;
        }
    }
}
