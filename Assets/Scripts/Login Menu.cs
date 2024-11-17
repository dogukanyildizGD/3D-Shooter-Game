using UnityEngine;
using System.Collections;
using DatabaseControl;  // Yeni DatabaseControl'e uygun
using TMPro;

public class LoginMenu : MonoBehaviour
{
    // UI Elemanları
    public GameObject loginParent;
    public GameObject registerParent;
    public GameObject loadingParent;

    // Input ve Hata Mesajları
    public TMP_InputField Login_UsernameField;
    public TMP_InputField Login_PasswordField;
    public TMP_InputField Register_UsernameField;
    public TMP_InputField Register_PasswordField;
    public TMP_InputField Register_ConfirmPasswordField;

    public TMP_Text Login_ErrorText;
    public TMP_Text Register_ErrorText;

    // Bu değişkenler sınıf genelinde kullanılabilir olmalı
    private string playerUsername = "";
    private string playerPassword = "";

    // UI'yi sıfırlama
    void ResetAllUIElements()
    {
        Login_UsernameField.text = "";
        Login_PasswordField.text = "";
        Register_UsernameField.text = "";
        Register_PasswordField.text = "";
        Register_ConfirmPasswordField.text = "";
        Login_ErrorText.text = "";
        Register_ErrorText.text = "";
    }

    // Giriş Yapma İşlemi
    IEnumerator LoginUser()
    {
        IEnumerator e = DCF.Login(playerUsername, playerPassword);  // DCF'nin yeni Login metodu
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string;

        if (response == "Success")
        {
            ResetAllUIElements();
            loadingParent.gameObject.SetActive(false);
            UserAccountManager.instance.LogIn(playerUsername, playerPassword);
            Debug.Log("Başarıyla giriş yapıldı.");
        }
        else
        {
            loadingParent.gameObject.SetActive(false);
            loginParent.gameObject.SetActive(true);
            Login_ErrorText.text = response == "UserError" ? "Kullanıcı bulunamadı." : "Şifre yanlış.";
        }
    }

    // Kayıt Olma İşlemi
    IEnumerator RegisterUser()
    {
        IEnumerator e = DCF.RegisterUser(playerUsername, playerPassword, "[KILLS]0/[DEATHS]0");  // Yeni Register metodu
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string;

        if (response == "Success")
        {
            ResetAllUIElements();
            loadingParent.gameObject.SetActive(false);
            UserAccountManager.instance.LogIn(playerUsername, playerPassword);
            Debug.Log("Başarıyla kayıt olundu.");
        }
        else
        {
            loadingParent.gameObject.SetActive(false);
            registerParent.gameObject.SetActive(true);
            Register_ErrorText.text = response == "UserError" ? "Kullanıcı adı alınmış." : "Bilinmeyen hata.";
        }
    }

    // UI Butonlarına Bağlı İşlemler
    public void Login_LoginButtonPressed()
    {
        // Giriş için kullanıcı adı ve şifre alınıyor
        playerUsername = Login_UsernameField.text;
        playerPassword = Login_PasswordField.text;

        if (playerUsername.Length > 3 && playerPassword.Length > 5)
        {
            loginParent.gameObject.SetActive(false);
            loadingParent.gameObject.SetActive(true);
            StartCoroutine(LoginUser());
        }
        else
        {
            Login_ErrorText.text = "Kullanıcı adı veya şifre çok kısa!";
        }
    }

    public void Login_RegisterButtonPressed()
    {
        // Giriş ekranından kayıt ekranına geçiş
        ResetAllUIElements();  // Tüm UI elemanlarını sıfırlar
        loginParent.gameObject.SetActive(false);  // Login ekranını gizler
        registerParent.gameObject.SetActive(true);  // Register ekranını gösterir
    }

    public void Register_RegisterButtonPressed()
    {
        // Kayıt işlemi için veriler alınıyor
        playerUsername = Register_UsernameField.text;
        playerPassword = Register_PasswordField.text;
        string confirmPass = Register_ConfirmPasswordField.text;

        if (playerUsername.Length > 3 && playerPassword.Length > 5 && playerPassword == confirmPass)
        {
            registerParent.gameObject.SetActive(false);
            loadingParent.gameObject.SetActive(true);
            StartCoroutine(RegisterUser());
        }
        else
        {
            Register_ErrorText.text = "Şifreler uyuşmuyor veya çok kısa!";
        }
    }

    public void Register_BackButtonPressed()
    {
        ResetAllUIElements();
        loginParent.gameObject.SetActive(true);
        registerParent.gameObject.SetActive(false);
    }
}
