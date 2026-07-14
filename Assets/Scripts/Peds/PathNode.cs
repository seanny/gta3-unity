using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GTA3Unity.Peds
{
    public class PathNode: MonoBehaviour
    {
        [SerializeField] private PathNode[] m_ConnectedNodes;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkRed;
            Gizmos.DrawSphere(transform.position, 0.5f);
            foreach(var node in m_ConnectedNodes)
            {
                Gizmos.DrawLine(transform.position, node.transform.position);
            }
        }

        public static PathNode GetNearestPathNode(Vector3 position)
        {
            var listOfPathNodes = GameObject.FindObjectsByType<PathNode>();
            PathNode node = null;
            float distance = float.MaxValue;
            foreach(var pathNode in listOfPathNodes)
            {
                float dist = Vector3.Distance(position, pathNode.transform.position);
                if(dist < distance)
                {
                    distance = dist;
                    node = pathNode;
                }
            }
            return node;
        }
    }
}