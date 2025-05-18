using UnityEngine;

public class Utils : MonoBehaviour
{

    public static Vector3 ScreenToWorld(Camera camera, Vector3 pos)
    {
        pos.z = camera.nearClipPlane;
        return camera.ScreenToWorldPoint(pos);
    }

}
