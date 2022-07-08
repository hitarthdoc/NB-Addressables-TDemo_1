using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class AddressableElementDownloadChecker
{
    public bool IsDownloaded(string key)
    {
        return DownloadSize(key).Result == 0;
    }

    private async Task<long> DownloadSize(string key)
    {
        return await Addressables.GetDownloadSizeAsync(key).Task;
    }

}