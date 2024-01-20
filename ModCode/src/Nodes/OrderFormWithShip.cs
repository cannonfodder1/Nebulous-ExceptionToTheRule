using UnityEngine;
using Missions;
using Missions.Nodes.Sequenced;
using Game.Units;
using Game.Orders;
using Game.Orders.Inputs;
using System.Collections.Generic;
using Game.Orders.Tasks;
using Game;

namespace ExceptionToTheRule
{
    [NodeWidth(250)]
	public class OrderFormWithShip : SequencedNode
	{
		public override bool Execute(IMissionGame gameControl)
		{
			ShipController leader = GetInputValue<ShipController>("ShipLeader", null);
			ShipController follower = GetInputValue<ShipController>("ShipFollower", null);
			HumanSkirmishPlayer player = GetInputValue<HumanSkirmishPlayer>("Player", null);

			if (leader != null)
			{
				if (follower != null)
				{
					if (Offset != Vector3.zero)
					{
						if (player != null)
						{
							PlayerOrder formOrder = new PlayerOrder(new Dictionary<string, IOrderInput>
							{
								{
									"Guide",
									new ConstantInput<ShipController>(leader)
								},
								{
									"Target",
									new ConstantInput<WorldPositionInput.Output>(new WorldPositionInput.Output
									{
										Origin = leader.Position,
										Paths = new List<List<Vector3>>
										{
											new List<Vector3>
											{
												leader.Position + Offset
											}
										}
									})
								}
							}, new List<OrderTask>
							{
								new KeepFormationTask(player, follower, null)
							});
							formOrder.PrepareTasks();
							player.SendShipOrder(formOrder);
						}
						else
						{
							Debug.LogError("OrderFormWithShip node given null input for Player value");
						}
					}
					else
					{
						Debug.LogError("OrderFormWithShip node given zero input for Offset value");
					}
				}
				else
				{
					Debug.LogError("OrderFormWithShip node given null input for ShipFollower value");
				}
			}
			else
			{
				Debug.LogError("OrderFormWithShip node given null input for ShipLeader value");
			}

			return true;
		}

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.None, false)]
		public ShipController ShipLeader;

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.None, false)]
		public ShipController ShipFollower;

		[SerializeField]
		public Vector3 Offset;

		[Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.None, false)]
		public HumanSkirmishPlayer Player;
	}
}
