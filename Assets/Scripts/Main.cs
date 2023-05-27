using UnityEngine;
using System.Collections.Generic;
using FairyGUI;
using HighFlyers.Core;
using HighFlyers.Utility;

namespace HighFlyers
{
    public class Main : MonoBehaviour
    {
        public TextAsset jsonText;
        public int playId = 0;

        private GComponent _mainView;
        private GObject _backBtn;
        private GObject _playBtn;
        private GObject _helpBtn;
        private GObject _closeBtn;
        private GObject _refreshBtn;
        private GComponent _progressBar;
        private GTextField _progressTxt;
        private GComponent _demoContainer;
        private Dictionary<int, ItemManager> _itemManagers;
        private List<int> _correctAnswers;
        private List<Common.AnswerItem> _answereds;
        private List<OptionsItem> _optionsItems;

        private bool _isCanBackStep = true;
        private int _clickedIndex = 0;
        private int _totalIndex = 0;
        private int _tryAnswerTimes = 0;
        private int _rowCount;
        private int _colCount;

        void Awake()
        {
            Application.targetFrameRate = 60;
            UIConfig.defaultFont = "Microsoft YaHei";
        }

        void Start()
        {
            Stage.inst.onKeyDown.Add(OnKeyDown);
            //get UI
            _mainView = this.GetComponent<UIPanel>().ui;
            //Play start animation.
            _mainView.GetTransition("appear").Play();
            //get Progress bar
            _progressBar = _mainView.GetChild("progress").asCom;
            _progressTxt = _progressBar.GetChild("progress").asTextField;
            _progressBar.onClick.Add(OnClickProgressBar);
            //get buttons
            _backBtn = _progressBar.GetChild("btn_back");
            _backBtn.visible = false;
            _backBtn.onClick.Add(OnClickBack);
            _playBtn = _mainView.GetChild("btn_play");
            _playBtn.onClick.Add(OnClickPlay);
            _helpBtn = _mainView.GetChild("btn_help");
            //_helpBtn.onClick.Add(onClickHelp);
            _closeBtn = _mainView.GetChild("btn_close");
            _closeBtn.onClick.Add(OnClickClose);
            _refreshBtn = _mainView.GetChild("btn_refresh");
            _refreshBtn.onClick.Add(OnClickRefresh);
            //get json data
            JsonHelper.Instance.Init(jsonText.text);
            QuestionsBody itemData = JsonHelper.Instance.GetQuestionsItemList[playId]?.Body;
            _correctAnswers = ConvertStringToInt(itemData?.answers[0]);
            _optionsItems = itemData?.options;
            _rowCount = (int)itemData?.tags[0]?.rows;
            _colCount = (int)itemData?.tags[0]?.cols;
            //get container
            _demoContainer = _mainView.GetChild("cont_1").asCom;
            CreateAllItems(_demoContainer, _optionsItems);
            //init items by answer
            InitCanClickItems(_correctAnswers[0]);
            //init progress
            SetProgress(0);
        }

        private void OnDestroy()
        {
            _mainView = null;
            _backBtn = null;
            _playBtn = null;
            _helpBtn = null;
            _closeBtn = null;
            _refreshBtn = null;
            _progressBar = null;
            _progressTxt = null;
            _demoContainer = null;
            _itemManagers?.Clear();
            _itemManagers = null;
            _correctAnswers?.Clear();
            _correctAnswers = null;
            _answereds?.Clear();
            _answereds = null;
            _optionsItems?.Clear();
            _optionsItems = null;
            jsonText = null;
        }

        /// <summary>
        /// create all items and set state to disable.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="items"></param>
        private void CreateAllItems(GComponent container, List<OptionsItem> items)
        {
            if (items == null || items.Count == 0)
            {
                Debug.LogError("get Options data error!");
                return;
            }

            //init dictionary
            _itemManagers = new Dictionary<int, ItemManager>();
            _answereds = new List<Common.AnswerItem>();
            _totalIndex = items.Count;
            for (int i = 0; i < _totalIndex; i++)
            {
                //init button
                OptionsItem item = items[i];
                var imgData = item?.image;
                var obj = UIPackage.CreateObject("CityAdventure", "groupItem").asLabel;
                ItemManager component = obj.displayObject.gameObject.AddComponent<ItemManager>();
                component.Init(i, obj, imgData?.sha1, item.rowIndex, item.colIndex, OnClickedItem);
                _itemManagers.Add(i, component);
                container.AddChild(obj);
            }
        }

        /// <summary>
        /// when clicked item.
        /// </summary>
        /// <param name="item"></param>
        private void OnClickedItem(ItemManager item)
        {
            if (IsGameOver()) return;
            if (!_isCanBackStep) return;
            Debug.Log($"On clicked Item :{item.id}");
            //had clicked some items.
            if (_answereds.Count > 0)
            {
                int aIndex = IsInAnswerList(item.id);
                if (aIndex != -1)
                {
                    if (aIndex == _answereds.Count - 1)
                        return;
                    if (aIndex == _answereds.Count - 2 && _answereds.Count > 1)
                    {
                        //back step
                        BackToLastStep(item.id);
                        return;
                    }
                }
            }

            if (item.buttonState != Common.ButtonState.Free)
            {
                Debug.Log($"Cannot Click this button:{item.id}");
                return;
            }

            //play clicked animation
            SetItemAnimation(item, Common.ArrowState.Clicked);
            //set to top
            _demoContainer.SetChildIndex(item.GetObject, _totalIndex - _clickedIndex);
            AfterClickedItem(item.id, _clickedIndex);
            _clickedIndex++;
            SetProgress(_clickedIndex);
        }

        /// <summary>
        /// set arrow background animation
        /// </summary>
        /// <param name="index"></param>
        /// <param name="clickIndex"></param>
        private void AfterClickedItem(int index, int clickIndex)
        {
            OptionsItem item = _optionsItems[index];
            //Debug.Log($"{item.rowIndex}, {item.colIndex}");
            Common.AnswerItem answerItem = new Common.AnswerItem()
                { index = index, col = item.colIndex, row = item.rowIndex };
            if (_answereds.Count > 0)
            {
                Common.AnswerItem aItem = _answereds[_answereds.Count - 1];
                ItemManager obj = SetItemState(aItem.row, aItem.col, true);
                Common.ArrowState dir = GetArrowDirection(_answereds[_answereds.Count - 1], answerItem);
                _answereds[_answereds.Count - 1].dir = dir;
                //play item background dir animation
                SetItemAnimation(obj, dir);
            }

            //clear other item.
            ResetAllItemSate();
            //last step not need find cross item.
            if (clickIndex == _correctAnswers.Count - 1)
                SetItemState(item.rowIndex, item.colIndex);
            else
                FindItemInCross(item.rowIndex, item.colIndex);
            _answereds.Add(answerItem);
        }

        /// <summary>
        /// init item when start. set first row item to free state. 
        /// </summary>
        /// <param name="startIndex"></param>
        private void InitCanClickItems(int startIndex)
        {
            //get the col of items
            var itemData = _optionsItems[startIndex];
            var index = itemData.colIndex * this._rowCount;
            var end = index + this._rowCount;
            for (int i = index; i < end; i++)
            {
                _itemManagers[i].SetButtonState(Common.ButtonState.Free);
            }
        }

        /// <summary>
        /// get item background arrow direction
        /// </summary>
        /// <param name="answerFrom"></param>
        /// <param name="answerTo"></param>
        /// <returns></returns>
        private Common.ArrowState GetArrowDirection(Common.AnswerItem answerFrom, Common.AnswerItem answerTo)
        {
            Common.AnswerItem aItem = answerFrom;
            if (answerTo.row == aItem.row)
            {
                if (answerTo.col > aItem.col)
                    return Common.ArrowState.Right;
                return Common.ArrowState.Left;
            }

            if (answerTo.col == aItem.col)
            {
                if (answerTo.row > aItem.row)
                    return Common.ArrowState.Down;
                return Common.ArrowState.Up;
            }

            return Common.ArrowState.None;
        }

        /// <summary>
        /// play item background animation by arrow state.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <param name="dir"></param>
        private void SetItemAnimation(ItemManager item, Common.ArrowState state, string dir = "")
        {
            item.GetObject.asCom.GetTransition(state + dir).Play();
        }

        /// <summary>
        /// set game progress
        /// </summary>
        /// <param name="progress"></param>
        private void SetProgress(int progress)
        {
            _progressTxt.text = $"{progress} / {_correctAnswers.Count}";
        }

        /// <summary>
        /// reset all item to disable state if state not be down, except force is true
        /// </summary>
        /// <param name="force">force set state to disable and show error icon</param>
        private void ResetAllItemSate(bool force = false)
        {
            for (int i = 0; i < _itemManagers.Count; i++)
            {
                var item = _itemManagers[i];
                if (force)
                {
                    item.SetButtonState(Common.ButtonState.Disable, true, true);
                    continue;
                }

                if (item.buttonState != Common.ButtonState.Down)
                    item.SetButtonState(Common.ButtonState.Disable);
            }
        }

        /// <summary>
        /// find the cross items by row and col
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="setArrow"></param>
        private void FindItemInCross(int row, int col, bool setArrow = false)
        {
            SetItemState(row, col, setArrow);
            SetItemState(row - 1, col); //up item
            SetItemState(row + 1, col); //down item
            SetItemState(row, col - 1); //left item
            SetItemState(row, col + 1); //right item
        }

        /// <summary>
        /// set item state
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="setArrow"></param>
        /// <returns></returns>
        private ItemManager SetItemState(int row, int col, bool setArrow = false)
        {
            var item = GetItemByXY(row - 1, col - 1);
            if (item == null) return null;
            int index = (row - 1) * _rowCount + col - 1;
            item.SetButtonState(setArrow ? Common.ButtonState.Down : Common.ButtonState.Free);
            return item;
        }

        private ItemManager GetItemByIndex(int index)
        {
            return _itemManagers[index];
        }

        private ItemManager GetItemByXY(int row, int col)
        {
            if (row < 0 || row >= _rowCount || col < 0 || col >= _colCount) return null;
            int index = (row * _rowCount + col);
            return _itemManagers[index];
        }

        /// <summary>
        /// Convert string array to int array
        /// </summary>
        /// <param name="strList"></param>
        /// <returns></returns>
        private List<int> ConvertStringToInt(List<string> strList)
        {
            return strList.ConvertAll(int.Parse);
        }

        /// <summary>
        /// check is this index had in answered list
        /// </summary>
        /// <param name="index"></param>
        /// <returns>-1: didn't in list</returns>
        private int IsInAnswerList(int index)
        {
            for (int i = 0; i < _answereds.Count; i++)
            {
                var item = _answereds[i];
                if (item.index == index)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// check all answer is correct?
        /// </summary>
        /// <returns></returns>
        private int CheckAnswerCorrect()
        {
            for (int i = 0; i < _answereds.Count; i++)
            {
                if (_answereds[i].index != _correctAnswers[i])
                {
                    return i;
                }
            }

            return -1; //all correct
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

        /// <summary>
        /// back one step
        /// </summary>
        /// <param name="index"></param>
        private void BackToLastStep(int index)
        {
            ResetAllItemSate();
            _answereds.RemoveAt(_answereds.Count - 1);
            ItemManager obj = GetItemByIndex(index);
            obj.SetButtonState(Common.ButtonState.Free, true);
            OptionsItem item = _optionsItems[index];
            FindItemInCross(item.rowIndex, item.colIndex);
            _clickedIndex--;
            SetProgress(_clickedIndex);
        }

        /// <summary>
        /// show wrong item with wrong icon when lost
        /// </summary>
        private void ShowWrongItemWhenLost()
        {
            for (int i = 0; i < _answereds.Count; i++)
            {
                var answerItem = _answereds[i];
                ItemManager obj = GetItemByIndex(answerItem.index);
                _demoContainer.SetChildIndex(obj.GetObject, answerItem.index);
                SetItemAnimation(obj, Common.ArrowState.WrongIcon);
                obj.SetButtonState(Common.ButtonState.Free, true);
            }
        }

        /// <summary>
        /// show all correct item when lost
        /// </summary>
        private void DisplayCorrectAnswer()
        {
            ResetAllItemSate(true);
            int count = _correctAnswers.Count;
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    int index = _correctAnswers[i];
                    OptionsItem item1 = _optionsItems[_correctAnswers[i - 1]];
                    OptionsItem item2 = _optionsItems[index];
                    ItemManager obj = SetItemState(item1.rowIndex, item1.colIndex, true);
                    _demoContainer.SetChildIndex(obj.GetObject, _totalIndex - i);
                    SetItemAnimation(obj,
                        GetArrowDirection(new Common.AnswerItem() { row = item1.rowIndex, col = item1.colIndex },
                            new Common.AnswerItem() { row = item2.rowIndex, col = item2.colIndex }));
                    if (i == count - 1)
                    {
                        obj = SetItemState(item2.rowIndex, item2.colIndex);
                        _demoContainer.SetChildIndex(obj.GetObject, _totalIndex - count);
                    }
                }
            }
        }

        private bool IsGameOver()
        {
            return (_tryAnswerTimes >= Common.CAN_ANSWER_TIME);
        }

        /// <summary>
        /// reset game when finished one time.
        /// </summary>
        private void GameReset()
        {
            _answereds.Clear();
            _clickedIndex = 0;
            _isCanBackStep = true;
            ResetAllItemSate(true);
            //init items by answer
            InitCanClickItems(_correctAnswers[0]);
            //init progress
            SetProgress(0);
            _progressBar.GetTransition("back").Play();
        }

        /// <summary>
        /// click progress bar switch to check answer button
        /// </summary>
        private void OnClickProgressBar()
        {
            if (_clickedIndex == _correctAnswers.Count)
            {
                _isCanBackStep = false;
                Invoke(nameof(ShowBackButton), 0.5f);
            }
        }

        /// <summary>
        /// change to check answer button
        /// </summary>
        private void ShowBackButton()
        {
            _progressBar.GetTransition("to").Play();
            Invoke(nameof(SetBackButton), 0.5f);
        }

        private void SetBackButton()
        {
            _backBtn.visible = true;
        }

        /// <summary>
        /// button clicked event
        /// </summary>
        private void OnClickBack()
        {
            if (IsGameOver()) return;
            int index = CheckAnswerCorrect();
            if (index == -1)
            {
                Debug.Log("ALL CORRECT!");
                ShowYouWin();
            }
            else
            {
                _tryAnswerTimes++;
                ItemManager obj = GetItemByIndex(_answereds[index].index);
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
                    SetItemAnimation(obj, Common.ArrowState.Wrong, _answereds[index].dir.ToString());
                    Invoke(nameof(GameReset), 2f);
                }
            }
        }

        private void OnClickRefresh()
        {
            _tryAnswerTimes = 0;
            GameReset();
        }

        private void OnClickPlay()
        {
            Debug.Log("play clicked!");
        }

        private void OnClickClose()
        {
            Application.Quit();
        }

        void OnKeyDown(EventContext context)
        {
            if (context.inputEvent.keyCode == KeyCode.Escape)
            {
                OnClickClose();
            }
        }
    }
}