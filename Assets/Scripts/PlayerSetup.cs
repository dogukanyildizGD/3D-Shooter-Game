using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player))] 
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    [SerializeField]
    GameObject playerUIPrefab;
    [HideInInspector]
    public GameObject playerUIInstance;

    // Start is called before the first frame update
    void Start()
    {
        // Disable components that sould only be
        // active on the player that we control
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssingRemotePlayer();
        }
        else
        {

            // Create PlayerUI
            playerUIInstance = Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;

            GetComponent<Player>().SetupPlayer();

            // Configure PlayerUI
            PlayersUI ui = playerUIInstance.GetComponent<PlayersUI>();
            if (ui == null)
                Debug.LogError("No PlayerUI component on PlayerUI prefab.");
            ui.SetPlayer(GetComponent<Player>());

            string _username = "Loading...";

            if (UserAccountManager.IsLoggedIn)
            {
                _username = UserAccountManager.playerUsername;
            }
            else
            {
                _username = transform.name;
            }

            CmdSetUsername(transform.name, _username);
        }
    }

    [Command]
    void CmdSetUsername(string playerID, string username)
    {
        Player player = GameManager.GetPlayer(playerID);
        if (player != null)
        {
            Debug.Log(username + " has joined.");
            player.username = username;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    void AssingRemotePlayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    // When we are destroyed
    void OnDisable()
    {
        if (playerUIInstance != null)  // Null kontrolü ekle
        {
            playerUIInstance.SetActive(false);
        }

        if (isLocalPlayer)
            GameManager.instance.SetSceneCameraActive(true);

        GameManager.UnRegisterPlayer(transform.name);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
