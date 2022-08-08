using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestGoogleAPICall : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //This is a really cool but also hacky way of sending a form
        //StartCoroutine(Upload());

        //using (UnityWebRequest www = UnityWebRequest.Post("https://docs.google.com/forms/d/e/1FAIpQLSd8pMam4OG1UzmDjPtcyit8NzWIU2PDdx9xCqUg6rm26BRItg/formResponse", "?usp=pp_url&entry.945305852=PhoneModel&entry.325639978=AndroidVersion&entry.1348906643=Yes&entry.323854378=Previous+Question+more+info&entry.1674220268=No&entry.117403324=Previous+Question+more+info&entry.80213971=Yes&entry.986079846=No&entry.1924872895=Yes&entry.794254423=No&entry.1072998611=1&entry.2098809431=2&entry.1555772170=Previous+Question+more+info&entry.1622573700=3&entry.1818127045=4&entry.1964610322=Yes&entry.1813478035=2p&entry.37981776=Previous+Question+more+info&entry.322010300=last+question"))
        //{
        //    yield return www.SendWebRequest();

        //    if (www.result != UnityWebRequest.Result.Success)
        //    {
        //        Debug.Log(www.error);
        //    }
        //    else
        //    {
        //        Debug.Log("Form upload complete!");
        //    }
        //}
    }


    IEnumerator Upload()
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.945305852", "PhoneModel");
        form.AddField("entry.325639978", "AndroidVersion");
        form.AddField("entry.1348906643", "Yes");
        form.AddField("entry.323854378", "Previous+Question+more+info");
        form.AddField("entry.1674220268", "No");
        form.AddField("entry.117403324", "Previous+Question+more+info");
        form.AddField("entry.80213971", "Yes");
        form.AddField("entry.986079846", "No");
        form.AddField("entry.1924872895", "Yes");
        form.AddField("entry.794254423", "No");
        form.AddField("entry.1072998611", "1");
        form.AddField("entry.2098809431", "2");
        form.AddField("entry.1555772170", "Previous + Question + more + info");
        form.AddField("entry.1622573700", "3");
        form.AddField("entry.1818127045", "4");
        form.AddField("entry.1964610322", "Yes");
        form.AddField("entry.1813478035", "2p");
        form.AddField("entry.37981776", "Previous + Question + more + info");
        form.AddField("entry.322010300", "last + question");

        using (UnityWebRequest www = UnityWebRequest.Post("https://docs.google.com/forms/d/e/1FAIpQLSd8pMam4OG1UzmDjPtcyit8NzWIU2PDdx9xCqUg6rm26BRItg/formResponse", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }
}
