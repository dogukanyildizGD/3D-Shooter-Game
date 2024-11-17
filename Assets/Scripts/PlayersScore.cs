using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayersScore : MonoBehaviour
{
    Player player;

    private int lastKills = 0;
    private int lastDeaths = 0;

    void Start()
    {
        player = GetComponent<Player>();
        StartCoroutine(SyncScoreLoop());
    }

    void OnDestroy()
    {
        if(player != null)
            SyncNow();
    }

    IEnumerator SyncScoreLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            SyncNow();
        }        
    }

    void SyncNow()
    {
        if (UserAccountManager.IsLoggedIn && player != null)
        {
            UserAccountManager.instance.GetData(OnReceivedData);
        }
    }

    void OnReceivedData(string data)
    {
        if (player.kills <= lastKills && player.deaths <= lastDeaths)
            return;

        int killsSinceLast = player.kills - lastKills;
        int deathsSinceLast = player.deaths - lastDeaths;

        int kills = DataTranslator.DataToKills(data);
        int deaths = DataTranslator.DataToDeaths(data);

        int newKills = killsSinceLast + kills;
        int newDeaths = deathsSinceLast + deaths;

        string newData = DataTranslator.ValueToData(newKills, newDeaths);

        Debug.Log("Syncing : " + newData);

        UserAccountManager.instance.SendData(newData);

        lastKills = player.kills;
        lastDeaths = player.deaths;
    }
}
