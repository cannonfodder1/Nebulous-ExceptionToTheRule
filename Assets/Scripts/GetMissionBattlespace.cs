using UnityEngine;
using Missions;
using XNode;
using Game.Map;
using Missions.Nodes.Flow;
using System.Linq;

namespace ExceptionToTheRule
{
    [NodeWidth(200)]
	public class GetMissionBattlespace : Node
	{
		public override object GetValue(NodePort port)
		{
			if (Battlespace != null)
			{
				if (port.fieldName == "TeamBHome")
				{
					return Battlespace.TeamBHomePosition ?? Vector3.zero;
				}
				else if (port.fieldName == "TeamAHome")
				{
					return Battlespace.TeamAHomePosition ?? Vector3.zero;
				}
				else if (port.fieldName == "HasCentralObjective")
				{
					return Battlespace.CentralObjectivePosition != null;
				}
				else if (port.fieldName == "DistributedObjectiveCount")
				{
					return Battlespace.DistributedObjectiveCount;
				}
				else if (port.fieldName == "HasTeamAHome")
				{
					return Battlespace.TeamAHomePosition != null;
				}
				else if (port.fieldName == "CentralObjective")
				{
					return Battlespace.CentralObjectivePosition ?? Vector3.zero;
				}
				else if (port.fieldName == "HasTeamBHome")
				{
					return Battlespace.TeamBHomePosition != null;
				}
				else if (port.fieldName == "MaxPlayers")
				{
					return Battlespace.MaxPlayers;
				}
				else if (port.fieldName == "DistributedObjectives")
				{
					return Battlespace.DistributedObjectives;
				}
                else
                {
					Debug.LogError("GetMissionBattlespace node does not recognize the requested port");
				}
			}
			else
			{
				Debug.LogError("GetMissionBattlespace node cannot find the current battlespace");
			}

			return null;
		}

		private Battlespace Battlespace
        {
            get
            {
				MissionGraph graph = this.graph as MissionGraph;
				MissionStartNode startNode = graph.nodes.FirstOrDefault((Node x) => x is MissionStartNode) as MissionStartNode;
				return startNode.MapGeo;
			}
        }

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public int MaxPlayers;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public bool HasCentralObjective;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public Vector3 CentralObjective;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public bool HasTeamAHome;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public Vector3 TeamAHome;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public bool HasTeamBHome;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public Vector3 TeamBHome;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public int DistributedObjectiveCount;

		[Node.OutputAttribute(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple, Node.TypeConstraint.None, false)]
		public Vector3[] DistributedObjectives;
    }
}
