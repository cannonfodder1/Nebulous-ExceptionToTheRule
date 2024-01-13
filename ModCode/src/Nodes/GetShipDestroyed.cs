using UnityEngine;
using XNode;
using Game.Units;

namespace ExceptionToTheRule
{
	[NodeWidth(200)]
	public class GetShipDestroyed : Node
	{
		public override object GetValue(NodePort port)
		{
			ShipController ship = base.GetInputValue<ShipController>("Ship", null);
			if (ship != null)
			{
				if (port.fieldName == "Destroyed")
				{
					Ships.EliminationReason status = ship.StatusSummary.Eliminated;
					return status == Ships.EliminationReason.Destroyed || status == Ships.EliminationReason.Retired || status == Ships.EliminationReason.Evacuated;
				}
				else
				{
					Debug.LogError("GetShipDestroyed node does not recognize the requested port");
				}
			}
			else
			{
				Debug.LogError("GetShipDestroyed node is missing Ship input");
			}

			return null;
		}

		[Node.InputAttribute(Node.ShowBackingValue.Unconnected, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false, connectionType = Node.ConnectionType.Override)]
		public ShipController Ship;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public bool Destroyed;
	}
}
