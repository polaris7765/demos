using System.Collections.Generic;
using UnityEngine;

public class JsonHelper
{
    private static JsonHelper _instance;

    //public TextAsset jsonText;
    //public string jsonPath;
    private data jsonData;

    public static JsonHelper Instance
    {
        get
        {
            if (_instance == null)
                _instance = new JsonHelper();
            return _instance;
        }
    }

    public void Init(string jsonText)
    {
        //TextAsset asset = Resources.Load<TextAsset>("jsonPath");
        string json = jsonText.Replace("-", "_");
        jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<data>>(json)[0];
        Debug.Log("get json data done!");
    }

    public data GetAllData => jsonData;
    public Activity GetActivity => jsonData.Activity;
    public List<StimulusItem> GetStimulusItemList => GetActivity.Stimulus;
    public List<QuestionsItem> GetQuestionsItemList => GetActivity.Questions;
}