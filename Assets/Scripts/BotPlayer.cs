using UnityEngine;
using Mirror;

public class BotPlayer : Player
{
    public string botName;

    public int teamIndex; // Tak�m indeksini belirleyen alan

    // Bot davran��lar� i�in basit bir komut (sald�r� vb.)
    [Command]
    public void CmdBotAction()
    {
        // Botun yapaca�� eylemleri burada tan�mla
    }

    // Bot spawn edilirken maxHealth ayarlamak i�in
    public void InitializeHealth(int health)
    {
        username = botName; // Botun ismi `username` alan�na atan�yor
        maxHealth = health;
        currentHealth = maxHealth;
    }
}
