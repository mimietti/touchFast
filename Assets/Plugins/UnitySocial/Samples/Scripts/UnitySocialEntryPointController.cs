using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

public class UnitySocialEntryPointController : MonoBehaviour
{
    public enum NotificationLocation { Hidden = 0, LeftTop, LeftBottom, RightTop, RightBottom };

    public int profileImageSize = 128;
    public NotificationLocation notificationLocation = NotificationLocation.LeftTop;
    public int notificationOffset = 0;
    public Image entryPointIconImage;
    public GameObject notificationBadge;
    public Text notificationBadgeCountText;

    #if UNITY_SOCIAL
    private Color m_EntryPointDefaultColor;
    private Sprite m_EntryPointDefaultSprite;
    private static int m_NotificationCount = 0;
    private static Sprite m_CurrentProfileSprite = null;

    void OnEnable() //Initialization happens on OnEnable so the button can be easily hidden completely by setting the gameObject as inactive
    {
        if (!m_EntryPointDefaultSprite && entryPointIconImage)   //Store default sprite
        {
            m_EntryPointDefaultSprite = entryPointIconImage.sprite;
            m_EntryPointDefaultColor = entryPointIconImage.color;
        }

        if (entryPointIconImage && notificationBadge && m_EntryPointDefaultSprite && notificationBadgeCountText)   //If all is good, subscribe to the notification updated event
        {
            UnitySocial.EntryPointStateUpdated += HandleUnitySocialEntryPointStateUpdate;

            UnitySocial.notificationLocation = (UnitySocial.NotificationLocation)notificationLocation;
            UnitySocial.notificationOffset = notificationOffset;
            UnitySocial.entryPointUpdatesEnabled = true;
            UnitySocial.entryPointImageSize = profileImageSize; //Configure the image size that the server returns

            notificationBadge.SetActive(m_NotificationCount != 0);

            if (m_CurrentProfileSprite != null)
            {
                entryPointIconImage.sprite = m_CurrentProfileSprite;
                entryPointIconImage.color = Color.white;
            }

            StartCoroutine(LoadInitialNotificationState());
        }
    }

    void OnDisable()
    {
        if (entryPointIconImage && notificationBadge && m_EntryPointDefaultSprite)   //No need to unsubscribe if it wasn't subscribed in the first place
        {
            UnitySocial.EntryPointStateUpdated -= HandleUnitySocialEntryPointStateUpdate;
            UnitySocial.notificationLocation = UnitySocial.NotificationLocation.Hidden;
            UnitySocial.entryPointUpdatesEnabled = false; //Do not fire notification events when the Entry Point is disabled
            notificationBadge.SetActive(false);
        }
    }

    private IEnumerator LoadInitialNotificationState()
    {
        yield return null;
        HandleUnitySocialEntryPointStateUpdate(UnitySocial.entryPointState);
    }

    private void HandleUnitySocialEntryPointStateUpdate(UnitySocial.EntryPointState newState) //newState contains ImageURL for profile image && NotificationCount
    {
        m_NotificationCount = newState.notificationCount;

        if (!string.IsNullOrEmpty(newState.imageURL))   //If we get a valid path, we want to grab the image from the internet
        {
            StartCoroutine(ChangeProfileImage(newState.imageURL)); //Coroutine because WWW queries take time
        }
        else //If the path is not valid, let's use the original image
        {
            entryPointIconImage.sprite = m_EntryPointDefaultSprite;
            entryPointIconImage.color = m_EntryPointDefaultColor;
            m_CurrentProfileSprite = null;
        }

        //Notification count update
        if (newState.notificationCount != 0)   //If the amount of notifications != 0, we want to show how many notifications the user has
        {
            notificationBadge.SetActive(true); //Set the badge active
            string newText = "9+";
            if (newState.notificationCount < 10 && newState.notificationCount > 0)   //If the count fits in one number, let's use that
            {
                newText = newState.notificationCount.ToString();
            }
            else
            {
                newText = " "; //In case the system returns a negative number, there are pending notifications but the amount is massive
            }
            notificationBadgeCountText.text = newText;
        }
        else //If the notification count is 0, we want to hide the notification badge
        {
            notificationBadge.SetActive(false);
        }
    }

    private IEnumerator ChangeProfileImage(string URL) //Used to pull the profile image from web and show it on the entry point button
    {
        Texture2D newTexture = new Texture2D(profileImageSize, profileImageSize, TextureFormat.ARGB32, false); //New texture to contain the image from web
        WWW request = new WWW(URL);
        yield return request;

        if (m_NotificationCount != 0)
        {
            request.LoadImageIntoTexture(newTexture);
            Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f)); //Create sprite from the texture

            entryPointIconImage.sprite = newSprite;
            entryPointIconImage.color = Color.white; //Remove possible tinting from image

            m_CurrentProfileSprite = newSprite;
        }
    }

    #endif

    public void OpenUnitySocial()
    {
        #if UNITY_SOCIAL
        UnitySocial.EntryPointClicked();
        #endif
    }

    #if UNITY_EDITOR
    private const string kUILayerName = "UI";
    private const string kSampleAssetFolder = "Assets/Plugins/UnitySocial/Samples/";

    [MenuItem("Edit/Unity Social/Create Entry Point")]
    public static void CreateEntryPoint()
    {
        if (GameObject.FindObjectOfType(typeof(UnitySocialEntryPointController)) != null)
        {
            EditorUtility.DisplayDialog("Unity Social Entry Point already exists", "New Entry Point was not added.", "Ok", null);
            return;
        }
        Canvas targetCanvas = GetTargetCanvas();

        if (targetCanvas != null)
        {
            GameObject container = new GameObject("US_EntryPointContainer", typeof(RectTransform), typeof(UnitySocialEntryPointController));
            GameObject entryPoint = new GameObject("US_EntryPoint", typeof(CanvasRenderer), typeof(Image), typeof(Button));
            GameObject entryPointIcon = new GameObject("US_EntryPointIcon", typeof(Image));
            GameObject notificationBadge = new GameObject("US_NotificationBadge", typeof(Image));
            GameObject notificationCount = new GameObject("US_NotificationCount", typeof(Text));

            if (container != null && entryPoint != null && entryPointIcon != null && notificationBadge != null && notificationCount != null)
            {
                container.transform.SetParent(targetCanvas.transform);
                container.layer = LayerMask.NameToLayer(kUILayerName);

                entryPoint.transform.SetParent(container.transform);
                entryPoint.layer = LayerMask.NameToLayer(kUILayerName);

                entryPointIcon.transform.SetParent(entryPoint.transform);
                entryPointIcon.layer = LayerMask.NameToLayer(kUILayerName);

                notificationBadge.transform.SetParent(container.transform.transform);
                notificationBadge.layer = LayerMask.NameToLayer(kUILayerName);

                notificationCount.transform.SetParent(notificationBadge.transform);
                notificationCount.layer = LayerMask.NameToLayer(kUILayerName);

                RectTransform containerRectTransform = container.GetComponent<RectTransform>();

                if (containerRectTransform != null)
                {
                    containerRectTransform.anchorMin = Vector2.zero;
                    containerRectTransform.anchorMax = Vector2.zero;
                    containerRectTransform.pivot = Vector2.zero;
                    containerRectTransform.position = new Vector2(16.0f, 16.0f);
                }

                Image entryPointImage = entryPoint.GetComponent<Image>();

                if (entryPointImage != null)
                {
                    entryPointImage.material = AssetDatabase.LoadAssetAtPath(kSampleAssetFolder + "Materials/UI-CircleDefault.mat", typeof(Material)) as Material;
                    entryPointImage.sprite = AssetDatabase.LoadAssetAtPath(kSampleAssetFolder + "Textures/social-btn-circle.png", typeof(Sprite)) as Sprite;
                }

                Image entryPointIconImage = entryPointIcon.GetComponent<Image>();

                if (entryPointIconImage != null)
                {
                    entryPointIconImage.material = AssetDatabase.LoadAssetAtPath(kSampleAssetFolder + "Materials/UI-CircleDefault.mat", typeof(Material)) as Material;
                    entryPointIconImage.sprite = AssetDatabase.LoadAssetAtPath(kSampleAssetFolder + "Textures/social-btn-icon.png", typeof(Sprite)) as Sprite;
                    entryPointIconImage.color = new Color(98.0f / 255.0f, 52.0f / 255.0f, 165.0f / 255.0f, 255.0f);
                }

                Image notificationBadgeImage = notificationBadge.GetComponent<Image>();

                if (notificationBadgeImage != null)
                {
                    notificationBadgeImage.sprite = AssetDatabase.LoadAssetAtPath(kSampleAssetFolder + "Textures/notification-badge.png", typeof(Sprite)) as Sprite;
                    notificationBadgeImage.rectTransform.sizeDelta = new Vector2(35.0f, 35.0f);
                    notificationBadgeImage.rectTransform.anchorMin = new Vector2(1.0f, 1.0f);
                    notificationBadgeImage.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                    notificationBadgeImage.rectTransform.anchoredPosition = new Vector3(-15.0f, -15.0f, 0.0f);
                }

                Text notificationBadgeCountText = notificationCount.GetComponent<Text>();

                if (notificationBadgeCountText != null)
                {
                    notificationBadgeCountText.rectTransform.sizeDelta = new Vector2(35.0f, 35.0f);
                    notificationBadgeCountText.alignment = TextAnchor.MiddleCenter;
                    notificationBadgeCountText.text = "9+";
                    notificationBadgeCountText.fontSize = 20;
                    notificationBadgeCountText.fontStyle = FontStyle.Bold;
                }

                Button button = entryPoint.GetComponent<Button>();

                if (button != null)
                {
                    UnitySocialEntryPointController entryPointController = container.GetComponent<UnitySocialEntryPointController>();

                    if (entryPointController != null)
                    {
                        UnityEventTools.AddPersistentListener(button.onClick, entryPointController.OpenUnitySocial);

                        entryPointController.entryPointIconImage = entryPointIconImage;
                        entryPointController.notificationBadge = notificationBadge;
                        entryPointController.notificationBadgeCountText = notificationBadgeCountText;
                    }
                }
            }
        }

        Debug.Log("Unity Social entrypoint created.");
    }

    private static Canvas GetTargetCanvas()
    {
        Canvas[] allCanvases = GameObject.FindObjectsOfType(typeof(Canvas)) as Canvas[];

        Canvas targetCanvas = null;

        if (allCanvases != null && allCanvases.Length > 0)
        {
            targetCanvas = allCanvases[0];
        }
        else
        {
            GameObject obj = new GameObject("Canvas", typeof(Canvas));
            if (obj != null)
            {
                targetCanvas = obj.GetComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                obj.AddComponent<CanvasScaler>();
                obj.AddComponent<GraphicRaycaster>();

                if (!GameObject.FindObjectOfType(typeof(EventSystem)))
                {
                    GameObject evt = new GameObject("EventSystem");

                    if (evt != null)
                    {
                        evt.AddComponent<EventSystem>();
                        evt.AddComponent<StandaloneInputModule>();
                    }
                }
            }
        }

        return targetCanvas;
    }

    #endif
}
