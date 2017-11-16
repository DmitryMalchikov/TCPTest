using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightServer : MonoBehaviour {
    public bool switchedOn = false;

    public int Id;

    public void SwitchLight()
    {
        switchedOn = !switchedOn;
        OnSwitch();
    }

    private void OnSwitch()
    {
        TCPServer.Instance.CallAllClientsMethod("SetLight", Id, switchedOn);
    }
}
