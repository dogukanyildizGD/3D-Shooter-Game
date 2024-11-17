using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class CustomNetworkManager : NetworkRoomManager
{
    private int localReadyCount = 0; // Ready olan oyuncu sayısını tutar.
    public TMP_Text readyCountText;

    public GameObject botPrefab; // Botlar için prefab tanımı
    public int teamCount = 2; // Takım sayısı, dinamik olarak ayarlanabilir
    public int maxBotsPerTeam = 2; // Her takım için maksimum bot sayısı
    public int maxPlayersPerTeam = 5; // Her takım için maksimum oyuncu sayısı (botlar dahil)

    private Dictionary<int, int> botsPerTeam = new Dictionary<int, int>(); // Takımlardaki bot sayısını tutar
    private Dictionary<int, int> playersPerTeam = new Dictionary<int, int>(); // Takımlardaki toplam oyuncu (botlar dahil) sayısını tutar
    private List<NetworkStartPosition> spawnPoints; // Tüm spawn noktalarını tutacak liste
    private int nextSpawnIndex = 0; // Bir sonraki kullanılacak spawn noktasının indeksini takip eder

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Takımlardaki oyuncu ve bot sayısını başlat
        for (int i = 0; i < teamCount; i++)
        {
            botsPerTeam[i] = 0;
            playersPerTeam[i] = 0;
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                DontDestroyOnLoad(conn.identity.gameObject);
            }
        }

        // Sahne yüklendikten sonra spawn noktalarını güncelliyoruz
        spawnPoints = new List<NetworkStartPosition>(FindObjectsOfType<NetworkStartPosition>());

        // Eğer oyun sahnesine geçiliyorsa botları eklemeden önce spawn noktalarını güncelle
        if (newSceneName == "MainLevel") // "MainLevel" sahnesinin adı bu olmalı
        {
            base.ServerChangeScene(newSceneName);
            // Sahne değiştikten sonra spawn noktalarını güncellemek için sahne yükleme bittiğinde çağrılan event kullanıyoruz
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            base.ServerChangeScene(newSceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainLevel")
        {
            // Sahne yüklendikten sonra spawn noktalarını güncelliyoruz
            spawnPoints = new List<NetworkStartPosition>(FindObjectsOfType<NetworkStartPosition>());

            if (spawnPoints.Count == 0)
            {
                Debug.LogError("MainLevel sahnesine yerleştirilmiş NetworkStartPosition bulunamadı!");
            }
            else
            {
                // Botları ekliyoruz
                AddBotsToGame();
            }

            // Event'ten kaldırmak gerekiyor aksi takdirde her sahne yüklendiğinde tetiklenir
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        Debug.Log("Yeni oyuncu eklendi: " + conn.identity.netId);

        LobbyPlayer newPlayer = conn.identity.GetComponent<LobbyPlayer>();
        if (newPlayer != null)
        {
            // Takım indeksini belirle ve yeni oyuncuya ata
            int teamIndex = GetNextTeamIndexForPlayer();
            if (teamIndex >= 0)
            {
                newPlayer.teamIndex = teamIndex; // Takım indeksini atıyoruz
                playersPerTeam[teamIndex]++; // Takımdaki oyuncu sayısını artırıyoruz
            }
            else
            {
                Debug.LogError("Tüm takımlar dolu, oyuncu eklenemiyor.");
            }

            LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                lobbyUI.CreatePlayerUI(newPlayer);

                // Tüm mevcut oyuncuları ve ready durumlarını topla
                List<uint> playerIds = new List<uint>();
                List<bool> readyStatuses = new List<bool>();

                foreach (var player in roomSlots.Cast<LobbyPlayer>())
                {
                    if (player != null)
                    {
                        playerIds.Add(player.netId);
                        readyStatuses.Add(player.isReady);
                    }
                }

                // roomSlots'tan herhangi bir LobbyPlayer örneği seç
                LobbyPlayer firstLobbyPlayer = roomSlots.Cast<LobbyPlayer>().FirstOrDefault();
                if (firstLobbyPlayer != null)
                {
                    firstLobbyPlayer.RpcUpdateAllPlayersUI(playerIds, readyStatuses);
                }
            }
            else
            {
                Debug.LogError("LobbyUI bulunamadı!");
            }
        }
        else
        {
            Debug.LogError("LobbyPlayer spawn edilemedi.");
        }

        UpdateLobbyUI();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        LobbyPlayer player = conn.identity.GetComponent<LobbyPlayer>();
        if (player != null)
        {
            LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                lobbyUI.RemovePlayerUI(player);
            }
        }

        base.OnServerDisconnect(conn);
        UpdateLobbyUI();
    }

    public override void OnRoomServerPlayersReady()
    {
        // Otomatik olarak oyunun başlamasını engelliyoruz
    }

    private void AddBotsToGame()
    {
        for (int i = 0; i < teamCount; i++)
        {
            int botsNeeded = maxPlayersPerTeam - playersPerTeam[i];
            for (int j = 0; j < botsNeeded && botsPerTeam[i] < maxBotsPerTeam; j++)
            {
                AddBotToTeam(i);
                botsPerTeam[i]++;
                playersPerTeam[i]++;
            }
        }
    }

    private void AddBotToTeam(int teamIndex)
    {
        if (botPrefab == null)
        {
            Debug.LogError("Bot prefab atanmadı!");
            return;
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("Bot için spawn noktası bulunamadı!");
            return;
        }

        // Rastgele bir spawn noktası seç
        int randomIndex = Random.Range(0, spawnPoints.Count);
        NetworkStartPosition spawnPoint = spawnPoints[randomIndex];

        // Bot prefab'ını kullanarak bir bot oluştur
        GameObject bot = Instantiate(botPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

        BotPlayer botPlayer = bot.GetComponent<BotPlayer>();

        if (botPlayer != null)
        {
            botPlayer.teamIndex = teamIndex; // Botun takım indeksini ayarla

            // Rastgele bir isim ata
            botPlayer.botName = BotNameGenerator.GetRandomBotName();

            botPlayer.InitializeHealth(100);

            // Botu ağda tüm istemciler için görünür hale getirmek için `NetworkServer.Spawn` çağrısı yap
            NetworkServer.Spawn(bot);

        }
        else
        {
            Debug.LogError("Bot prefab'inde BotPlayer scripti eksik!");
        }
    }

    public static class BotNameGenerator
    {
        private static readonly string[] botNames = {
        "Alpha", "Bravo", "Charlie", "Delta", "Echo",
        "Foxtrot", "Golf", "Hotel", "India", "Juliet"
    };

        public static string GetRandomBotName()
        {
            int randomIndex = Random.Range(0, botNames.Length);
            return botNames[randomIndex];
        }
    }

    private int GetNextTeamIndexForPlayer()
    {
        for (int i = 0; i < teamCount; i++)
        {
            if (playersPerTeam[i] < maxPlayersPerTeam)
            {
                return i;
            }
        }

        // Eğer tüm takımlar dolmuşsa -1 döndür
        return -1;
    }

    public void UpdateLocalReadyCount(bool isReady)
    {
        localReadyCount += isReady ? 1 : -1;

        // TMP_Text bileşeninin referansının atanmış olup olmadığını kontrol et
        if (readyCountText != null)
        {
            readyCountText.text = $"Ready : {localReadyCount}";
        }
        else
        {
            Debug.LogError("Ready count text referansı atanmadı!");
        }

        Debug.Log("Lokal Ready Sayısı: " + localReadyCount);
    }

    public int GetLocalReadyCount()
    {
        return localReadyCount;
    }

    public int GetPlayerCount()
    {
        return roomSlots.Count;
    }

    private void UpdateLobbyUI()
    {
        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            Debug.Log("Bu sahnede LobbyUI güncellenmeyecek.");
            return;
        }

        LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
        if (lobbyUI != null)
        {
            List<LobbyPlayer> players = new List<LobbyPlayer>(roomSlots.Cast<LobbyPlayer>());
            lobbyUI.UpdateUIForAllPlayers(players);
        }
        else
        {
            Debug.LogError("LobbyUI bulunamadı!");
        }
    }
}