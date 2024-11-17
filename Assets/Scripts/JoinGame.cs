using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using System.IO;  // JSON iþlemleri için gerekli
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
            Debug.LogError("NetworkManager veya NetworkDiscovery bulunamadý!");
            status.text = "NetworkManager veya NetworkDiscovery bulunamadý!";
            return;
        }

        // JSON dosyasýnýn yolunu belirliyoruz
        filePath = Path.Combine(Application.persistentDataPath, "roomData.json");

        networkDiscovery.StartDiscovery(); // Sunucularý aramaya baþlýyoruz
        Debug.Log("Sunucu taramasý baþlatýldý.");
        status.text = "Sunucu taramasý baþlatýldý...";

        // Oyuncu sayýsýný güncellemek için her 2 saniyede bir JSON kontrolü baþlat
        InvokeRepeating("CheckForPlayerCountUpdate", 1f, 2f);
    }

    // Oda adýný JSON dosyasýndan yükleme fonksiyonu
    public string GetRoomName()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);  // JSON dosyasýný okuyoruz
            RoomData data = JsonUtility.FromJson<RoomData>(json);  // JSON veriyi RoomData objesine çeviriyoruz
            Debug.Log("JSON dosyasýndan oda adý alýndý: " + data.roomName);
            return data.roomName;
        }
        else
        {
            Debug.LogWarning("Oda adý JSON dosyasýnda bulunamadý.");
            return string.Empty;
        }
    }

    public void RefreshRoomList()
    {
        ClearRoomList();

        if (networkDiscovery == null)
        {
            Debug.LogError("NetworkDiscovery boþ!");
            status.text = "NetworkDiscovery bulunamadý!";
            return;
        }

        networkDiscovery.StartDiscovery();
        Debug.Log("Sunucu taramasý baþladý.");
        status.text = "Sunucu taramasý baþladý...";
    }

    public void OnDiscoveredServer(ServerResponse response)
    {
        Debug.Log("BAÐLANDIIIIIII");
        Debug.Log("Sunucu keþfedildi: " + response.EndPoint.Address);

        if (response.EndPoint == null || response.EndPoint.Address == null)
        {
            Debug.LogError("Sunucu cevabý geçersiz!");
            return;
        }

        // Oda adýný JSON dosyasýndan alýyoruz
        roomName = GetRoomName();
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "DefaultRoom";  // Eðer oda adý yoksa varsayýlan bir oda adý
        }

        // Ayný IP adresi veya oda adý ile zaten listede olup olmadýðýný kontrol et
        foreach (GameObject room in roomList)
        {
            TMP_Text roomText = room.GetComponentInChildren<TMP_Text>();
            if (roomText != null && roomText.text.Contains(roomName))
            {
                Debug.LogWarning("Bu oda zaten listede: " + roomName);
                return; // Eðer oda zaten listede varsa ekleme yapmýyoruz
            }
        }

        // Eðer listede deðilse ekle
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
            Debug.Log("Oda adý eklendi: " + roomName);
        }
        else
        {
            Debug.LogError("Room prefabinde TMP_Text bulunamadý.");
        }

        // PlayerCountText'i prefab içindeki dinamik objeden bulma
        TMP_Text playerCountText = _roomListItemGO.transform.Find("PlayerCountText").GetComponent<TMP_Text>();

        if (playerCountText != null)
        {
            // Ýlk baþta (0/6) olarak ayarlayalým
            playerCountText.text = "0/6";
            Debug.Log("Player count text eklendi");
        }
        else
        {
            Debug.LogError("PlayerCountText prefab içinde bulunamadý.");
        }

        _roomListItemGO.GetComponent<Button>().onClick.AddListener(() => JoinRoom());

        roomList.Add(_roomListItemGO);

        Debug.Log("Room listeye eklendi: " + roomName);
    }

    // Localhost üzerinden odaya otomatik baðlanma
    public void JoinRoom()
    {
        Debug.Log("Odaya baðlanýlýyor: 127.0.0.1");

        // Varsayýlan localhost adresini kullanarak baðlanýyoruz
        customNetworkManager.StartClient();
        status.text = "Odaya baðlanýlýyor: 127.0.0.1";
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

    // JSON dosyasýný kontrol eden ve UI'yi güncelleyen fonksiyon
    private void CheckForPlayerCountUpdate()
    {
        string jsonFilePath = Application.persistentDataPath + "/playerCount.json";  // Oyuncu sayýsýnýn olduðu JSON dosyasý

        if (File.Exists(jsonFilePath))  // JSON dosyasýný kontrol edin
        {
            string json = File.ReadAllText(jsonFilePath);
            Debug.Log("JSON dosyasýndan okunan veri: " + json);  // JSON verisini loglayalým

            PlayerCountData data = JsonUtility.FromJson<PlayerCountData>(json);

            // JSON'dan veri doðru þekilde alýnýyor mu kontrol edelim
            if (data != null)
            {
                Debug.Log($"JSON verisi çözümlendi. Oyuncu sayýsý: {data.currentCount}, Maksimum oyuncu: {data.maxCount}");
            }
            else
            {
                Debug.LogError("JSON verisi çözümlenemedi!");
            }

            // UI güncelleme iþlemi
            foreach (GameObject room in roomList)
            {
                TMP_Text playerCountText = room.transform.Find("PlayerCountText").GetComponent<TMP_Text>();

                if (playerCountText != null)
                {
                    playerCountText.text = $"{data.currentCount}/{data.maxCount}";
                    Debug.Log("PlayerCountText güncellendi: " + playerCountText.text);
                }
            }
        }
        else
        {
            Debug.LogError("JSON dosyasý bulunamadý: " + jsonFilePath);
        }
    }

    [System.Serializable]
    public class PlayerCountData
    {
        public int currentCount;
        public int maxCount;
    }
}

