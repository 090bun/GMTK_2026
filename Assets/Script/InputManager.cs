using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Vector2 moveInput;
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
