using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nukebox.Games.CC.Addressables
{
    using UnityEngine.AddressableAssets;

    public enum AddressableType
    {
        Default = 0,
        Texture = 1,
        Sprite = 2,
        Shader = 3,
        Material = 4,

        AudioClip = 8,
        AudioMixer = 9,

        Scene = 16,
        Prefab = 17,

        ScriptableObject = 32,

    }

    public interface IAddressable
    {
        [field:SerializeField]
        public AssetReference AddressableAsset { get; set; }

        [field:SerializeField]
        public AddressableType AddressableType { get; set; }
    }
}
