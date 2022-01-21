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
            if (NetworkManager.Singleton.StartHost())
                Logger.Instance.LogInfo("Host started...");
            else
                Logger.Instance.LogInfo("Unable to start host...");
        });

        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
                Logger.Instance.LogInfo("Client started...");
            else
                Logger.Instance.LogInfo("Unable to start client...");
        });

        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
                Logger.Instance.LogInfo("Server started...");
            else
                Logger.Instance.LogInfo("Unable to start server...");
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
