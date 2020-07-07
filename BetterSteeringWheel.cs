using System.Reflection;
using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection.Emit;
using System.Linq;

[ModTitle("Better Steering Wheel")]
[ModDescription("Steering Wheel will return to default rotation if it's not locked")]
[ModAuthor("Whitebrim")]
[ModIconUrl("http://258215.selcdn.com/Filestorage/BetterSteeringWheelIcon.png")]
[ModWallpaperUrl("http://258215.selcdn.com/Filestorage/BetterSteeringWheelBanner.png")]
[ModVersionCheckUrl("http://258215.selcdn.com/Filestorage/BetterSteeringWheelVersion.txt")]
[ModVersion("v1.0.0")]
[RaftVersion("10")]
[ModIsPermanent(false)]
public class BetterSteeringWheel : Mod
{
    private HarmonyInstance harmonyInstance;

    public void Start()
    {
        harmonyInstance = HarmonyInstance.Create("com.whitebrim.bettersteeringwheel");
        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        RConsole.Log("BetterSteeringWheel has been loaded!");
    }

    public void OnModUnload()
    {
        RConsole.Log("BetterSteeringWheel has been unloaded!");
        Destroy(gameObject);
    }
}

[HarmonyPatch(typeof(SteeringWheel)), HarmonyPatch("OnIsRayed")]
internal class SteeringWheelPatchRayEnter
{
    private static void Postfix(SteeringWheel __instance, ref DisplayTextManager ___displayText, ref float ___targetLocalEulerZ, Semih_Network ___network, ref Network_Player ___localPlayer)
    {
        if (MyInput.GetButtonUp("Rotate"))
        {
            var mouseLookXScript = ___localPlayer.PlayerScript.mouseLookXScript;
            var mouseLookYScript = ___localPlayer.PlayerScript.mouseLookYScript;

            if (mouseLookYScript.isChild)
                Traverse.Create(mouseLookYScript).Field("targetRotY").SetValue(Mathf.Clamp(mouseLookYScript.transform.localEulerAngles.x, mouseLookYScript.minimumY, mouseLookYScript.maximumY));
            else
                Traverse.Create(mouseLookYScript).Field("targetRotY").SetValue(Mathf.Clamp(mouseLookYScript.transform.eulerAngles.x, mouseLookYScript.minimumY, mouseLookYScript.maximumY));

            if (mouseLookXScript.isChild)
                Traverse.Create(mouseLookXScript).Field("targetRotX").SetValue(Mathf.Clamp(mouseLookXScript.transform.localEulerAngles.y, mouseLookXScript.minimumX, mouseLookXScript.maximumX));
            else
                Traverse.Create(mouseLookXScript).Field("targetRotX").SetValue(Mathf.Clamp(mouseLookXScript.transform.eulerAngles.y, mouseLookXScript.minimumX, mouseLookXScript.maximumX));
        }

        if (!__instance.IsLocked())
            ___displayText.ShowText("Press to LOCK rotation", MyInput.Keybinds["RMB"].MainKey, 2, 0, false);
        else
            ___displayText.ShowText("Press to UNLOCK rotation", MyInput.Keybinds["RMB"].MainKey, 2, 0, false);
        ___displayText.ShowText("Press to Reset rotation", MyInput.Keybinds["LMB"].MainKey, 1, 0, false);

        if (MyInput.GetButtonDown("RMB"))
        {
            __instance.Lock(!__instance.IsLocked());
            if (!__instance.IsLocked())
            {
                float rotation = -___targetLocalEulerZ;
                Message_SteeringWheel_Rotate message = new Message_SteeringWheel_Rotate(Messages.SteeringWheelRotate, ___network.NetworkIDManager, __instance.ObjectIndex, rotation);
                if (Semih_Network.IsHost)
                {
                    Traverse.Create(__instance).Method("Rotate", rotation).GetValue();
                    return;
                }
            }
        }
        if (MyInput.GetButtonDown("LMB"))
        {
            __instance.Lock(false);
            float rotation = -___targetLocalEulerZ;
            Message_SteeringWheel_Rotate message = new Message_SteeringWheel_Rotate(Messages.SteeringWheelRotate, ___network.NetworkIDManager, __instance.ObjectIndex, rotation);
            if (Semih_Network.IsHost)
            {
                Traverse.Create(__instance).Method("Rotate", rotation).GetValue();
                return;
            }
        }
    }
}

[HarmonyPatch(typeof(SteeringWheel)), HarmonyPatch("OnRayExit")]
internal class SteeringWheelPatchRayExit
{
    private static void Postfix(SteeringWheel __instance, ref float ___targetLocalEulerZ, Semih_Network ___network)
    {
        if (!__instance.IsLocked())
        {
            float rotation = -___targetLocalEulerZ;
            Message_SteeringWheel_Rotate message = new Message_SteeringWheel_Rotate(Messages.SteeringWheelRotate, ___network.NetworkIDManager, __instance.ObjectIndex, rotation);
            if (Semih_Network.IsHost)
            {
                Traverse.Create(__instance).Method("Rotate", rotation).GetValue();
                return;
            }
        }
    }
}

[HarmonyPatch(typeof(SteeringWheel)), HarmonyPatch("Update")]
internal class SteeringWheelPatchUpdate
{
    private static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldc_R4)
            {
                var strOperand = (float)codes[i].operand;
                if (strOperand == 1)
                {
                    codes[i].operand = 0.001f;
                    break;
                }
            }
        }
        return codes.AsEnumerable();
    }
}

public static class SteeringWheelExtension
{
    public static Dictionary<SteeringWheel, bool> isLocked = new Dictionary<SteeringWheel, bool>();

    public static bool IsLocked(this SteeringWheel steeringWheel)
    {
        if (!isLocked.ContainsKey(steeringWheel))
        {
            isLocked.Add(steeringWheel, false);
        }
        return isLocked[steeringWheel];
    }

    public static void Lock(this SteeringWheel steeringWheel, bool value)
    {
        if (!isLocked.ContainsKey(steeringWheel))
        {
            isLocked.Add(steeringWheel, value);
            return;
        }
        isLocked[steeringWheel] = value;
    }
}