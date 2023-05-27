using System.Collections.Generic;
using HighFlyers.Core;
using UnityEngine;

namespace HighFlyers.Utility
{
    public class JsonHelper
    {
        private static JsonHelper _instance;
        private Data jsonData;

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
            //TextAsset asset = Resources.Load<TextAsset>(jsonPath);
            string json = jsonText.Replace("-", "_");
            jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Data>>(json)[0];
            Debug.Log("get json data done!");
        }

        /// <summary>
        /// get data in json.
        /// </summary>
        public Data GetAllData => jsonData;

        public Activity GetActivity => jsonData.Activity;
        public List<StimulusItem> GetStimulusItemList => GetActivity.Stimulus;
        public List<QuestionsItem> GetQuestionsItemList => GetActivity.Questions;
    }
}