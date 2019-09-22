using UnityEngine;
using Mirror;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private PlayerWeapon primaryWeapon;
    private PlayerWeapon currentWeapon;

    void Start()
    {
        EquipWeapon(primaryWeapon);
    }


    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }


    void EquipWeapon (PlayerWeapon _weapon)
    {
        currentWeapon = _weapon;

        GameObject _weaponInstance = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);
        _weaponInstance.transform.SetParent(weaponHolder);
        if (isLocalPlayer)
            _weaponInstance.layer = LayerMask.NameToLayer(weaponLayerName);
    }
}
