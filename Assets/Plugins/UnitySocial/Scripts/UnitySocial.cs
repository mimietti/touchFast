#if UNITY_SOCIAL

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
#define UNITY_SOCIAL_SUPPORTED
#endif

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// Unity Social public API
/// </summary>
public class UnitySocial : MonoBehaviour
{
    public struct EntryPointState
    {
        public string imageURL;
        public int notificationCount;
    }

    public enum NotificationLocation { Hidden = 0, LeftTop, LeftBottom, RightTop, RightBottom };

    #if UNITY_SOCIAL_SUPPORTED
    private static UnitySocial s_UnitySocialInstance = null;
    private static bool s_AppIsClosing = false;
    #endif

    private static UnitySocial UnitySocialInstance
    {
        get
        {
            #if UNITY_SOCIAL_SUPPORTED
            if (s_UnitySocialInstance == null && !s_AppIsClosing)
            {
                UnitySocialSettings settings = (UnitySocialSettings) Resources.Load("UnitySocialSettings");

                if (settings != null)
                {
                    if (settings.IsEnabled)
                    {
                        GameObject gameObject = new GameObject("UnitySocial");

                        if (gameObject != null)
                        {
                            gameObject.name = gameObject.name + gameObject.GetInstanceID();
                            s_UnitySocialInstance = gameObject.AddComponent<UnitySocial>();

                            if (s_UnitySocialInstance != null)
                            {
                                UnitySocialInitialize(settings.clientId, s_UnitySocialInstance.name);
                            }

                            DontDestroyOnLoad(gameObject);
                        }
                    }
                }
            }

            return s_UnitySocialInstance;
            #else
            return null;
            #endif
        }
    }

    private static bool s_UnityPauseEngineAutomatically = true;

    // Public API

    /// <summary>
    /// Occurs when Unity Social view is opened
    /// </summary>
    public static event Action GameShouldPause;

    /// <summary>
    /// Occurs when Unity Social view is hidden
    /// </summary>
    public static event Action GameShouldResume;

    /// <summary>
    /// Occurs when entry point should update
    /// </summary>
    public static event Action<EntryPointState> EntryPointStateUpdated;

    /// <summary>
    /// Occurs when Unity Social is initialized
    /// </summary>
    public static event Action<bool> Initialized;

    /// <summary>
    /// Occurs when the user has earned a reward
    /// </summary>
    public static event Action<Dictionary<string, object>> RewardClaimed;

    /// <summary>
    /// Occurs when a new challenge should start
    /// </summary>
    public static event Action<Dictionary<string, object>, Dictionary<string, object>> ChallengeStarted;

    /// <summary>
    /// Initializes Unity Social
    /// </summary>
    public static void Initialize()
    {
        // If s_UnitySocialInstance is not yet initialized, calling UnitySocialInstance property getter will trigger the initialization
        if (UnitySocialInstance == null)
        {
            Debug.Log("Unable to initialize Unity Social. Unity Social is not enabled or available for this platform.");
        }
    }

    /// <summary>
    /// Gets or sets a boolean that determines if Unity player should be paused automatically when Unity Social view is opened
    /// </summary>
    public static bool pauseEngineAutomatically
    {
        get
        {
            return s_UnityPauseEngineAutomatically;
        }
        set
        {
            s_UnityPauseEngineAutomatically = value;
        }
    }

    /// <summary>
    /// Is Unity Social supported on this device
    /// </summary>
    public static bool isSupported
    {
        #if UNITY_SOCIAL_SUPPORTED
        get
        {
            return UnitySocialIsSupported();
        }
        #else
        get
        {
            return false;
        }
        #endif
    }

    /// <summary>
    /// Is Unity Social initialized and ready to be used
    /// </summary>
    public static bool isReady
    {
        get
        {
            #if UNITY_SOCIAL_SUPPORTED
            if (UnitySocialInstance != null)
            {
                return UnitySocialIsReady();
            }
            #endif
            return false;
        }
    }

    /// <summary>
    /// Gets entry point state
    /// </summary>
    public static EntryPointState entryPointState
    {
        get
        {
            #if UNITY_SOCIAL_SUPPORTED
            if (UnitySocialInstance != null)
            {
                string json = UnitySocialGetEntryPointState();
                if (json != null && json.Length > 0)
                {
                    Dictionary<string, object> dict = UnitySocialTools.DictionaryExtensions.JsonToDictionary(json);
                    EntryPointState newState = new EntryPointState();
                    if (UnitySocialTools.DictionaryExtensions.TryGetValue(dict, "imageURL", out newState.imageURL) &&
                        UnitySocialTools.DictionaryExtensions.TryGetValue(dict, "notificationCount", out newState.notificationCount))
                    {
                        return newState;
                    }
                }
            }
            #endif
            return default(EntryPointState);
        }
    }

    /// <summary>
    /// Gets or sets entry point image size
    /// </summary>
    private static bool s_EntryPointUpdatesEnabled = false;
    public static bool entryPointUpdatesEnabled
    {
        get
        {
            return s_EntryPointUpdatesEnabled;
        }
        set
        {
            s_EntryPointUpdatesEnabled = value;
            UpdateEntryPointSettings();
        }
    }

    /// <summary>
    /// Gets or sets entry point image size in pixels
    /// </summary>
    private static int s_EntryPointImageSize = 128;
    public static int entryPointImageSize
    {
        get
        {
            return s_EntryPointImageSize;
        }
        set
        {
            s_EntryPointImageSize = value;
            UpdateEntryPointSettings();
        }
    }

    /// <summary>
    /// Gets or sets the notification origin
    /// </summary>
    private static NotificationLocation s_NotificationLocation = NotificationLocation.LeftTop;
    public static NotificationLocation notificationLocation
    {
        get
        {
            return s_NotificationLocation;
        }
        set
        {
            s_NotificationLocation = value;
            UpdateNotificationSettings();
        }
    }

    /// <summary>
    /// Gets or sets the notification y offset from it's origin in pixels
    /// </summary>
    private static int s_NotificationOffset = 0;
    public static int notificationOffset
    {
        get
        {
            return s_NotificationOffset;
        }
        set
        {
            s_NotificationOffset = value;
            UpdateNotificationSettings();
        }
    }

    /// <summary>
    /// Starts a new Unity Social session
    /// </summary>
    public static void StartSession()
    {
        #if UNITY_SOCIAL_SUPPORTED
        if (UnitySocialInstance != null)
        {
            UnitySocialStartSession();
        }
        #endif
    }

    /// <summary>
    /// End the current Unity Social session
    /// </summary>
    /// <param name="data">A dictionary containing the session related metadata, e.g. score</param>
    public static void EndSession(Dictionary<string, object> data)
    {
        #if UNITY_SOCIAL_SUPPORTED
        if (UnitySocialInstance != null)
        {
            UnitySocialEndSession((data != null) ? UnitySocialTools.Json.Serialize(data) : null);
        }
        #endif
    }

    /// <summary>
    /// Called when the in game entry point is clicked
    /// </summary>
    public static void EntryPointClicked()
    {
        #if UNITY_SOCIAL_SUPPORTED
        if (UnitySocialInstance != null)
        {
            UnitySocialEntryPointClicked();
        }
        #endif
    }

    /// <summary>
    /// Set Unity Social Manifest Server
    /// </summary>
    /// <param name="manifestServer">A string pointing to the manifest server</param>
    public static void SetManifestServer(string manifestServer)
    {
        #if UNITY_SOCIAL_SUPPORTED
        if (UnitySocialInstance != null)
        {
            UnitySocialSetManifestServer(manifestServer);
        }
        #endif
    }

    /// <summary>
    /// Set Unity Social color theme
    /// </summary>
    /// <param name="theme">A dictionary containing the color theme. Must have keys "main" and "accent"</param>
    public static void SetColorTheme(Dictionary<string, string> theme)
    {
        #if UNITY_SOCIAL_SUPPORTED
        if (UnitySocialInstance != null)
        {
            UnitySocialSetColorTheme((theme != null) ? UnitySocialTools.Json.Serialize(theme) : null);
        }
        #endif
    }

    /// <summary>
    /// Add multiple tags
    /// </summary>
    /// <param name="tags">One or more string tag parameters to be added</param>
    public static void AddTags(params string[] tags)
    {
        #if UNITY_SOCIAL_SUPPORTED
        if (UnitySocialInstance != null && tags != null && tags.Length > 0)
        {
            UnitySocialAddTags(UnitySocialTools.Json.Serialize(tags));
        }
        #endif
    }

    /// <summary>
    /// Remove multiple tags
    /// </summary>
    /// <param name="tags">One or more string tag parameters to be removed</param>
    public static void RemoveTags(params string[] tags)
    {
        #if UNITY_SOCIAL_SUPPORTED
        if (UnitySocialInstance != null && tags != null && tags.Length > 0)
        {
            UnitySocialRemoveTags(UnitySocialTools.Json.Serialize(tags));
        }
        #endif
    }

    // Batch updates

    private static bool s_UpdatingEntryPointSettings = false;
    private static void UpdateEntryPointSettings()
    {
        if (!s_UpdatingEntryPointSettings)
        {
            #if UNITY_SOCIAL_SUPPORTED
            if (UnitySocialInstance != null)
            {
                UnitySocialInstance.StartCoroutine(BatchEntryPointSettings());
            }
            #endif
            s_UpdatingEntryPointSettings = true;
        }
    }

    private static IEnumerator BatchEntryPointSettings()
    {
        yield return new WaitForEndOfFrame();
        #if UNITY_SOCIAL_SUPPORTED
        if (s_EntryPointUpdatesEnabled)
        {
            UnitySocialEnableEntryPointUpdatesWithImageSize(s_EntryPointImageSize);
        }
        else
        {
            UnitySocialDisableEntryPointUpdates();
        }
        #endif
        s_UpdatingEntryPointSettings = false;
    }

    private static bool s_UpdatingNotificationSettings = false;
    private static void UpdateNotificationSettings()
    {
        if (!s_UpdatingNotificationSettings)
        {
            #if UNITY_SOCIAL_SUPPORTED
            if (UnitySocialInstance != null)
            {
                UnitySocialInstance.StartCoroutine(BatchNotificationSettings());
            }
            #endif
            s_UpdatingNotificationSettings = true;
        }
    }

    private static IEnumerator BatchNotificationSettings()
    {
        yield return new WaitForEndOfFrame();
        #if UNITY_SOCIAL_SUPPORTED
        if (UnitySocialInstance != null)
        {
            switch (s_NotificationLocation)
            {
            case NotificationLocation.LeftTop:
                UnitySocialShowNotificationActorOnLeftTop(s_NotificationOffset);
                break;

            case NotificationLocation.LeftBottom:
                UnitySocialShowNotificationActorOnLeftBottom(s_NotificationOffset);
                break;

            case NotificationLocation.RightTop:
                UnitySocialShowNotificationActorOnRightTop(s_NotificationOffset);
                break;

            case NotificationLocation.RightBottom:
                UnitySocialShowNotificationActorOnRightBottom(s_NotificationOffset);
                break;

            default:
                UnitySocial.UnitySocialHideNotifications();
                break;
            }
        }
        #endif
        s_UpdatingNotificationSettings = false;
    }

    // MonoBehaviour methods

    void OnApplicationQuit()
    {
        #if UNITY_SOCIAL_SUPPORTED
        s_UnitySocialInstance = null;
        s_AppIsClosing = true;
        #endif

        Initialized = null;

        GameShouldPause = null;
        GameShouldResume = null;

        RewardClaimed = null;
        ChallengeStarted = null;
    }

    // From native

    private void UnitySocialGameShouldPause()
    {
        if (GameShouldPause != null)
        {
            GameShouldPause();
        }

        #if UNITY_SOCIAL_SUPPORTED
        if (s_UnityPauseEngineAutomatically)
        {
            UnitySocialPauseEngine(true);
        }
        #endif
    }

    private void UnitySocialGameShouldResume()
    {
        if (GameShouldResume != null)
        {
            GameShouldResume();
        }
    }

    private void UnitySocialUpdateEntryPointState(string data)
    {
        if (EntryPointStateUpdated != null && data != null && data.Length > 0)
        {
            Dictionary<string, object> dict = UnitySocialTools.DictionaryExtensions.JsonToDictionary(data);
            EntryPointState newState = new EntryPointState();
            if (UnitySocialTools.DictionaryExtensions.TryGetValue(dict, "imageURL", out newState.imageURL) &&
                UnitySocialTools.DictionaryExtensions.TryGetValue(dict, "notificationCount", out newState.notificationCount))
            {
                EntryPointStateUpdated(newState);
            }
        }
    }

    private void UnitySocialInitialized(string result)
    {
        if (Initialized != null && result != null && result.Length > 0)
        {
            Dictionary<string, object> dict = UnitySocialTools.DictionaryExtensions.JsonToDictionary(result);
            bool isSupported;

            if (UnitySocialTools.DictionaryExtensions.TryGetValue(dict, "isSupported", out isSupported))
            {
                Initialized(isSupported);
            }
        }
    }

    private void UnitySocialRewardClaimed(string metadata)
    {
        if (RewardClaimed != null && metadata != null && metadata.Length > 0)
        {
            Dictionary<string, object> metadataDictionary = UnitySocialTools.DictionaryExtensions.JsonToDictionary(metadata);
            RewardClaimed(metadataDictionary);
        }
    }

    private void UnitySocialChallengeStarted(string challengeAndMetadata)
    {
        if (ChallengeStarted != null && challengeAndMetadata != null && challengeAndMetadata.Length > 0)
        {
            Dictionary<string, object> dictionary = UnitySocialTools.DictionaryExtensions.JsonToDictionary(challengeAndMetadata);
            if (dictionary != null && dictionary.ContainsKey("challenge") && dictionary.ContainsKey("metadata"))
            {
                Dictionary<string, object> challengeDictionary = dictionary["challenge"] as Dictionary<string, object>;
                Dictionary<string, object> metadataDictionary = dictionary["metadata"] as Dictionary<string, object>;

                if (challengeDictionary != null && metadataDictionary != null)
                {
                    ChallengeStarted(challengeDictionary, metadataDictionary);
                }
            }
        }
    }

    // To native

    #if UNITY_SOCIAL_SUPPORTED

    #if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void UnitySocialInitialize(string clientId, string eventListenerName);

    [DllImport("__Internal")]
    private static extern void UnitySocialStartSession();

    [DllImport("__Internal")]
    private static extern bool UnitySocialIsSupported();

    [DllImport("__Internal")]
    private static extern bool UnitySocialIsReady();

    [DllImport("__Internal")]
    private static extern void UnitySocialEndSession(string data);

    [DllImport("__Internal")]
    private static extern void UnitySocialPauseEngine(bool pause);

    [DllImport("__Internal")]
    private static extern void UnitySocialShowNotificationActorOnLeftTop(float top);

    [DllImport("__Internal")]
    private static extern void UnitySocialShowNotificationActorOnLeftBottom(float bottom);

    [DllImport("__Internal")]
    private static extern void UnitySocialShowNotificationActorOnRightTop(float top);

    [DllImport("__Internal")]
    private static extern void UnitySocialShowNotificationActorOnRightBottom(float bottom);

    [DllImport("__Internal")]
    private static extern void UnitySocialHideNotifications();

    [DllImport("__Internal")]
    private static extern void UnitySocialEntryPointClicked();

    [DllImport("__Internal")]
    private static extern string UnitySocialGetEntryPointState();

    [DllImport("__Internal")]
    private static extern void UnitySocialEnableEntryPointUpdatesWithImageSize(int sizeInPhysicalPixels);

    [DllImport("__Internal")]
    private static extern void UnitySocialDisableEntryPointUpdates();

    [DllImport("__Internal")]
    private static extern void UnitySocialSetColorTheme(string theme);

    [DllImport("__Internal")]
    private static extern void UnitySocialSetManifestServer(string manifestServer);

    [DllImport("__Internal")]
    private static extern void UnitySocialAddTags(string data);

    [DllImport("__Internal")]
    private static extern void UnitySocialRemoveTags(string data);

    [DllImport("__Internal")]
    private static extern void UnitySocialUnityMessageReceived(string eventName, string data);

    #elif UNITY_ANDROID

    private static AndroidJavaObject s_UnitySocialClass = null;

    private static void UnitySocialInitialize(string clientId, string eventListenerName)
    {
        if (s_UnitySocialClass == null)
        {
            s_UnitySocialClass = new AndroidJavaObject("com.unity.unitysocial.UnityWrapper");
            s_UnitySocialClass.Call("initialize", clientId, eventListenerName);
        }
    }

    private static void UnitySocialHideNotifications()
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("hideNotifications");
        }
    }

    private static void UnitySocialShowNotificationActorOnLeftTop(float top)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("showNotificationActorOnLeftTop", top);
        }
    }

    private static void UnitySocialShowNotificationActorOnLeftBottom(float bottom)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("showNotificationActorOnLeftBottom", bottom);
        }
    }

    private static void UnitySocialShowNotificationActorOnRightTop(float top)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("showNotificationActorOnRightTop", top);
        }
    }

    private static void UnitySocialShowNotificationActorOnRightBottom(float bottom)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("showNotificationActorOnRightBottom", bottom);
        }
    }

    private static void UnitySocialDisableEntryPointUpdates()
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("disableEntryPointUpdates");
        }
    }

    private static void UnitySocialEnableEntryPointUpdatesWithImageSize(int sizeInPhysicalPixels)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("enableEntryPointUpdatesWithImageSize", sizeInPhysicalPixels);
        }
    }

    private static String UnitySocialGetEntryPointState()
    {
        if (s_UnitySocialClass != null)
        {
            return s_UnitySocialClass.Call<string>("getEntryPointState");
        }
        return null;
    }

    private static void UnitySocialEntryPointClicked()
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("entryPointClicked");
        }
    }

    private static void UnitySocialStartSession()
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("startSession");
        }
    }

    private static bool UnitySocialIsSupported()
    {
        if (s_UnitySocialClass != null)
        {
            return s_UnitySocialClass.Call<bool>("isSupported");
        }
        return false;
    }

    private static bool UnitySocialIsReady()
    {
        if (s_UnitySocialClass != null)
        {
            return s_UnitySocialClass.Call<bool>("isReady");
        }
        return false;
    }

    private static void UnitySocialEndSession(string data)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("endSession", data);
        }
    }

    private static void UnitySocialSetColorTheme(string theme)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("setColorTheme", theme);
        }
    }

    private static void UnitySocialSetManifestServer(string manifestServer)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("setManifestServer", manifestServer);
        }
    }

    private static void UnitySocialPauseEngine(bool pause)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("pauseEngine", pause);
        }
    }

    private static void UnitySocialAddTags(string data)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("addTags", data);
        }
    }

    private static void UnitySocialRemoveTags(string data)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("removeTags", data);
        }
    }

    private static void UnitySocialUnityMessageReceived(string methodName, string data)
    {
        if (s_UnitySocialClass != null)
        {
            s_UnitySocialClass.Call("unityMessageReceived", methodName, data);
        }
    }

    #endif
    #endif//UNITY_SOCIAL_SUPPORTED
}

#endif
