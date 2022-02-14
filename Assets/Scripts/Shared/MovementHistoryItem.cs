using System;

namespace Assets.Scripts.Models
{
    [Serializable]
    public class MovementHistoryItem
    {
        public float xPosition;
        public float yPosition;
        public float zPosition;
        public float frame;
        public Usercmd usercmd;
    }
}
