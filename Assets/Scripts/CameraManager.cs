using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    private void Start()
    {
        if (_camera == null)
            _camera = gameObject.GetComponent<Camera>();
    }


    public IEnumerator LerpCamera(Vector3 targetPos, Quaternion targetRot)
    {
        Debug.Log("[CameraManager] LerpCamera");

        for (float time = 0f; time <= 1f; time += Time.deltaTime)
        { 
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPos, time);
            _camera.transform.rotation = Quaternion.Lerp(_camera.transform.rotation, targetRot, time);
            yield return null;
        }
    } 
}
