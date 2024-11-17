using UnityEngine;
using Mirror;

public class PauseGame : MonoBehaviour
{
    public static bool IsOn = false;

    private CustomNetworkManager customNetworkManager;

    void Start()
    {
        customNetworkManager = FindFirstObjectByType<CustomNetworkManager>();
    }

    public void LeaveRoom()
    {
        customNetworkManager.StopHost();
    }
}
