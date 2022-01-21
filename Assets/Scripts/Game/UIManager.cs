using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private Button startServerButton;

    [SerializeField]
    private Button executePhysicsButton;

    [SerializeField]
    private TextMeshProUGUI playersInGameText;

    private bool serverStarted = false;

    private void Awake()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        // Update the Players in game text GUI
        playersInGameText.text = $"Players in game: {PlayerManager.Instance.PlayersInGame}";
    }

    private void Start()
    {
        // Give functions to the GUI start buttons
        startHostButton.onClick.AddListener(() => 
        {
            NetworkManager.Singleton.StartHost();
        });

        startClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });

        startServerButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            serverStarted = true;
        };

        executePhysicsButton.onClick.AddListener(() =>
        {
            if (!serverStarted) return;

            SpawnerControl.Instance.SpawnObjects();
        });
    }
}
