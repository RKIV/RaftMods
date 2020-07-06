using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Linq;
using Steamworks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

[ModTitle("Cinema Mod")]
[ModDescription("Adds screens into Raft! You can watch Youtube videos, Soon twitch streams. It has custom sounds, custom blocks, custom items and way more!")]
[ModAuthor("TeKGameR")]
[ModIconUrl("https://i.imgur.com/9903JvN.png")]
[ModWallpaperUrl("https://i.imgur.com/il9eGoq.jpg")]
[ModVersionCheckUrl("https://raftmodding.com/api/v1/mods/cinema-mod/version.txt")]
[ModVersion("1.6")]
[RaftVersion("Update 11")]
[ModIsPermanent(true)]
public class CinemaMod : Mod
{
    public static GameObject menu;
    public static bool isMenuShown;
    public static CinemaScreen currentScreen;
    public static AssetBundle assetbundle;
    public static Material novideoMaterial;
    public static CinemaMod instance;

    AssetBundleRequest request = null;

    IEnumerator Start()
    {
        instance = this;
        RNotification notification = FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.spinning, "Loading CinemaMod...");

        var bundleLoadRequest = AssetBundle.LoadFromFileAsync("mods\\ModData\\CinemaMod\\cinemamod.assets");
        yield return bundleLoadRequest;
        assetbundle = bundleLoadRequest.assetBundle;

        if (assetbundle == null)
        {
            RConsole.LogError("Failed to load AssetBundle for CinemaMod!");
            notification.Close();
            FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "CinemaMod is not properly installed!").SetIcon(RNotify.ErrorSprite);

            yield return null;
        }

        request = assetbundle.LoadAssetAsync<Material>("Black");
        yield return request;
        novideoMaterial = request.asset as Material;

        request = assetbundle.LoadAssetAsync<GameObject>("MenuCanvas");
        yield return request;
        GameObject MenuCanvas = request.asset as GameObject;

        menu = Instantiate(MenuCanvas, transform);
        menu.GetComponent<CanvasGroup>().alpha = 0;
        menu.GetComponent<CanvasGroup>().interactable = false;
        menu.GetComponent<CanvasGroup>().blocksRaycasts = false;
        menu.transform.Find("Background").Find("TopBar").Find("CloseButton").GetComponent<Button>().onClick.AddListener(CloseCinemaMenu);
        menu.transform.Find("Background").Find("PlayButton").GetComponent<Button>().onClick.AddListener(() => { if (currentScreen != null) { currentScreen.Play(); MessageHandler.SendMessage(new CMessage_CinemaModPosition((Messages)MessageHandler.NetworkMessages.CinemaMod_Play, currentScreen.transform.localPosition)); } });
        menu.transform.Find("Background").Find("PauseButton").GetComponent<Button>().onClick.AddListener(() => { if (currentScreen != null) { currentScreen.Pause(); MessageHandler.SendMessage(new CMessage_CinemaModPosition((Messages)MessageHandler.NetworkMessages.CinemaMod_Pause, currentScreen.transform.localPosition)); } });
        menu.transform.Find("Background").Find("StopButton").GetComponent<Button>().onClick.AddListener(() => { if (currentScreen != null) { currentScreen.Stop(); MessageHandler.SendMessage(new CMessage_CinemaModPosition((Messages)MessageHandler.NetworkMessages.CinemaMod_Stop, currentScreen.transform.localPosition)); } });

        request = assetbundle.LoadAssetAsync<Item_Base>("raftsungtv_item");
        yield return request;
        Item_Base TV = request.asset as Item_Base;
        TV.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<CinemaScreen>();
        TV.settings_buildable.GetBlockPrefab(1).gameObject.AddComponent<CinemaScreen>();
        RAPI.RegisterItem(TV);
        RAPI.AddItemToBlockQuadType(TV, RBlockQuadType.quad_floor);
        RAPI.AddItemToBlockQuadType(TV, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(TV, RBlockQuadType.quad_table);
        RAPI.AddItemToBlockQuadType(TV, RBlockQuadType.quad_wall);

        request = assetbundle.LoadAssetAsync<Item_Base>("laptoppc_item");
        yield return request;
        Item_Base LaptopPC = request.asset as Item_Base;
        LaptopPC.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<CinemaScreen>();
        RAPI.RegisterItem(LaptopPC);
        RAPI.AddItemToBlockQuadType(LaptopPC, RBlockQuadType.quad_floor);
        RAPI.AddItemToBlockQuadType(LaptopPC, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(LaptopPC, RBlockQuadType.quad_table);

        request = assetbundle.LoadAssetAsync<Item_Base>("pcscreen_item");
        yield return request;
        Item_Base PCScreen = request.asset as Item_Base;
        PCScreen.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<CinemaScreen>();
        RAPI.RegisterItem(PCScreen);
        RAPI.AddItemToBlockQuadType(PCScreen, RBlockQuadType.quad_floor);
        RAPI.AddItemToBlockQuadType(PCScreen, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(PCScreen, RBlockQuadType.quad_table);

        request = assetbundle.LoadAssetAsync<Item_Base>("oldtv_item");
        yield return request;
        Item_Base OldTV = request.asset as Item_Base;
        OldTV.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<CinemaScreen>();
        RAPI.RegisterItem(OldTV);
        RAPI.AddItemToBlockQuadType(OldTV, RBlockQuadType.quad_floor);
        RAPI.AddItemToBlockQuadType(OldTV, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(OldTV, RBlockQuadType.quad_table);

        request = assetbundle.LoadAssetAsync<Item_Base>("largeraftsungtv_item");
        yield return request;
        Item_Base LargeTV = request.asset as Item_Base;
        LargeTV.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<CinemaScreen>();
        LargeTV.settings_buildable.GetBlockPrefab(1).gameObject.AddComponent<CinemaScreen>();
        RAPI.RegisterItem(LargeTV);
        RAPI.AddItemToBlockQuadType(LargeTV, RBlockQuadType.quad_floor);
        RAPI.AddItemToBlockQuadType(LargeTV, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(LargeTV, RBlockQuadType.quad_table);
        RAPI.AddItemToBlockQuadType(LargeTV, RBlockQuadType.quad_wall);

        request = assetbundle.LoadAssetAsync<Item_Base>("xlraftsungtv_item");
        yield return request;
        Item_Base XLTV = request.asset as Item_Base;
        XLTV.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<CinemaScreen>();
        XLTV.settings_buildable.GetBlockPrefab(1).gameObject.AddComponent<CinemaScreen>();
        RAPI.RegisterItem(XLTV);
        RAPI.AddItemToBlockQuadType(XLTV, RBlockQuadType.quad_floor);
        RAPI.AddItemToBlockQuadType(XLTV, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(XLTV, RBlockQuadType.quad_table);
        RAPI.AddItemToBlockQuadType(XLTV, RBlockQuadType.quad_wall);

        request = assetbundle.LoadAssetAsync<Item_Base>("xxlraftsungtv_item");
        yield return request;
        Item_Base XXLTV = request.asset as Item_Base;
        XXLTV.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<CinemaScreen>();
        XXLTV.settings_buildable.GetBlockPrefab(1).gameObject.AddComponent<CinemaScreen>();
        RAPI.RegisterItem(XXLTV);
        RAPI.AddItemToBlockQuadType(XXLTV, RBlockQuadType.quad_floor);
        RAPI.AddItemToBlockQuadType(XXLTV, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(XXLTV, RBlockQuadType.quad_table);
        RAPI.AddItemToBlockQuadType(XXLTV, RBlockQuadType.quad_wall);

        request = assetbundle.LoadAssetAsync<Item_Base>("CinemaMod_Remote");
        yield return request;
        Item_Base CinemaRemote = request.asset as Item_Base;
        RAPI.RegisterItem(CinemaRemote);
        request = assetbundle.LoadAssetAsync<GameObject>("remote_prefab");
        yield return request;
        GameObject remote = request.asset as GameObject;
        remote.AddComponent<CinemaRemote>();
        RAPI.SetItemObject(CinemaRemote, remote);

        notification.Close();
    }

    void Update()
    {
        if (Semih_Network.InLobbyScene) { return; }
        MessageHandler.ReadP2P_Channel_CinemaMod();
    }

    public static void OpenCinemaMenu(CinemaScreen screen)
    {
        if (screen == null) { return; }
        currentScreen = screen;
        menu.transform.Find("Background").Find("VolumeSlider").GetComponent<Slider>().value = screen.audiosource.volume;
        menu.transform.Find("Background").Find("VolumeSlider").Find("Value").GetComponent<TextMeshProUGUI>().text = Mathf.Round(screen.audiosource.volume * 100) + "%";
        menu.transform.Find("Background").Find("VolumeSlider").GetComponent<Slider>().onValueChanged.RemoveAllListeners();
        menu.transform.Find("Background").Find("VolumeSlider").GetComponent<Slider>().onValueChanged.AddListener((val) => { if (currentScreen != null) { currentScreen.SetVolume(val); menu.transform.Find("Background").Find("VolumeSlider").Find("Value").GetComponent<TextMeshProUGUI>().text = Mathf.Round(val * 100) + "%"; } });
        menu.transform.Find("Background").Find("YoutubeVideo").GetComponentInChildren<TMP_InputField>().text = "";
        menu.transform.Find("Background").Find("YoutubeVideo").GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        menu.transform.Find("Background").Find("YoutubeVideo").GetComponentInChildren<Button>().onClick.AddListener(() => instance.StartCoroutine(LoadYoutubeVideo(currentScreen)));
        menu.transform.Find("Background").Find("TwitchStream").GetComponentInChildren<TMP_InputField>().text = "";
        menu.transform.Find("Background").Find("TwitchStream").GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        menu.transform.Find("Background").Find("TwitchStream").GetComponentInChildren<Button>().onClick.AddListener(() => LoadTwitchStream());
        menu.transform.Find("Background").Find("NormalVideo").GetComponentInChildren<TMP_InputField>().text = "";
        menu.transform.Find("Background").Find("NormalVideo").GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        menu.transform.Find("Background").Find("NormalVideo").GetComponentInChildren<Button>().onClick.AddListener(() => LoadNormalVideo());
        menu.GetComponent<CanvasGroup>().alpha = 1;
        menu.GetComponent<CanvasGroup>().interactable = true;
        menu.GetComponent<CanvasGroup>().blocksRaycasts = true;
        RAPI.ToggleCursor(true);
        isMenuShown = true;
    }

    public static IEnumerator LoadYoutubeVideo(CinemaScreen screen)
    {
        if (screen == null) { yield return null; }
        CloseCinemaMenu();
        screen.isLoadingVideo = true;
        RNotification notification = FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.spinning, "Requesting Youtube Video...");
        string url = menu.transform.Find("Background").Find("YoutubeVideo").GetComponentInChildren<TMP_InputField>().text;
        if (url.Length > 3)
        {
            using (UnityWebRequest www = UnityWebRequest.Get("https://youtube-downloader3.herokuapp.com/video_info.php?url=" + url))
            {
                yield return www.SendWebRequest();
                string response = www.downloadHandler.text;
                if (response == "[]")
                {
                    notification.Close();
                    FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "Invalid Youtube URL!", 5, RNotify.ErrorSprite);
                }
                else if (response.Length > 100)
                {
                    try
                    {
                        linksCLass links = JsonConvert.DeserializeObject<linksCLass>(response);
                        YoutubeData[] StreamsWithAudioAndVideo = links.links.Where(x => x.format.Contains("video") && x.format.Contains("audio")).ToArray();
                        if (StreamsWithAudioAndVideo.Length == 0)
                        {
                            notification.Close();
                            FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "This video can't be played!", 5, RNotify.ErrorSprite);
                        }
                        else
                        {
                            YoutubeData[] hd_videos = StreamsWithAudioAndVideo.Where(x => x.format.Contains("720p")).ToArray();
                            if (hd_videos.Length >= 1)
                            {
                                if (screen == null)
                                {
                                    notification.Close();
                                    FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "The Cinema Screen can't be found!", 5, RNotify.ErrorSprite);
                                }
                                else
                                {
                                    MessageHandler.SendMessage(new CMessage_SetVideo((Messages)MessageHandler.NetworkMessages.CinemaMod_SetVideo, screen.transform.localPosition, hd_videos.FirstOrDefault().url));
                                    screen.SetVideo(hd_videos.FirstOrDefault().url);
                                    notification.Close();
                                    FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "Youtube Video Found!", 5, RNotify.CheckSprite);
                                }
                            }
                            else
                            {
                                YoutubeData[] four_videos = StreamsWithAudioAndVideo.Where(x => x.format.Contains("480p")).ToArray();
                                if (four_videos.Length >= 1)
                                {
                                    if (screen == null)
                                    {
                                        notification.Close();
                                        FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "The Cinema Screen can't be found!", 5, RNotify.ErrorSprite);
                                    }
                                    else
                                    {
                                        MessageHandler.SendMessage(new CMessage_SetVideo((Messages)MessageHandler.NetworkMessages.CinemaMod_SetVideo, screen.transform.localPosition, four_videos.FirstOrDefault().url));
                                        screen.SetVideo(four_videos.FirstOrDefault().url);
                                        notification.Close();
                                        FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "Youtube Video Found!", 5, RNotify.CheckSprite);
                                    }
                                }
                                else
                                {
                                    YoutubeData[] third_videos = StreamsWithAudioAndVideo.Where(x => x.format.Contains("360p")).ToArray();
                                    if (third_videos.Length >= 1)
                                    {
                                        if (screen == null)
                                        {
                                            notification.Close();
                                            FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "The Cinema Screen can't be found!", 5, RNotify.ErrorSprite);
                                        }
                                        else
                                        {
                                            MessageHandler.SendMessage(new CMessage_SetVideo((Messages)MessageHandler.NetworkMessages.CinemaMod_SetVideo, screen.transform.localPosition, third_videos.FirstOrDefault().url));
                                            screen.SetVideo(third_videos.FirstOrDefault().url);
                                            notification.Close();
                                            FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "Youtube Video Found!", 5, RNotify.CheckSprite);
                                        }
                                    }
                                    else
                                    {
                                        notification.Close();
                                        FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "This video can't be played!", 5, RNotify.ErrorSprite);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.ToString());
                        notification.Close();
                        FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "Invalid Youtube Data #1!", 5, RNotify.ErrorSprite);
                    }
                }
                else
                {
                    notification.Close();
                    FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "Invalid Youtube Data #2!", 5, RNotify.ErrorSprite);
                }
            }
        }
        else
        {
            notification.Close();
            FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.normal, "Invalid Youtube URL!", 5, RNotify.ErrorSprite);
        }
        screen.isLoadingVideo = false;
    }

    public static void LoadTwitchStream()
    {
        if (currentScreen == null) { return; }
    }

    public static void LoadNormalVideo()
    {
        if (currentScreen == null) { return; }
        MessageHandler.SendMessage(new CMessage_SetVideo((Messages)MessageHandler.NetworkMessages.CinemaMod_SetVideo, currentScreen.transform.localPosition, menu.transform.Find("Background").Find("NormalVideo").GetComponentInChildren<TMP_InputField>().text));
        currentScreen.SetVideo(menu.transform.Find("Background").Find("NormalVideo").GetComponentInChildren<TMP_InputField>().text);
    }

    public static void CloseCinemaMenu()
    {
        currentScreen = null;
        menu.GetComponent<CanvasGroup>().alpha = 0;
        menu.GetComponent<CanvasGroup>().interactable = false;
        menu.GetComponent<CanvasGroup>().blocksRaycasts = false;
        RAPI.ToggleCursor(false);
        isMenuShown = false;
    }

    public void OnModUnload()
    {
        RConsole.Log("CinemaMod can't be unloaded!");
    }
}

public class CinemaRemote : MonoBehaviour
{
    GameObject validLight;
    GameObject invalidLight;

    void Start()
    {
        validLight = transform.Find("CinemaRemoteModel").Find("ValidLight").gameObject;
        invalidLight = transform.Find("CinemaRemoteModel").Find("InvalidLight").gameObject;
        invalidLight.SetActive(false);
        validLight.SetActive(false);
    }

    void Update()
    {
        try
        {
            if (!GetComponentInParent<Network_Player>().IsLocalPlayer) { return; }
            if (CanvasHelper.ActiveMenu != MenuType.None) { return; }
            RaycastHit hit;
            if (Physics.Raycast(RAPI.getLocalPlayer().Camera.transform.position, RAPI.getLocalPlayer().Camera.transform.forward, out hit, 50, LayerMasks.MASK_Block))
            {
                CinemaScreen screen = hit.transform.GetComponentInParent<CinemaScreen>();
                if (screen == null)
                {
                    screen = hit.transform.GetComponentInChildren<CinemaScreen>();
                }
                validLight.SetActive(screen != null);
                invalidLight.SetActive(screen == null);
                if (screen != null && !CinemaMod.isMenuShown && !PlayerItemManager.IsBusy)
                {
                    if (Input.GetMouseButton(0))
                    {
                        CinemaMod.OpenCinemaMenu(screen);
                    }
                }
            }
            else
            {
                validLight.SetActive(false);
                invalidLight.SetActive(true);
            }
        }
        catch { }
    }
}

public class CinemaScreen : MonoBehaviour, IRaycastable
{
    public string currentUrl = "";
    public bool isLoadingVideo = false;
    public string hoverText = "Press E to open the cinema mod menu.";
    public RenderTexture rendertexture;
    public Material material;
    public AudioSource audiosource;
    public CanvasHelper canvas;
    public VideoPlayer videoplayer;
    public bool showingText;
    public bool hasBeenPlaced;

    public void Start()
    {
        canvas = ComponentManager<CanvasHelper>.Value;
        videoplayer = GetComponentInChildren<VideoPlayer>();
        audiosource = GetComponentInChildren<AudioSource>();
    }

    public void CreateRenderTexture()
    {
        if (rendertexture == null || !rendertexture.IsCreated())
        {
            rendertexture = new RenderTexture(1280, 720, 0);
            rendertexture.Create();
        }
        rendertexture.Release();
        if (material == null)
        {
            material = new Material(CinemaMod.novideoMaterial.shader);
        }
        material.mainTexture = rendertexture;
        transform.Find("CinemaScreen").GetComponent<Renderer>().material = material;
        videoplayer.targetTexture = rendertexture;
    }

    public void Pause()
    {
        videoplayer.Pause();
    }

    public void Play()
    {
        videoplayer.Play();
    }

    public void Stop()
    {
        videoplayer.url = "";
        videoplayer.Stop();
        videoplayer.clip = null;
        CreateRenderTexture();
    }

    public void SetVideo(string url)
    {
        Stop();
        videoplayer.url = url;
        videoplayer.Play();
    }

    public void SetVolume(float _volume)
    {
        audiosource.volume = Mathf.Clamp(_volume, 0, 1);
    }

    private void OnBlockPlaced()
    {
        hasBeenPlaced = true;
    }

    public void OnIsRayed()
    {
        if (!hasBeenPlaced) { return; }
        if (canvas == null)
        {
            canvas = ComponentManager<CanvasHelper>.Value;
            return;
        }
        if (CanvasHelper.ActiveMenu != MenuType.None || !Helper.LocalPlayerIsWithinDistance(transform.position, Player.UseDistance))
        {
            if (showingText)
            {
                canvas.displayTextManager.HideDisplayTexts();
                showingText = false;
            }
            return;
        }
        else
        {
            canvas.displayTextManager.ShowText(isLoadingVideo ? "Loading Video..." : hoverText, 0, true, 0);
            showingText = true;
            if (isLoadingVideo) { return; }
            if (Input.GetKeyDown(KeyCode.E))
            {
                CinemaMod.OpenCinemaMenu(this);
            }
        }
        return;
    }

    public void OnRayEnter() { }

    public void OnRayExit()
    {
        if (showingText)
        {
            canvas.displayTextManager.HideDisplayTexts();
            showingText = false;
        }
    }
}

[Serializable]
public class linksCLass
{
    public YoutubeData[] links;
}

[Serializable]
public class YoutubeData
{
    public string url;
    public int itag;
    public string format;
}

public class MessageHandler : MonoBehaviour
{
    public enum NetworkMessages
    {
        CinemaMod_SetVideo = 6000,
        CinemaMod_Stop = 6001,
        CinemaMod_Pause = 6002,
        CinemaMod_Play = 6003
    }

    public static void SendMessage(Message message)
    {
        if (Semih_Network.IsHost)
        {
            RAPI.GetLocalPlayer().Network.RPC(message, Target.Other, EP2PSend.k_EP2PSendReliable, (NetworkChannel)70);
        }
        else
        {
            RAPI.GetLocalPlayer().SendP2P(message, EP2PSend.k_EP2PSendReliable, (NetworkChannel)70);
        }
    }

    public static void ReadP2P_Channel_CinemaMod()
    {
        if (Semih_Network.InLobbyScene) { return; }
        uint num;
        while (SteamNetworking.IsP2PPacketAvailable(out num, 70))
        {
            byte[] array = new byte[num];
            uint num2;
            CSteamID csteamID;
            if (SteamNetworking.ReadP2PPacket(array, num, out num2, out csteamID, 70))
            {
                MemoryStream serializationStream = new MemoryStream(array);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Binder = new PreMergeToMergedDeserializationBinder();
                Packet packet = bf.Deserialize(serializationStream) as Packet;
                Packet_Multiple packet_Multiple;
                if (packet.PacketType == PacketType.Single)
                {
                    Packet_Single packet_Single = packet as Packet_Single;
                    packet_Multiple = new Packet_Multiple(packet_Single.SendType);
                    packet_Multiple.messages = new Message[]
                    {
                        packet_Single.message
                    };
                }
                else
                {
                    packet_Multiple = (packet as Packet_Multiple);
                }
                List<Message> list = packet_Multiple.messages.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    Message message = list[i];
                    if (message != null)
                    {
                        Messages type = message.Type;
                        switch (type)
                        {
                            case (Messages)NetworkMessages.CinemaMod_SetVideo:
                                {
                                    CMessage_SetVideo msg = message as CMessage_SetVideo;
                                    foreach (CinemaScreen cs in FindObjectsOfType<CinemaScreen>())
                                    {
                                        if (cs.transform.localPosition == msg.position)
                                        {
                                            cs.SetVideo(msg.url);
                                            return;
                                        }
                                    }
                                    Debug.LogError("/!\\ CAN'T FIND CINEMA SCREEN AT POSITION " + msg.position + " /!\\");
                                    break;
                                }
                            case (Messages)NetworkMessages.CinemaMod_Stop:
                                {
                                    CMessage_CinemaModPosition msg = message as CMessage_CinemaModPosition;
                                    foreach (CinemaScreen cs in FindObjectsOfType<CinemaScreen>())
                                    {
                                        if (cs.transform.localPosition == msg.position)
                                        {
                                            cs.Stop();
                                            return;
                                        }
                                    }
                                    Debug.LogError("/!\\ CAN'T FIND CINEMA SCREEN AT POSITION " + msg.position + " /!\\");
                                    break;
                                }
                            case (Messages)NetworkMessages.CinemaMod_Pause:
                                {
                                    CMessage_CinemaModPosition msg = message as CMessage_CinemaModPosition;
                                    foreach (CinemaScreen cs in FindObjectsOfType<CinemaScreen>())
                                    {
                                        if (cs.transform.localPosition == msg.position)
                                        {
                                            cs.Pause();
                                            return;
                                        }
                                    }
                                    Debug.LogError("/!\\ CAN'T FIND CINEMA SCREEN AT POSITION " + msg.position + " /!\\");
                                    break;
                                }
                            case (Messages)NetworkMessages.CinemaMod_Play:
                                {
                                    CMessage_CinemaModPosition msg = message as CMessage_CinemaModPosition;
                                    foreach (CinemaScreen cs in FindObjectsOfType<CinemaScreen>())
                                    {
                                        if (cs.transform.localPosition == msg.position)
                                        {
                                            cs.Play();
                                            return;
                                        }
                                    }
                                    Debug.LogError("/!\\ CAN'T FIND CINEMA SCREEN AT POSITION " + msg.position + " /!\\");
                                    break;
                                }
                        }
                    }

                }
            }
        }
    }
}

[Serializable]
public class CMessage_SetVideo : Message
{
    public Vector3 position { get { return new Vector3(posX, posY, posZ); } private set { posX = value.x; posY = value.y; posZ = value.z; } }
    private float posX, posY, posZ;
    public string url;

    public CMessage_SetVideo(Messages type, Vector3 _position, string _url) : base(type)
    {
        position = _position;
        url = _url;
    }
}

[Serializable]
public class CMessage_CinemaModPosition : Message
{
    public Vector3 position { get { return new Vector3(posX, posY, posZ); } private set { posX = value.x; posY = value.y; posZ = value.z; } }
    private float posX, posY, posZ;

    public CMessage_CinemaModPosition(Messages type, Vector3 _position) : base(type)
    {
        position = _position;
    }
}

sealed class PreMergeToMergedDeserializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type typeToDeserialize = null;
        String exeAssembly = Assembly.GetExecutingAssembly().FullName;
        typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, exeAssembly));
        return typeToDeserialize;
    }
}