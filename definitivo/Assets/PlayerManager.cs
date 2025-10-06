using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Prefab do player")]
    public GameObject playerPrefab;

    private void Start()
    {
        // Verifica se há pelo menos 2 gamepads conectados
        if (Gamepad.all.Count < 2)
        {
            Debug.LogError("Conecte 2 gamepads para testar!");
            return;
        }

        // === Player 1 ===
        var p1 = PlayerInput.Instantiate(
            playerPrefab,
            controlScheme: "Gamepad",
            pairWithDevice: Gamepad.all[0]
        );
        var p1Handler = p1.GetComponent<PlayerInputHandler>();
        p1Handler.playerColor = Color.blue;
        p1Handler.playerName = "Player 1";

        // === Player 2 ===
        var p2 = PlayerInput.Instantiate(
            playerPrefab,
            controlScheme: "Gamepad",
            pairWithDevice: Gamepad.all[1]
        );
        var p2Handler = p2.GetComponent<PlayerInputHandler>();
        p2Handler.playerColor = Color.red;
        p2Handler.playerName = "Player 2";

        // Debug final
        Debug.Log("Dois jogadores instanciados com sucesso!");
        Debug.Log($"Player 1 pareado com: {Gamepad.all[0].displayName}");
        Debug.Log($"Player 2 pareado com: {Gamepad.all[1].displayName}");
    }
}
