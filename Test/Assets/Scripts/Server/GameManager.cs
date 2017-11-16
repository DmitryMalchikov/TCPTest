using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<LightServer> Lights;

    private LightServer GetLight(int id)
    {
        return Lights.Find(l => l.Id == id);
    }

    public static void SwitchLight(int id)
    {
        Instance.GetLight(id).SwitchLight();
    }

    public static void Explosion(Vector3 position, Vector3 normal)
    {
        TCPServer.Instance.CallAllClientsMethod("MakeExplosion", position, normal);
    }
}
