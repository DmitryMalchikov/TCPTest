using UnityEngine;

public class LightClient : MonoBehaviour {
    public bool switchedOn = false;

    public int Id;

    new Light light;

    private void Start()
    {
        light = GetComponent<Light>();
    }

    public void SetLight(bool on)
    {
        switchedOn = on;
        light.enabled = switchedOn;
    }
}
