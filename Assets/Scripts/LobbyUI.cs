using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mirror;

public class LobbyUI : MonoBehaviour
{
    public GameObject readyButtonPrefab;
    public Transform playerListParent;
    private Dictionary<uint, GameObject> playerUIDictionary = new Dictionary<uint, GameObject>();
    private Dictionary<uint, bool> playerReadyStates = new Dictionary<uint, bool>();    

    public void ClearAllPlayerUI()
    {
        foreach (var playerUI in playerUIDictionary.Values)
        {
            Destroy(playerUI);
        }

        playerUIDictionary.Clear();
    }

    public void UpdateUIForAllPlayers(List<LobbyPlayer> players)
    {
        ClearAllPlayerUI();
        foreach (var player in players)
        {
            CreatePlayerUI(player);
        }
    }

    public void CreatePlayerUI(LobbyPlayer player)
    {
        if (playerUIDictionary.ContainsKey(player.netId))
        {
            return;
        }

        GameObject newPlayerUI = Instantiate(readyButtonPrefab, playerListParent, false);
        Button readyButton = newPlayerUI.GetComponent<Button>();
        if (readyButton != null)
        {
            SetupPlayerUI(newPlayerUI, player);
            readyButton.onClick.AddListener(() => OnReadyClicked(player));
        }

        playerUIDictionary[player.netId] = newPlayerUI;

        UpdateReadyState(player, player.isReady);
    }

    public void SetupPlayerUI(GameObject newPlayerUI, LobbyPlayer player)
    {
        TMP_Text playerNameText = newPlayerUI.transform.Find("PlayerName")?.GetComponent<TMP_Text>();
        TMP_Text readyStatusText = newPlayerUI.transform.Find("ReadyStatus")?.GetComponent<TMP_Text>();

        if (playerNameText == null || readyStatusText == null)
        {
            Debug.LogError("PlayerName veya ReadyStatus bulunamadı.");
            return;
        }

        playerNameText.text = $"Player {player.netId}";
        readyStatusText.text = player.isReady ? "Ready" : "Not Ready";
    }

    public void OnReadyClicked(LobbyPlayer player)
    {
        if (player.isLocalPlayer)
        {
            player.CmdToggleReady();
        }
    }

    public void UpdateReadyStateByPlayerId(uint playerId, bool isReady)
    {
        if (playerUIDictionary.TryGetValue(playerId, out GameObject playerUI))
        {
            TMP_Text readyStatusText = playerUI.transform.Find("ReadyStatus")?.GetComponent<TMP_Text>();

            if (readyStatusText != null)
            {
                readyStatusText.text = isReady ? "Ready" : "Not Ready";
            }

            // Ready durumu sözlüğünü güncelle
            playerReadyStates[playerId] = isReady;
        }
    }

    public void UpdateReadyState(LobbyPlayer player, bool isReady)
    {
        if (playerUIDictionary.TryGetValue(player.netId, out GameObject playerUI))
        {
            TMP_Text readyStatusText = playerUI.transform.Find("ReadyStatus")?.GetComponent<TMP_Text>();

            if (readyStatusText != null)
            {
                readyStatusText.text = isReady ? "Ready" : "Not Ready";
            }
        }
    }

    public void RemovePlayerUI(LobbyPlayer player)
    {
        if (playerUIDictionary.TryGetValue(player.netId, out GameObject playerUI))
        {
            Destroy(playerUI);
            playerUIDictionary.Remove(player.netId);
        }
    }
}
