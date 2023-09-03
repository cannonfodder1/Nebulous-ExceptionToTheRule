using System.Reflection;
using UnityEngine;
using Game;
using Missions;
using Missions.Nodes.Sequenced;

namespace KribensisIncursion
{
    [NodeWidth(200)]
	public class SetGameTime : SequencedNode
	{
		public override bool Execute(IMissionGame gameControl)
		{
			Debug.Log("Executing SetGameTime");

			//SkirmishGameManager._gameEndTime = GameTime;

			FieldInfo field = typeof(SkirmishGameManager).GetField("_gameEndTime", BindingFlags.NonPublic | BindingFlags.Instance);

			if (field != null)
			{
				field.SetValue(SkirmishGameManager.Instance, GameTime);
			}

			return true;
		}

		[SerializeField]
		public int GameTime;
	}
}
