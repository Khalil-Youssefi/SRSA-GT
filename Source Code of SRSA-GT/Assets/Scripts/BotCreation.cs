using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class BotCreation : MonoBehaviour
{
    public bool Load_NofRobots_from_File = false;
    public int NofRobots_ = 10;
    public int GreenAreaX0 = -100;
    public int GreenAreaX1 = 0;
    public int GreenAreaY0 = 0;
    public int GreenAreaY1 = 0;
    public static int NofRobots;
    Vector3 randomPos()
    {
        return new Vector3(Random.Range(GreenAreaX0, GreenAreaX1), 0, Random.Range(GreenAreaY0, GreenAreaY1));
    }
    void Start()
    {
        if (Load_NofRobots_from_File)
        {
            var lines = File.ReadAllLines("SRSAv2_GT.cfg");
            NofRobots = int.Parse(lines[19].Split('=')[1].Trim());
        }else
        {
            NofRobots = NofRobots_;
        }
        var Bot0 = GameObject.Find("Bot 0");
        var BotTeam = GameObject.Find("Bot Team");
        Bot0.transform.position = randomPos();
        var bots = new List<GameObject>();
        bots.Add(Bot0);
        for (int i = 1; i < NofRobots; i++)
        {
            GameObject newBot = GameObject.Instantiate(Bot0);
            newBot.name = "Bot " + i;
            bool flag = true;
            while (flag)
            {
                flag = false;
                newBot.transform.position = randomPos();
                newBot.transform.parent = BotTeam.transform;
                foreach (var bot in bots)
                {
                    if ((bot.transform.position - newBot.transform.position).magnitude < 4)
                    {
                        flag = true;
                    }
                }
            }
            bots.Add(newBot);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
