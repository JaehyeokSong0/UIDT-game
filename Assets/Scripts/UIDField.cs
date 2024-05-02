using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class UIDField : MonoBehaviour
{
    public TMP_Text UID_field;
    
    // Start is called before the first frame update
    void Start()
    {
        SetUIDField();
    }

    public void SetUIDField()
    {
        UID_field.text = "UID : " + PhotonNetwork.LocalPlayer.NickName;
    }
}
