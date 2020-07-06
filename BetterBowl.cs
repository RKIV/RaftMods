using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Harmony;
using UnityEngine;

[ModTitle("BetterBowl")]
[ModDescription("Gives you an empty bowl when you ate soup.")]
[ModAuthor("TeigRolle and traxam")]
[ModIconUrl("https://traxam.s-ul.eu/raft-mods/p39PsRLx.png")]
[ModWallpaperUrl("https://traxam.s-ul.eu/raft-mods/tOOiGRgE.png")]
[ModVersion("1.0.1")]
[RaftVersion("Update 11 (4677160)")]
[ModVersionCheckUrl("https://raftmodding.com/api/v1/mods/betterbowl/version.txt")]
public class BetterBowlMod : Mod
{
  /// <summary>
  /// The harmony identifier.
  /// </summary>
  private const string HARMONY_ID = "com.teigrolle.betterbowl";
  /// <summary>
  /// The PlayerPrefs key for the 'current uses' variable.
  /// </summary>
  private const string PP_CURRENT_USES = "TR.BETTERBOWL.CURRENTUSES";
  /// <summary>
  /// The PlayerPrefs key for the 'goal uses' variable.
  /// </summary>
  private const string PP_GOAL_USES = "TR.BETTERBOWL.GOALUSES";
  /// <summary>
  /// The PlayerPrefs key for the 'min uses' variable.
  /// </summary>
  private const string PP_MIN_USES = "TR.BETTERBOWL.MINUSES";
  /// <summary>
  /// The PlayerPrefs key for the 'max uses' variable.
  /// </summary>
  private const string PP_MAX_USES = "TR.BETTERBOWL.MAXUSES";

  private HarmonyInstance harmonyInstance;
  private int minUses = 0;
	private int maxUses = 0;
	private int currentUses = 0;
	private int goalUses = 0;

#region Startup and shutdown
	public void Start()
	{
		ComponentManager<BetterBowlMod>.Value = this;

    harmonyInstance = HarmonyInstance.Create(HARMONY_ID);
		harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

		RConsole.registerCommand(typeof(BetterBowlMod), "Sets the interval in which the bowl breaks", "breakInt", HandleBreakIntCommand);
		
    LoadState();
		
    LogInfo("BetterBowl has been loaded!");
	}

	public void OnModUnload()
	{
    harmonyInstance.UnpatchAll(HARMONY_ID);

		ComponentManager<BetterBowlMod>.Value = null;

    LogInfo("BetterBowl has been unloaded!");
	}
#endregion

#region Bowl return
	/// <summary>
	/// Method that should be called when an item is consumed.
	/// </summary>
	/// <param name="item">the item that was consumed</param>
	public void OnConsumeItem(Item_Base item)
	{
		if (IsBowlItem(item))
		{
			ReturnBowl();
		}
	}

	/// <summary>
	/// Returns a bowl to the local player, if it does not break.
	/// </summary>
	private void ReturnBowl()
	{
		currentUses++;
		if (this.currentUses >= this.goalUses)
		{
			ResetCurrentUses();
		}
		else
		{
			RAPI.GetLocalPlayer().Inventory.AddItem(ItemManager.GetItemByName("Claybowl_Empty").UniqueName, 1);
		}
		SaveState();
	}

	/// <summary>
	/// Resets the current uses and generates a new goalUses value, based on
	/// minUses and maxUses. Does not save the state.
	/// </summary>
	private void ResetCurrentUses() {
		currentUses = 0;
		goalUses = UnityEngine.Random.Range(minUses, maxUses);
	}

	/// <summary>
	/// Checks whether an item contains a bowl.
	/// </summary>
	/// <param name="item">the item to check</param>
	/// <returns>true if the given item contains a bowl and false otherwise</returns>
	public bool IsBowlItem(Item_Base item) {
		return item.name.Contains("Claybowl") || item.name.Contains("ClayPlate");
	}
#endregion

#region Commands
	/// <summary>
	/// Parses and handles the 'breakInt' command.
	/// </summary>
	private void HandleBreakIntCommand()
	{
		string[] arguments = RConsole.lastCommands.LastOrDefault<string>().Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
		if (arguments.Length == 1)
		{
			LogInfo("The current break interval is <color=green>" + minUses + " - " + maxUses + "</color>.");
			LogFollowUp("Type <color=green>breakInt <minUses> <maxUses></color> to change the break interval, i.e. <color=green>breakInt 5 10</color> (3 is the default min-uses value and 5 is the default max-uses value).");
		}
		else if (arguments.Length != 3)
		{
			LogError("Invalid syntax!");
			LogFollowUp("Type <color=green>breakInt <minUses> <maxUses></color> to change the break interval, i.e. <color=green>breakInt 5 10</color> (3 is the default min-uses value and 5 is the default max-uses value).");
		}
		else
		{
			// parse int values
			int newMinUses;
			if (!int.TryParse(arguments[1], out newMinUses))
			{
				LogError("<color=green>" + arguments[1] + "</color> is not a valid integer value.");
				return;
			}
			int newMaxUses;
			if (!int.TryParse(arguments[2], out newMaxUses))
			{
				LogError("<color=green>" + arguments[2] + "</color> is not a valid integer value.");
				return;
			}

			// check range of new values
			if (newMinUses < 0)
			{
				LogError("<color=green><minUses></color> must not be a negative value.");
				return;
			}
			else if (newMaxUses < 0)
			{
				LogError("<color=green><maxUses></color> must not be a negative value.");
				return;
			}
			else if (newMaxUses < newMinUses)
			{
				LogError("<color=green><maxUses></color> can not be less than <color=green><minUses></color>.");
				return;
			}

			// apply new values
			this.minUses = newMinUses;
			this.maxUses = newMaxUses;
			ResetCurrentUses();
			SaveState();
			LogInfo("The break interval was set to <color=green>" + minUses + " - " + maxUses + "</color>.");
		}
	}
#endregion

#region Save and Load
	/// <summary>
	/// Loads the state (bowl usage and settings data) from PlayerPrefs.
	/// </summary>
  private void LoadState() {
    this.minUses = PlayerPrefs.GetInt(PP_MIN_USES, 3);
		this.maxUses = PlayerPrefs.GetInt(PP_MAX_USES, 5);
		this.currentUses = PlayerPrefs.GetInt(PP_CURRENT_USES, 0);
		this.goalUses = PlayerPrefs.GetInt(PP_GOAL_USES, 4);
		LogInfo("The current break interval is <color=green>" + minUses + " - " + maxUses + "</color>.");
  }

	/// <summary>
	/// Saves the state (bowl usage and settings data) to PlayerPrefs.
	/// </summary>
	private void SaveState() {
		PlayerPrefs.SetInt(PP_MIN_USES, this.minUses);
		PlayerPrefs.SetInt(PP_MAX_USES, this.maxUses);
		PlayerPrefs.SetInt(PP_CURRENT_USES, this.currentUses);
		PlayerPrefs.SetInt(PP_GOAL_USES, this.goalUses);
		PlayerPrefs.Save();
	}
#endregion
  
#region Logging
  /// <summary>
  /// Logs an informative message to the console.
  /// </summary>
  private void LogInfo(string message) {
      RConsole.Log("<color=#3498db>[info]</color>\t<b>Better Bowl mod:</b> " + message);
  }

  /// <summary>
  /// Logs an error message to the console.
  /// </summary>
  private void LogError(string message) {
      RConsole.LogError("<color=#e74c3c>[error]</color>\t<b>Better Bowl mod:</b> " + message);
  }

  /// <summary>
  /// Logs an follow-up message (without prefix) to the console.
  /// </summary>
  private void LogFollowUp(string message) {
      RConsole.Log("\t" + message);
  }
#endregion
}

#region Patches
/// <summary>
/// This patch hooks into the item consumption event to trigger the bowl return.
/// </summary>
[HarmonyPatch(typeof(PlayerStats))]
[HarmonyPatch("Consume")]
internal class Patch_PlayerStats_Consume
{
	private static void Postfix(PlayerStats __instance, Item_Base edibleItem)
	{
		if (__instance.GetComponent(typeof(Network_Player)) == RAPI.GetLocalPlayer()) 
		{
			ComponentManager<BetterBowlMod>.Value.OnConsumeItem(edibleItem);
		}
	}
}
#endregion
