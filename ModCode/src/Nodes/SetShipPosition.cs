using UnityEngine;
using Missions;
using Missions.Nodes.Sequenced;
using Game.Units;

namespace ExceptionToTheRule
{
    [NodeWidth(250)]
	public class SetShipPosition : SequencedNode
	{
		public override bool Execute(IMissionGame gameControl)
		{
			ShipController ship = GetInputValue<ShipController>("Ship", null);
			Vector3 position = GetInputValue<Vector3>("Position", Vector3.zero);

			if (ship != null)
			{
				if (position != Vector3.zero)
				{
					ship.transform.position = position;
				}
				else
                {
					Debug.LogError("SetShipPosition node given null input for Position value");
                }
			}
			else
			{
				Debug.LogError("SetShipPosition node given null input for Ship value");
			}

			return true;
		}

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.None, false)]
		public ShipController Ship;

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.None, false)]
		public Vector3 Position;
	}
}
