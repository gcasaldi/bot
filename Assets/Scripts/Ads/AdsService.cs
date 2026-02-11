using UnityEngine;

namespace PopAndStack
{
    public static class AdsService
    {
        public static bool IsReady { get; private set; }

        public static void Initialize()
        {
            IsReady = false;
            Debug.Log("AdsService: placeholder initialized. Add AdMob SDK to enable ads.");
        }

        public static void ShowInterstitial()
        {
            Debug.Log("AdsService: interstitial placeholder.");
        }

        public static void ShowRewarded(System.Action onReward)
        {
            Debug.Log("AdsService: rewarded placeholder.");
            onReward?.Invoke();
        }
    }
}
