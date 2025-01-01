using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : GameCharacterSubScript
{
    [SerializeField] private string _moveUpButton = "MoveUp";
    [SerializeField] private string _moveDownButton = "MoveDown";
    [SerializeField] private string _moveLeftButton = "MoveLeft";
    [SerializeField] private string _moveRightButton = "MoveRight";

    [SerializeField] private string _MouseMoveX = "Mouse X";
    [SerializeField] private string _MouseMoveY = "Mouse Y";

    public Vector3 _pr_directionByInput { get; private set; }
    public Vector2 _pr_mouseMove { get; private set; }
    public float _pr_aimDegree { get; set; }

    private bool _inventoryOpen = false;
    public bool GetInventoryOpen() { return _inventoryOpen; }


    public override void Init(CharacterScript owner)
    {
        _myType = typeof(InputController);
        _owner = owner;
    }


    public override void SubScriptStart()
    {

    }

    void Update()
    {
        //상자열기 체크
        {
            if (Input.GetKeyDown(KeyCode.I) == true)
            {
                _inventoryOpen = true;
            }
            else
            {
                _inventoryOpen = false;
            }
        }

    }

    private void FixedUpdate()
    {
        CalculateDirByInput();
    }

    private void CalculateDirByInput()
    {
        float verticalValue = 0.0f;
        float horizontalValue = 0.0f;

        if (Input.GetButton(_moveUpButton) != Input.GetButton(_moveDownButton))
        {
            verticalValue = (Input.GetButton(_moveUpButton) == true)
                ? 1.0f
                : -1.0f;
        }

        if (Input.GetButton(_moveLeftButton) != Input.GetButton(_moveRightButton))
        {
            horizontalValue = (Input.GetButton(_moveRightButton) == true)
                ? 1.0f
                : -1.0f;
        }

        _pr_directionByInput = new Vector3(horizontalValue, 0.0f, verticalValue).normalized;
        _pr_mouseMove = new Vector2(Input.GetAxis(_MouseMoveX), Input.GetAxis(_MouseMoveY));
    }
}
