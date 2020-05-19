using Assets.Scripts.Controls;
using Assets.Scripts.UnitManager;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapGenerator
{
    public class MapData : MonoBehaviour
    {
        public TileTypeData[] tileTypes;
        public GameObject selectedUnit;
        int[,] tiles;
        int mapSizeX = 20;
        int mapSizeY = 20;
        int mapSizeZ = 20;
        List<string> allTiles;
        public MovementControls movementControls;

        float xOffset = 0.882f; // only for HexTiles
        float zOffset = 0.764f;

        int facingLeft = 90;
        int facingRight = 270;
        int facingRightUp = 210;
        int facingLeftUp = 150;
        int facingLeftDown = 35;
        int facingRightDown = 325;

        void Start()
        {
            selectedUnit.GetComponent<MoveableUnit>().tileX = (int)selectedUnit.transform.position.x;
            selectedUnit.GetComponent<MoveableUnit>().tileY = (int)selectedUnit.transform.position.z;

            GenerateMapData();
            allTiles = GenerateMapVisual();

        }
        void GenerateMapData()
        {
            tiles = new int[mapSizeX, mapSizeZ];
            int x, y;
            for (x = 0; x < mapSizeX; x++)
            {
                for (y = 0; y < mapSizeY; y++)
                {
                    tiles[x, y] = 0;
                }
            }
            for (x = 3; x <= 5; x++)
            {
                for (y = 0; y < 4; y++)
                {
                    tiles[x, y] = 2;
                }
            }
            tiles[4, 5] = 1;
            tiles[5, 5] = 1;
            tiles[6, 5] = 1;
            tiles[7, 5] = 1;
            tiles[8, 5] = 1;

            tiles[4, 6] = 1;
            tiles[4, 7] = 1;
            tiles[8, 6] = 1;
            tiles[8, 7] = 1;
        }

        List<string> GenerateMapVisual()
        {
            var tileList = new List<string>();
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeZ; y++)
                {
                    var randomZGen = "0," + Random.Range(0, mapSizeZ); // funciona, mas atrapalha os testes. 
                    var zConvert = float.TryParse(randomZGen, out var z);

                    if (!zConvert)
                    {
                        Debug.LogError("Algum tile não teve o valor Z convertido corretamente.");
                    }

                    float xPosition = x * xOffset;
                    if (y % 2 == 1)
                    {
                        xPosition += xOffset / 2f;
                    }
                    if (x == 0 && y == 0)
                    {
                        selectedUnit.GetComponent<MoveableUnit>().tileZ = selectedUnit.transform.position.y;
                        selectedUnit.transform.position = new Vector3(x, 1, y); //substituir o 1 por "z+1" para o player começar na altura do Hex
                    }
                    #region Região usada para testes. Substituir pelo randomico
                    if (y > 19)
                    {
                        var tt = tileTypes[tiles[x, y]];
                        GameObject hex_go = Instantiate(tt.tileVisualPrefab, new Vector3(xPosition, 2, y * zOffset), Quaternion.identity); // substituir o zero por "z" para gerar alturas aleatorias.
                        HexData hex = hex_go.GetComponentInChildren<HexData>();
                        hex_go.transform.SetParent(this.transform);
                        hex_go.name = "Hex_" + x + "_" + y;
                        tileList.Add(hex_go.name);

                        hex.tileX = x;
                        hex.tileY = y;
                        hex.tileZ = 2; // alterar o zero para Z para registrar a altura aleatoria.
                        hex.tileMap = this;
                    }
                    else if (x > 19)
                    {
                        var tt = tileTypes[tiles[x, y]];
                        GameObject hex_go = Instantiate(tt.tileVisualPrefab, new Vector3(xPosition, 0.6f, y * zOffset), Quaternion.identity); // substituir o zero por "z" para gerar alturas aleatorias.
                        HexData hex = hex_go.GetComponentInChildren<HexData>();
                        hex_go.transform.SetParent(this.transform);
                        hex_go.name = "Hex_" + x + "_" + y;
                        tileList.Add(hex_go.name);

                        hex.tileX = x;
                        hex.tileY = y;
                        hex.tileZ = 0.6f; // alterar o zero para Z para registrar a altura aleatoria.
                        hex.tileMap = this;
                    }
                    else
                    {
                        var tt = tileTypes[tiles[x, y]];
                        GameObject hex_go = Instantiate(tt.tileVisualPrefab, new Vector3(xPosition, 0, y * zOffset), Quaternion.identity); // substituir o zero por "z" para gerar alturas aleatorias.
                        HexData hex = hex_go.GetComponentInChildren<HexData>();
                        hex_go.transform.SetParent(this.transform);
                        hex_go.name = "Hex_" + x + "_" + y;
                        tileList.Add(hex_go.name);

                        hex.tileX = x;
                        hex.tileY = y;
                        hex.tileZ = 0; // alterar o zero para Z para registrar a altura aleatoria.
                        hex.tileMap = this;

                    }
                    #endregion

                }
            }
            return tileList;
        }

        public List<string> GeneratePathToMove(string source, string target) // passar a posição da unidade com source e a posição do hex destino como target.
        {
            if (!UnityCanEnterTile(target))
            {
                Debug.LogWarning("You can't walk there!");
            }
            var dist = new Dictionary<string, float>();
            var prev = new Dictionary<string, string>();

            var unvisited = new List<string>();

            dist[source] = 0;
            prev[source] = null;

            foreach (var v in allTiles)
            {
                if (v != source)
                {
                    dist[v] = Mathf.Infinity;
                    prev[v] = null;
                }
                unvisited.Add(v);
            }
            while (unvisited.Count > 0)
            {
                string u = null;
                foreach (var possiblePath in unvisited)
                {
                    if (u == null || dist[possiblePath] < dist[u])
                    {
                        u = possiblePath;
                    }
                }
                if (u == target)
                {
                    break;
                }

                unvisited.Remove(u);

                foreach (var v in GenerateNeighboursData(u))
                {
                    float alt = dist[u] + CostToEnterTile(u, v);
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                }
            }
            if (prev[target] == null)
            {
                Debug.LogWarning("No rout found!");
                return null;
            }

            var currentPath = new List<string>();
            var curr = target;

            while (curr != null)
            {
                currentPath.Add(curr);
                curr = prev[curr];
            }
            currentPath.Reverse();

            return currentPath;

        }

        public float CostToEnterTile(string source, string target)
        {
            var tileSourcePosition = GameObject.Find(source).GetComponentInChildren<HexData>();
            var tileTargetPosition = GameObject.Find(target).GetComponentInChildren<HexData>();

            var tileType = tileTypes[tiles[tileSourcePosition.tileX, tileSourcePosition.tileY]];

            float heightDistance = tileTargetPosition.tileZ - tileSourcePosition.tileZ;

            float cost = tileType.movementCost;
            if (heightDistance > 0.5)
            {
                cost++;
            }
            if (tileTargetPosition.tileZ - tileSourcePosition.tileZ > 1)
                return Mathf.Infinity;
            if (!UnityCanEnterTile(target))
                return Mathf.Infinity;


            return cost;

        }
        public bool UnityCanEnterTile(string tile)
        {
            var tileSourcePosition = GameObject.Find(tile).GetComponentInChildren<HexData>();
            var tileType = tileTypes[tiles[tileSourcePosition.tileX, tileSourcePosition.tileY]];
            return tileType.isWalkable;
        }
        public Vector3 TileToCoordinate(int x, int y, float z)
        {
            float xPosition = x * xOffset;
            if (y % 2 == 1)
            {
                xPosition += xOffset / 2f;
            }
            return new Vector3(xPosition, z, y * zOffset);
        }
        public List<string> GenerateNeighboursData(string tile)
        {
            var neighbours = new List<string>();

            var position = GameObject.Find(tile).GetComponentInChildren<HexData>();
            var x = position.tileX;
            var y = position.tileY;

            var hasTileRight = x > 0;
            var hasTileLeft = x < mapSizeX - 1;


            var hasEvenTileUpRight = y < mapSizeY - 1;
            var hasEvenTileUpLeft = x > 0 && y < mapSizeY - 1;
            var hasEvenTileDownLeft = y > 0 && x > 0;
            var hasEvenTileDownRight = y > 0;

            var hasOddTileUpRight = x < mapSizeX - 1 && y < mapSizeY - 1;
            var hasOddTileUpLeft = y < mapSizeY - 1;
            var hasOddTileDownRight = x < mapSizeX - 1 && y > 0;
            var hasOddTileDownLeft = y > 0;

            var tileRight = "Hex_" + (x - 1) + "_" + y;
            var tileLeft = "Hex_" + (x + 1) + "_" + y;

            var evenTileUpRight = "Hex_" + x + "_" + (y + 1);
            var evenTileUpLeft = "Hex_" + (x - 1) + "_" + (y + 1);
            var evenTileDownLeft = "Hex_" + (x - 1) + "_" + (y - 1);
            var evenTileDownRight = "Hex_" + x + "_" + (y - 1);

            var oddTileUpRight = "Hex_" + (x + 1) + "_" + (y + 1);
            var oddTileUpLeft = "Hex_" + x + "_" + (y + 1);
            var oddTileDownRight = "Hex_" + (x + 1) + "_" + (y - 1);
            var oddTileDownLeft = "Hex_" + x + "_" + (y - 1);

            if (hasTileRight)
            {
                neighbours.Add(tileRight);
            }

            if (hasTileLeft)
            {
                neighbours.Add(tileLeft);
            }

            var isEven = y % 2 == 0;

            if (isEven)
            {
                if (hasEvenTileUpRight)
                {
                    neighbours.Add(evenTileUpRight);
                }

                if (hasEvenTileUpLeft)
                {
                    neighbours.Add(evenTileUpLeft);
                }

                if (hasEvenTileDownLeft)
                {
                    neighbours.Add(evenTileDownLeft);
                }

                if (hasEvenTileDownRight)
                {
                    neighbours.Add(evenTileDownRight);
                }

            }
            else
            {
                if (hasOddTileUpRight)
                {
                    neighbours.Add(oddTileUpRight);
                }

                if (hasOddTileUpLeft)
                {
                    neighbours.Add(oddTileUpLeft);
                }

                if (hasOddTileDownLeft)
                {
                    neighbours.Add(oddTileDownLeft);
                }

                if (hasOddTileDownRight)
                {
                    neighbours.Add(oddTileDownRight);
                }
            }

            return neighbours;
        }

        List<string> FrontTiles(string tile)
        {
            var tileSource = GameObject.Find(tile).GetComponentInChildren<HexData>();
            var x = tileSource.tileX;
            var y = tileSource.tileY;

            var frontTiles = new List<string>();
            var facingDirection = selectedUnit.gameObject.transform.rotation.eulerAngles.y;



            var isEven = y % 2 == 0;

            if (isEven)
            {
                if (facingDirection == facingRight)
                {
                    frontTiles.Add("Hex_" + (x + 1) + "_" + y);
                    frontTiles.Add("Hex_" + x + "_" + (y + 1));
                    frontTiles.Add("Hex_" + x + "_" + (y - 1));
                }

                if (facingDirection == facingLeft)
                {
                    frontTiles.Add("Hex_" + (x - 1) + "_" + y);
                    frontTiles.Add("Hex_" + (x - 1) + "_" + (y + 1));
                    frontTiles.Add("Hex_" + (x - 1) + "_" + (y - 1));
                }

                if (facingDirection == facingRightUp)
                {
                    frontTiles.Add("Hex_" + x + "_" + (y + 1));
                    frontTiles.Add("Hex_" + (x + 1) + "_" + y);
                    frontTiles.Add("Hex_" + (x - 1) + "_" + (y + 1));

                }

                if (facingDirection == facingRightDown)
                {
                    frontTiles.Add("Hex_" + (x - 1) + "_" + (y - 1));
                    frontTiles.Add("Hex_" + x + "_" + (y - 1));
                    frontTiles.Add("Hex_" + (x + 1) + "_" + y);

                }

                if (facingDirection == facingLeftDown)
                {
                    frontTiles.Add("Hex_" + (x - 1) + "_" + y);
                    frontTiles.Add("Hex_" + (x - 1) + "_" + (y - 1));
                    frontTiles.Add("Hex_" + x + "_" + (y - 1));
                }

                if (facingDirection == facingLeftUp)
                {
                    frontTiles.Add("Hex_" + (x - 1) + "_" + y);
                    frontTiles.Add("Hex_" + (x - 1) + "_" + (y + 1));
                    frontTiles.Add("Hex_" + x + "_" + (y + 1));
                }
            }
            else //Odd front
            {
                if (facingDirection == facingRight)
                {
                    frontTiles.Add("Hex_" + (x + 1) + "_" + y);
                    frontTiles.Add("Hex_" + (x + 1) + "_" + (y + 1));
                    frontTiles.Add("Hex_" + (x + 1) + "_" + (y - 1));
                }

                if (facingDirection == facingLeft)
                {
                    frontTiles.Add("Hex_" + (x - 1) + "_" + y);
                    frontTiles.Add("Hex_" + x + "_" + (y + 1));
                    frontTiles.Add("Hex_" + x + "_" + (y - 1));
                }

                if (facingDirection == facingRightUp)
                {
                    frontTiles.Add("Hex_" + x + "_" + (y + 1));
                    frontTiles.Add("Hex_" + (x + 1) + "_" + (y + 1));
                    frontTiles.Add("Hex_" + (x + 1) + "_" + y);

                }

                if (facingDirection == facingRightDown)
                {
                    frontTiles.Add("Hex_" + x + "_" + (y - 1));
                    frontTiles.Add("Hex_" + (x + 1) + "_" + (y - 1));
                    frontTiles.Add("Hex_" + (x + 1) + "_" + y);

                }

                if (facingDirection == facingLeftDown)
                {
                    frontTiles.Add("Hex_" + (x - 1) + "_" + y);
                    frontTiles.Add("Hex_" + x + "_" + (y - 1));
                    frontTiles.Add("Hex_" + (x + 1) + "_" + (y - 1));
                }

                if (facingDirection == facingLeftUp)
                {
                    frontTiles.Add("Hex_" + (x - 1) + "_" + y);
                    frontTiles.Add("Hex_" + x + "_" + (y + 1));
                    frontTiles.Add("Hex_" + (x + 1) + "_" + (y + 1));
                }
            }

            return frontTiles;
        }
        List<string> BackTiles(string tile)
        {
            var tileSource = GameObject.Find(tile).GetComponentInChildren<HexData>();
            var x = tileSource.tileX;
            var y = tileSource.tileY;
            var facingDirection = selectedUnit.gameObject.transform.rotation.eulerAngles.y;

            var backTiles = new List<string>();



            var isEven = y % 2 == 0;

            if (isEven)
            {
                if (facingDirection == facingRight)
                {
                    backTiles.Add("Hex_" + (x - 1) + "_" + y);
                    backTiles.Add("Hex_" + (x - 1) + "_" + (y + 1));
                    backTiles.Add("Hex_" + (x - 1) + "_" + (y - 1));
                }

                if (facingDirection == facingLeft)
                {
                    backTiles.Add("Hex_" + (x + 1) + "_" + y);
                    backTiles.Add("Hex_" + x + "_" + (y + 1));
                    backTiles.Add("Hex_" + x + "_" + (y - 1));
                }

                if (facingDirection == facingRightUp)
                {
                    backTiles.Add("Hex_" + x + "_" + (y - 1));
                    backTiles.Add("Hex_" + (x - 1) + "_" + y);
                    backTiles.Add("Hex_" + (x - 1) + "_" + (y - 1));

                }

                if (facingDirection == facingRightDown)
                {
                    backTiles.Add("Hex_" + (x - 1) + "_" + y);
                    backTiles.Add("Hex_" + (x - 1) + "_" + (y + 1));
                    backTiles.Add("Hex_" + x + "_" + (y + 1));

                }

                if (facingDirection == facingLeftDown)
                {
                    backTiles.Add("Hex_" + (x - 1) + "_" + (y + 1));
                    backTiles.Add("Hex_" + x + "_" + (y + 1));
                    backTiles.Add("Hex_" + (x + 1) + "_" + y);
                }

                if (facingDirection == facingLeftUp)
                {
                    backTiles.Add("Hex_" + (x - 1) + "_" + (y - 1));
                    backTiles.Add("Hex_" + x + "_" + (y - 1));
                    backTiles.Add("Hex_" + (x + 1) + "_" + y);
                }
            }
            else // Odd Back
            {
                if (facingDirection == facingRight)
                {
                    backTiles.Add("Hex_" + (x - 1) + "_" + y);
                    backTiles.Add("Hex_" + x + "_" + (y + 1));
                    backTiles.Add("Hex_" + x + "_" + (y - 1));
                }

                if (facingDirection == facingLeft)
                {
                    backTiles.Add("Hex_" + (x + 1) + "_" + y);
                    backTiles.Add("Hex_" + (x + 1) + "_" + (y + 1));
                    backTiles.Add("Hex_" + (x + 1) + "_" + (y - 1));
                }

                if (facingDirection == facingRightUp)
                {
                    backTiles.Add("Hex_" + (x - 1) + "_" + y);
                    backTiles.Add("Hex_" + x + "_" + (y - 1));
                    backTiles.Add("Hex_" + (x + 1) + "_" + (y - 1));

                }

                if (facingDirection == facingRightDown)
                {
                    backTiles.Add("Hex_" + (x - 1) + "_" + y);
                    backTiles.Add("Hex_" + x + "_" + (y + 1));
                    backTiles.Add("Hex_" + (x + 1) + "_" + (y + 1));

                }

                if (facingDirection == facingLeftDown)
                {
                    backTiles.Add("Hex_" + x + "_" + (y + 1));
                    backTiles.Add("Hex_" + (x + 1) + "_" + (y + 1));
                    backTiles.Add("Hex_" + (x + 1) + "_" + y);
                }

                if (facingDirection == facingLeftUp)
                {
                    backTiles.Add("Hex_" + x + "_" + (y - 1));
                    backTiles.Add("Hex_" + (x + 1) + "_" + (y - 1));
                    backTiles.Add("Hex_" + (x + 1) + "_" + y);
                }
            }

            return backTiles;
        }


        public List<string> VisionSight(string tile, int rotation)
        {
            var tileSource = GameObject.Find(tile).GetComponentInChildren<HexData>();
            var visionDistance = selectedUnit.GetComponentInChildren<MoveableUnit>().visionDistance;
            var x = tileSource.tileX;
            var y = tileSource.tileY;
            var isEven = y % 2 == 0;
            if (rotation == facingRight)
            {
                return VisionRight(x, y, visionDistance, isEven);
            }
            if (rotation == facingLeft)
            {
                return VisionLeft(x, y, visionDistance, isEven);
            }
            if (rotation == facingRightDown)
            {
                return VisionRightDown(x, y, visionDistance, isEven);
            }
            return new List<string>();
        }


        List<string> VisionRight(int x, int y, int visionDistance, bool isEven)
        {
            var vision = new List<string>();
            int loopCounter = 0;
            int startingX = x;
            int linesAdded = 0;
            int maxValueX = visionDistance;
            bool addedX = false;
            while (loopCounter <= visionDistance)
            {
                if (loopCounter != 0)
                {
                    vision.Add("Hex_" + (x + loopCounter) + "_" + y); // adiciona a linha pra direita
                }
                if (addedX)
                {
                    if (isEven)
                    {
                        startingX++;
                    }
                    else
                    {
                        maxValueX--;
                    }
                    addedX = false;
                }
                else
                {
                    if (!isEven)
                    {
                        startingX++;
                    }
                    else
                    {
                        maxValueX--;
                    }
                    addedX = true;
                }
                int innerLoop = 0;
                linesAdded++;
                while ((startingX - x) + innerLoop <= maxValueX)
                {
                    vision.Add("Hex_" + (startingX + innerLoop) + "_" + (y + linesAdded)); // diagonal pra cima
                    vision.Add("Hex_" + (startingX + innerLoop) + "_" + (y - linesAdded)); // diagonal pra b
                    innerLoop++;
                }
                loopCounter++;
            }
            foreach (var v in vision)
            {
                var hex = GameObject.Find(v);
                if (hex != null)
                {
                    hex.GetComponentInChildren<Renderer>().material.color = Color.yellow;
                }
                Debug.Log(vision.Count);
            }
            return vision;
        }

        List<string> VisionLeft(int x, int y, int visionDistance, bool isEven)
        {
            var vision = new List<string>();
            int loopCounter = 0;
            int startingX = x;
            int linesAdded = 0;
            int maxValueX = visionDistance;
            bool addedX = false;
            int negativeX = x;
            while (loopCounter <= visionDistance)
            {
                if (loopCounter != 0)
                {
                    vision.Add("Hex_" + (x - loopCounter) + "_" + y); // adiciona a linha pra esquerda
                }
                if (addedX)
                {
                    if (isEven)
                    {
                        maxValueX--;
                    }
                    else
                    {
                        negativeX--;
                        startingX++;
                    }
                    addedX = false;
                }
                else
                {
                    if (!isEven)
                    {
                        maxValueX--;
                    }
                    else
                    {
                        negativeX--;
                        startingX++;
                    }
                    addedX = true;
                }
                int innerLoop = 0;
                linesAdded++;
                while ((startingX - x) + innerLoop <= maxValueX)
                {
                    vision.Add("Hex_" + (negativeX - innerLoop) + "_" + (y + linesAdded)); // diagonal pra cima
                    vision.Add("Hex_" + (negativeX - innerLoop) + "_" + (y - linesAdded)); // diagonal pra baixo
                    innerLoop++;
                }
                loopCounter++;
            }
            foreach (var v in vision)
            {
                var hex = GameObject.Find(v);
                if (hex != null)
                {
                    hex.GetComponentInChildren<Renderer>().material.color = Color.yellow;
                }
                Debug.Log(vision.Count);
            }
            return vision;
        }

        List<string> VisionRightDown(int x, int y, int visionDistance, bool isEven)
        {
            var vision = new List<string>();
            int loopCounter = 0;
            int startingX = x;
            int negativeStartingX = x;
            int linesAdded = 0;
            int maxValueX = visionDistance;
            int negativeLinesAdded = maxValueX;
            bool addedX = false;
            int negativeX = x;
            while (loopCounter <= visionDistance)
            {
                if (loopCounter != 0)
                {
                    vision.Add("Hex_" + (x + loopCounter) + "_" + y); // adiciona a linha pra direita
                }
                if (addedX)
                {
                    if (isEven)
                    {
                        startingX++;
                    }
                    else
                    {
                        negativeX--;
                        maxValueX--;
                    }
                    addedX = false;
                }
                else
                {
                    if (!isEven)
                    {
                        startingX++;
                    }
                    else
                    {
                        negativeX--;
                        maxValueX--;
                    }
                    addedX = true;
                }
                int innerLoop = 0;
                linesAdded++;
                negativeLinesAdded--;
                while ((startingX - x) + innerLoop <= maxValueX)
                {
                    vision.Add("Hex_" + (startingX + innerLoop) + "_" + (y - linesAdded)); // wonderful
                    vision.Add("Hex_" + (x - startingX) + "_" + (y - negativeLinesAdded)); // diagonal pra baixo
                    innerLoop++;
                }
                loopCounter++;
            }
            foreach (var v in vision)
            {
                var hex = GameObject.Find(v);
                if (hex != null)
                {
                    hex.GetComponentInChildren<Renderer>().material.color = Color.yellow;
                }
                Debug.Log(vision.Count);
            }
            return vision;
        }
    }
}
