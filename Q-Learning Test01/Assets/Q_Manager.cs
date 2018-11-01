using System.IO;
using UnityEngine;
using UnityEditor;

public class Q_Manager : MonoBehaviour {

    public GameObject Floor, Wall, Reward, Player;
    public int RoomSize, AgentCount;
    GameObject reward;
    GameObject[] Players;
    private int SpaceX, SpaceY;//RoomSize
    string Recorder;
    public int ReadyCheck;

    private int[,] RoomSpace;//Check Notes for Values
    private int SpawnX = 3, SpawnY=3;//Agent Origin Spawn
    private int ValueX =27, ValueY = 27;//Coordinate Value
    //private GameObject[,] SpaceValue;

    // Use this for initialization
    void Start () {
        Recorder = Path.GetFullPath(Application.dataPath + "\\..\\Assets\\DataStore\\DataRecorder.csv");
        Players = new GameObject[AgentCount];
        SpaceX = RoomSize;
        SpaceY = RoomSize;

        //generates world space
        RoomSpace = new int[SpaceX, SpaceY];//Assigns MultiD-Array

        for (int y = 0; y < SpaceY; y++) {
            for (int x = 0; x < SpaceX; x++) {

                Instantiate(Floor, new Vector3(x, 0, y), Quaternion.identity);//Sets floor space
                //Walls for interlocking rooms + doorways
                if (0 == x % 6 || 0 == y % 6) {
                    if (0 != (y - 3) % 6 && 0 != (x - 3) % 6) {
                        RoomSpace[x, y] = -1;
                        Instantiate(Wall, new Vector3(x, 1, y), Quaternion.identity);
                    }
                }
                if (x == 0 || y == 0 || y == SpaceY-1 || x == SpaceX - 1 ) {
                    //assigns the wall value within the environment as a impassable areas for agent and reward placement
                    RoomSpace[x, y] = -1;
                    Instantiate(Wall, new Vector3(x, 1, y), Quaternion.identity);

                    //print("Roomspace: " + x + "," + y + " is stored as " + RoomSpace[x, y]);
                } else {
                    //Leaves area as empty space that be populated with a value
                    RoomSpace[x, y] = 0;
                    //Instantiate(Wall, new Vector3(x, 1, y), Quaternion.identity);
                }
            }
            //if (y == SpaceY-1) { print("Roomspace: " + SpawnX+ "," + SpawnY + " is stored as " + RoomSpace[SpawnX, SpawnY]);}
        }
        //Generates the agent and reward value to be tracked
        RandomiseZones();
        EditorApplication.isPaused = true;
    }
    // Update is called once per frame
    void Update () {
        if (Input.GetButtonDown("Submit")) {
            RandomiseZones();
        }
    }

    void RandomiseZones() {
        ////Resets Spawn value of agent
        //SpawnX = 0; SpawnY = 0;
        //print("Spawn Is: " + SpawnX + "," + SpawnY);
        //while (RoomSpace[SpawnX, SpawnY] != 0) {
        //    //Randomises the agent's origin
        //    SpawnX = UnityEngine.Random.Range(1, RoomSize - 1);
        //    SpawnY = UnityEngine.Random.Range(1, RoomSize - 1);
        //    print("Spawn Is: " + SpawnX + "," + SpawnY);
        //    //Randomises the reward's placement within environment
        //    ValueX = UnityEngine.Random.Range(1, RoomSize - 1);
        //    ValueY = UnityEngine.Random.Range(1, RoomSize - 1);
        //    print("Value Is: " + ValueX + "," + ValueY);
        //    //If value's placement is the same as agent's, this will regenerate it

        //    while (ValueX == SpawnX && ValueY == SpawnY) {
        //        ValueX = UnityEngine.Random.Range(1, RoomSize - 1);
        //        ValueY = UnityEngine.Random.Range(1, RoomSize - 1);
        //    }
        //    //checks if reward has been placed within the environment

        //    if (reward != null) {
        //        //moves the reward value into a new area
        //        reward.transform.position = new Vector3(ValueX, 1, ValueY);
        //        print("Value Moved to: " + ValueX + "," + ValueY);
        //    } else {
        //        //spawns the reward value within environment
                reward = Instantiate(Reward, new Vector3(ValueX, 1, ValueY), Quaternion.identity) as GameObject;
                print("Value Spawned at: " + ValueX + "," + ValueY);
        //    }
        //}

        //Value must set in inspector prior running
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i] != null)
            {
                //resets the agent's position to spawn
                Players[i].transform.position = new Vector3(SpawnX, 1, SpawnY);
                Players[i].GetComponent<Agent>().PushTable();
                Players[i].GetComponent<Agent>().PullTable(SpaceX, SpaceY, SpawnX, SpawnY, i, ValueX, ValueY);
            }
            else
            {
                Players[i] = Instantiate(Player, new Vector3(SpawnX, 1, SpawnY), Quaternion.identity) as GameObject;
                Players[i].GetComponent<Agent>().PullTable(SpaceX, SpaceY, SpawnX, SpawnY, i, ValueX, ValueY);
                Players[i].GetComponent<Agent>().Manager = gameObject;
            }
        }
        using (FileStream fs = new FileStream(Recorder, FileMode.Append))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                var line = string.Format("{0},{1}", "SpawnPoint: " + SpawnX + "," + SpawnY, "RewardPos: " + ValueX + "," + ValueY);
                sw.WriteLine(line);
                line = string.Format("{0},{1},{2}", "Agent", "Step Count", "Entropy Produced");
                sw.WriteLine(line);
            }
        }
    }
    public void ReadyPull() {
        print("ReadyPull!");
        ReadyCheck++;
        if (ReadyCheck >= AgentCount) {
            EditorApplication.isPaused = true;
            //RandomiseZones();
        }
    }
}