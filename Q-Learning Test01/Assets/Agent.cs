using System;
using System.IO;
using UnityEngine;

public class Agent : MonoBehaviour {
    public GameObject Manager;
    public int OriginX, OriginY, Step, GoalCount;
    public float gamma = 0.7f;                          //Epsilon value for trust value between value and policy
    float Discount = 0.7f, discount, maxReward = 1;     //Flooring method to stop the agent from generating large values over time, promotes exploration over exploitation
    float floor = -0.95f;                               //Stops the value from becoming -1 that designates the wall.
    float step = -0.05f;                                //How much the agent will devalue its next step, updating the previous tile if it doesn't reach its intended goal
    private float[,] RoomSpace;                         //Check Notes for Values
    int maxstep = 500;                                  //Maximum number of moves before the agent "dies"
    int maxx, maxy;                                     //Pushed by World Manager -> Stored entry of how the world space 
    int id;                                             //Pushed by World Manager -> Agent's relative ID
    int GoalReached, GoalTarget = 10;                   //Counter and measure before the agent's cycle of learning is stopped
    float entropy;                                      //Measure of change when moving from one tile to another
    private int PosX, PosY;                             //Coordinate Value of current postion
    int ValX, ValY;                                     //Pushed by World Manager -> Stored location of the goal
    float[] MatrixValue;                                //Sets the array for how many points the agent will check before making an action
    float timer, time = 0.03f;                          //Delay between each update to the Agent's Matrix check
    string FilePath,Recorder;                           //Filepath to store the data recorded

    void Start() {
        MatrixValue = new float[4];
        FilePath = Path.GetFullPath(Application.dataPath + "\\..\\Assets\\DataStore\\DataStore.csv");
        Recorder = Path.GetFullPath(Application.dataPath + "\\..\\Assets\\DataStore\\DataRecorder.csv");
    }

    void Update() {
        timer += Time.deltaTime;
        if (/*RoomSpace != null && */time < timer) {
            timer = 0;
            UpdatePos();
        }
    }

    public void PushTable() {
        using (FileStream fs = new FileStream(FilePath, FileMode.Append)) {
            using (StreamWriter sw = new StreamWriter(fs)) {
                var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31}",
                    "Agent ID: " +id, 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31);
                sw.WriteLine(line);

                for (int i = 0; i < Mathf.Sqrt(RoomSpace.Length); i++) {
                    line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31}", i,
                    RoomSpace[i, 0], RoomSpace[i, 1], RoomSpace[i, 2], RoomSpace[i, 3], RoomSpace[i, 4], RoomSpace[i, 5], RoomSpace[i, 6]
                        , RoomSpace[i, 7] , RoomSpace[i, 8], RoomSpace[i, 9], RoomSpace[i, 10], RoomSpace[i, 11], RoomSpace[i, 12], RoomSpace[i, 13], RoomSpace[i, 14], RoomSpace[i, 15], RoomSpace[i, 16], RoomSpace[i, 17],  RoomSpace[i, 18], RoomSpace[i, 19], RoomSpace[i, 20], RoomSpace[i, 21], RoomSpace[i, 22], RoomSpace[i, 23], RoomSpace[i, 24]
                        , RoomSpace[i, 25], RoomSpace[i, 26], RoomSpace[i, 27], RoomSpace[i, 28], RoomSpace[i, 29], RoomSpace[i, 30]
                        );
                    sw.WriteLine(line);
                }
            }
        }
    }

    public void PullTable(int QSpaceX, int QSpaceY, int originx, int originy, int identity, int valx, int valy) {
        //generates local Q-Matrix that the agent utilises
        RoomSpace = new float[QSpaceX, QSpaceY];

        maxx = QSpaceX;
        maxy = QSpaceY;

        id = identity;

        PosX = originx;
        PosY = originy;

        OriginX = originx;
        OriginY = originy;

        ValX = valx;
        ValY = valy;

        RoomSpace[PosX, PosY] = -1;
        discount = Discount;

        gameObject.transform.position = new Vector3(PosX, 1, PosY);

    }

    public void UpdatePos() {
        Step++;

        if (PosX >= maxx-2 ) {
            MatrixValue[0] = -100;
        } else {
            if (0 == PosY % 6 || 0 == (PosX+1) % 6) {
                if (0 != (PosY - 3) % 6 && 0 != (PosX - 2) % 6) {
                    MatrixValue[0] = -100;
                }
            } else {
                MatrixValue[0] = RoomSpace[PosX + 1, PosY];//Checks Right Tile
            }
        }
        if (PosX == 1) {
            MatrixValue[1] = -100;
        } else {
            if (0 == PosY % 6 || 0 == (PosX-1) % 6) {
                if (0 != (PosY - 3) % 6 && 0 != (PosX - 4) % 6) {
                    MatrixValue[1] = -100;
                }
            } else {
                MatrixValue[1] = RoomSpace[PosX - 1, PosY];//Checks left Tile
            }
        }
        if (PosY>=maxy-2) {
            MatrixValue[2] = -100;
        } else {
            if (0 == (PosY+1) % 6 || 0 == PosX % 6) {
                if (0 != (PosY - 2) % 6 && 0 != (PosX - 3) % 6) {
                    MatrixValue[2] = -100;
                }
            } else {
                MatrixValue[2] = RoomSpace[PosX, PosY + 1];//Checks Above Tile
            }
        }
        if (PosY==1) {
            MatrixValue[3] = -100;
        } else {
            if (0 == (PosY-1) % 6 || 0 == PosX % 6) {
                if (0 != (PosY - 4) % 6 && 0 != (PosX - 3) % 6) {
                    MatrixValue[3] = -100;
                }
            } else {
                MatrixValue[3] = RoomSpace[PosX, PosY - 1];//Checks Below Tile
            }
        }

        //Resets array so that it can be used for finding values that are equal to the max
        bool[] valuearray;
        valuearray = new bool[MatrixValue.Length];//Sets Array length
        int equalchance = 0;
        for (int i = 0; i < MatrixValue.Length; i++) {
            if (Mathf.Max(MatrixValue) == MatrixValue[i])//Finds the highest value within the MatrixValue Array and sees if there are multiple instances of it.
            {
                valuearray[i] = true;
                equalchance++;
            } else {
                valuearray[i] = false;
            }

            //Only runs this section of the code if the value of i is 3, which should be the last of the array check
            if (i == MatrixValue.Length-1) {
                if (equalchance == 1) {
                    //move to that value                
                    for (int k = 0; k < MatrixValue.Length; k++) {
                        if (valuearray[k] == true) {
                            switch (k) {
                                case 0:
                                    ChangePos(1,0);
                                    break;
                                case 1:
                                    ChangePos(-1, 0);
                                    break;
                                case 2:
                                    ChangePos(0,1);
                                    break;
                                case 3:
                                    ChangePos(0,-1);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                } else { //If there is more than one instance of the highest reward potential, this section of the code will run.

                    float RandomValue = UnityEngine.Random.value;
                    //print("Agent's random choice of RandomValue is: " + RandomValue);
                    float flatprobability = (1 / (float)equalchance);
                    //print(flatprobability);
                    for (int j = 0; j < equalchance; j++) {
                        //print(0 + (flatprobability * j));
                        //print(flatprobability + (flatprobability * j));
                        if (RandomValue > 0 + (flatprobability*j) && RandomValue < flatprobability + (flatprobability * j)) {
                            int boolcheck = 0;
                            //move to that value
                            for (int l = 0; l < MatrixValue.Length; l++) {
                                if (valuearray[l] == true) {
                                    if (boolcheck == j) {
                                        switch (l) {
                                            case 0:
                                                ChangePos(1, 0);
                                                break;
                                            case 1:
                                                ChangePos(-1, 0);
                                                break;
                                            case 2:
                                                ChangePos(0, 1);
                                                break;
                                            case 3:
                                                ChangePos(0, -1);
                                                break;
                                            default:
                                                break;
                                        }
                                        j = equalchance;
                                    } else {
                                        boolcheck++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void ChangePos(int inputx, int inputy) {
        gameObject.transform.position = new Vector3(PosX + inputx, 1, PosY + inputy);
        if (RoomSpace[PosX, PosY] <= floor) {
            //print("Flooring: " + PosX + "," + PosY);
            RoomSpace[PosX, PosY] = floor;
        } else {
            float max = 0;
            for (int i = 0; i < Mathf.Sqrt(RoomSpace.Length); i++) {
                for (int j = 0; j < Mathf.Sqrt(RoomSpace.Length); j++) {
                    if (RoomSpace[i,j] > max) {
                        max = RoomSpace[i, j];
                    }
                }
            }
            entropy += Math.Abs(RoomSpace[PosX + inputx, PosY + inputy] - Math.Abs(step + (discount * RoomSpace[PosX, PosY])));//difference between last update to the newest one
            //RoomSpace[PosX + inputx, PosY + inputy] += discount * (step + gamma * max - RoomSpace[PosX, PosY]);
            RoomSpace[PosX, PosY] += step + (discount * RoomSpace[PosX + inputx, PosY + inputy]);
        }
        PosX += inputx;
        PosY += inputy;
        if (PosX == ValX && PosY == ValY) {
            print("Goal Found");
            RoomSpace[PosX, PosY] += maxReward;
            ResetAgent();
        }
        //print("current pos is: " + PosX + "," + PosY);
        if (Step >= maxstep) {
            ResetAgent();
        }
    }

    void ResetAgent() {
        GoalCount++;
        gameObject.transform.position = new Vector3(OriginX, 1, OriginY);
        PosX = OriginX;
        PosY = OriginY;
        using (FileStream fs = new FileStream(Recorder, FileMode.Append)) {
            using (StreamWriter sw = new StreamWriter(fs)) {
                var line = string.Format("{0},{1},{2}", "Agent ID: " + id, Step, entropy);
                sw.WriteLine(line);
            }
        }
        if (GoalReached>=GoalTarget) {
            PushTable();
            Manager.GetComponent<Q_Manager>().ReadyPull();
            gameObject.SetActive(false);
        }
        GoalReached++;
        Step = 0;
        entropy = 0;
    }
}