using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.spacepuppy;
using com.spacepuppy.Collections;

public class zTest01 : MonoBehaviour
{

    public AmmoTable Table;


    [System.Serializable]
    public class AmmoTable : SerializableDictionaryBase<AmmoType, DiscreteFloat>
    {
    }

    public enum AmmoType
    {
        NineMM = 0,
        Shotgun = 1,
        Magnum32 = 2,
        Pellets = 3,
        ElephantGun = 4,
        HuntingRifle243 = 5,
        Pug22 = 6,
        CollapserRound = 7,
        InfiniteAmmoWeapon = 8
    }

}
