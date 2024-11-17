using UnityEngine;
using Mirror;

public class BotPlayer : Player
{
    public string botName;

    public int teamIndex; // Takým indeksini belirleyen alan

    // Bot davranýþlarý için basit bir komut (saldýrý vb.)
    [Command]
    public void CmdBotAction()
    {
        // Botun yapacaðý eylemleri burada tanýmla
    }

    // Bot spawn edilirken maxHealth ayarlamak için
    public void InitializeHealth(int health)
    {
        username = botName; // Botun ismi `username` alanýna atanýyor
        maxHealth = health;
        currentHealth = maxHealth;
    }
}
