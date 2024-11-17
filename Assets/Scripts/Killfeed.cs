using UnityEngine;

public class Killfeed : MonoBehaviour
{
    [SerializeField]
    GameObject killfeedItem;

    void Start()
    {
        GameManager.instance.onPlayerKilledCallBack += OnKill;
    }

    void OnKill(string player,string source)
    {
        GameObject go = Instantiate(killfeedItem, this.transform);
        go.GetComponent<KillfeedItem>().Setup(player, source);

        Destroy(go, 4f);
    }    
}
