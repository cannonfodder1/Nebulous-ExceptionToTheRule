using UnityEngine;
using Missions;
using Missions.Nodes.Sequenced;
using Game.Units;

namespace KribensisIncursion
{
    [NodeWidth(150)]
	public class RotateShip45 : SequencedNode
	{
		public override bool Execute(IMissionGame gameControl)
		{
			ShipController ship = GetInputValue<ShipController>("Ship", null);

			if (ship != null)
			{
				ship.transform.eulerAngles = ship.transform.eulerAngles + new Vector3(0, -45, 0);
            }
            else
			{
				Debug.LogError("RotateShip45 node given null input for Ship value");
			}

			return true;
		}

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.None, false)]
		public ShipController Ship;
	}
}
