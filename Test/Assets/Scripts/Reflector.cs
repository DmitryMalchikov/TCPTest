using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using UnityEngine;

public class Reflector
{
    public static void InvokeMethod<T>(T instance, string methodCall)
    {
        try
        {
            MethodCall mc = JsonConvert.DeserializeObject<MethodCall>(methodCall);
        MethodBase method = typeof(T).GetMethod(mc.MethodName);
        ParameterInfo[] parameters = method.GetParameters();

        for (int i = 0; i < mc.Parameters.Length; i++)
        {
            JObject param = mc.Parameters[i] as JObject;

            if (param != null)
            {
                mc.Parameters[i] = param.ToObject(parameters[i].ParameterType);
            }
            else
            {
                mc.Parameters[i] = Convert.ChangeType(mc.Parameters[i], parameters[i].ParameterType);
            }
        }

        method.Invoke(instance, mc.Parameters);
    }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }
}

public struct MethodCall
{
    public string MethodName;
    public object[] Parameters;
}
