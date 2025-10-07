using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private set => _rotateLeftPressed = value;
    }

    public bool RotateRight
    {
        get => _rotateRightPressed;
        private set => _rotateRightPressed = value;
    }

    public bool HardDrop
    {
        get => _hardDropPressed;
        private set => _hardDropPressed = value;
    }

}

public class InputManager : MonoBehaviour
{
    #region Input System
    InputAction moveAction;
    InputAction rotateAction;
    #endregion

    public PlayerInput PlayerInput;

    #region Keyboard Mouse Inputs
    float km_Horiz;
    float km_Vert;
    #endregion Keyboard Mouse Inputs

    public void Start()
    {
        Init_InputAction();
    }

    void Init_InputAction()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }

    public void GameLogicUpdate()
    {
        PlayerInput = new PlayerInput();

        MovementButtons();

        RotateButtons();



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

    }
}
