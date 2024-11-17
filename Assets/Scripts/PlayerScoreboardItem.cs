using UnityEngine;
using TMPro;

public class PlayerScoreboardItem : MonoBehaviour
{
    [SerializeField]
    TMP_Text usernameText;

    [SerializeField]
    TMP_Text killsText;

    [SerializeField]
    TMP_Text deathsText;

    public void Setup(string username, int kills, int deaths)
    {
        usernameText.text = username;
        killsText.text = "Kills : " + kills;
        deathsText.text = "Deaths : " + deaths;
    }

}
