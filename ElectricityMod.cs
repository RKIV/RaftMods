using System;
using System.Collections;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using Steamworks;
using System.Reflection;

[ModTitle("Electricity Mod")] // The mod name.
[ModDescription("Adds Electric Items")] // Short description for the mod.
[ModAuthor("Derek")] // The author name of the mod.
[ModIconUrl("https://i.imgur.com/VFEolRT.png")] // An icon for your mod. Its recommended to be 128x128px and in .jpg format.
[ModWallpaperUrl("https://i.imgur.com/g6a5eVR.png")] // A banner for your mod. Its recommended to be 330x100px and in .jpg format.
[ModVersionCheckUrl("https://raftmodding.com/api/v1/mods/modelectricity/version.txt")] // This is for update checking. Needs to be a .txt file with the latest mod version.
[ModVersion("2.2")] // This is the mod version.
[RaftVersion("Update 10.07")] // This is the recommended raft version.
[ModIsPermanent(true)] // If your mod add new blocks, new items or just content you should set that to true. It loads the mod on start and prevents unloading.
public class ElectricityMod : Mod
{
    // The Start() method is being called when your mod gets loaded.
    public IEnumerator Start()
    {
        RNotification notification = FindObjectOfType<RNotify>().AddNotification(RNotify.NotificationType.spinning, "Loading Electricity Mod...");
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync("mods\\ModData\\ElectricityMod\\electricity.assets");
        var bundleLoadRequesttwo = AssetBundle.LoadFromFileAsync("mods\\ModData\\ElectricityMod\\electricityAssets2.assets");
        yield return bundleLoadRequest;
        yield return bundleLoadRequesttwo;

        AssetBundle assetBundle = bundleLoadRequest.assetBundle;
        AssetBundle assetBundle2 = bundleLoadRequesttwo.assetBundle;
        if (assetBundle == null || assetBundle2 == null)
        {
            RConsole.Log("Failed to load Asset Bundle for Electricity Mod.");
            notification.Close();
            yield return null;
        }

        var harmony = HarmonyInstance.Create("com.derek.electrictymod");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        Item_Base electricMotor = assetBundle2.LoadAsset<Item_Base>("placeable_electricmotor");
        electricMotor.settings_buildable.GetBlockPrefab(0).gameObject.GetComponentInChildren<Battery>().gameObject.layer = 23;
        electricMotor.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<ElectricMotor>();
        MonoBehaviour_ID_Network obj = electricMotor.settings_buildable.GetBlockPrefab(0).gameObject.GetComponent<MotorWheel>();
        Traverse.Create(electricMotor.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<Battery>()).Field("networkBehaviourID").SetValue(obj);
        // Asset bundle fucks up some shit. so we have to fix by hand.
        Traverse.Create(electricMotor.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<MotorWheel>()).Field("eventEmitter_engine").Field("eventEmitter").SetValue(electricMotor.settings_buildable.GetBlockPrefab(0).transform.Find("Parent").gameObject.AddComponent<FMODUnity.StudioEventEmitter>());
        Traverse.Create(electricMotor.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<MotorWheel>()).Field("eventEmitter_waterWheel").Field("eventEmitter").SetValue(electricMotor.settings_buildable.GetBlockPrefab(0).transform.Find("Parent").gameObject.AddComponent<FMODUnity.StudioEventEmitter>());
        Traverse.Create(electricMotor.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<MotorWheel>()).Field("eventEmitter_boiler").Field("eventEmitter").SetValue(electricMotor.settings_buildable.GetBlockPrefab(0).transform.Find("Parent").gameObject.AddComponent<FMODUnity.StudioEventEmitter>());
        RAPI.RegisterItem(electricMotor);

        RAPI.AddItemToBlockQuadType(electricMotor, RBlockQuadType.quad_foundation);

        Item_Base electricGrill = assetBundle2.LoadAsset<Item_Base>("ElectricGrillRemake");

        RAPI.RegisterItem(electricGrill);

        obj = electricGrill.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<ElectricGrill>();
        Traverse.Create(electricGrill.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<Battery>()).Field("networkBehaviourID").SetValue(obj);
        electricGrill.settings_buildable.GetBlockPrefab(0).GetComponent<CookingStand>().networkedIDBehaviour = obj;
        electricGrill.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<Battery>().gameObject.layer = 23;
        //electricGrill.settings_buildable.GetBlockPrefab(0).gameObject.layer = 23;

        RAPI.AddItemToBlockQuadType(electricGrill, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(electricGrill, RBlockQuadType.quad_floor);

        Item_Base electricWindmill = assetBundle.LoadAsset<Item_Base>("ElectricWindmill");

        RAPI.RegisterItem(electricWindmill);

        electricWindmill.settings_buildable.GetBlockPrefab(0).transform.Find("Collider").gameObject.layer = 10;
        obj = electricWindmill.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<ElectricWindmill>();
        electricWindmill.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<WindmillSpinner>();
        Traverse.Create(electricWindmill.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<Battery>()).Field("networkBehaviourID").SetValue(obj);
        electricWindmill.settings_buildable.GetBlockPrefab(0).GetComponent<Block>().networkedIDBehaviour = obj;
        electricWindmill.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<Battery>().gameObject.layer = 23;
        electricWindmill.settings_buildable.GetBlockPrefab(0).gameObject.layer = 23;

        RAPI.AddItemToBlockQuadType(electricWindmill, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(electricWindmill, RBlockQuadType.quad_floor);

        Item_Base electricConductor = assetBundle.LoadAsset<Item_Base>("ElectricConductor");

        RAPI.RegisterItem(electricConductor);

        electricConductor.settings_buildable.GetBlockPrefab(0).transform.Find("Collider").gameObject.layer = 10;
        obj = electricConductor.settings_buildable.GetBlockPrefab(0).gameObject.AddComponent<ElectricConductor>();
        Traverse.Create(electricConductor.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<Battery>()).Field("networkBehaviourID").SetValue(obj);
        electricConductor.settings_buildable.GetBlockPrefab(0).GetComponent<Block>().networkedIDBehaviour = obj;
        electricConductor.settings_buildable.GetBlockPrefab(0).GetComponentInChildren<Battery>().gameObject.layer = 23;
        electricConductor.settings_buildable.GetBlockPrefab(0).gameObject.layer = 23;

        RAPI.AddItemToBlockQuadType(electricConductor, RBlockQuadType.quad_foundation);
        RAPI.AddItemToBlockQuadType(electricConductor, RBlockQuadType.quad_floor);

        notification.Close();
        RConsole.Log("ElectricityMod was loaded successfully!");
    }

    // The Update() method is being called every frame. Have fun!
    public void Update()
    {

    }

    // The OnModUnload() method is being called when your mod gets unloaded.
    public void OnModUnload()
    {
        RConsole.Log("ElectricityMod has been unloaded!");
        Destroy(gameObject); // Please do not remove that line!
    }
}

[HarmonyPatch(typeof(MotorWheel))]
[HarmonyPatch("RotateWheel")]
class PatchWheelErrors
{
    static bool Prefix(MotorWheel __instance, Transform ___motorWheel, float ___jitterChance, float ___wheelRotationSpeed, bool ___rotatesForward, ref float ___currentRotationSpeed, StudioEventEmitterSustain ___eventEmitter_engine, StudioEventEmitterSustain ___eventEmitter_waterWheel, StudioEventEmitterSustain ___eventEmitter_boiler)
    {
        if (___motorWheel == null)
        {
            Debug.Log("Motor wheel is null, add to inspector");
            return false;
        }
        
        bool flag = false;
        float num = 1f - RaftVelocityManager.MotorWheelWeightStrengthNormalized;
        if (num == 0f && UnityEngine.Random.value > 1f - ___jitterChance)
        {
            flag = true;
        }
        float num2;
        if (num == 0f)
        {
            num2 = ___wheelRotationSpeed * 0.25f * (float)(__instance.rotatesForward ? -1 : 1);
        }
        else
        {
            num2 = ___wheelRotationSpeed * num * (float)(__instance.rotatesForward ? -1 : 1);
        }
        ___currentRotationSpeed = Mathf.Lerp(___currentRotationSpeed, num2, Time.deltaTime * 2f);
        float target = 0f;
        if (num == 1f)
        {
            target = 3f;
        }
        else if (num > 0f)
        {
            target = 1f;
        }
        if ((num2 > 0f && ___currentRotationSpeed < 0f) || (num2 < 0f && ___currentRotationSpeed > 0f))
        {
            target = 0f;
        }
        if (!flag)
        {
            ___motorWheel.Rotate(Vector3.forward * ___currentRotationSpeed * Time.deltaTime);
        }
        try
        {
            float num3 = ___eventEmitter_engine.eventEmitter.GetParameter("RPM");
            num3 = Mathf.MoveTowards(num3, target, Time.deltaTime * 4f);
            ___eventEmitter_engine.eventEmitter.SetParameter("RPM", num3);
            ___eventEmitter_waterWheel.eventEmitter.SetParameter("RPM", num3);
        }
        catch
        {
            // ignore.
        }

        return false;
    }
}

[HarmonyPatch(typeof(MotorWheel))]
[HarmonyPatch("HandleSounds")]
class PatchSoundErrors
{
    static bool Prefix(MotorWheel __instance, bool ____motorState, StudioEventEmitterSustain ___eventEmitter_engine, StudioEventEmitterSustain ___eventEmitter_waterWheel, StudioEventEmitterSustain ___eventEmitter_boiler)
    {
        try
        {
            if (____motorState)
            {
                ___eventEmitter_engine.Play();
                ___eventEmitter_waterWheel.Play();
                ___eventEmitter_boiler.Play();
                return false;
            }
            ___eventEmitter_engine.Stop();
            ___eventEmitter_boiler.Stop();
            ___eventEmitter_waterWheel.Stop();
            return false;
        }
        catch
        {
            return false;
        }
    }
}

[HarmonyPatch(typeof(MotorWheel))]
[HarmonyPatch("Deserialize")]
class PatchMotorWheel
{
    static void Prefix(MotorWheel __instance, ref Message_NetworkBehaviour msg, ref CSteamID remoteID)
    {
        if (__instance.GetComponentInChildren<Battery>() != null)
        {
            Messages type = msg.Type;
            switch (type)
            {
                case Messages.Battery_Insert:
                case Messages.Battery_Take:
                case Messages.Battery_Update:
                case Messages.Battery_OnOff:
                    __instance.GetComponentInChildren<Battery>().OnBatteryMessage(msg);
                    // This is jank and took literally 2 hours of constant debugging to find out players need to get forwarded the event after the host receives it. Dont know why but it works.
                    if(Semih_Network.IsHost)
                        RAPI.GetLocalPlayer().Network.RPC(msg, Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                    break;
            }
        }
    }
}

[HarmonyPatch(typeof(CookingSlot))]
[HarmonyPatch("EnableRawItem")]
class CookingSlotRawFoodPatch
{
    static void Prefix(CookingSlot __instance, ref Item_Base cookableItem, List<CookItemConnection> ___itemConnections)
    {
        if (__instance.transform.root.GetComponentInChildren<Battery>() != null)
        {
            // its our electric grill! Fix the model
            foreach (CookItemConnection cookItemConnection in ___itemConnections)
            {
                if (cookItemConnection.cookableItem.UniqueIndex == cookableItem.UniqueIndex)
                {
                    cookItemConnection.SetRawState(true);
                    cookItemConnection.SetCookedState(false);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(CookingSlot))]
[HarmonyPatch("EnableCookedItem")]
class CookingSlotCookedFoodPatch
{
    static void Prefix(CookingSlot __instance, ref Item_Base cookableItem, List<CookItemConnection> ___itemConnections)
    {
        if (__instance.transform.root.GetComponentInChildren<Battery>() != null)
        {
            // its our electric grill! Fix the model
            foreach (CookItemConnection cookItemConnection in ___itemConnections)
            {
                if (cookItemConnection.cookableItem.UniqueIndex == cookableItem.UniqueIndex)
                {
                    cookItemConnection.SetRawState(false);
                    cookItemConnection.SetCookedState(true);
                }
            }
        }
    }
}

public class ElectricMotor : MonoBehaviour_ID_Network
{
    public Battery battery;
    public MotorWheel mw;

    void Start()
    {
        battery = GetComponentInChildren<Battery>();
        mw = GetComponent<MotorWheel>();
        if (mw.FuelTank.CurrentTankAmount > 0)
        {
            battery.Insert(null, Mathf.RoundToInt(mw.FuelTank.CurrentTankAmount));
        }

        InvokeRepeating("UpdateLateClients", 5, 5);
    }

    void UpdateLateClients()
    {
        // Used to update clients that join late, this will force a battery sync event to occur.
        DechargeBattery(0);
    }

    void Update()
    {
        if (!battery.BatterySlotIsEmpty)
        {
            if (mw.FuelTank.CurrentTankAmount <= 0 && battery.GetBatteryInstance().Uses > 0)
            {
                mw.FuelTank.ModifyTank(null, 1);
                DechargeBattery(1);
            }
        }
    }

    public bool DechargeBattery(int amount)
    {
        if (Semih_Network.IsHost && !battery.BatterySlotIsEmpty)
        {
            battery.GetBatteryInstance().Uses -= amount;
            RAPI.GetLocalPlayer().Network.RPC(new Message_Battery(Messages.Battery_Update, RAPI.GetLocalPlayer().Network.NetworkIDManager, RAPI.GetLocalPlayer().steamID, mw.ObjectIndex, battery.GetBatteryInstance().Uses), Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
            return true;
        }
        return false;

    }

    public override RGD Serialize_Save()
    {
        if (battery.BatterySlotIsEmpty)
        {
            mw.FuelTank.SetTankAmount(0);
        }
        else
        {
            mw.FuelTank.SetTankAmount(battery.GetBatteryInstance().Uses);
        }
        return null;
    }
}

public class ElectricGrill : MonoBehaviour_ID_Network
{
    public Battery battery;
    public CookingStand cs;

    void Start()
    {
        battery = GetComponentInChildren<Battery>();
        cs = GetComponent<CookingStand>();
        if (cs.fuel.GetFuelCount() > 0)
        {
            battery.Insert(null, Mathf.RoundToInt(cs.fuel.GetFuelCount()));
        }

        InvokeRepeating("UpdateLateClients", 5, 5);
    }

    public override bool Deserialize(Message_NetworkBehaviour msg, CSteamID remoteID)
    {
        Messages type = msg.Type;
        switch (type)
        {
            case Messages.Battery_Insert:
            case Messages.Battery_Take:
            case Messages.Battery_Update:
            case Messages.Battery_OnOff:
                return this.battery.OnBatteryMessage(msg);
        }
        return base.Deserialize(msg, remoteID);
    }

    void UpdateLateClients()
    {
        // Used to update clients that join late, this will force a battery sync event to occur.
        DechargeBattery(0);
    }

    void Update()
    {
        if (Semih_Network.IsHost && !battery.BatterySlotIsEmpty)
        {
            if (cs.fuel.GetFuelCount() <= 0 && battery.GetBatteryInstance().Uses > 1)
            {
                cs.fuel.AddFuel(1);
                DechargeBattery(2);
            }
        }
    }

    public bool DechargeBattery(int amount)
    {
        if (Semih_Network.IsHost && !battery.BatterySlotIsEmpty)
        {
            battery.GetBatteryInstance().Uses -= amount;
            RAPI.GetLocalPlayer().Network.RPC(new Message_Battery(Messages.Battery_Update, RAPI.GetLocalPlayer().Network.NetworkIDManager, RAPI.GetLocalPlayer().steamID, cs.ObjectIndex, battery.GetBatteryInstance().Uses), Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
            return true;
        }
        return false;

    }

    public override RGD Serialize_Save()
    {
        if (battery.BatterySlotIsEmpty)
        {
            cs.fuel.SetFuelCount(0);
        }
        else
        {
            cs.fuel.SetFuelCount(battery.GetBatteryInstance().Uses);
        }
        return null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkIDManager.RemoveNetworkID(this);
    }

    public void OnBlockPlaced()
    {
        NetworkIDManager.AddNetworkID(this);
    }
}

public class ElectricConductor : MonoBehaviour_ID_Network, IRaycastable
{
    public Battery battery;

    void Start()
    {
        battery = GetComponentInChildren<Battery>();
        InvokeRepeating("CheckMotors", 5, 5);
    }

    public override bool Deserialize(Message_NetworkBehaviour msg, CSteamID remoteID)
    {
        Messages type = msg.Type;
        switch (type)
        {
            case Messages.Battery_Insert:
            case Messages.Battery_Take:
            case Messages.Battery_Update:
            case Messages.Battery_OnOff:
                return this.battery.OnBatteryMessage(msg);
        }
        return base.Deserialize(msg, remoteID);
    }

    public override RGD Serialize_Save()
    {
        GetComponentInChildren<Sprinkler>().ObjectIndex = ObjectIndex;
        return null;
    }

    void CheckMotors()
    {
        if (Semih_Network.IsHost)
        {
            foreach (MotorWheel mw in GameObject.FindObjectsOfType<MotorWheel>())
            {
                if (!battery.BatterySlotIsEmpty && battery.GetBatteryInstance().Uses > 0 && mw.FuelTank.CurrentTankAmount < mw.FuelTank.maxCapacity - 2)
                {
                    DechargeBattery(1);
                    mw.FuelTank.ModifyTank(null, 2);
                }
            }
        }
    }

    public bool DechargeBattery(int amount)
    {
        if (Semih_Network.IsHost && !battery.BatterySlotIsEmpty)
        {
            battery.GetBatteryInstance().Uses -= amount;
            RAPI.GetLocalPlayer().Network.RPC(new Message_Battery(Messages.Battery_Update, RAPI.GetLocalPlayer().Network.NetworkIDManager, RAPI.GetLocalPlayer().steamID, this.ObjectIndex, battery.GetBatteryInstance().Uses), Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
            return true;
        }
        return false;
    }

    public void OnIsRayed()
    {
    }

    public void OnRayEnter()
    {
    }

    void IRaycastable.OnRayExit()
    {
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkIDManager.RemoveNetworkID(this);
    }

    public void OnBlockPlaced()
    {
        NetworkIDManager.AddNetworkID(this);
    }
}

public class WindmillSpinner : MonoBehaviour
{
    public Transform windmill;

    void Start()
    {
        windmill = transform.Find("Clock/EngineWheel");
    }

    void Update()
    {
        windmill.Rotate(0, 0, 20 * Time.deltaTime, Space.Self);
    }
}

public class ElectricWindmill : MonoBehaviour_ID_Network, IRaycastable
{
    private CanvasHelper canvas;
    private Network_Player localPlayer;
    private bool showingText;
    public Battery battery;

    float lastRechargeTime = 0;
    float rechargeWait = 5;

    public int batteryMaxUses;

    void Start()
    {
        battery = GetComponentInChildren<Battery>();
        batteryMaxUses = ItemManager.GetItemByName("Battery").MaxUses;
        InvokeRepeating("UpdateLateClients", 5, 5);
    }

    void UpdateLateClients()
    {
        // Used to update clients that join late, this will force a battery sync event to occur.
        AddBatteryCharge(0);
    }

    public override RGD Serialize_Save()
    {
        GetComponentInChildren<Sprinkler>().ObjectIndex = ObjectIndex;
        return null;
    }

    public override bool Deserialize(Message_NetworkBehaviour msg, CSteamID remoteID)
    {
        Messages type = msg.Type;
        switch (type)
        {
            case Messages.Battery_Insert:
            case Messages.Battery_Take:
            case Messages.Battery_Update:
            case Messages.Battery_OnOff:
                return this.battery.OnBatteryMessage(msg);
        }
        return base.Deserialize(msg, remoteID);
    }

    void Update()
    {
        if (!battery.BatterySlotIsEmpty && Semih_Network.IsHost)
        {
            if (battery.GetBatteryInstance().Uses < batteryMaxUses && Time.time - lastRechargeTime >= rechargeWait)
            {
                lastRechargeTime = Time.time;
                AddBatteryCharge(1);
                return;
            }
        }
    }

    public bool AddBatteryCharge(int amount)
    {
        if (Semih_Network.IsHost && !battery.BatterySlotIsEmpty)
        {
            battery.GetBatteryInstance().Uses += amount;
            RAPI.GetLocalPlayer().Network.RPC(new Message_Battery(Messages.Battery_Update, RAPI.GetLocalPlayer().Network.NetworkIDManager, RAPI.GetLocalPlayer().steamID, this.ObjectIndex, battery.GetBatteryInstance().Uses), Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
            return true;
        }
        return false;

    }

    public void OnIsRayed()
    {
        if (this.canvas == null)
        {
            this.canvas = ComponentManager<CanvasHelper>.Value;
        }
        if (this.localPlayer == null)
        {
            this.localPlayer = ComponentManager<Network_Player>.Value;
        }
        if (CanvasHelper.ActiveMenu != MenuType.None || !Helper.LocalPlayerIsWithinDistance(base.transform.position, Player.UseDistance))
        {
            if (this.showingText)
            {
                this.canvas.displayTextManager.HideDisplayTexts();
                this.showingText = false;
            }
            return;
        }
        if (!battery.BatterySlotIsEmpty)
        {
            if (battery.GetBatteryInstance().Uses < batteryMaxUses)
            {
                this.canvas.displayTextManager.ShowText("Charging...", 0, true, 0);
                this.showingText = true;
            }
            else
            {
                this.canvas.displayTextManager.ShowText("Battery Full!", 0, true, 0);
                this.showingText = true;
            }
        }
        else
        {
            this.canvas.displayTextManager.ShowText("No Battery!", 0, true, 0);
            this.showingText = true;
            return;
        }
    }

    public void OnRayEnter()
    {
    }

    void IRaycastable.OnRayExit()
    {
        if (this.showingText)
        {
            this.canvas.displayTextManager.HideDisplayTexts();
            this.showingText = false;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkIDManager.RemoveNetworkID(this);
    }

    public void OnBlockPlaced()
    {
        NetworkIDManager.AddNetworkID(this);
    }
}