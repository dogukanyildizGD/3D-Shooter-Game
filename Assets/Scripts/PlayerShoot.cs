using UnityEngine;
using Mirror;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;


    private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;

    // Start is called before the first frame update
    void Start()
    {
        if (cam == null)
        {
            Debug.LogError("PlayerShoot : No camera referenced!");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Mevcut silahý alýyoruz
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (PauseGame.IsOn)
            return;

        if(currentWeapon.bullets < currentWeapon.maxBullets)
        {
            if (Input.GetButtonDown("Reload"))
            {
                weaponManager.Reload();
                return;
            }
        }        

        // Fire rate pozitif ise otomatik ateþ sistemi çalýþtýr
        if (currentWeapon.fireRate > 0f)
        {
            // "Fire1" tuþuna basýldýðýnda (sol fare tuþu) ateþe baþla
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate); // Ateþ etmeye baþla
            }
            // "Fire1" tuþu býrakýldýðýnda ateþi durdur
            else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot"); // Ateþi durdur
            }
        }
        // Fire rate 0 veya negatifse tek seferlik ateþ sistemi
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot(); // Tek seferlik ateþ
            }
        }
    }

    // Is called on the server when a player shoots
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    // Is called on all clients when we need to do
    // a shoot effect
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    // Is called on the server when we hit something
    // Takes in the hit point and the normal of the surface
    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }

    // Is called on all clients
    // Here we can spawn in cool effects
    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }

    [Client]
    void Shoot()
    {
        if (!isLocalPlayer || weaponManager.isReloading)
        {
            return;
        }

        if (currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
            return;
        }

        currentWeapon.bullets--;

        Debug.Log("Kalan mermi : " + currentWeapon.bullets);

        // Referanslarýn durumu
        if (cam == null)
        {
            Debug.LogError("PlayerShoot: Camera is null in Shoot()");
            return;
        }

        if (currentWeapon == null)
        {
            Debug.LogError("PlayerShoot: Current weapon is null in Shoot()");
            return;
        }

        // Log player shot action
        //Debug.Log("Player is shooting");

        // We are shooting, call the OnShoot method on the server
        CmdOnShoot();

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            //Debug.Log("Raycast hit something");

            if (_hit.collider.tag == PLAYER_TAG)
            {
                // Silahýn damage deðeri loglanýyor
                //Debug.Log("Weapon Damage: " + currentWeapon.damage);
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage, transform.name);
            }

            // We hit something, call the OnHit mothed on the server
            CmdOnHit(_hit.point, _hit.normal);
        }

        if (currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
        }
    }

    [Command]
    void CmdPlayerShot(string _playerID, int _damage, string _sourceID)
    {
        Debug.Log("CmdPlayerShot called. Damage: " + _damage + " to player " + _playerID);
        Debug.Log(_playerID + "Has been shot.");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _sourceID);
    }

}
