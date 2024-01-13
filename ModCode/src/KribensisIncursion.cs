using HarmonyLib;
using Modding;
using System.Reflection;
using XNode;

namespace KribensisIncursion
{
    public class KribensisIncursion : IModEntryPoint
	{
		public void PreLoad()
		{
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
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(GetPlayerPointValue) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(GetShipWithdrawn) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(GetShipDestroyed) });
			cachePortsMethodInfo.Invoke(null, new object[] { typeof(SpawnCalloutMarker) });
		}

		public void PostLoad()
		{
			// we don't have any Harmony patches, but leaving this here in case we do add any
			Harmony harmony = new Harmony("nebulous.exception-to-the-rule");
			harmony.PatchAll();
		}
	}
}
