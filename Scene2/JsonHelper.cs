// using System;
// using UnityEngine;

// public static class JsonHelper
// {
//     public static T[] FromJson<T>(string json)
//     {
//         Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(WrapJson(json));
//         return wrapper.Items;
//     }

//     private static string WrapJson(string raw)
//     {
//         return "{\"Items\":" + raw + "}";
//     }

//     [Serializable]
//     private class Wrapper<T>
//     {
//         public T[] Items;
//     }
// }

using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper
{
    public static List<T> FromJsonList<T>(string json)
    {
        string wrapped = "{\"items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return new List<T>(wrapper.items);
    }

    [Serializable]
    private class Wrapper<T> { public T[] items; }
}

