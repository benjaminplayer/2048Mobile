using TMPro;
using UnityEngine;

public class TestScene : MonoBehaviour
{

    public TextMeshProUGUI framerate;
    public TextMeshProUGUI refreshRates;
    public TextMeshProUGUI statsText;
    private string stats = "";
    private Resolution res = new Resolution();
    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        framerate.SetText("Framerate: "+(1f/Time.deltaTime));
        refreshRates.SetText("Refresh rates: "+ res.refreshRateRatio);
        if (stats.Length > 0) 
        {
            statsText.text = stats;
        }
    }

    public void toggleRunInBg() 
    {
        Application.runInBackground = !Application.runInBackground;
        stats += "; Runs in bg? "+Application.runInBackground;
    }

}
