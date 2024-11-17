using UnityEngine;
using Mirror;
using System.IO;  // JSON işlemleri için gerekli
using Mirror.Discovery;

[System.Serializable]
public class RoomData
{
    public string roomName;
}

public class HostGame : MonoBehaviour
{
    [SerializeField]
    private uint roomSize = 6;

    private CustomNetworkManager lobbyManager;
    private NetworkDiscovery networkDiscovery;

    private string filePath;

    void Start()
    {
        lobbyManager = (CustomNetworkManager)NetworkManager.singleton;
        networkDiscovery = FindFirstObjectByType<NetworkDiscovery>();

        if (lobbyManager == null)
        {
            Debug.LogError("NetworkManager bulunamadı!");
            return;
        }

        // JSON dosyasının yolunu belirliyoruz
        filePath = Path.Combine(Application.persistentDataPath, "roomData.json");
    }

    // Oda adını JSON dosyasına kaydetme fonksiyonu
    public void SetRoomName(string _name)
    {
        if (string.IsNullOrEmpty(_name))
        {
            Debug.LogWarning("Oda adı boş geldi.");
            return;
        }

        RoomData data = new RoomData { roomName = _name };
        string json = JsonUtility.ToJson(data);

        File.WriteAllText(filePath, json);  // JSON dosyasına yaz
        Debug.Log("Oda adı JSON dosyasına kaydedildi: " + _name);
    }

    // Oda adını JSON dosyasından yükleme fonksiyonu
    public string GetRoomName()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);  // JSON dosyasını okuyoruz
            RoomData data = JsonUtility.FromJson<RoomData>(json);  // JSON veriyi RoomData objesine çeviriyoruz
            Debug.Log("JSON dosyasından oda adı alındı: " + data.roomName);
            return data.roomName;
        }
        else
        {
            Debug.LogWarning("Oda adı JSON dosyasında bulunamadı.");
            return string.Empty;
        }
    }

    public void CreateRoom()
    {
        string roomName = GetRoomName();  // Oda adı JSON dosyasından alınır

        if (!string.IsNullOrEmpty(roomName))
        {
            Debug.Log("Oda oluşturuluyor: " + roomName + " oyuncu sayısı: " + roomSize);

            // Maksimum oyuncu sayısını ayarla
            lobbyManager.maxConnections = (int)roomSize;
            lobbyManager.networkAddress = "127.0.0.1";
            lobbyManager.StartHost();  // Sunucuyu başlat

            // Sunucuyu keşfe aç
            networkDiscovery.AdvertiseServer();

            Debug.Log("Host başlatıldı. Sunucu IP adresi: " + lobbyManager.networkAddress);
        }
        else
        {
            Debug.LogWarning("Oda adı boş olamaz.");
        }
    }

    public void StartGame()
    {
        if (lobbyManager == null)
        {
            Debug.LogError("CustomNetworkManager atanmamış!");
            return;
        }

        int readyCount = lobbyManager.GetLocalReadyCount();
        int playerCount = lobbyManager.GetPlayerCount();

        // Tüm oyuncular ready ise oyunu başlat
        if (readyCount == playerCount)
        {
            Debug.Log("Oyun başlatılıyor...");
            lobbyManager.ServerChangeScene("MainLevel");  // Oyun sahnesine geçiş
        }
        else
        {
            Debug.LogWarning("Tüm oyuncular hazır değil. Hazır oyuncu sayısı: " + readyCount);
        }
    }

}
