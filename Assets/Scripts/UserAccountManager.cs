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

    // Bu de�i�kenler giri� yapt�ktan sonra kullan�c� ve �ifreyi saklar
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
        Debug.Log("Kullan�c� ��k�� yapt�.");

        SceneManager.LoadScene(loggedOutSceneName);
    }

    public void LogIn(string username, string password)
    {
        playerUsername = username;
        playerPassword = password;

        IsLoggedIn = true;
        Debug.Log($"Kullan�c� giri� yapt�: {username}");

        SceneManager.LoadScene(loggedInSceneName);
    }

    // Veriyi sunucuya g�nderme i�lemi
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
            Debug.Log("Veri ba�ar�yla sunucuya g�nderildi.");
        }
        else
        {
            Debug.LogError("Veri g�nderme hatas�: " + response);
        }
    }

    // Veriyi sunucudan alma i�lemi
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
            Debug.LogError("Veri alma hatas�.");
        }
        else if (response == "ContainsUnsupportedSymbol")
        {
            Debug.LogError("Get Data Error: Contains Unsupported Symbol '-'");
        }
        else
        {
            Debug.Log("Veri sunucudan al�nd�: " + response);
            onDataReceived?.Invoke(response);
        }
    }
}
