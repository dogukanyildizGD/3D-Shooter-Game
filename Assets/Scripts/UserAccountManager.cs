using UnityEngine;
using System.Collections;
using DatabaseControl;
using UnityEngine.SceneManagement;

public class UserAccountManager : MonoBehaviour
{
    public static UserAccountManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    // Bu deðiþkenler giriþ yaptýktan sonra kullanýcý ve þifreyi saklar
    public static string playerUsername { get; private set; }
    private string playerPassword = "";

    public static bool IsLoggedIn { get; private set; }

    public string loggedInSceneName = "Lobby";
    public string loggedOutSceneName = "Login";

    public delegate void OnDataReceivedCallBack(string data);

    public void LogOut()
    {
        playerUsername = "";
        playerPassword = "";

        IsLoggedIn = false;
        Debug.Log("Kullanýcý çýkýþ yaptý.");

        SceneManager.LoadScene(loggedOutSceneName);
    }

    public void LogIn(string username, string password)
    {
        playerUsername = username;
        playerPassword = password;

        IsLoggedIn = true;
        Debug.Log($"Kullanýcý giriþ yaptý: {username}");

        SceneManager.LoadScene(loggedInSceneName);
    }

    // Veriyi sunucuya gönderme iþlemi
    public void SendData(string data)
    {
        if (IsLoggedIn)
        {
            StartCoroutine(SendDataRequest(playerUsername, playerPassword, data));
        }
    }

    private IEnumerator SendDataRequest(string username, string password, string data)
    {
        IEnumerator e = DCF.SetUserData(username, password, data);  // Yeni DatabaseControl fonksiyonu
        while (e.MoveNext())
        {
            yield return e.Current;
        }

        string response = e.Current as string;
        if (response == "Success")
        {
            Debug.Log("Veri baþarýyla sunucuya gönderildi.");
        }
        else
        {
            Debug.LogError("Veri gönderme hatasý: " + response);
        }
    }

    // Veriyi sunucudan alma iþlemi
    public void GetData(OnDataReceivedCallBack onDataReceived)
    {
        if (IsLoggedIn)
        {
            StartCoroutine(GetDataRequest(playerUsername, playerPassword, onDataReceived));
        }
    }

    private IEnumerator GetDataRequest(string username, string password, OnDataReceivedCallBack onDataReceived)
    {
        IEnumerator e = DCF.GetUserData(username, password);
        while (e.MoveNext())
        {
            yield return e.Current;
        }

        string response = e.Current as string;
        if (response == "Error")
        {
            Debug.LogError("Veri alma hatasý.");
        }
        else if (response == "ContainsUnsupportedSymbol")
        {
            Debug.LogError("Get Data Error: Contains Unsupported Symbol '-'");
        }
        else
        {
            Debug.Log("Veri sunucudan alýndý: " + response);
            onDataReceived?.Invoke(response);
        }
    }
}
