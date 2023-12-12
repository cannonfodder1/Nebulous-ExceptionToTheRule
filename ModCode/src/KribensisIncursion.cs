using Bundles;
using Game;
using Game.Map;
using HarmonyLib;
using Missions;
using Missions.Nodes;
using Missions.Nodes.Flow;
using Missions.Nodes.Sequenced;
using Modding;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using TMPro;
using UI;
using UnityEngine;
using Utility;
using XNode;

namespace KribensisIncursion
{
    public class KribensisIncursion : IModEntryPoint
	{
		// Used later to determine which mod to load mission graphs from
		public static string ModName = "Kribensis Incursion";

		public static GameObject FacilityPrefab = null;

		public void PostLoad()
		{
			// Thank you to SomeUsername6 for the below code!
			// Here we grab the Facility from Station Capture mode

			ScenarioGraph stationCapture = null;
			foreach (ScenarioGraph scenario in BundleManager.Instance.AllScenarios)
            {
				if (scenario.ScenarioName == "Station Capture")
                {
					stationCapture = scenario;
					break;
                }
            }
			if (stationCapture == null)
			{
				Debug.LogError("Could not find ScenarioGraph by the name of Station Capture");
				return;
			}

			GameObject facilityPrefab = null;
			foreach (Node node in stationCapture.nodes)
			{
				CreateCapturePoint createCapturePoint = node as CreateCapturePoint;
				if (createCapturePoint != null)
				{
					facilityPrefab = createCapturePoint.Prefab;
					break;
				}
			}
			if (facilityPrefab == null)
			{
				Debug.LogError("ScenarioGraph does not contain a CreateCapturePoint node");
				return;
			}

			KribensisIncursion.FacilityPrefab = facilityPrefab;
		}

		public void PreLoad()
		{
			// we're patching BundleManager so the Harmony register has to happen in PreLoad
			Harmony harmony = new Harmony("nebulous.kribensis-incursion");
			harmony.PatchAll();

			// Thank you to SomeUsername6 for the below code!
			// We need to cache the ports of our custom nodes for XNode before the mod scenarios are loaded
			MethodInfo cachePortsMethodInfo = typeof(NodeDataCache).GetMethod("CachePorts", BindingFlags.NonPublic | BindingFlags.Static);

			cachePortsMethodInfo.Invoke(null, new object[] { typeof(PauseAllBotPlayers) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(SetGameTime) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(RotateShip90) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(RotateShip45) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(SetShipPosition) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(SetShipFormation) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(GetMissionBattlespace) });
		}
	}

	// Data types used for the Harmony patches later
	public class MissionLoaderManifest : BundleManifest
	{
		public MissionLoaderManifest()
		{

		}

		public List<MissionGraphEntry> MissionGraphMappings;
		
		public class MissionGraphEntry
		{
			[XmlAttribute]
			public string MissionName;

			[XmlAttribute]
			public string GraphAddress;

			public MissionGraphEntry()
			{

			}
		}

		public List<MissionBattlespaceEntry> MissionBattlespaceMappings;

		public class MissionBattlespaceEntry
		{
			[XmlAttribute]
			public string MissionName;

			[XmlAttribute]
			public string BattlespaceName;

			public MissionBattlespaceEntry()
			{

			}
		}
	}

	// Reimplementation of this function to allow for the static loading of mission graphs and inserting default battlespaces into missions
	[HarmonyPatch(typeof(BundleManager), "ProcessAssetBundle")]
	class Patch_BundleManager_ProcessAssetBundle
	{
		static bool Prefix(ref BundleManager __instance, AssetBundle bundle, ModInfo fromMod)
		{
			if (fromMod.ModName != KribensisIncursion.ModName)
			{
				return true;
			}

			Debug.Log("Processing asset bundle " + bundle.name);
			BundleManifest manifest = bundle.ReadXMLTextAsset<BundleManifest>("manifest.xml");
			bool flag = manifest == null;
			if (flag)
			{
				Debug.LogError("Could not process asset bundle.  No manifest found");
			}
			else
			{
				bool flag2 = !string.IsNullOrEmpty(manifest.ResourceFile);
				if (flag2)
				{
					ResourceDefinitions.ResourceFile resources = bundle.ReadXMLTextAsset<ResourceDefinitions.ResourceFile>(manifest.ResourceFile);
					bool flag3 = resources != null;
					if (flag3)
					{
						ResourceDefinitions.Instance.LoadResources(resources);
					}
					else
					{
						Debug.Log("Failed to read resources file '" + manifest.ResourceFile + "'");
					}
				}

				List<MissionSet> missionSets = (List<MissionSet>)Utilities.GetPrivateField(__instance, "_missionSets");

				MethodInfo method = __instance.GetType().GetMethod("LoadListEntries", BindingFlags.NonPublic | BindingFlags.Instance);
				MethodInfo generic = method.MakeGenericMethod(typeof(MissionSet));
				generic.Invoke(__instance, new object[] { manifest, manifest.MissionSets, bundle, missionSets, fromMod });

				// Keep the bundle loaded after the normal function is run, so we can access the missions from it in the postfix
				//bundle.Unload(false);
			}

			return false;
		}

		static void Postfix(ref BundleManager __instance, AssetBundle bundle, ModInfo fromMod)
		{
			if (fromMod.ModName != KribensisIncursion.ModName)
			{
				return;
			}

			Debug.Log("MissionGraphLoader evaluating asset bundle " + bundle.name);

			//BundleManifest manifest = bundle.ReadXMLTextAsset<BundleManifest>("manifest.xml");
			MissionLoaderManifest missionSettings = bundle.ReadXMLTextAsset<MissionLoaderManifest>("missions.xml");

			if (missionSettings == null)
			{
				Debug.Log("MissionGraphLoader not required for mod " + fromMod.ModName);

				return;
			}

			/*
			if (!string.IsNullOrEmpty(manifest.ResourceFile))
			{
				ResourceDefinitions.ResourceFile resources = bundle.ReadXMLTextAsset<ResourceDefinitions.ResourceFile>(manifest.ResourceFile);

				if (resources != null)
				{
					ResourceDefinitions.Instance.LoadResources(resources);
				}
				else
				{
					Debug.Log("Failed to read resources file '" + manifest.ResourceFile + "'");
				}
			}
			*/

			LoadMissionNodeGraphs(__instance, bundle, fromMod, missionSettings);

			LoadMissionBattlespaces(__instance, missionSettings);
		}

		private static void LoadMissionNodeGraphs(BundleManager __instance, AssetBundle bundle, ModInfo fromMod, MissionLoaderManifest missionSettings)
		{
			List<BundleManifest.Entry> missionGraphEntries = new List<BundleManifest.Entry>();

			foreach (MissionLoaderManifest.MissionGraphEntry mapping in missionSettings.MissionGraphMappings)
			{
				BundleManifest.Entry entry = new BundleManifest.Entry();
				entry.Name = mapping.MissionName;
				entry.Address = mapping.GraphAddress;

				missionGraphEntries.Add(entry);
			}

			Debug.Log("MissionGraphLoader nodegraph mappings found: " + missionGraphEntries.Count);

			List<MissionGraph> missionGraphs = new List<MissionGraph>();

			MethodInfo method = __instance.GetType().GetMethod("LoadListEntries", BindingFlags.NonPublic | BindingFlags.Instance);
			MethodInfo generic = method.MakeGenericMethod(typeof(MissionGraph));
			generic.Invoke(__instance, new object[] { missionSettings, missionGraphEntries, bundle, missionGraphs, fromMod });
			//Utilities.CallPrivateVoidMethod(__instance, "LoadListEntries", new object[] { manifest, missionGraphEntries, bundle, missionGraphs, fromMod });
			//this.LoadListEntries<ScenarioGraph>(manifest, manifest.Scenarios, bundle, missionGraphs, fromMod);

			Debug.Log("MissionGraphLoader nodegraphs found: " + missionGraphs.Count);

			if (missionGraphs.Count == 0)
            {
				return;
            }

			foreach (MissionSet missionSet in __instance.MissionSets)
			{
				Debug.Log("MissionGraphLoader iterating MissionSet: " + missionSet.CampaignName);

				foreach (Mission mission in missionSet.Missions)
				{
					Debug.Log("MissionGraphLoader iterating Mission: " + mission.MissionName);

					for (int i = 0; i < missionSettings.MissionGraphMappings.Count; i++)
					{
						MissionLoaderManifest.MissionGraphEntry mapping = missionSettings.MissionGraphMappings[i];

						Debug.Log("MissionGraphLoader iterating MissionGraph: " + mapping.MissionName);

						if (mission.MissionName == mapping.MissionName)
						{
							Debug.Log("MissionGraphLoader found match!");

							MissionGraph graph = missionGraphs[i];
							Utilities.SetPrivateField(mission, "_loadedGraph", graph);

							Debug.Log("MissionGraphLoader assigned nodegraph to mission: " + (mission.Graph != null));
						}
					}
				}
			}
		}

		private static void LoadMissionBattlespaces(BundleManager __instance, MissionLoaderManifest missionSettings)
		{
			Debug.Log("MissionGraphLoader nodegraph mappings found: " + missionSettings.MissionBattlespaceMappings.Count);

			List<Battlespace> battlespaces = __instance.AllMaps.ToList();

			Debug.Log("MissionGraphLoader battlespaces found: " + battlespaces.Count);

			foreach (MissionLoaderManifest.MissionBattlespaceEntry mapping in missionSettings.MissionBattlespaceMappings)
			{
				Battlespace targetBattlespace = battlespaces.Find(x => x.MapName == mapping.BattlespaceName);

				Debug.Log("MissionGraphLoader found " + mapping.BattlespaceName + ": " + targetBattlespace != null);

				foreach (MissionSet missionSet in __instance.MissionSets)
				{
					Debug.Log("MissionGraphLoader iterating MissionSet: " + missionSet.CampaignName);

					foreach (Mission mission in missionSet.Missions)
					{
						Debug.Log("MissionGraphLoader iterating Mission: " + mission.MissionName);

						if (mission.MissionName == mapping.MissionName)
						{
							Debug.Log("MissionGraphLoader found match!");

							MissionStartNode startNode = mission.Graph.nodes.FirstOrDefault((Node x) => x is MissionStartNode) as MissionStartNode;

							if (startNode != null)
							{
								startNode.MapGeo = targetBattlespace;

								Debug.Log("MissionGraphLoader assigned battlespace to mission: " + (startNode.MapGeo != null));
							}
                        }
					}
				}
			}
		}
	}

	// Additional logging to make developing and debugging mission graphs easier
	[HarmonyPatch(typeof(BaseMissionNode), "ExecuteStepSequence")]
	class Patch_BaseMissionNode_ExecuteStepSequence
	{
		static bool Prefix(ref BaseMissionNode __instance)
		{
			Debug.Log("Beginning Sequence from Flow Node " + __instance.name + " of Type " + __instance.GetType().FullName);

			return true;
		}
	}

	// Additional logging to make developing and debugging mission graphs easier
	[HarmonyPatch(typeof(SequencedNode), "GetNextStep")]
	class Patch_SequencedNode_GetNextStep
	{
		static bool Prefix(ref SequencedNode __instance)
		{
			Debug.Log("Executed Sequence Node " + __instance.name + " of Type " + __instance.GetType().FullName);

			NodePort port = __instance.GetOutputPort("NextStep");
			if (port == null)
			{
				Debug.Log("Failed to find output port");
				return true;
			}

			NodePort connection = port.Connection;
			if (connection == null)
            {
				Debug.Log("Failed to find valid connection");
				return true;
			}

			SequencedNode next = ((connection != null) ? connection.node : null) as SequencedNode;
			if (connection == null)
			{
				Debug.Log("Failed to find following node");
				return true;
			}

			Debug.Log("Proceeding to Execute following Node " + next.name);

			return true;
		}
	}

	// Disable the unloading of the nodegraph after mission exit
	// (which if unloaded would cause errors if player wanted to replay the same mission)
	[HarmonyPatch(typeof(Mission), "Unload")]
	class Patch_Mission_Unload
	{
		static bool Prefix(ref Mission __instance)
		{
			// do not run this function
			return false;
		}
	}
	
	// Renames the button on the main menu from Campaign/Tutorial to Campaign
	[HarmonyPatch(typeof(MainMenu), "SingleplayerButton")]
	class Patch_MainMenu_SingleplayerButton
	{
		static void Postfix(ref MainMenu __instance)
		{
			GameObject submenu = (GameObject)Utilities.GetPrivateField(__instance, "_singleplayerSubmenu");

			foreach (TextMeshProUGUI text in submenu.GetComponentsInChildren<TextMeshProUGUI>())
			{
				if (text.text == "Campaign/Tutorial")
                {
					text.text = "Campaign";

					return;
                }
			}
		}
	}

	// Copy the Facility model from the basegame Station Capture into our custom mission
	[HarmonyPatch(typeof(MissionGraph), "InitializeLobby")]
	class Patch_MissionGraph_InitializeLobby
	{
		static bool Prefix(ref MissionGraph __instance)
		{
			Mission mission = (Mission)Utilities.GetPrivateField(GameManager.Instance, "_mission");

			if (mission != null
			&& mission.Graph.name == __instance.name 
			&& mission.MissionName == "Righteous Fire")
			{
				if (KribensisIncursion.FacilityPrefab == null)
				{
					Debug.LogError("FacilityPrefab was not correctly set prior to mission dynamic load");
					return true;
				}

				foreach (Node node in __instance.nodes)
				{
					Debug.Log(node.GetType());
					CreateCapturePoint createCapturePoint = node as CreateCapturePoint;
					if (createCapturePoint != null)
					{
						createCapturePoint.Prefab = KribensisIncursion.FacilityPrefab;
					}
				}
			}

			return true;
		}
	}

	// Enables a mission's specified badge to override the player's set custom badge
	[HarmonyPatch(typeof(MissionGraph), "SetupPlayer")]
	class Patch_MissionGraph_SetupPlayer
	{
		static void Postfix(ref MissionGraph __instance, IPlayer player)
		{
			SkirmishLobbyPlayer lobbyPlayer = player as SkirmishLobbyPlayer;

			if (lobbyPlayer != null)
			{
				MissionStartNode startNode = (MissionStartNode)Utilities.GetPrivateField(__instance, "_startNode");

				if (startNode != null)
				{
					if (startNode.HumanPlayer.Badge != null)
					{
						lobbyPlayer.SetBadge(HullBadge.GetBadge(startNode.HumanPlayer.Badge.name));
					}
				}
			}
		}
	}

	// Enables missions to assign badges to ally and enemy bots
	[HarmonyPatch(typeof(SkirmishLobbyPlayer), "InitializePlayerWith")]
	class Patch_SkirmishLobbyPlayer_InitializePlayerWith
	{
		static bool Prefix(ref SkirmishLobbyPlayer __instance, ref HullBadge badge)
		{
			if (badge != null)
			{
				if (badge.Texture != null)
				{
					if (badge.Texture.name != null)
					{
						badge = HullBadge.GetBadge(badge.Texture.name);
					}
				}
			}

			return true;
		}
	}

	// Copies the mission definition's preview image over to the map recon image panel
	// nonfunctional, no way to convert a texture into a rawimage, would be difficult to dynamic or static load it
	/*
	[HarmonyPatch(typeof(SkirmishLobbyManager), "HostPrivateGame")]
	class Patch_SkirmishLobbyManager_HostPrivateGame
	{
		static void Postfix(ref SkirmishLobbyManager __instance, HostSkirmishLobbyData data)
		{
			if (data.Mission != null)
			{
				SkirmishMissionMenu missionMenu = (SkirmishMissionMenu)Utilities.GetPrivateField(__instance, "_lobbyMenu");

				if (missionMenu != null)
				{
					Debug.LogError("setting mission image");
					Utilities.SetPrivateField(missionMenu, "_missionImage", new RawImage(data.Mission.Screenshot.texture));
				}
			}
		}
	}
	*/
}
