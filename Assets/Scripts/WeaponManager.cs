using UnityEngine;
using Mirror;
using System.Collections;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private PlayerWeapon primaryweapon;

    [SerializeField]
    private PlayerWeapon secondaryWeapon;

    [SerializeField]
    private PlayerWeapon thirdWeapon;

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;

    private GameObject primaryWeaponInstance;
    private GameObject secondaryWeaponInstance;
    private GameObject thirdWeaponInstance;

    public bool isReloading = false;

    // Silah türünü að üzerinde senkronize etmek için bir deðiþken
    [SyncVar(hook = nameof(OnWeaponChanged))]
    private int weaponIndex;

    void Start()
    {
        // Oyun baþýnda tüm silahlarý instantiate et
        primaryWeaponInstance = Instantiate(primaryweapon.graphics, weaponHolder.position, weaponHolder.rotation);
        primaryWeaponInstance.transform.SetParent(weaponHolder);

        secondaryWeaponInstance = Instantiate(secondaryWeapon.graphics, weaponHolder.position, weaponHolder.rotation);
        secondaryWeaponInstance.transform.SetParent(weaponHolder);

        thirdWeaponInstance = Instantiate(thirdWeapon.graphics, weaponHolder.position, weaponHolder.rotation);
        thirdWeaponInstance.transform.SetParent(weaponHolder);

        // Silahlarýn baþlangýçtaki mermi sayýsýný ayarla
        primaryweapon.bullets = primaryweapon.maxBullets;
        secondaryWeapon.bullets = secondaryWeapon.maxBullets;
        thirdWeapon.bullets = thirdWeapon.maxBullets;

        // Baþlangýçta sadece primary weapon aktif olacak
        primaryWeaponInstance.SetActive(true);
        secondaryWeaponInstance.SetActive(false);
        thirdWeaponInstance.SetActive(false);

        EquipWeapon(primaryweapon);
    }

    void Update()
    {
        // Eðer yerel oyuncu deðilse, hiçbir þey yapma
        if (!isLocalPlayer) return;

        // Eðer '1' tuþuna basýldýysa primary weapon kuþan
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CmdChangeWeapon(0);
        }

        // Eðer '2' tuþuna basýldýysa secondary weapon kuþan
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CmdChangeWeapon(1);
        }

        // Eðer '3' tuþuna basýldýysa secondary weapon kuþan
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CmdChangeWeapon(2);
        }
    }

    [Command]
    void CmdChangeWeapon(int weaponIndex)
    {
        this.weaponIndex = weaponIndex; // Sunucu tarafýnda silahý güncelle, tüm istemcilere yayýlacak
    }

    // Hook metodu: Silah deðiþtiðinde bu metod çaðrýlýr
    void OnWeaponChanged(int oldIndex, int newIndex)
    {
        if (newIndex == 0)
        {
            EquipWeapon(primaryweapon);
        }
        else if (newIndex == 1)
        {
            EquipWeapon(secondaryWeapon);
        }
        else if (newIndex == 2)
        {
            EquipWeapon(thirdWeapon);
        }
    }

    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    void EquipWeapon(PlayerWeapon _weapon)
    {
        // Silahý aktif/pasif yap
        if (_weapon == primaryweapon)
        {
            primaryWeaponInstance.SetActive(true);
            secondaryWeaponInstance.SetActive(false);
            thirdWeaponInstance.SetActive(false);
        }
        else if (_weapon == secondaryWeapon)
        {
            primaryWeaponInstance.SetActive(false);
            secondaryWeaponInstance.SetActive(true);
            thirdWeaponInstance.SetActive(false);
        }
        else if (_weapon == thirdWeapon)
        {
            primaryWeaponInstance.SetActive(false);
            secondaryWeaponInstance.SetActive(false);
            thirdWeaponInstance.SetActive(true);
        }

        currentWeapon = _weapon;
        currentGraphics = (currentWeapon == primaryweapon) ?
                       primaryWeaponInstance.GetComponent<WeaponGraphics>() :
                       (currentWeapon == secondaryWeapon) ?
                       secondaryWeaponInstance.GetComponent<WeaponGraphics>() :
                       thirdWeaponInstance.GetComponent<WeaponGraphics>();

        if (currentGraphics == null)
            Debug.LogError("No WeaponGraphics component on the weapon object.");

        if (isLocalPlayer)
            // Silahýn tüm parçalarýný 'Weapon' layer'ýna ayarlýyoruz
            Util.SetLayerRecursivly(currentGraphics.gameObject, LayerMask.NameToLayer(weaponLayerName));
    }

    public void Reload()
    {
        if (isReloading)
            return;

        StartCoroutine(Reload_Coroutine());
    }

    private IEnumerator Reload_Coroutine()
    {
        Debug.Log("Reloading...");

        isReloading = true;

        CmdOnReload();

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.bullets = currentWeapon.maxBullets;

        isReloading = false;
    }

    [Command]
    void CmdOnReload()
    {
        RpcOnReload();
    }

    [ClientRpc]
    void RpcOnReload()
    {
        Animator anim = currentGraphics.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Reload");
        }
    }
}
