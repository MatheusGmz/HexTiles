using Assets.Scripts.MapGenerator;
using Assets.Scripts.UnitManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controls
{
    public class MovementControls : MonoBehaviour
    {
        public Text counter;
        public MapData mapData;
        int movedSpaces = 0;
        public MoveableUnit selectedUnit;
        public List<string> currentPath = null;
        int maxMoves = 10;
        public PlayerCommands playerCommands;
        public bool isClimbing;
        int movesLeft;

        void Update()
        {
            var facingDirection = (int)selectedUnit.gameObject.transform.rotation.eulerAngles.y;
            mapData.VisionSight("Hex_" + 10 + "_" + 9, facingDirection);
            movesLeft = maxMoves - movedSpaces;
            counter.text = "Moves left " + movesLeft;
            var canWalk = movedSpaces < maxMoves && currentPath != null;
            int currentNeighbour = 0;

            if (canWalk)
            {
                if ((currentPath.Count - 1) < movesLeft)
                {
                    while (currentNeighbour < currentPath.Count - 1)
                    {
                        var currentStartPosition = GameObject.Find(currentPath[currentNeighbour]).GetComponentInChildren<HexData>();
                        var currentEndPosition = GameObject.Find(currentPath[currentNeighbour + 1]).GetComponentInChildren<HexData>();

                        Vector3 start = mapData.TileToCoordinate(currentStartPosition.tileX, currentStartPosition.tileY, currentStartPosition.tileZ) + new Vector3(0, 0.5f, 0);
                        Vector3 end = mapData.TileToCoordinate(currentEndPosition.tileX, currentEndPosition.tileY, currentEndPosition.tileZ) + new Vector3(0, 0.5f, 0);

                        Debug.DrawLine(start, end, Color.red);
                        currentNeighbour++;
                    }
                }
            }
            else
            {
                currentPath = null;
            }
        }

        public void LoadPath(int moves, string source, string target)
        {
            currentPath = null;
            maxMoves = moves;

            currentPath = mapData.GeneratePathToMove(source, target);
        }

        public void ResetMovement()
        {
            movedSpaces = 0;
        }
        public void BeginClimb(string source, string target)
        {
            var neighbours = mapData.GenerateNeighboursData(source);
            if (neighbours == null)
            {
                Debug.LogError("No neighbour was found.");
            }
            if (neighbours.Contains(target))
            {

                var sourceHeight = GameObject.Find(source).GetComponentInChildren<HexData>().tileZ;
                var targetHeight = GameObject.Find(target).GetComponentInChildren<HexData>().tileZ;

                if (targetHeight - sourceHeight >= 2)
                {
                    Debug.Log("You're climbing");
                    isClimbing = true;
                }
                else
                {
                    Debug.LogWarning("You can't climb here.");
                }
            }
            else
            {
                Debug.LogError("The target is not a neighbour.");
            }
        }
        public void ClimbUp(string target)
        {
            var targetHex = GameObject.Find(target).GetComponentInChildren<HexData>();

            if (isClimbing)
            {
                if (movesLeft > 0)
                {

                    if (targetHex.tileZ >= (selectedUnit.tileZ - 1))
                    {
                        var position = selectedUnit.transform.position;
                        selectedUnit.transform.position = new Vector3(position.x, position.y, position.z) + new Vector3(0, 0.25f, 0);
                        selectedUnit.tileZ = selectedUnit.tileZ + 0.25f;
                        movedSpaces++;
                    }
                    else
                    {
                        Debug.Log("You reach your destiny! You're not climbing anymore.");
                        selectedUnit.transform.position = mapData.TileToCoordinate(targetHex.tileX, targetHex.tileY, targetHex.tileZ + 1);
                        selectedUnit.tileX = targetHex.tileX;
                        selectedUnit.tileY = targetHex.tileY;
                        selectedUnit.tileZ = targetHex.tileZ + 1;
                        isClimbing = false;
                    }
                }
            }
            else
            {
                Debug.Log("You're not climbing!");
            }
        }
        public void ClimbDown()
        {
            if (isClimbing)
            {
                if (movesLeft > 0)
                {

                    var currentTile = GameObject.Find("Hex_" + selectedUnit.tileX + "_" + selectedUnit.tileY).GetComponentInChildren<HexData>().tileZ;
                    Debug.Log("Tile Z:" + currentTile);
                    if ((selectedUnit.tileZ - 1) > (currentTile + 0.25f))
                    {
                        var position = selectedUnit.transform.position;
                        selectedUnit.transform.position = new Vector3(position.x, position.y, position.z) + new Vector3(0, -0.25f, 0);
                        selectedUnit.tileZ = selectedUnit.tileZ - 0.25f;
                        movedSpaces++;
                    }
                    else
                    {
                        Debug.Log("You reach the ground, you're not climbing anymore.");
                        selectedUnit.transform.position = mapData.TileToCoordinate(selectedUnit.tileX, selectedUnit.tileY, currentTile + 1);
                        selectedUnit.tileZ = currentTile + 1;
                        isClimbing = false;
                        movedSpaces++;
                    }
                }
            }
            else
            {
                Debug.Log("You're not climbing!");
            }
        }
        public void DropFromClimb()
        {
            if (isClimbing)
            {
                var currentTile = GameObject.Find("Hex_" + selectedUnit.tileX + "_" + selectedUnit.tileY).GetComponentInChildren<HexData>().tileZ;
                selectedUnit.transform.position = mapData.TileToCoordinate(selectedUnit.tileX, selectedUnit.tileY, currentTile + 1);
                selectedUnit.tileZ = currentTile + 1;
                isClimbing = false;
            }
        }
        public void MoveNextTile()
        {
            float remaingMovement = 1;
            while (remaingMovement > 0)
            {
                if (currentPath == null)
                    break;
                var currentPosition = GameObject.Find(currentPath[0]).GetComponentInChildren<HexData>();
                var destinationPostion = GameObject.Find(currentPath[1]).GetComponentInChildren<HexData>();

                var tileCost = mapData.CostToEnterTile(currentPosition.name, destinationPostion.transform.parent.name);

                if ((int)tileCost + movedSpaces > selectedUnit.maxMoves)
                {
                    break;
                }
                remaingMovement -= tileCost;

                movedSpaces += (int)mapData.CostToEnterTile(currentPosition.transform.parent.name, destinationPostion.transform.parent.name);

                var currentHex = GameObject.Find("Hex_" + destinationPostion.tileX + "_" + destinationPostion.tileY).GetComponentInChildren<HexData>();

                if (isClimbing)
                {
                    var possibleNeighbours = new List<string>();
                    var destinyNeighbours = mapData.GenerateNeighboursData(destinationPostion.transform.parent.name);
                    foreach (var hex in destinyNeighbours)
                    {
                        var hexHeight = GameObject.Find(hex).GetComponentInChildren<HexData>().tileZ;
                        if (currentHex.tileZ < hexHeight)
                        {
                            possibleNeighbours.Add(hex);
                        }
                    }
                    if (possibleNeighbours.Count > 0)
                    {
                        selectedUnit.tileX = destinationPostion.tileX;
                        selectedUnit.tileY = destinationPostion.tileY;
                        selectedUnit.transform.position = mapData.TileToCoordinate(destinationPostion.tileX, destinationPostion.tileY, selectedUnit.tileZ);
                    }
                }
                else
                {
                    selectedUnit.tileX = destinationPostion.tileX;
                    selectedUnit.tileY = destinationPostion.tileY;
                    selectedUnit.transform.position = mapData.TileToCoordinate(destinationPostion.tileX, destinationPostion.tileY, currentHex.tileZ + 1);
                }

                currentPath.RemoveAt(0);

                if (currentPath.Count == 1)
                {
                    currentPath = null;
                }
                if (movedSpaces >= selectedUnit.maxMoves)
                {
                    currentPath = null;
                }
            }
        }
    }
}
