using System.Collections.Generic;
using UnityEngine;

namespace Echoes.VN
{
    [CreateAssetMenu(fileName = "VN_ChoiceRegistry", menuName = "Echoes/VN_ChoiceRegistry", order = 0)]
    public class VN_ChoiceRegistry : ScriptableObject
    {
        [SerializeField] List<VN_ChoiceNode> nodes = new();

        public IReadOnlyList<VN_ChoiceNode> Nodes => nodes;

        public VN_ChoiceNode GetNode(int levelIndex, bool isMicro = false)
        {
            if (nodes == null) return null;
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.LevelIndex == levelIndex && n.IsMicroChoice == isMicro)
                    return n;
            }
            return null;
        }

        public int Count => nodes != null ? nodes.Count : 0;
    }
}
