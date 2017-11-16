using System.Collections.Generic;
using UnityEngine;

public class GameManagerClient : Singleton<GameManagerClient>
{
    public List<LightClient> Lights;
    public GameObject Explosion;

    private LightClient GetLight(int id)
    {
        return Lights.Find(l => l.Id == id);
    }

    public void SetLight(int id, bool on)
    {
        GetLight(id).SetLight(on);
    }

    public void SetLights(Dictionary<int, bool> states)
    {
        foreach (var state in states)
        {
            SetLight(state.Key, state.Value);
        }
    }

    public void MakeExplosion(Vector3 position, Vector3 normal)
    {
        var rotation = Quaternion.FromToRotation(transform.up, normal);
        Instantiate(Explosion, position, rotation);
    }
}
