using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public MatchSettings matchSettings;

    [SerializeField]
    private GameObject sceneCamera;

    public delegate void OnPlayerKilledCallBack(string player, string source);

    public OnPlayerKilledCallBack onPlayerKilledCallBack;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("More than one GameManager in scene");
        }
        else
        {
            instance = this;
        }
    }

    public void SetSceneCameraActive(bool isActive)
    {
        if (sceneCamera == null)
            return;

        sceneCamera.SetActive(isActive);
    }

    #region Player Tracking

    private const string PLAYER_ID_PREFIX = "Player ";

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    public static void RegisterPlayer(string _netID, Player _player)
    {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
    }

    public static void UnRegisterPlayer(string _playerID)
    {
        if (players.ContainsKey(_playerID))
        {
            players.Remove(_playerID);
        }
        else
        {
            Debug.LogWarning("UnRegisterPlayer: Player ID bulunamadý: " + _playerID);
        }
    }


    public static Player GetPlayer(string _playerID)
    {
        if (players.ContainsKey(_playerID))  // Dictionary'de olup olmadýðýný kontrol et
        {
            return players[_playerID];
        }
        else
        {
            Debug.LogError("Player ID bulunamadý: " + _playerID);  // Hata mesajý
            return null;
        }
    }

    public static Player[] GetAllPlayers()
    {
        return players.Values.ToArray();
    }

    #endregion

}
