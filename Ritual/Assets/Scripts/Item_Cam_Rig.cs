using Unity.Cinemachine;
using UnityEngine;
using Yarn.Unity;

public class Item_Cam_Rig : MonoBehaviour
{
    public Transform item_loc;
    public int pNum = 20;
    public CinemachineCamera item_cCam;

    [YarnCommand("cam_move")]
    public void cam_move(string locName)
    {
        if (locName == "item")
        {
            item_cCam.transform.position = item_loc.position;
            item_cCam.Priority = pNum;
        }
        else if (locName == "return")
        {
            item_cCam.Priority = 0;
        }
    }
}
