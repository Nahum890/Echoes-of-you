using UnityEngine;

namespace Echoes.VN
{
    [System.Serializable]
    public class VN_ChoiceNode
    {
        [SerializeField] string nodeId = "ch1_choice_1";
        [SerializeField] int levelIndex = 1;
        [SerializeField] bool isMicroChoice = false;
        [SerializeField] string promptKey = "vn.ch1.choice.1";
        [SerializeField] string cyanFlag = "allow_to_see";
        [SerializeField] string amberFlag = "avoid_looking";

        public string NodeId => nodeId;
        public int LevelIndex => levelIndex;
        public bool IsMicroChoice => isMicroChoice;
        public string PromptKey => promptKey;
        public string CyanFlag => cyanFlag;
        public string AmberFlag => amberFlag;
    }
}
