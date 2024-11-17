using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using System.IO;  // JSON i�lemleri i�in gerekli
using TMPro;
using System.Collections;

public class JoinGame : MonoBehaviour
{
    List<GameObject> roomList = new List<GameObject>();

    [SerializeField]
    private TMP_Text status;

    [SerializeField]
    private GameObject roomListItemPrefab;

    [SerializeField]
    private Transform roomListParent;

    private CustomNetworkManager customNetworkManager;
    private NetworkDiscovery networkDiscovery;

    private string roomName;
    private string filePath;

    void Start()
    {
        customNetworkManager = FindFirstObjectByType<CustomNetworkManager>();
        networkDiscovery = FindFirstObjectByType<NetworkDiscovery>();

        if (customNetworkManager == null || networkDiscovery == null)
        {
            Debug.LogError("NetworkManager veya NetworkDiscovery bulunamad�!");
            status.text = "NetworkManager veya NetworkDiscovery bulunamad�!";
            return;
        }

        // JSON dosyas�n�n yolunu belirliyoruz
        filePath = Path.Combine(Application.persistentDataPath, "roomData.json");

        networkDiscovery.StartDiscovery(); // Sunucular� aramaya ba�l�yoruz
        Debug.Log("Sunucu taramas� ba�lat�ld�.");
        status.text = "Sunucu taramas� ba�lat�ld�...";

        // Oyuncu say�s�n� g�ncellemek i�in her 2 saniyede bir JSON kontrol� ba�lat
        InvokeRepeating("CheckForPlayerCountUpdate", 1f, 2f);
    }

    // Oda ad�n� JSON dosyas�ndan y�kleme fonksiyonu
    public string GetRoomName()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);  // JSON dosyas�n� okuyoruz
            RoomData data = JsonUtility.FromJson<RoomData>(json);  // JSON veriyi RoomData objesine �eviriyoruz
            Debug.Log("JSON dosyas�ndan oda ad� al�nd�: " + data.roomName);
            return data.roomName;
        }
        else
        {
            Debug.LogWarning("Oda ad� JSON dosyas�nda bulunamad�.");
            return string.Empty;
        }
    }

    public void RefreshRoomList()
    {
        ClearRoomList();

        if (networkDiscovery == null)
        {
            Debug.LogError("NetworkDiscovery bo�!");
            status.text = "NetworkDiscovery bulunamad�!";
            return;
        }

        networkDiscovery.StartDiscovery();
        Debug.Log("Sunucu taramas� ba�lad�.");
        status.text = "Sunucu taramas� ba�lad�...";
    }

    public void OnDiscoveredServer(ServerResponse response)
    {
        Debug.Log("BA�LANDIIIIIII");
        Debug.Log("Sunucu ke�fedildi: " + response.EndPoint.Address);

        if (response.EndPoint == null || response.EndPoint.Address == null)
        {
            Debug.LogError("Sunucu cevab� ge�ersiz!");
            return;
        }

        // Oda ad�n� JSON dosyas�ndan al�yoruz
        roomName = GetRoomName();
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "DefaultRoom";  // E�er oda ad� yoksa varsay�lan bir oda ad�
        }

        // Ayn� IP adresi veya oda ad� ile zaten listede olup olmad���n� kontrol et
        foreach (GameObject room in roomList)
        {
            TMP_Text roomText = room.GetComponentInChildren<TMP_Text>();
            if (roomText != null && roomText.text.Contains(roomName))
            {
                Debug.LogWarning("Bu oda zaten listede: " + roomName);
                return; // E�er oda zaten listede varsa ekleme yapm�yoruz
            }
        }

        // E�er listede de�ilse ekle
        AddRoomToList(roomName);
    }

    public void AddRoomToList(string roomName)
    {
        Debug.Log("Room eklenecek: " + roomName);

        GameObject _roomListItemGO = Instantiate(roomListItemPrefab, roomListParent);

        TMP_Text roomText = _roomListItemGO.GetComponentInChildren<TMP_Text>();
        if (roomText != null)
        {
            roomText.text = "Room: " + roomName;
            Debug.Log("Oda ad� eklendi: " + roomName);
        }
        else
        {
            Debug.LogError("Room prefabinde TMP_Text bulunamad�.");
        }

        // PlayerCountText'i prefab i�indeki dinamik objeden bulma
        TMP_Text playerCountText = _roomListItemGO.transform.Find("PlayerCountText").GetComponent<TMP_Text>();

        if (playerCountText != null)
        {
            // �lk ba�ta (0/6) olarak ayarlayal�m
            playerCountText.text = "0/6";
            Debug.Log("Player count text eklendi");
        }
        else
        {
            Debug.LogError("PlayerCountText prefab i�inde bulunamad�.");
        }

        _roomListItemGO.GetComponent<Button>().onClick.AddListener(() => JoinRoom());

        roomList.Add(_roomListItemGO);

        Debug.Log("Room listeye eklendi: " + roomName);
    }

    // Localhost �zerinden odaya otomatik ba�lanma
    public void JoinRoom()
    {
        Debug.Log("Odaya ba�lan�l�yor: 127.0.0.1");

        // Varsay�lan localhost adresini kullanarak ba�lan�yoruz
        customNetworkManager.StartClient();
        status.text = "Odaya ba�lan�l�yor: 127.0.0.1";
        StartCoroutine(WaitForJoin());
    }

    IEnumerator WaitForJoin()
    {
        ClearRoomList();

        int countdown = 5;

        while (countdown > 0)
        {
            status.text = "Joining... (" + countdown + ")";

            yield return new WaitForSeconds(3);

            countdown--;
        }

        // Failed to connect
        status.text = "Failed to connect.";
        yield return new WaitForSeconds(1);

        RefreshRoomList();
    }

    void ClearRoomList()
    {
        foreach (GameObject room in roomList)
        {
            Destroy(room);
        }

        roomList.Clear();
        status.text = "Oda listesi temizlendi.";
    }

    // JSON dosyas�n� kontrol eden ve UI'yi g�ncelleyen fonksiyon
    private void CheckForPlayerCountUpdate()
    {
        string jsonFilePath = Application.persistentDataPath + "/playerCount.json";  // Oyuncu say�s�n�n oldu�u JSON dosyas�

        if (File.Exists(jsonFilePath))  // JSON dosyas�n� kontrol edin
        {
            string json = File.ReadAllText(jsonFilePath);
            Debug.Log("JSON dosyas�ndan okunan veri: " + json);  // JSON verisini loglayal�m

            PlayerCountData data = JsonUtility.FromJson<PlayerCountData>(json);

            // JSON'dan veri do�ru �ekilde al�n�yor mu kontrol edelim
            if (data != null)
            {
                Debug.Log($"JSON verisi ��z�mlendi. Oyuncu say�s�: {data.currentCount}, Maksimum oyuncu: {data.maxCount}");
            }
            else
            {
                Debug.LogError("JSON verisi ��z�mlenemedi!");
            }

            // UI g�ncelleme i�lemi
            foreach (GameObject room in roomList)
            {
                TMP_Text playerCountText = room.transform.Find("PlayerCountText").GetComponent<TMP_Text>();

                if (playerCountText != null)
                {
                    playerCountText.text = $"{data.currentCount}/{data.maxCount}";
                    Debug.Log("PlayerCountText g�ncellendi: " + playerCountText.text);
                }
            }
        }
        else
        {
            Debug.LogError("JSON dosyas� bulunamad�: " + jsonFilePath);
        }
    }

    [System.Serializable]
    public class PlayerCountData
    {
        public int currentCount;
        public int maxCount;
    }
}

