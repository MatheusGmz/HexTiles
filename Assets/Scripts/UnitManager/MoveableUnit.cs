using UnityEngine;

namespace Assets.Scripts.UnitManager
{
    public class MoveableUnit : MonoBehaviour
    {
        public int tileX;
        public int tileY;
        public float tileZ;
        public bool isClimbing;
        public float moveSpeed = 1;
        public int maxMoves = 10;
        public Vector3 destination;
        public int visionDistance = 5;

    }
}
