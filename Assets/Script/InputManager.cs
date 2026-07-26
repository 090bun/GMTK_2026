using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Vector2 moveInput;
    public bool isMenuPressed;

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    public void OnMenu(InputValue value){
        isMenuPressed = value.isPressed;
    }
}
