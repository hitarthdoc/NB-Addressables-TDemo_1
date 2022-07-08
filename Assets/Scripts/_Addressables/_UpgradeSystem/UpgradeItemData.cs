using System;
using UnityEngine.AddressableAssets;

namespace Nukebox.UpgradeSystem.Addressables
{
    [Serializable]
    public class UpgradeItemData
    {
        public string id;
        public AssetReference[] upgradeSprites;
    }
}