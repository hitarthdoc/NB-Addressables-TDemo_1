using System;
using System.Collections.Generic;

namespace Nukebox.UpgradeSystem.Addressables
{
    [Serializable]
    public class RestaurantData
    {
        public uint worldId;
        public uint restaurantId;
        public List<UpgradeItemData> upgradeItemData;
    }
}