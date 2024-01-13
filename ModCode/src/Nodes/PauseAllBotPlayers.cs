using System.Reflection;
using UnityEngine;
using Game;
using Missions;
using Missions.Nodes.Sequenced;

namespace ExceptionToTheRule
{
    [NodeWidth(200)]
	public class PauseAllBotPlayers : SequencedNode
	{
		public override bool Execute(IMissionGame gameControl)
		{
			//BotSkirmishPlayer._botsPaused = Paused;

			FieldInfo field = typeof(BotSkirmishPlayer).GetField("_botsPaused", BindingFlags.NonPublic | BindingFlags.Static);

			if (field != null)
			{
				field.SetValue(null, Paused);
			}

			return true;
		}

		[SerializeField]
		public bool Paused = true;
	}
}
