using UnityEngine;
using TMPro;

public class PlayersUI : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField]
    private RectTransform healthBarFill;

    [SerializeField]
    private TMP_Text ammoText;

    [SerializeField]
    GameObject scoreboard;

    private Player player;
    private WeaponManager weaponManager;

    public void SetPlayer(Player _player)
    {
        player = _player;
        weaponManager = player.GetComponent<WeaponManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PauseGame.IsOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        SetHealtAmount(player.GethealthPct());
        SetAmmoAmount(weaponManager.GetCurrentWeapon().bullets);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseGame();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboard.SetActive(!scoreboard.activeSelf); // Aktifliði tersine çevir
        }
    }

    public void TogglePauseGame()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        PauseGame.IsOn = pauseMenu.activeSelf;
    }

    private void SetHealtAmount(float _amount)
    {
        healthBarFill.localScale = new Vector3(_amount, 1f, 1f);
    }

    private void SetAmmoAmount(int _amount)
    {
        ammoText.text = _amount.ToString();
    }
}
