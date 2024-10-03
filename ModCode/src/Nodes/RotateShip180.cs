using UnityEngine;
using Missions;
using Missions.Nodes.Sequenced;
using Game.Units;

namespace ExceptionToTheRule
{
	[NodeWidth(150)]
	public class RotateShip180 : SequencedNode
	{
		public override bool Execute(IMissionGame gameControl)
		{
			ShipController ship = GetInputValue<ShipController>("Ship", null);

			if (ship != null)
			{
				ship.transform.eulerAngles = ship.transform.eulerAngles + new Vector3(0, 180, 0);
			}
			else
			{
				Debug.LogError("RotateShip180 node given null input for Ship value");
			}

			return true;
		}

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.None, false)]
		public ShipController Ship;
	}
}