using UnityEngine.Events;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;

public class CubeularAdMobManager : MonoBehaviour
{
    public static CubeularAdMobManager current;

    [SerializeField]
    private int rewardAdIntervalInMinutes = 20;

    private DateTime lastSuccessfulRewardAd;

    public bool CanShowRewardAd 
    {
        get 
        {
            return System.Math.Abs((DateTime.UtcNow - lastSuccessfulRewardAd).TotalMinutes) > rewardAdIntervalInMinutes;
        }
    }

    private readonly TimeSpan APPOPEN_TIMEOUT = TimeSpan.FromHours(4);
    private DateTime appOpenExpireTime;
    private AppOpenAd appOpenAd;
    private BannerView bannerView;
    private RewardedInterstitialAd rewardedInterstitialAd;

    private bool isShowingAppOpenAd;

    public RectTransform GameUI;
    public float AdSeparationSize = 20f;
    
    public bool AutoRetryConnectionIfAddFailsToLoad = true;
    public float AutoRetryIncrement = 30f;


    #region UNITY MONOBEHAVIOR METHODS

    public void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        //Set time of last reward so that it is just past the point where another add can be shown
        lastSuccessfulRewardAd = DateTime.UtcNow.Add(new TimeSpan(0, -(rewardAdIntervalInMinutes + 2), 0));
    }

    public void Start()
    {
        MobileAds.SetiOSAppPauseOnBackground(true);

        List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

        // Add some test device IDs (replace with your own device IDs).
#if UNITY_IPHONE
            deviceIds.Add("96e23e80653bb28980d3f40beb58915c");
#elif UNITY_ANDROID
        deviceIds.Add("75EF8D155528C04DACBBA6F36F433035");
        deviceIds.Add("7cff281238e19fd8");
        deviceIds.Add("10277FC8797C0427BF8CEF3CE77CF43E");
#endif

        // Configure TagForChildDirectedTreatment and test device IDs.
        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder()
            .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
            .SetTestDeviceIds(deviceIds).build();
        MobileAds.SetRequestConfiguration(requestConfiguration);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(HandleInitCompleteAction);

        // Listen to application foreground / background events.
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        Debug.Log("Initialization complete.");

        // Callbacks from GoogleMobileAds are not guaranteed to be called on
        // the main thread.
        // In this example we use MobileAdsEventExecutor to schedule these calls on
        // the next Update() loop.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
                //statusText.text = "Initialization complete.";
                RequestBannerAd();
        });
    }

    private void Update()
    {
        //if (showFpsMeter)
        //{
        //    fpsMeter.gameObject.SetActive(true);
        //    deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        //    float fps = 1.0f / deltaTime;
        //    fpsMeter.text = string.Format("{0:0.} fps", fps);
        //}
        //else
        //{
        //    fpsMeter.gameObject.SetActive(false);
        //}
    }

    #endregion

    #region HELPER METHODS

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddKeyword("unity-admob-sample")
            .Build();
    }

    private IEnumerator RetryBannerAddRequest()
    {
        PrintStatus("Retrying Request for Banner ad in " + AutoRetryIncrement + " seconds.");
        yield return new WaitForSeconds(AutoRetryIncrement);
        RequestBannerAd();
    }

    #endregion

    #region BANNER ADS

    public void RequestBannerAd()
    {
        PrintStatus("Requesting Banner ad.");

        // These ad units are configured to always serve test ads.
        //string adUnitId = "ca-app-pub-3940256099942544/6300978111"; //Test ad id for android
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = "ca-app-pub-7650965011443409/2268390676";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
            string adUnitId = "unexpected_platform";
#endif

        // Clean up banner before reusing
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        // Create a 320x50 banner at top of the screen
        //bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);


        // Add Event Handlers
        bannerView.OnAdLoaded += (sender, args) =>
        {
            PrintStatus("Banner ad loaded.");
            SetGameUIToFitBottomAddHeight(bannerView.GetHeightInPixels());
                //OnAdLoadedEvent.Invoke();
            };
        bannerView.OnAdFailedToLoad += (sender, args) =>
        {
            PrintStatus("Banner ad failed to load with error: " + args.LoadAdError.GetMessage());
            if (AutoRetryConnectionIfAddFailsToLoad && args.LoadAdError.GetCode() == 0)
            {
                StartCoroutine(RetryBannerAddRequest());
            }
                //OnAdFailedToLoadEvent.Invoke();
            };
        bannerView.OnAdOpening += (sender, args) =>
        {
            PrintStatus("Banner ad opening.");
                //OnAdOpeningEvent.Invoke();
            };
        bannerView.OnAdClosed += (sender, args) =>
        {
            PrintStatus("Banner ad closed.");
                //OnAdClosedEvent.Invoke();
            };
        bannerView.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Banner ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
            PrintStatus(msg);
        };

        // Load a banner ad
        bannerView.LoadAd(CreateAdRequest());
    }

    public void DestroyBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
        }
    }

    #endregion

    #region APPOPEN ADS

    public bool IsAppOpenAdAvailable
    {
        get
        {
            return (!isShowingAppOpenAd
                    && appOpenAd != null
                    && DateTime.Now < appOpenExpireTime);
        }
    }

    public void OnAppStateChanged(AppState state)
    {
        // Display the app open ad when the app is foregrounded.
        UnityEngine.Debug.Log("App State is " + state);

        // OnAppStateChanged is not guaranteed to execute on the Unity UI thread.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (state == AppState.Foreground)
            {
                ShowAppOpenAd();
            }
        });
    }

    public void RequestAndLoadAppOpenAd()
    {
        PrintStatus("Requesting App Open ad.");

        //string adUnitId = "ca-app-pub-3940256099942544/3419835294"; //Test intersticial for android
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = "ca-app-pub-7650965011443409/1750952718";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/5662855259";
#else
            string adUnitId = "unexpected_platform";
#endif
        // create new app open ad instance
        AppOpenAd.LoadAd(adUnitId,
                         ScreenOrientation.Portrait,
                         CreateAdRequest(),
                         OnAppOpenAdLoad);
    }

    private void OnAppOpenAdLoad(AppOpenAd ad, AdFailedToLoadEventArgs error)
    {
        if (error != null)
        {
            PrintStatus("App Open ad failed to load with error: " + error);
            return;
        }

        PrintStatus("App Open ad loaded. Please background the app and return.");
        this.appOpenAd = ad;
        this.appOpenExpireTime = DateTime.Now + APPOPEN_TIMEOUT;
    }

    public void ShowAppOpenAd()
    {
        if (!IsAppOpenAdAvailable)
        {
            return;
        }

        // Register for ad events.
        this.appOpenAd.OnAdDidDismissFullScreenContent += (sender, args) =>
        {
            PrintStatus("App Open ad dismissed.");
            isShowingAppOpenAd = false;
            if (this.appOpenAd != null)
            {
                this.appOpenAd.Destroy();
                this.appOpenAd = null;
            }
        };
        this.appOpenAd.OnAdFailedToPresentFullScreenContent += (sender, args) =>
        {
            PrintStatus("App Open ad failed to present with error: " + args.AdError.GetMessage());

            isShowingAppOpenAd = false;
            if (this.appOpenAd != null)
            {
                this.appOpenAd.Destroy();
                this.appOpenAd = null;
            }
        };
        this.appOpenAd.OnAdDidPresentFullScreenContent += (sender, args) =>
        {
            PrintStatus("App Open ad opened.");
        };
        this.appOpenAd.OnAdDidRecordImpression += (sender, args) =>
        {
            PrintStatus("App Open ad recorded an impression.");
        };
        this.appOpenAd.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "App Open ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
            PrintStatus(msg);
        };

        isShowingAppOpenAd = true;
        appOpenAd.Show();
    }

    #endregion

    #region REWARDED ADS

    public void RequestLoadAndShowRewardedInterstitialAd(Action<bool> OnResult)
    {
        PrintStatus("Requesting Rewarded Interstitial ad.");

        // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
                string adUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
                string adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
                string adUnitId = "unexpected_platform";
#endif

        // Create an interstitial.
        RewardedInterstitialAd.LoadAd(adUnitId, CreateAdRequest(), (rewardedInterstitialAd, error) =>
        {
            if (error != null)
            {
                PrintStatus("Rewarded Interstitial ad load failed with error: " + error);
                OnResult(false);
                return;
            }

                //bool contentShown = false;

                this.rewardedInterstitialAd = rewardedInterstitialAd;
            PrintStatus("Rewarded Interstitial ad loaded.");

                // Register for ad events.
                this.rewardedInterstitialAd.OnAdDidPresentFullScreenContent += (sender, args) =>
            {
                PrintStatus("Rewarded Interstitial ad presented.");
            };
            this.rewardedInterstitialAd.OnAdDidDismissFullScreenContent += (sender, args) =>
            {
                PrintStatus("Rewarded Interstitial ad dismissed.");
                this.rewardedInterstitialAd = null;

            };
            this.rewardedInterstitialAd.OnAdFailedToPresentFullScreenContent += (sender, args) =>
            {
                PrintStatus("Rewarded Interstitial ad failed to present with error: " +
                                                                        args.AdError.GetMessage());
                this.rewardedInterstitialAd = null;
            };
            this.rewardedInterstitialAd.OnPaidEvent += (sender, args) =>
            {
                string msg = string.Format("{0} (currency: {1}, value: {2}",
                                            "Rewarded Interstitial ad received a paid event.",
                                            args.AdValue.CurrencyCode,
                                            args.AdValue.Value);
                PrintStatus(msg);
            };
            this.rewardedInterstitialAd.OnAdDidRecordImpression += (sender, args) =>
            {
                PrintStatus("Rewarded Interstitial ad recorded an impression.");
            };

            ShowRewardedInterstitialAd(OnResult);
        });
    }

    private void ShowRewardedInterstitialAd(Action<bool> contentShown)
    {
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Show((reward) =>
            {
                PrintStatus("Rewarded Interstitial ad Rewarded : " + reward.Amount);
                contentShown(true);
                lastSuccessfulRewardAd = DateTime.UtcNow;
            });
        }
        else
        {
            contentShown(false);
            PrintStatus("Ad was unable to load in time.");
        }
    }

    #endregion

    #region AD INSPECTOR

    public void OpenAdInspector()
    {
        PrintStatus("Open ad Inspector.");

        MobileAds.OpenAdInspector((error) =>
        {
            if (error != null)
            {
                PrintStatus("ad Inspector failed to open with error: " + error);
            }
            else
            {
                PrintStatus("Ad Inspector opened successfully.");
            }
        });
    }

    #endregion

    #region Utility

    ///<summary>
    /// Log the message and update the status text on the main thread.
    ///<summary>
    private void PrintStatus(string message)
    {
        Debug.Log(message);
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
                //statusText.text = message;
            });
    }


    private void SetGameUIToFitBottomAddHeight(float height)
    {
        if (GameUI != null)
        {
            GameUI.offsetMin = new Vector2(GameUI.offsetMin.x, height + AdSeparationSize);
            //Rect rectToChange = GameUI.rect;
            //rectToChange.yMax = height + AdSeparationSize;
            //GameUI.rect.yMax = 0f;
        }
    }
    #endregion

}

