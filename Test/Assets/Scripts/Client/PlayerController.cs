using UnityEngine;

public class PlayerController : MonoBehaviour
{
    new Camera camera;

    private void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(camera.transform.position, camera.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10))
            {
                TCPClient.Instance.CallServerMethod("Explosion", hit.point, hit.normal);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            TCPClient.Instance.CallServerMethod("SwitchLight", 0);
        }
    }
}
