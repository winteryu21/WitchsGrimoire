using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private Vector2 moveDirection;
    public float moveSpeed = 5f;

    void Update()
    {
        transform.Translate(moveDirection * Time.deltaTime * moveSpeed);
    }

    public void OnMove(InputValue value)
    {
        moveDirection = value.Get<Vector2>();
    }
}
