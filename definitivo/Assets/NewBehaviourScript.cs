using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerInputHandler : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveInput;

    [Header("Configurações do jogador")]
    public float speed = 5f;
    public Color playerColor = Color.white;
    public Material playerMaterial;

    [Header("Debug")]
    public string playerName = "Player";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (playerMaterial != null)
            playerMaterial.color = playerColor;
    }

    // Método chamado pelo PlayerInput → Unity Event
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"{playerName} Move input: {moveInput}");
    }

    private void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
    }
}
