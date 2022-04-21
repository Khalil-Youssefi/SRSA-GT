using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
public class SRSA_GT : MonoBehaviour
{
    //Metric 1
    static int loads = 1;
    static int frames_past = 0;
    static float time_past = 0.0f;
    static float NMEC = 0.0f;
    Vector3 NMEC_track;
    //Log
    static float TotalWalks = 0.0f;
    float PathLengh = 0.0f;
    uint g = 0;
    static uint N = 0;
    List<string> positions_string = new List<string>();
    // Configurations
    [Header("# Environment")]
    public string Experiment = "Experiment 1";
    public int Memory_Size = 500;
    public float Max_F = 70.0f;
    public int MaxTime = 5;
    public float Target_R = 10.0f;
    public float FTHR = 3000.0f;
    [Header("# SRSA")]
    public float C1 = 2.0f;
    public float C2 = 4.0f;
    public float T_RR = 0.8f;
    public float MPR = 7.5f;
    public float MD = 3.5f;
    public float MPP1 = 0.017f;
    public float MPP2 = 0.017f;
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
    Vector3 Vc,Vn;
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
    //-------------------------------------GT-------------------------------------------------------------------------------------//
    [Header("# GT")]
    public bool USE_GT = true;
    public float NR = 25;
    public float esp = 1.3f;
    public float sep = 0.7f;
    //static bool GT_SAVED = false;
    public static Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
    static Dictionary<string, int> Neighbors = new Dictionary<string, int>();
    static Dictionary<string, float> f = new Dictionary<string, float>();
    bool selected_strategy = true;
    int countNeighbors()
    {
        int neighbors = -1;
        foreach (var item in positions)
        {
            float dis = (transform.position - item.Value).magnitude;
            if (dis < NR)
            {
                neighbors++;
            }
        }
        return neighbors;
    }
    int ss = 0, se = 0, es = 0, ee = 0;
    static int sss = 0, sse = 0, ses = 0, see = 0;
    static List<int> sssl = new List<int>(), sesl = new List<int>(), ssel = new List<int>(), seel = new List<int>();
    static int qty = 0;
    bool strategyeffect = true;
    bool Strategy()
    {
        int N = countNeighbors();
        int RwMN = int.MinValue, RwLN = int.MaxValue;
        float MN = 0, Mf = 0, Bf = int.MinValue;
        foreach (var item in Neighbors)
        {
            MN += item.Value;
            if (item.Value > RwMN)
                RwMN = item.Value;
            if (item.Value < RwLN)
                RwLN = item.Value;
        }
        foreach (var item in f)
        {
            Mf += item.Value;
            if (item.Value > Bf)
                Bf = item.Value;
        }
        MN /= positions.Count;
        Mf /= positions.Count;
        float G_n = Bf / RwMN;
        float Mf_MN = Mf / MN;
        if (G_n > Mf_MN) //Search
        {
            if (sep*RwMN - N > 0)
            {
                ss++;
                Debug.Log("SS");
                strategyeffect = true;
                return true;
            }
            else
            {
                se++;
                Debug.Log("SE");
                strategyeffect = false;
                return false;
            }
        }
        else //Explore
        {
            if (esp*RwLN - N >= 0)
            {
                es++;
                Debug.Log("ES");
                strategyeffect = true;
                return true;
            }
            else
            {
                ee++;
                Debug.Log("EE");
                strategyeffect = false;
                return false;
            }
        }
    }
    //----------------------------------------------------------------------------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    //-----------------------------------------------------The Algorithm----------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------//
    // These two method initialize the particle.
    // Restart Method can be called many times in some cases.
    [Header("# Use CFG File")]
    public bool Load_CFG = true;
    string record = "";
    void LoadCFG()
    {
        record = "Scene " + SceneManager.GetActiveScene().name + ",";
        var lines = File.ReadAllLines("SRSAv2_GT.cfg");
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
        NR = float.Parse(lines[15]);
        esp = float.Parse(lines[16]);
        sep = float.Parse(lines[17]);
        USE_GT = lines[18] == "1";
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
        positions.Add(this.name, last_pos);
        f.Add(this.name, 1 / (sc_target.pos - last_pos).magnitude);
        Neighbors.Add(this.name, countNeighbors());
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
        Vn = Vc;
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
        //Debug.Log(rg.velocity.magnitude.ToString());
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
            //GT-DATA
            Neighbors[this.name] = countNeighbors();
            positions[this.name] = transform.position;
            f[this.name] = current_distance_to_target;
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
                Restart();
            }
            else
            {
                Vn = ((USE_GT) ? selected_strategy : true) ? Vc + w1 * Random.value * C1 * (personal_best.pos - transform.position) + w2 * Random.value * C2 * (global_best.pos - transform.position) : Vn;
                force = (Vn-Vc) / Time.deltaTime;
            }
        }
        else
        {
            rg.velocity = Vector3.zero;
            rg.angularVelocity = Vector3.zero;            
        }
    }
    private void Update()
    {
        selected_strategy = Strategy();
        positions_string.Add(rg.position.x.ToString("F3") + "," + rg.position.z.ToString("F3"));
        if (sssl.Count < Time.frameCount)
        {
            sssl.Add(ss);
            ssel.Add(se);
            sesl.Add(es);
            seel.Add(ee);
        }
        else
        {
            sssl[Time.frameCount - 1] += ss;
            ssel[Time.frameCount - 1] += se;
            sesl[Time.frameCount - 1] += es;
            seel[Time.frameCount - 1] += ee;
        }
    }
    static bool saved = false;
    void LateUpdate()
    {
        if (!saved && !freez && (sc_target.pos - transform.position).magnitude < Target_R || Time.realtimeSinceStartup - time_past > MaxTime * 60.0f)
        {
            saved = true;
            record = N + "," + record;
            record += Time.time + "," + g + "," + PathLengh + "," + TotalWalks;
            File.AppendAllText(Experiment + "_Records.csv", record + "\n");
            string posfn = Experiment + "_Positions_" + USE_GT.ToString() + ".csv";
            File.AppendAllLines(posfn , positions_string);

            //GT
            string[] lines = new string[sssl.Count];
            lines[0] = "SS,SE,ES,EE,SM,_F,Search,Explore,Obey,Ignore,Actual_Search,Actual_Explore";
            double sm;
            for (int i = 1; i < sssl.Count - 1; i++)
            {
                sm = sssl[i] + ssel[i] + sesl[i] + seel[i];
                lines[i] =  sssl[i].ToString() + ","
                     + ssel[i].ToString() + ","
                     + sesl[i].ToString() + ","
                     + seel[i].ToString() + ","
                     + sm.ToString() + ","
                     + (sm / (Time.frameCount - 1)).ToString() + ","
                     + (sssl[i] + ssel[i]).ToString() + ","
                     + (sesl[i] + seel[i]).ToString() + ","
                     + (sssl[i] + seel[i]).ToString() + ","
                     + (ssel[i] + sesl[i]).ToString() + ","
                     + (sssl[i] + sesl[i]).ToString() + ","
                     + (ssel[i] + seel[i]).ToString();
            }
            System.IO.File.WriteAllLines(Experiment + "_GTStrategies.csv", lines);
            BotsDistances.save(Experiment,USE_GT);
            freez = true;
            Time.timeScale = 0;
            /*if (Load_CFG)
                Application.Quit();*/
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
        if (Use_Memory)
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
                if (Use_Memory && strategyeffect)
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
        personal_bests[gameObject.name] = personal_best;
        UpdateGlobalBest();
    }

    // This Method simply finds the fitest one the PersonalBests set, and set it as Global Best
    // To simulate CError, a copy of the main is created, in which a chance for missing some updates is considered
    Dictionary<string, MemoryCell> personal_bests_local_copy = new Dictionary<string, MemoryCell>();
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
