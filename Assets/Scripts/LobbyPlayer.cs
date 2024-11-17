using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class LobbyPlayer : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnReadyStateChanged))]
    public bool isReady;

    public int teamIndex; // Takım indeksini belirleyen alan

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Oyuncu UI'sini sadece yerel oyuncu değil, her oyuncu için oluştur
        LobbyUI ui = FindObjectOfType<LobbyUI>();
        if (ui != null)
        {
            ui.CreatePlayerUI(this);
        }
    }

    void OnReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        LobbyUI ui = FindObjectOfType<LobbyUI>();
        if (ui != null)
        {
            ui.UpdateReadyState(this, newReadyState);
        }
    }

    [Command]
    public void CmdSetReady(bool readyStatus)
    {
        isReady = readyStatus;
        CmdChangeReadyState(readyStatus);
    }

    [Command]
    public void CmdToggleReady()
    {
        // Ready durumunu değiştir
        bool newReadyState = !isReady;
        isReady = newReadyState;

        // Diğer oyuncuların ready durumunu güncellemesi için ClientRpc çağrısı
        RpcUpdateReadyCount(netId, newReadyState);
    }

    [ClientRpc]
    void RpcUpdateReadyCount(uint playerId, bool newReadyState)
    {
        LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
        if (lobbyUI != null)
        {
            lobbyUI.UpdateReadyStateByPlayerId(playerId, newReadyState);
        }

        // Ready sayaç güncellemesini yap
        CustomNetworkManager networkManager = (CustomNetworkManager)NetworkManager.singleton;
        if (networkManager != null)
        {
            networkManager.UpdateLocalReadyCount(newReadyState);
        }
    }

    [ClientRpc]
    public void RpcUpdateAllPlayersUI(List<uint> playerIds, List<bool> readyStatuses)
    {
        LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
        if (lobbyUI != null)
        {
            for (int i = 0; i < playerIds.Count; i++)
            {
                LobbyPlayer player = FindPlayerByNetId(playerIds[i]);
                if (player != null)
                {
                    player.isReady = readyStatuses[i];
                    lobbyUI.UpdateReadyState(player, readyStatuses[i]);
                }
            }
        }
    }

    private LobbyPlayer FindPlayerByNetId(uint netId)
    {
        foreach (var player in FindObjectsOfType<LobbyPlayer>())
        {
            if (player.netId == netId)
                return player;
        }
        return null;
    }
}
