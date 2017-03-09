using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndLevel : MonoBehaviour {

    public Text _hitsText, _missedText, _maxComboText, _timeReloadText, _deadeyeKillsText, _totalScoreText;

    public int TotalHits { get; set; }
    public int TotalMissed { get; set; }
    public int MaxCombo { get; set; }
    public float TimeSpentReloading { get; set; }
    public int DeadeyeKills { get; set; }
    public int TotalScore { get; set; }

    private void Update()
    {
        _hitsText.text = "" + TotalHits;
        _missedText.text = "" + TotalMissed;
        _maxComboText.text = "" + MaxCombo + "x";
        _timeReloadText.text = "" + TimeSpentReloading;
        _deadeyeKillsText.text = "" + DeadeyeKills;
        _totalScoreText.text = "" + TotalScore;
    }
}
