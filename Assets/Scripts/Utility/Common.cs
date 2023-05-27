namespace HighFlyers.Utility
{
    public class Common
    {
        public const int ITEM_WIDTH = 236;
        public const int ITEM_HEIGHT = 236;
        public const int START_X = 300;
        public const int START_Y = -125;
        public const int CAN_ANSWER_TIME = 2;
        
        /// <summary>
        /// answer object.
        /// </summary>
        public class AnswerItem
        {
            public int row;
            public int col;
            public int index;
            public Common.ArrowState dir;
        }
        
        /// <summary>
        ///button states 
        /// </summary>
        public enum ButtonState
        {
            Disable,
            Free,
            Down
        }

        /// <summary>
        /// item background animation state.
        /// </summary>
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
    }
}