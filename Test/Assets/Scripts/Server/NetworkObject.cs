using System.Collections.Generic;
using UnityEngine;

public class NetworkObject : MonoBehaviour {

    public int NetworkId;

    public void SwitchLight(int id)
    {
        GameManager.SwitchLight(id);
    }

    public void Explosion(Vector3 pos, Vector3 normal)
    {
        GameManager.Explosion(pos, normal);
    }

    public void OnClientConnected()
    {
        Dictionary<int, bool> states = new Dictionary<int, bool>();
        for (int i = 0; i < GameManager.Instance.Lights.Count; i++)
        {
            states.Add(GameManager.Instance.Lights[i].Id, GameManager.Instance.Lights[i].switchedOn);
        }

        TCPServer.Instance.CallClientMethod("SetLights", NetworkId, states);
    }
}
