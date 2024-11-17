using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public TMP_Text killCount;
    public TMP_Text deathCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(UserAccountManager.IsLoggedIn)
            UserAccountManager.instance.GetData(OnReceivedData);
    }

    void OnReceivedData(string data)
    {
        if (killCount == null || deathCount == null)
            return;

        killCount.text = DataTranslator.DataToKills(data).ToString() + " KILLS";
        deathCount.text = DataTranslator.DataToDeaths(data).ToString() + " DEATHS";

    }
}
