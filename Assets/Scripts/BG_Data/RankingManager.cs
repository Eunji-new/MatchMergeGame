using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using BansheeGz.BGDatabase;

public class RankingManager : MonoBehaviour
{
    public static RankingManager instance;
    public GameObject rankPrefab;
    public GameObject rankContent;

    private void Awake() {
        if(instance == null)
            instance = this;
    }
    
    List<DB_Ranking> ScoreList = new List<DB_Ranking>();

    int drop = 0;
    int index;
    public bool HasSavedFile
    {
        get { return File.Exists(SaveFilePath); }
    }
    public string SaveFilePath
    {
        get { return Path.Combine(Application.persistentDataPath, "BG_Save.dat"); }
    }

    void Start()
    {
        
        LoadData();
        SetRanking();
        //LoadData();
        //DropDownTest();
        Debug.Log(Application.persistentDataPath);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
            CreateRanking("test", 111);
    }

    public List<DB_Ranking> SortRanking()
    {
        return DB_Ranking.FindEntities(entity => 
            !string.IsNullOrEmpty(entity.Name), null, (e1, e2) => e2.score.CompareTo(e1.score));
    }

    public void RemoveAllRank()
    {
        Transform[] childs = rankContent.GetComponentsInChildren<Transform>();
        for(int i = 1; i < childs.Length; i++)
        {
            Destroy(childs[i].gameObject);
        }
    }
    public void SetRanking()
    {
        if(rankContent.transform.childCount > 0)
            RemoveAllRank();
        int idx = 1;
        //점수기준 오름차순 필요
        List<DB_Ranking> entities = SortRanking();
        foreach(var entity in entities)
        {
            GameObject rank = Instantiate(this.rankPrefab, rankContent.transform);
            rank.GetComponent<RankPrefab>().SetRankText(idx.ToString(), entity.name, entity.score.ToString());
            idx++;
        }
        // DB_Ranking.ForEachEntity((entity) =>
        // {   
        //     GameObject rank = Instantiate(this.rankPrefab, rankContent.transform);
        //     rank.GetComponent<RankPrefab>().SetRankText(idx.ToString(), entity.name, entity.score.ToString());
        //     idx++;
        // });
    }

    #region 점수 저장
    public bool CreateRanking(string name, int score)
    {
        if(IsExistName(name))
        {
            Debug.Log("이름이 없거나 이미 존재합니다. 다른 이름으로 설정해주세요.");
            return false;
        }
    
        var entity = DB_Ranking.NewEntity();
        entity.name = name;
        entity.score = score;
        SaveData();
        LoadData();
        SetRanking();
        return true;
    }
    
    public bool IsExistName(string name)
    {
        List<DB_Ranking> entities = DB_Ranking.FindEntities(entity => 
            !string.IsNullOrEmpty(entity.Name) && entity.Name.Equals(name));
        if(entities.Count > 0)
            return true;
        else
            return false;
    }
    #endregion

    // #region 점수 추가
    // public void AddScore_English()
    // {
    //     DB_Ranking.GetEntity(index).English += 10;
    //     Debug.Log(DB_Ranking.English);
    // }
    // #endregion


    // #region 점수 감소
    // public void SubScore_Math()
    // {
    //     DB_Ranking.GetEntity(index).Math -= 10;
    //     Debug.Log(DB_Ranking.Math);
    // }

    // #endregion


    #region 저장관련

    public void SaveData()
    {
        Debug.Log("Save");
        var bytes = BGRepo.I.Addons.Get<BGAddonSaveLoad>().Save();
        File.WriteAllBytes(SaveFilePath, bytes);
    }


    public void LoadData()
    {
        if (HasSavedFile)
        {
            Debug.Log("Load");
            var content = File.ReadAllBytes(SaveFilePath);
            BGRepo.I.Addons.Get<BGAddonSaveLoad>().Load(content);
        }
        else
        {
            SaveData();
        }
                
    }
    #endregion
}
