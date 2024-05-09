using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UIDField : MonoBehaviour
{
    [FormerlySerializedAs("UID_field")]
    [SerializeField]
    private TMP_Text _idField;
    
    private void Start()
    {
        SetUIDField();
    }

    private void SetUIDField()
    {
        _idField.text = "UID : " + PhotonNetwork.LocalPlayer.NickName;
    }
}
