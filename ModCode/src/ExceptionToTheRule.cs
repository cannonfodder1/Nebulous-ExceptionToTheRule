using Game;
using Game.AI;
using Game.Orders;
using HarmonyLib;
using Modding;
using Ships.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utility;
using XNode;

namespace ExceptionToTheRule
{
    public class ExceptionToTheRule : IModEntryPoint
	{
		public void PreLoad()
		{
			// Thank you to SomeUsername6 for the below code!
			// We need to cache the ports of our custom nodes for XNode before the mod scenarios are loaded
			MethodInfo cachePortsMethodInfo = typeof(NodeDataCache).GetMethod("CachePorts", BindingFlags.NonPublic | BindingFlags.Static);

			cachePortsMethodInfo.Invoke(null, new object[] { typeof(PauseAllBotPlayers) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(SetGameTime) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(SetShipPosition) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(SetShipFormation) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(GetMissionBattlespace) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(GetPlayerPointValue) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(GetShipWithdrawn) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(GetShipDestroyed) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(SpawnCalloutMarker) });
		}

		public void PostLoad()
		{
			Harmony harmony = new Harmony("nebulous.exception-to-the-rule");
			harmony.PatchAll();
		}
	}

	// Forces the AI to move towards the player in mission 4, instead of wandering off into Canyon
	[HarmonyPatch(typeof(BotSkirmishPlayer), "GetInitialSearchLocations")]
	class Patch_BotSkirmishPlayer_GetInitialSearchLocations
	{
		static bool Prefix(ref BotSkirmishPlayer __instance, ref List<Vector3> __result, int count)
		{
			if (!SkirmishLobbyManager.Instance.IsSoloGame)
			{
				return true;
			}

			Missions.Mission mission = (Missions.Mission)Utilities.GetPrivateField(SkirmishLobbyManager.Instance, "_mission");

			if (mission.MissionName == "Calculated Hubris")
			{
				__result = new List<Vector3>();
				for (int i = 0; i < count; i++)
				{
					__result.Add(SkirmishGameManager.Instance.SkirmishLocalPlayer.PlayerFleet.GetShip(i).Controller.Position);
				}
				return false;
			}

			return true;
		}
	}
	/*
	// Prevents the AI from firing at ELINT bearings or crossfixes
	[HarmonyPatch(typeof(TaskUnit), "PickTargetsForShips")]
	class Patch_TaskUnit_PickTargetsForShips
	{
		static bool Prefix(ref TaskUnit __instance, List<Blackboard.TrackedEnemyShip> targets)
		{
			if (targets == null)
			{
				return true;
			}

			List<Blackboard.TrackedEnemyShip> validTargets = new List<Blackboard.TrackedEnemyShip>();

			foreach (Blackboard.TrackedEnemyShip target in targets)
			{
				if (!target.LOBOnly && target.BestTQ > 7)
				{
					validTargets.Add(target);
				}
			}

			if (validTargets.Count == 0)
			{
				return false;
			}

			return true;
		}
	}
	*/
	/*
	[HarmonyPatch(typeof(AICaptain), "AssignWeaponTargets")]
	class Patch_AICaptain_AssignWeaponTargets
	{
		static bool Prefix(ref AICaptain __instance, ref List<PlayerOrder> __result, List<Blackboard.TrackedEnemyShip> targets, Blackboard.TrackedEnemyShip priority)
		{
			if (targets == null && priority == null)
			{
				return true;
			}

			List<Blackboard.TrackedEnemyShip> validTargets = new List<Blackboard.TrackedEnemyShip>();

			foreach (Blackboard.TrackedEnemyShip target in targets)
			{
				if (!target.LOBOnly && target.BestTQ > 7)
				{
					validTargets.Add(target);
				}
			}

			if (validTargets.Count == 0 && priority == null)
			{
				__result = new List<PlayerOrder>();
				return false;
			}

			return true;
		}
	}
	*/
	/*
	[HarmonyPatch(typeof(AICaptain), "GetOptimalTargetForWeapon")]
	class Patch_AICaptain_GetOptimalTargetForWeapon
	{
		static bool Prefix(ref AICaptain __instance, ref Blackboard.TrackedEnemyShip __result, IWeaponGroup group, List<Blackboard.TrackedEnemyShip> targets, bool allowSuboptimalMatch = true, bool checkRange = false)
		{
			bool flag = group == null;
			Blackboard.TrackedEnemyShip result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Vector3 shipPosition = __instance.transform.position;

				Blackboard.TrackedEnemyShip exactMatch = targets.FirstOrDefault((Blackboard.TrackedEnemyShip x) => x.Ship.HullClass == group.OptimalTargetWeight && (!checkRange || Vector3.Distance(shipPosition, x.LastKnownPosition) < (group.GetMaxEffectiveRange() ?? float.MaxValue)));
				bool flag2 = exactMatch != null;
				if (flag2)
				{
					result = exactMatch;
				}
				else
				{
					List<Blackboard.TrackedEnemyShip> validTargets = new List<Blackboard.TrackedEnemyShip>();

					foreach (Blackboard.TrackedEnemyShip target in targets)
					{
						if (!target.LOBOnly && target.BestTQ > 7)
						{
							validTargets.Add(target);
						}
					}

					bool flag3 = group.OptimalTargetWeight >= WeightClass.Heavy;
					Blackboard.TrackedEnemyShip match;
					if (flag3)
					{
						match = validTargets.FirstOrDefault((Blackboard.TrackedEnemyShip x) => x.Ship.HullClass >= group.OptimalTargetWeight && (!checkRange || Vector3.Distance(shipPosition, x.LastKnownPosition) < (group.GetMaxEffectiveRange() ?? float.MaxValue)));
					}
					else
					{
						match = validTargets.FirstOrDefault((Blackboard.TrackedEnemyShip x) => x.Ship.HullClass <= group.OptimalTargetWeight && (!checkRange || Vector3.Distance(shipPosition, x.LastKnownPosition) < (group.GetMaxEffectiveRange() ?? float.MaxValue)));
					}
					bool flag4 = match == null && allowSuboptimalMatch;
					if (flag4)
					{
						match = validTargets.FirstOrDefault<Blackboard.TrackedEnemyShip>();
					}
					result = match;
				}
			}

			__result = result;
			return false;
		}
	}
	*/
}
