using UnityEngine;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine.UI;
using Image = FairyGUI.Image;

public class BasicsMain : MonoBehaviour
{
    private GComponent _mainView;
    private GObject _backBtn;
    private GObject _playBtn;
    private GObject _helpBtn;
    private GObject _closeBtn;
    private GObject _refreshBtn;
    private GComponent _progressBar;
    private GTextField _progressTxt;
    private GComponent _demoContainer;
    private Dictionary<string, GObject> _itemObjects;
    private Dictionary<string, ButtonState> _itemStates;
    private List<string> _correctAnswers;
    private List<AnswerItem> _answereds;
    private List<OptionsItem> _optionsItems;
    public TextAsset jsonText;
    public Gradient lineGradient;
    public int playId = 0;

    private int _clickedIndex = 0;
    private int _totalIndex = 0;
    private int _tryAnswerTimes = 0;
    private const int CAN_ANSWER_TIME = 2;
    private int _rowCount;
    private int _colCount;
    private const int ITEM_WIDTH = 236;
    private const int ITEM_HEIGHT = 236;
    private const int START_X = 300;
    private const int START_Y = -125;

    public class AnswerItem
    {
        public int row;
        public int col;
        public string index;
        public ArrowState dir;
    }

    void Awake()
    {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
        //Use the font names directly
        UIConfig.defaultFont = "Microsoft YaHei";
#else
        //Need to put a ttf file into Resources folder. Here is the file name of the ttf file.
        UIConfig.defaultFont = "afont";
#endif
    }

    enum ButtonState
    {
        Disable,
        Normal,
        Free,
        Down
    }

    public enum ArrowState
    {
        None,
        Up,
        Down,
        Left,
        Right,
        Clicked,
        Wrong,
        WrongIcon
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);
        //get UI
        _mainView = this.GetComponent<UIPanel>().ui;
        //Play start animation.
        Transition trans = _mainView.GetTransition("appear");
        trans.Play();
        //get Progress bar
        _progressBar = _mainView.GetChild("progress").asCom;
        _progressTxt = _progressBar.GetChild("progress").asTextField;
        _progressBar.onClick.Add(ONClickProgressBar);

        //get buttons
        _backBtn = _progressBar.GetChild("btn_back");
        _backBtn.visible = false;
        _backBtn.onClick.Add(ONClickBack);
        _playBtn = _mainView.GetChild("btn_play");
        _playBtn.onClick.Add(onClickPlay);
        _helpBtn = _mainView.GetChild("btn_help");
        //_helpBtn.onClick.Add(onClickHelp);
        _closeBtn = _mainView.GetChild("btn_close");
        _closeBtn.onClick.Add(onClickClose);
        _refreshBtn = _mainView.GetChild("btn_refresh");
        //_refreshBtn.onClick.Add(onClickRefresh);
        //get json data
        JsonHelper.Instance.Init(jsonText.text);
        QuestionsBody itemData = JsonHelper.Instance.GetQuestionsItemList[playId]?.Body;
        _correctAnswers = itemData?.answers[0];
        _optionsItems = itemData?.options;
        _rowCount = (int) itemData?.tags[0]?.rows;
        _colCount = (int) itemData?.tags[0]?.cols;
        //get container
        _demoContainer = _mainView.GetChild("cont_1").asCom;
        CreateItems(_demoContainer, _optionsItems);
        //init items by answer
        InitAllItems(_correctAnswers[0]);
        //init progress
        SetProgress(0);
    }

    private void InitAllItems(string startIndex)
    {
        //get the col of items
        var itemData = _optionsItems[int.Parse(startIndex)];
        var index = itemData.colIndex * this._rowCount;
        var end = index + this._rowCount;
        for (int i = index; i < end; i++)
        {
            var item = _itemObjects[i.ToString()].asCom;
            SetButtonState(item, i.ToString(), ButtonState.Free);
        }
    }

    /// <summary>
    /// create all items.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="items"></param>
    private void CreateItems(GComponent container, List<OptionsItem> items)
    {
        if (items == null || items.Count == 0)
        {
            Debug.LogError("get Options data error!");
            return;
        }

        //get container
        _itemObjects = new Dictionary<string, GObject>();
        _itemStates = new Dictionary<string, ButtonState>();
        _answereds = new List<AnswerItem>();
        _totalIndex = items.Count;
        for (int i = 0; i < _totalIndex; i++)
        {
            //init button
            OptionsItem item = items[i];
            var imgData = item?.image;
            var obj = UIPackage.CreateObject("CityAdventure", "groupItem").asLabel;
            obj.name = $"item_{i}";
            obj.scaleX = 0.6f;
            obj.scaleY = 0.6f;
            obj.x = START_X + item.colIndex * ITEM_WIDTH;
            obj.y = START_Y + item.rowIndex * ITEM_HEIGHT;
            obj.onClick.Add(OnClickedItem);
            //set image
            var loader = obj.GetChild("pic").asLoader;
            loader.x = 60;
            loader.y = 60;
            Texture2D texture = Resources.Load<Texture2D>($"MazeUI/texture/{imgData?.sha1}");
            loader.texture = new NTexture(texture);
            _itemObjects.Add(i.ToString(), obj);
            _itemStates.Add(i.ToString(), ButtonState.Disable);
            container.AddChild(obj);
            //set item state
            SetButtonState(obj, i.ToString(), ButtonState.Disable);
        }
    }

    /*
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / (float) targetWidth);
        float incY = (1.0f / (float) targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float) px % targetWidth),
                incY * ((float) Mathf.Floor(px / targetWidth)));
        }

        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }
*/
    private void SetButtonState(GObject button, string index, ButtonState state, bool resetArrow = false, bool resetWrongIcon = false)
    {
        if (button == null) return;
        button.asCom.GetChild("background_3").visible = false;
        button.asCom.GetChild("background_2").visible = false;
        button.asCom.GetChild("background_1").visible = false;
        if (resetArrow)
            button.asCom.GetChild("background_arrow").visible = false;
        if (resetWrongIcon)
            button.asCom.GetChild("wrong").visible = false;
        if (state == ButtonState.Disable)
            button.asCom.GetChild("background_1").visible = true;
        else if (state == ButtonState.Free)
            button.asCom.GetChild("background_3").visible = true;
        else if (state == ButtonState.Down)
            button.asCom.GetChild("background_arrow").visible = true;
        _itemStates[index] = state;
    }

    private ButtonState GetButtonState(string index)
    {
        return _itemStates[index];
    }


    private void AfterClickedItem(string index, int clickIndex)
    {
        OptionsItem item = _optionsItems[int.Parse(index)];
        Debug.Log($"{item.rowIndex}, {item.colIndex}");
        AnswerItem answerItem = new AnswerItem() {index = index, col = item.colIndex, row = item.rowIndex};

        if (_answereds.Count > 0)
        {
            AnswerItem aItem = _answereds[_answereds.Count - 1];
            GObject obj = SetItemState(aItem.row, aItem.col, true);
            ArrowState dir = GetArrowDirection(_answereds[_answereds.Count - 1], answerItem);
            _answereds[_answereds.Count - 1].dir = dir;
            SetAnimation(obj, dir);
        }

        ResetAllItemSate();
        if (clickIndex == _correctAnswers.Count - 1)
            SetItemState(item.rowIndex, item.colIndex);
        else
        {
            FindItemInCross(item.rowIndex, item.colIndex);
        }

        _answereds.Add(answerItem);
    }

    private ArrowState GetArrowDirection(AnswerItem answerfrom, AnswerItem answerTo)
    {
        AnswerItem aItem = answerfrom;
        if (answerTo.row == aItem.row)
        {
            if (answerTo.col > aItem.col)
                return ArrowState.Right;
            else
                return ArrowState.Left;
        }


        if (answerTo.col == aItem.col)
        {
            if (answerTo.row > aItem.row)
                return ArrowState.Down;
            else
                return ArrowState.Up;
        }

        return ArrowState.None;
    }

    private void SetAnimation(GObject item, ArrowState state, string dir = "")
    {
        //Debug.Log($"SetAnimation :{state}");
        item.asCom.GetTransition(state.ToString() + dir).Play();
    }

    private void SetProgress(int progress)
    {
        _progressTxt.text = $"{progress} / {_correctAnswers.Count}";
    }

    private void ResetAllItemSate(bool force = false)
    {
        for (int i = 0; i < _itemObjects.Count; i++)
        {
            var item = _itemObjects[i.ToString()];
            if (force)
            {
                SetButtonState(item, i.ToString(), ButtonState.Disable, true, true);
                continue;
            }

            var buttonState = _itemStates[i.ToString()];
            if (buttonState != ButtonState.Down)
                SetButtonState(item, i.ToString(), ButtonState.Disable);
        }
    }

    private void FindItemInCross(int row, int col, bool setArrow = false)
    {
        SetItemState(row, col, setArrow);
        SetItemState(row - 1, col, false); //up item
        SetItemState(row + 1, col, false); //down item
        SetItemState(row, col - 1, false); //left item
        SetItemState(row, col + 1, false); //right item
    }

    private GObject SetItemState(int row, int col, bool setArrow = false)
    {
        var item = GetItemByXY(row - 1, col - 1);
        if (item == null) return null;
        int index = (row - 1) * _rowCount + col - 1;
        SetButtonState(item, index.ToString(), setArrow ? ButtonState.Down : ButtonState.Free);
        return item;
    }

    private GObject GetItemByIndex(string index)
    {
        return _itemObjects[index];
    }

    private GObject GetItemByXY(int row, int col)
    {
        if (row < 0 || row >= _rowCount || col < 0 || col >= _colCount) return null;
        string index = (row * _rowCount + col).ToString();
        return _itemObjects[index];
    }

    private void OnClickedItem(EventContext context)
    {
        if (IsGameOver()) return;
        if (!_isCanBackStep) return;
        string nameStr = ((GObject) (context.sender)).name;
        Debug.Log($"On clicked Item :{nameStr}");
        string index = ((GObject) (context.sender)).name.Substring(5);
        if (_answereds.Count > 0)
        {
            int aIndex = IsInAnswerList(index);
            if (aIndex != -1)
            {
                if (aIndex == _answereds.Count - 1)
                    return;
                if (aIndex == _answereds.Count - 2 && _answereds.Count > 1)
                {
                    BackToLastStep(index);
                    return;
                }
            }
        }

        if (GetButtonState(index) != ButtonState.Free)
        {
            Debug.Log($"Cannot Click this button:{index}");
            return;
        }

        GObject obj = GetItemByIndex(index);
        if (obj != null)
        {
            Debug.Log("On clicked Item!");
            //SetButtonState(obj, index, ButtonState.Down);
            SetAnimation(obj, ArrowState.Clicked);
            _demoContainer.SetChildIndex(obj, _totalIndex - _clickedIndex);
            AfterClickedItem(index, _clickedIndex);
            _clickedIndex++;
            SetProgress(_clickedIndex);
        }
    }

    private int IsInAnswerList(string index)
    {
        for (int i = 0; i < _answereds.Count; i++)
        {
            var item = _answereds[i];
            if (item.index == index)
                return i;
        }

        return -1;
    }

    private void ShowBackButton()
    {
        _progressBar.GetTransition("to").Play();
        Invoke(nameof(SetBackButton), 0.5f);
    }

    private void SetBackButton()
    {
        _backBtn.visible = true;
    }

    private bool _isCanBackStep = true;

    private void ONClickProgressBar()
    {
        if (_clickedIndex == _correctAnswers.Count)
        {
            _isCanBackStep = false;
            Invoke(nameof(ShowBackButton), 0.5f);
        }
    }

    private void ONClickBack()
    {
        if (IsGameOver()) return;
        Debug.Log("back clicked!");
        int index = CheckAnswer();
        if (index == -1)
        {
            Debug.Log("ALL CORRECT!");
            ShowYouWin();
        }
        else
        {
            _tryAnswerTimes++;
            GObject obj = GetItemByIndex(_answereds[index].index);
            if (IsGameOver())
            {
                Debug.Log("You are lost this round!");
                ShowWrongItemWhenLost();
                Invoke(nameof(DisplayCorrectAnswer), 3f);
                Invoke(nameof(ShowYouLost), 6f);
            }
            else
            {
                Debug.Log("Have Wrong Answer, Try Again!");
                SetAnimation(obj, ArrowState.Wrong, _answereds[index].dir.ToString());
                Invoke(nameof(GameReset), 2f);
            }
        }
    }

    private void ShowYouWin()
    {
        var win = _mainView.GetChild("YouWin").asCom;
        win.visible = true;
        win.GetTransition("t0").Play();
    }

    private void ShowYouLost()
    {
        var lose = _mainView.GetChild("YouLost").asCom;
        lose.visible = true;
        lose.GetTransition("t0").Play();
    }

    private void BackToLastStep(string index)
    {
        ResetAllItemSate();
        _answereds.RemoveAt(_answereds.Count - 1);
        GObject obj = GetItemByIndex(index);
        SetButtonState(obj, index, ButtonState.Free, true);
        OptionsItem item = _optionsItems[int.Parse(index)];
        FindItemInCross(item.rowIndex, item.colIndex);
        _clickedIndex--;
        SetProgress(_clickedIndex);
    }

    private void ShowWrongItemWhenLost()
    {
        for (int i = 0; i < _answereds.Count; i++)
        {
            var answerItem = _answereds[i];
            GObject obj = GetItemByIndex(answerItem.index);
            _demoContainer.SetChildIndex(obj, int.Parse(answerItem.index));
            SetAnimation(obj, ArrowState.WrongIcon);
            SetButtonState(obj, answerItem.index, ButtonState.Free, true);
        }
    }

    private void DisplayCorrectAnswer()
    {
        ResetAllItemSate(true);
        int count = _correctAnswers.Count;
        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                string index = _correctAnswers[i];
                OptionsItem item1 = _optionsItems[int.Parse(_correctAnswers[i - 1])];
                OptionsItem item2 = _optionsItems[int.Parse(index)];
                GObject obj = SetItemState(item1.rowIndex, item1.colIndex, true);
                _demoContainer.SetChildIndex(obj, _totalIndex - i);
                SetAnimation(obj, GetArrowDirection(new AnswerItem() {row = item1.rowIndex, col = item1.colIndex}, new AnswerItem() {row = item2.rowIndex, col = item2.colIndex}));
                if (i == count - 1)
                {
                    obj = SetItemState(item2.rowIndex, item2.colIndex);
                    _demoContainer.SetChildIndex(obj, _totalIndex - count);
                }
            }
        }
    }

    private bool IsGameOver()
    {
        return (_tryAnswerTimes >= CAN_ANSWER_TIME);
    }

    private void GameReset()
    {
        _answereds.Clear();
        _clickedIndex = 0;
        _isCanBackStep = true;
        ResetAllItemSate(true);
        //init items by answer
        InitAllItems(_correctAnswers[0]);
        //init progress
        SetProgress(0);
        _progressBar.GetTransition("back").Play();
    }

    private int CheckAnswer()
    {
        for (int i = 0; i < _answereds.Count; i++)
        {
            if (_answereds[i].index != _correctAnswers[i])
            {
                return i;
            }
        }

        return -1; //全部正确
    }

    private void onClickPlay()
    {
        Debug.Log("play clicked!");
    }

    private void onClickClose()
    {
        Application.Quit();
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            onClickClose();
        }
    }
}