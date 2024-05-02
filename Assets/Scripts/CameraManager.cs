using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;

    // private float moveSpeed = 1.0f;
    public IEnumerator LerpCamera(Vector3 targetPos, Quaternion targetRot)
    {
        Debug.Log("[CameraManager] LerpCamera");

        for (float time = 0f; time <= 1f; time += Time.deltaTime)
        { 
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, time);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRot, time);
            yield return null;

        }
    } 
}
