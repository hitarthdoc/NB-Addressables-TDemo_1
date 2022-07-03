using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Nukebox.Games.CC.Addressables
{
    public class AddressablePool : Singleton<AddressablePool>
    {

        #region TESTING
        //#if UNITY_EDITOR
        //        [SerializeField]
        //        private int index;
        //        [SerializeField]
        //        private AssetReference[] all;

        //        [ContextMenu("Get")]
        //        public void GetObject()
        //        {
        //            GetObject(all[index], null);
        //        }

        //        [ContextMenu("GetAll")]
        //        public void GetAllObjects()
        //        {
        //            foreach (AssetReference assetReference in all)
        //                GetObject(assetReference, null);
        //        }

        //        [ContextMenu("Release")]
        //        public void ReleaseObject()
        //        {
        //            ReleaseObject(used[index].instantiatedObject);
        //        }
        //#endif
        #endregion

        /// <summary>
        /// List of available addressable objects 
        /// </summary>
        private List<AddressableObject> avail = new List<AddressableObject>();

        /// <summary>
        /// List of addressable objects currently being used in game
        /// </summary>
        private List<AddressableObject> used = new List<AddressableObject>();

        /// <summary>
        /// Is any addressable being loaded
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Current/Last addressable object that is being loaded/was loaded
        /// </summary>
        private AddressableObject currentObject;

        /// <summary>
        /// Pending addressable objects that have to be loaded after current addressable completes loading
        /// </summary>
        private Queue<AddressableObject> pendingReferences = new Queue<AddressableObject>();


        /// <summary>
        /// Return instantiated object for given asset reference if available in pool or Load new object for given asset reference and return instanatiated object
        /// </summary>
        public void GetObject(AssetReference assetReference, Action<GameObject> Result)
        {
            if (avail.Exists(item => item.isReference == true && item.reference == assetReference))
            {
                AddressableObject availableObject = avail.Find(item => item.isReference == true && item.reference == assetReference);
                avail.Remove(availableObject);
                used.Add(availableObject);
                Result?.Invoke(availableObject.instantiatedObject);
            }
            else if (isLoading == false)
            {
                isLoading = true;
                currentObject = new AddressableObject(assetReference, Result);
                used.Add(currentObject);
                AddressableAssetLoader.Instance.Load(assetReference, DidLoad, true);
            }
            else
            {
                pendingReferences.Enqueue(new AddressableObject(assetReference, Result));
            }
        }

        /// <summary>
        /// Callback when addressable system loads the addressable
        /// </summary>
        /// <param name="result"></param>
        private void DidLoad(AddressableAssetLoadResult result)
        {
            isLoading = false;
            currentObject.instantiatedObject = (GameObject)Instantiate(result.LoadedAsset);
            currentObject.Result?.Invoke(currentObject.instantiatedObject);
            if (pendingReferences.Count > 0)
            {
                AddressableObject addressableObject = pendingReferences.Dequeue();
                GetObject(addressableObject.reference, addressableObject.Result);
            }
        }

        /// <summary>
        /// Move instantiated object to avail for later use
        /// </summary>
        public void ReleaseObject(GameObject instantiatedObject)
        {
            AddressableObject unloadedObject = used.Find(item => item.isReference == true && item.instantiatedObject == instantiatedObject);
            unloadedObject.instantiatedObject.transform.SetParent(transform);
            used.Remove(unloadedObject);
            avail.Add(unloadedObject);
        }

        /// <summary>
        /// Release all available objects via addressables
        /// </summary>
        public void ReleaseAll()
        {
            int index = 0;
            while (index < avail.Count)
            {
                AddressableAssetLoader.Instance.Unload(avail[index].reference);
                index++;
            }
            avail.Clear();
            used.Clear();
            pendingReferences.Clear();
            isLoading = false;
            currentObject = null;
        }

    }


    [Serializable]
    public class AddressableObject
    {
        public string key;
        public AssetReference reference;
        public bool isReference { get; private set; }
        public GameObject instantiatedObject;
        public Action<GameObject> Result;

        /// <summary>
        /// Constructor using key
        /// </summary>
        public AddressableObject(string key, Action<GameObject> Result)
        {
            this.key = key;
            this.Result = Result;
            isReference = false;
        }

        /// <summary>
        /// Constructor using reference
        /// </summary>
        public AddressableObject(AssetReference reference, Action<GameObject> Result)
        {
            this.reference = reference;
            this.Result = Result;
            isReference = true;
        }

    }
}
