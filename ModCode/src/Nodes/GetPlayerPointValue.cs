using UnityEngine;
using XNode;
using Game;
using Game.Units;
using Ships;

namespace ExceptionToTheRule
{
    [NodeWidth(200)]
	public class GetPlayerPointValue : Node
	{
		public override object GetValue(NodePort port)
		{
			SkirmishPlayer player = base.GetInputValue<SkirmishPlayer>("Player", null);
			if (player != null)
			{
				if (port.fieldName == "TotalPointValue")
				{
					return player.PlayerFleet.InitialFleetValue;
				}
				else if (port.fieldName == "IntactPointValue")
				{
					int intactPoints = 0;
					foreach (Ship ship in player.PlayerFleet.FleetShips)
                    {
						if (!ship.Controller.IsEliminated)
                        {
							intactPoints += ship.GetPointCost(true);
                        }
					}
					return intactPoints;
				}
                else
                {
					Debug.LogError("GetPlayerPointValue node does not recognize the requested port");
				}
			}
			else
			{
				Debug.LogError("GetPlayerPointValue node is missing Player input");
			}

			return null;
		}

		[Node.InputAttribute(Node.ShowBackingValue.Unconnected, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false, connectionType = Node.ConnectionType.Override)]
		public SkirmishPlayer Player;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public int TotalPointValue;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public int IntactPointValue;
	}
}
