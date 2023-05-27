using System;
using FairyGUI;
using HighFlyers.Utility;
using UnityEngine;

namespace HighFlyers.Core
{
    public class ItemManager : MonoBehaviour
    {
        private GLabel _gObject;
        private float _scale = 0.6f;
        private Action<ItemManager> _callback;

        public int id { get; set; }
        public Common.ButtonState buttonState { get; set; }

        /// <summary>
        /// init item and load item picture
        /// </summary>
        /// <param name="index"></param>
        /// <param name="obj"></param>
        /// <param name="imgName"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="callback"></param>
        public void Init(int index, GObject obj, string imgName, int row, int col, Action<ItemManager> callback)
        {
            id = index;
            _callback = callback;
            _gObject = obj.asLabel;
            _gObject.name = $"item_{index}";
            _gObject.scaleX = _scale;
            _gObject.scaleY = _scale;
            _gObject.x = Common.START_X + col * Common.ITEM_WIDTH;
            _gObject.y = Common.START_Y + row * Common.ITEM_HEIGHT;
            //set image
            var loader = _gObject.GetChild("pic").asLoader;
            loader.x = 30;
            loader.y = 30;
            loader.scaleX = 1.2f;
            loader.scaleY = 1.2f;
            Texture2D texture = Resources.Load<Texture2D>($"Texture/{imgName}");
            loader.texture = new NTexture(texture);
            //set item state
            SetButtonState(Common.ButtonState.Disable);
            _gObject.onClick.Add(OnClickedItem);
        }

        /// <summary>
        /// set item state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="resetArrow"></param>
        /// <param name="resetWrongIcon"></param>
        public void SetButtonState(Common.ButtonState state, bool resetArrow = false, bool resetWrongIcon = false)
        {
            if (_gObject == null) return;
            _gObject.asCom.GetChild("background_3").visible = false;
            _gObject.asCom.GetChild("background_2").visible = false;
            _gObject.asCom.GetChild("background_1").visible = false;
            if (resetArrow)
                _gObject.asCom.GetChild("background_arrow").visible = false;
            if (resetWrongIcon)
                _gObject.asCom.GetChild("wrong").visible = false;
            if (state == Common.ButtonState.Disable)
                _gObject.asCom.GetChild("background_1").visible = true;
            else if (state == Common.ButtonState.Free)
                _gObject.asCom.GetChild("background_3").visible = true;
            else if (state == Common.ButtonState.Down)
                _gObject.asCom.GetChild("background_arrow").visible = true;
            buttonState = state;
        }

        /// <summary>
        /// get item GUI object
        /// </summary>
        public GObject GetObject => _gObject;

        /// <summary>
        /// clicked item callback
        /// </summary>
        /// <param name="context"></param>
        private void OnClickedItem(EventContext context)
        {
            _callback?.Invoke(this);
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
    }
}