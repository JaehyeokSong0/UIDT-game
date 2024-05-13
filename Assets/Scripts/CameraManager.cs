using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
    static bool _isMoving = false;

    private void Start()
    {
        if (_camera == null)
            _camera = gameObject.GetComponent<Camera>();
    }


    public IEnumerator LerpCamera(Vector3 targetPos, Quaternion targetRot)
    {
        if (_isMoving)
            yield break;

        Debug.Log("[CameraManager] LerpCamera");

        float time = 0f;
        float lerpSpeed = 0.3f;

        _isMoving = true;
        while(Vector3.Distance(_camera.transform.position, targetPos) > 0.05f)
        { 
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPos, time * lerpSpeed);
            _camera.transform.rotation = Quaternion.Lerp(_camera.transform.rotation, targetRot, time * lerpSpeed);
            time += Time.deltaTime;

            yield return null;
        }
        _isMoving = false;
    } 
}
