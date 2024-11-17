using UnityEngine;

public class Util
{
    // Recursive olarak bir GameObject ve onun tüm alt nesnelerine layer ayarlayan fonksiyon
    public static void SetLayerRecursivly(GameObject _obj, int _newLayer)
    {
        if (_obj == null)
            return;

        // Mevcut nesnenin layer'ýný ayarlayýn
        _obj.layer = _newLayer;

        // Alt nesneleri gezerek onlarýn da layer'ýný ayarlayýn
        foreach (Transform _child in _obj.transform)
        {
            if (_child == null)
                return;

            SetLayerRecursivly(_child.gameObject, _newLayer);
        }
    }

}
