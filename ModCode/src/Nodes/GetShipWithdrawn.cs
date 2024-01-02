using UnityEngine;
using XNode;
using Game.Units;

namespace KribensisIncursion
{
	[NodeWidth(200)]
	public class GetShipWithdrawn : Node
	{
		public override object GetValue(NodePort port)
		{
			ShipController ship = base.GetInputValue<ShipController>("Ship", null);
			if (ship != null)
			{
				if (port.fieldName == "Withdrawn")
				{
					return ship.StatusSummary.Eliminated == Ships.EliminationReason.Withdrew;
				}
				else
				{
					Debug.LogError("GetShipWithdrawn node does not recognize the requested port");
				}
			}
			else
			{
				Debug.LogError("GetShipWithdrawn node is missing Ship input");
			}

			return null;
		}

		[Node.InputAttribute(Node.ShowBackingValue.Unconnected, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false, connectionType = Node.ConnectionType.Override)]
		public ShipController Ship;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public bool Withdrawn;
	}
}
