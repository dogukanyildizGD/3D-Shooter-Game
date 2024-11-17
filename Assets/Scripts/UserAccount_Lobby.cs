using UnityEngine;
using TMPro;

public class UserAccount_Lobby : MonoBehaviour
{
    public TMP_Text usernameText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (UserAccountManager.IsLoggedIn)
            usernameText.text = UserAccountManager.playerUsername;
    }

    public void LogOut()
    {
        if (UserAccountManager.IsLoggedIn)
            UserAccountManager.instance.LogOut();
    }
}
