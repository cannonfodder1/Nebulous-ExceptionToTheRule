using UnityEngine;
using Missions;
using Missions.Nodes.Sequenced;
using Game.Units;

namespace ExceptionToTheRule
{
    [NodeWidth(250)]
	public class SetShipFormation : SequencedNode
	{
		public override bool Execute(IMissionGame gameControl)
		{
			ShipController leader = GetInputValue<ShipController>("ShipLeader", null);
			ShipController follower = GetInputValue<ShipController>("ShipFollower", null);

			if (leader != null)
			{
				if (follower != null)
				{
					follower.SetGuideShip(leader);
				}
				else
				{
					Debug.LogError("SetShipRotation node given null input for ShipFollower value");
				}
			}
			else
			{
				Debug.LogError("SetShipRotation node given null input for ShipLeader value");
			}

			return true;
		}

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.None, false)]
		public ShipController ShipLeader;

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.None, false)]
		public ShipController ShipFollower;
	}
}
