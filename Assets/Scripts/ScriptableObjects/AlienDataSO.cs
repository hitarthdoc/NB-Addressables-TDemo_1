using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class AlienData
{
    public Vector3 spawnPosition;
    public AssetReference alien;
}

[CreateAssetMenu]
public class AlienDataSO : ScriptableObject
{
    public AlienData AlienData;
}
