using UnityEngine;
using System.Collections;

public class UnitySocialSettings : ScriptableObject
{
    public string clientId;

    public bool iosSupportEnabled;
    public bool androidSupportEnabled;

    public bool IsEnabled
    {
        get
        {
            #if UNITY_IPHONE
            return iosSupportEnabled;
            #elif UNITY_ANDROID
            return androidSupportEnabled;
            #else
            return false;
            #endif
        }
    }

    public bool IsValid
    {
        get
        {
            if (clientId != null && clientId.Trim().Length > 0)
            {
                return true;
            }
            return false;
        }
    }
}
