using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankPrefab : MonoBehaviour
{
    public Text rank;
    public Text name;
    public Text score;

    public void SetRankText(string rank, string name, string score)
    {
        this.rank.text = rank;
        this.name.text = name;
        this.score.text = score;
    }
}
