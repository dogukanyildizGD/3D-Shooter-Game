using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MiniMap : MonoBehaviour
{
    public RawImage minimapImage;
    public Sprite playerIconSprite;
    public Sprite mapObjectIconSprite;
    public Sprite towerIconSprite;
    public Sprite baseIconSprite;
    public Vector2 playerIconSize = new Vector2(20f, 20f);
    public Vector2 mapObjectIconSize = new Vector2(15f, 15f);
    public Vector2 towerIconSize = new Vector2(25f, 25f);
    public Vector2 baseIconSize = new Vector2(30f, 30f);
    public float mapScale = 10f;

    private Transform mapCenter;
    private Image playerIconInstance;
    private List<Image> otherPlayerIcons = new List<Image>();
    private List<Image> mapObjectIcons = new List<Image>();
    private List<Image> towerIcons = new List<Image>();
    private List<Image> baseIcons = new List<Image>();
    private Vector2 groundSize;

    void Start()
    {
        GenerateDynamicMinimap();

        if (mapCenter == null && gameObject.CompareTag("Player"))
        {
            mapCenter = transform;
        }

        GameObject groundObject = GameObject.FindGameObjectWithTag("Ground");
        if (groundObject != null)
        {
            groundSize = new Vector2(groundObject.transform.localScale.x, groundObject.transform.localScale.z);
        }

        playerIconInstance = new GameObject("PlayerIcon").AddComponent<Image>();
        playerIconInstance.sprite = playerIconSprite;
        playerIconInstance.transform.SetParent(minimapImage.transform, false);
        playerIconInstance.rectTransform.sizeDelta = playerIconSize;

        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject otherPlayer in otherPlayers)
        {
            if (otherPlayer.transform != mapCenter)
            {
                Image otherPlayerIcon = new GameObject("OtherPlayerIcon").AddComponent<Image>();
                otherPlayerIcon.sprite = playerIconSprite;
                otherPlayerIcon.transform.SetParent(minimapImage.transform, false);
                otherPlayerIcon.rectTransform.sizeDelta = playerIconSize;
                otherPlayerIcons.Add(otherPlayerIcon);
            }
        }

        GameObject[] mapObjects = GameObject.FindGameObjectsWithTag("MapObject");
        foreach (GameObject obj in mapObjects)
        {
            Image mapObjectIcon = new GameObject("MapObjectIcon").AddComponent<Image>();
            mapObjectIcon.sprite = mapObjectIconSprite;
            mapObjectIcon.transform.SetParent(minimapImage.transform, false);
            mapObjectIcon.rectTransform.sizeDelta = mapObjectIconSize;
            mapObjectIcons.Add(mapObjectIcon);
        }

        /*GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (GameObject tower in towers)
        {
            Image towerIcon = new GameObject("TowerIcon").AddComponent<Image>();
            towerIcon.sprite = towerIconSprite;
            towerIcon.transform.SetParent(minimapImage.transform, false);
            towerIcon.rectTransform.sizeDelta = towerIconSize;
            towerIcons.Add(towerIcon);
        }

        GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
        foreach (GameObject baseObj in bases)
        {
            Image baseIcon = new GameObject("BaseIcon").AddComponent<Image>();
            baseIcon.sprite = baseIconSprite;
            baseIcon.transform.SetParent(minimapImage.transform, false);
            baseIcon.rectTransform.sizeDelta = baseIconSize;
            baseIcons.Add(baseIcon);
        }*/
    }

    void Update()
    {
        if (mapCenter == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    mapCenter = player.transform;
                    break;
                }
            }
            return;
        }
        UpdateMinimap();
    }

    void UpdateMinimap()
    {
        foreach (var icon in otherPlayerIcons)
        {
            Destroy(icon.gameObject);
        }
        otherPlayerIcons.Clear();
        if (minimapImage.texture == null || playerIconInstance == null)
            return;

        Vector2 playerPosition = CalculateMinimapPosition(mapCenter.position);
        playerIconInstance.rectTransform.anchoredPosition = playerPosition;
        playerIconInstance.rectTransform.rotation = Quaternion.Euler(0, 0, -mapCenter.eulerAngles.y);

        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject otherPlayer in otherPlayers)
        {
            Player playerScript = otherPlayer.GetComponent<Player>(); // Player scriptine eriþim

            if (otherPlayer.transform != mapCenter && playerScript != null && !playerScript.isDead) // Ölü oyuncularý atla
            {
                Image otherPlayerIcon = new GameObject("OtherPlayerIcon").AddComponent<Image>();
                otherPlayerIcon.sprite = playerIconSprite;
                otherPlayerIcon.transform.SetParent(minimapImage.transform, false);
                otherPlayerIcon.rectTransform.sizeDelta = playerIconSize;
                otherPlayerIcons.Add(otherPlayerIcon);

                Vector2 otherPlayerPosition = CalculateMinimapPosition(otherPlayer.transform.position);
                otherPlayerIcon.rectTransform.anchoredPosition = otherPlayerPosition;
                otherPlayerIcon.rectTransform.rotation = Quaternion.Euler(0, 0, -otherPlayer.transform.eulerAngles.y);
            }
        }

        GameObject[] mapObjects = GameObject.FindGameObjectsWithTag("MapObject");
        for (int i = 0; i < mapObjects.Length; i++)
        {
            Vector2 objectPosition = CalculateMinimapPosition(mapObjects[i].transform.position);
            mapObjectIcons[i].rectTransform.anchoredPosition = objectPosition;
        }

        /*GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        for (int i = 0; i < towers.Length; i++)
        {
            Vector2 towerPosition = CalculateMinimapPosition(towers[i].transform.position);
            towerIcons[i].rectTransform.anchoredPosition = towerPosition;
        }

        GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
        for (int i = 0; i < bases.Length; i++)
        {
            Vector2 basePosition = CalculateMinimapPosition(bases[i].transform.position);
            baseIcons[i].rectTransform.anchoredPosition = basePosition;
        }*/
    }

    Vector2 CalculateMinimapPosition(Vector3 worldPosition)
    {
        GameObject groundObject = GameObject.FindGameObjectWithTag("Ground");
        float relativeX = (worldPosition.x - groundObject.transform.position.x) / groundSize.x;
        float relativeZ = (worldPosition.z - groundObject.transform.position.z) / groundSize.y;

        float minimapWidth = minimapImage.rectTransform.rect.width;
        float minimapHeight = minimapImage.rectTransform.rect.height;

        return new Vector2(relativeX * minimapWidth, relativeZ * minimapHeight);
    }

    void GenerateDynamicMinimap()
    {
        Texture2D minimapTexture = new Texture2D(256, 256);
        minimapImage.texture = minimapTexture;
    }
}
