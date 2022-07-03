using System.Collections.Generic;
using UnityEngine;

namespace Nukebox.UpgradeSystem.Addressables
{
    [CreateAssetMenu(fileName = "UpgradeSystemAddressablesData", menuName = "ScriptableObjects/Data/UpgradeSystem/AddressablesData", order = 1)]
    public class GameData : ScriptableObject
    {
        public List<RestaurantData> restaurantData;
    }
}