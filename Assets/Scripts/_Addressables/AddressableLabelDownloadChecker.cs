using System;

namespace Nukebox.Games.CC.Addressables
{
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    using static Nukebox.Utilities.StringExtensions;

    public class AddressableLabelDownloadChecker
    {

        #region Constructor
        private static AddressableLabelDownloadChecker instance;

        public static AddressableLabelDownloadChecker Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AddressableLabelDownloadChecker();
                }
                return instance;
            }
        }

        private AddressableLabelDownloadChecker()
        {
        }
        #endregion

        public void IsDownloaded(string key, Action<string, bool, float> Result)
        {
            void DidGetDownloadSize(AsyncOperationHandle<long> asyncOperation)
            {
                UnityEngine.Debug.Log($"[World Map]Label={key}, Result={asyncOperation.Result} {asyncOperation.Status}".ToColoredString(UnityEngine.Color.grey));
                Result(key, asyncOperation.Result == 0, asyncOperation.Result);
                Addressables.Release(asyncOperation);
            }

            Addressables.GetDownloadSizeAsync(key).Completed += DidGetDownloadSize;
        }


    }

}
