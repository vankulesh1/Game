using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class ADS_Manager : MonoBehaviour, IUnityAdsListener
{
    public GameObject btn_resume;
    public Play_Script play_script;

    [SerializeField] bool _testMode = true;

    string _gameid = "4764066";

    string _video = "Interstitial_Android";
    string _rewardedVideo = "Rewarded_Android";
    string _banner = "Banner_Android";

    private void Start() //������������� ��������
    {
        Advertisement.AddListener(this);
        Advertisement.Initialize(_gameid, _testMode);

        if (PlayerPrefs.HasKey("ADS") && PlayerPrefs.GetInt("ADS") == 0)
        {
            //������� ���������
        }
        else
        {
            //������ �������� - ���������� �����
            #region Banner

            StartCoroutine(ShowBannerWhenInitialized());
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);

            #endregion
        }
    }

    public void ADS_OFF()
    {
        StopCoroutine(ShowBannerWhenInitialized());
        Advertisement.Banner.Hide();
    }

    public static void ShowAdsVideo(string placementId) //������������� ������� �� ����
    {
        //Advertisement.Is

        if (Advertisement.IsReady())
        {
            Advertisement.Show(placementId);
        }
        else
        {
            Debug.Log("Advertisement not ready!");
        }
    }

    IEnumerator ShowBannerWhenInitialized()
    {
        while (!Advertisement.isInitialized)
        {
            yield return new WaitForSeconds(0.5f);
        }
        Advertisement.Banner.Show(_banner);
    }

    public void OnUnityAdsReady(string placementId)
    {
        if (placementId == _rewardedVideo)
        {
            btn_resume.SetActive(true);
        }
    }

    public void OnUnityAdsDidError(string message)
    {
        //������ �������
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        //������ ��������� �������
    }

    public void OnUnityAdsDidFinish(string placmentId, ShowResult showResult)
    {
        if (showResult == ShowResult.Finished)
        {
            //��������� �������
            if (placmentId == _rewardedVideo)
                play_script.Resume_after_ads();
            else if (placmentId == _video)
                play_script.Orc_dying_temp();
        }
        else if (showResult == ShowResult.Skipped)
        {
            //������� �������
            if (placmentId == _video)
                play_script.Orc_dying_temp();
        }
        else if (showResult == ShowResult.Failed)
        {
            //������
        }
    }
}