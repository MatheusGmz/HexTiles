using Assets.Scripts.Controls;
using Assets.Scripts.MapGenerator;
using UnityEngine;

namespace Assets.Scripts.UnitManager
{
    public class PlayerCommands : MonoBehaviour
    {
        public GameObject selectedUnit;
        public MovementControls movementControls;
        public MapData mapData;
        HexData selectedHex;
        void Update()
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                var ourHitObject = hitInfo.collider.transform.parent.gameObject;

                if (ourHitObject.GetComponentInChildren<HexData>() != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        selectedHex = ourHitObject.GetComponentInChildren<HexData>();
                    }
                    if (selectedHex != null && Input.GetMouseButtonDown(0))
                    {
                        MoveUnit(selectedHex);
                       
                    }
                    if (Input.GetMouseButtonDown(1))
                    {
                        Debug.Log("Click!");
                        var neigh = mapData.GenerateNeighboursData(ourHitObject.name);
                        foreach (var n in neigh)
                        {
                            Debug.Log(n);
                        }
                    }
                }
            }
        }
        void MoveUnit(HexData target)
        {
            var source = GameObject.Find("Hex_" + selectedUnit.GetComponent<MoveableUnit>().tileX + "_" + selectedUnit.GetComponent<MoveableUnit>().tileY);
            if (Input.GetMouseButtonDown(0))
            {
                movementControls.LoadPath(selectedUnit.GetComponent<MoveableUnit>().maxMoves, source.name, target.transform.parent.name);
            }
        }
        public void BeginClimb()
        {
            var position = selectedUnit.GetComponent<MoveableUnit>();
            var source = "Hex_" + position.tileX + "_" + position.tileY;
            Debug.Log("The selected hex is: "+ selectedHex.transform.parent.name);
            movementControls.BeginClimb(source, selectedHex.transform.parent.name);
        }
    
        public void ClimbUp()
        {
            movementControls.ClimbUp(selectedHex.transform.parent.name);
        }
        public void ClimbDown()
        {
            movementControls.ClimbDown();
        }
        public void DropFromClimb()
        {
            movementControls.DropFromClimb();
        }
       public void ResetMovement()
        {
            movementControls.ResetMovement();

        }
    }
}
