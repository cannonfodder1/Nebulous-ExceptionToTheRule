using UnityEngine;
using Missions;
using Missions.Nodes.Sequenced;
using Game;
using Game.Orders;
using System.Collections.Generic;
using Game.Orders.Inputs;
using Game.Orders.Tasks;
using Utility;

namespace KribensisIncursion
{
    [NodeWidth(200)]
	public class SpawnCalloutMarker : SequencedNode
	{
		public override bool Execute(IMissionGame gameControl)
		{
			HumanSkirmishPlayer player = GetInputValue<HumanSkirmishPlayer>("Player", null);
			Vector3 position = GetInputValue<Vector3>("Position", Vector3.zero);

			if (player != null)
			{
				if (position != Vector3.zero)
				{
					PlayerOrder calloutOrder = new PlayerOrder(new Dictionary<string, IOrderInput>
					{
						{
							"Target",
							new ConstantInput<WorldPositionInput.Output>(new WorldPositionInput.Output
							{
								Paths = new List<List<Vector3>>
								{
									new List<Vector3>
									{
										position
									}
								}
							})
						}
					}, new List<OrderTask>
					{
						new CalloutTask(player, (CalloutType)Callout, null)
					});
					player.QueueOrder(calloutOrder);
				}
				else
                {
					Debug.LogError("SpawnCalloutMarker node given null input for Position value");
                }
			}
			else
			{
				Debug.LogError("SpawnCalloutMarker node given null input for Player value");
			}

			return true;
		}

		[Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.None, false, connectionType = ConnectionType.Override)]
		public HumanSkirmishPlayer Player;

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.None, false)]
		public Vector3 Position;

		[SerializeField]
		public int Callout;
	}
}
