using System;
using UnityEngine;

namespace Assets.Scripts.MapGenerator
{
    [Serializable]
    public class TileTypeData
    {
        public string name;
        public GameObject tileVisualPrefab;

        public bool isWalkable;
        public float movementCost;
    }
}
