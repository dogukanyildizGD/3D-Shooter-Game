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

    // Silah t�r�n� a� �zerinde senkronize etmek i�in bir de�i�ken
    [SyncVar(hook = nameof(OnWeaponChanged))]
    private int weaponIndex;

    void Start()
    {
        // Oyun ba��nda t�m silahlar� instantiate et
        primaryWeaponInstance = Instantiate(primaryweapon.graphics, weaponHolder.position, weaponHolder.rotation);
        primaryWeaponInstance.transform.SetParent(weaponHolder);

        secondaryWeaponInstance = Instantiate(secondaryWeapon.graphics, weaponHolder.position, weaponHolder.rotation);
        secondaryWeaponInstance.transform.SetParent(weaponHolder);

        thirdWeaponInstance = Instantiate(thirdWeapon.graphics, weaponHolder.position, weaponHolder.rotation);
        thirdWeaponInstance.transform.SetParent(weaponHolder);

        // Silahlar�n ba�lang��taki mermi say�s�n� ayarla
        primaryweapon.bullets = primaryweapon.maxBullets;
        secondaryWeapon.bullets = secondaryWeapon.maxBullets;
        thirdWeapon.bullets = thirdWeapon.maxBullets;

        // Ba�lang��ta sadece primary weapon aktif olacak
        primaryWeaponInstance.SetActive(true);
        secondaryWeaponInstance.SetActive(false);
        thirdWeaponInstance.SetActive(false);

        EquipWeapon(primaryweapon);
    }

    void Update()
    {
        // E�er yerel oyuncu de�ilse, hi�bir �ey yapma
        if (!isLocalPlayer) return;

        // E�er '1' tu�una bas�ld�ysa primary weapon ku�an
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CmdChangeWeapon(0);
        }

        // E�er '2' tu�una bas�ld�ysa secondary weapon ku�an
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CmdChangeWeapon(1);
        }

        // E�er '3' tu�una bas�ld�ysa secondary weapon ku�an
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CmdChangeWeapon(2);
        }
    }

    [Command]
    void CmdChangeWeapon(int weaponIndex)
    {
        this.weaponIndex = weaponIndex; // Sunucu taraf�nda silah� g�ncelle, t�m istemcilere yay�lacak
    }

    // Hook metodu: Silah de�i�ti�inde bu metod �a�r�l�r
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
        // Silah� aktif/pasif yap
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
            // Silah�n t�m par�alar�n� 'Weapon' layer'�na ayarl�yoruz
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
