using UnityEngine;

public class Util
{
    // Recursive olarak bir GameObject ve onun t�m alt nesnelerine layer ayarlayan fonksiyon
    public static void SetLayerRecursivly(GameObject _obj, int _newLayer)
    {
        if (_obj == null)
            return;

        // Mevcut nesnenin layer'�n� ayarlay�n
        _obj.layer = _newLayer;

        // Alt nesneleri gezerek onlar�n da layer'�n� ayarlay�n
        foreach (Transform _child in _obj.transform)
        {
            if (_child == null)
                return;

            SetLayerRecursivly(_child.gameObject, _newLayer);
        }
    }

}
