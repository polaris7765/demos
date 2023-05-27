using System.Collections.Generic;
/*
public class data
{
    public string ProductName;
    public string BookName;
    public string UnitName;
    public string LessonName;
    public ActivityData Activity;
    public string Owner;
    public string Key;
    public string CreatedStamp;
    public string LastUpdatedStamp;
    public string State;
}

public class ActivityData
{
    public List<StimulusData> Stimulus;
    public List<QuestionsData> Questions;
    public string Title;
    public string Key;
    public ActivityBodyData Body;
    public string Tags;
    public string Type;
    public string activityQuestionMd5;
    public string part;
    public string ContentId;
    public string ContentRevision;

}
//--------------------------ActivityBodyData--------------------------
public struct ActivityBodyData
{
    public List<MappingItem> mappings;
    public TagItem tags;
}

public struct MappingItem
{
    public string s;
    public string q;
    public string anchor;
}

public struct TagItem
{
    public string[] primary_skill_set;
    public string skillType;
}
//--------------------------End  ActivityBodyData----------------------
//--------------------------QuestionsData------------------------------
public struct QuestionsData
{
    public string Key;
    public QuestionsBodyData Body;
    public string Tags;
    public string Type;
    public StimulusOfQuestionData stimulusOfQuestion;
    public float  questionAnchor;
    public float  answerAnchor;
    public bool  isForModeling;

}

public struct QuestionsBodyData
{
    public string item;
    public string version;
    public List<QuestionBodyTagData> tags;
    public string skillSet;
    public string[] tests;
    public QuestionsBodyOptionsData options;
    public string layoutMode;
    public string[] answers;
    public string mode;
    public string background;
    public string asrEngine;
    public int hintTime;
}

public struct QuestionBodyTagData
{
    public string compassTags;
    public string subSkillSet;
    public string vocabulary;
    public int rows;
    public int cols;
    public string primary_skill_set;
}

public struct QuestionsBodyOptionsData
{
    public string type;
    public string id;
    public string text;
    public string prompt;
    public string hideText;
    public QuestionsBodyOptionsImageData image;
    public string audio;
    public string pdf;
    public string video;
    public string audioLocal;
    public string academic;
    public string showMode;
    public string subtitles;
    public int rowIndex;
    public int colIndex;
    public bool lockedPosition;
    public string expected;
    public string speaker;
    public string table;
    public int startRow;
    public int endRow;
    public int startCol;
    public int endCol;
    public string cells;
}

public struct QuestionsBodyOptionsImageData
{
    public string id;
    public string url;
    public int size;
    public string sha1;
    public string mimeType;
    public int width;
    public int height;
    public string language;
    public string title;
    public int duration;
    public string thumbnails;
}

public struct StimulusOfQuestionData
{
    public string Key;
    public BodyData Body;
    public string Tags;
    public string Type;
    public string stimulusOfQuestion;
    public float questionAnchor;
    public float answerAnchor;
    public bool isForModeling;
}

//-------------------------- End QuestionsData--------------
//-------------------------StimulusData---------------------
public struct StimulusData
{
    public string Key;
    public BodyData Body;
    public string Tags;
    public string Type;
    public string stimulusOfQuestion;
    public string questionAnchor;
    public string answerAnchor;
    public string isForModeling;
}
//----------------------- end StimulusData ------------------------------

public struct BodyData
{
    public BodyItemData item;
    public string version;
    public string tags;
    public string skillSet;
    public string tests;
    public string options;
    public string layoutMode;
    public string answers;
    public string mode;
    public string background;
    public string asrEngine;
    public string hintTime;
}

public struct BodyItemData
{
    public string type;
    public string id;
    public string text;
    public string prompt;
    public string hideText;
    public string image;
    public AudioData audio;
    public string pdf;
    public string video;
    public string audioLocal;
    public string academic;
    public string showMode;
    public string subtitles;
    public int rowIndex;
    public int colIndex;
    public bool lockedPosition;
    public string expected;
    public string speaker;
    public string table;
    public int startRow;
    public int endRow;
    public int startCol;
    public int endCol;
    public string cells;
}

public struct AudioData
{
    public string id;
    public string url;
    public int size;
    public string sha1;
    public string mimeType;
    public int width;
    public int height;
    public string language;
    public string title;
    public int duration;
    public string thumbnails;
}*/