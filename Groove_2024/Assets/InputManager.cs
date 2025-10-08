using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

public struct PlayerInput
{
    private bool _leftPressed;
    private bool _rightPressed;
    private bool _downPressed;

    private bool _rotateLeftPressed;
    private bool _rotateRightPressed;

    private bool _hardDropPressed;

    /*
    public PlayerInput()
    {
        _leftPressed = false;
        _rightPressed = false;
        _downPressed = false;

        _rotateLeftPressed = false;
        _rotateRightPressed = false;

        _hardDropPressed = false;
    }
    */

    public bool Left
    {
        get => _leftPressed;
        internal set => _leftPressed = value;
    }

    public bool Right
    {
        get => _rightPressed;
        internal set => _rightPressed = value;
    }

    public bool Down
    {
        get => _downPressed;
        internal set => _downPressed = value;
    }


    public bool RotateLeft
    {
        get => _rotateLeftPressed;
        internal set => _rotateLeftPressed = value;
    }

    public bool RotateRight
    {
        get => _rotateRightPressed;
        internal set => _rotateRightPressed = value;
    }

    public bool HardDrop
    {
        get => _hardDropPressed;
        internal set => _hardDropPressed = value;
    }

}

public class InputManager : MonoBehaviour
{
    #region Input System
    InputActionAsset inputActions;

    InputAction moveAction;
    InputAction rotateAction_CCW;
    InputAction rotateAction_CW;
    InputAction hardDrop;
    #endregion

    // Accessed in GameLogic for player input
    public PlayerInput PlayerInput;

    private void OnEnable()
    {
        inputActions.FindActionMap("Gameplay").Enable();
    }

    public void Start()
    {
        Init_InputAction();
    }

    void Init_InputAction()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        rotateAction_CCW = InputSystem.actions.FindAction("RotateCounterClock");
        rotateAction_CW = InputSystem.actions.FindAction("RotateClockwise");
        hardDrop = InputSystem.actions.FindAction("HardDrop");
    }

    public void GameLogicUpdate()
    {
        PlayerInput = new PlayerInput();

        MovementButtons();

        RotateButtons();

        HardDropButtons();

    }

    void MovementButtons()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        if (moveValue.magnitude < 0.35f)
            return;

        moveValue.Normalize();

        if (moveValue != new Vector2())
        {
            if (moveValue.y > 0.75f)
                return;

            if (moveValue.y < -0.75f)
            {
                PlayerInput.Down = true;
            }
            else
            {
                if (moveValue.x > 0.75f)
                    PlayerInput.Right = true;
                else if (moveValue.x < -0.75f)
                    PlayerInput.Left = true;
            }
        }
    }

    void RotateButtons()
    {
        PlayerInput.RotateLeft = rotateAction_CCW.IsPressed();
        PlayerInput.RotateRight = rotateAction_CW.IsPressed();
    }

    void HardDropButtons()
    {
        PlayerInput.HardDrop = hardDrop.IsPressed();
    }
}
