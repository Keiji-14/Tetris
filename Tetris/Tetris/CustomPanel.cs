using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// 子パネルに使用するカスタムパネル
    /// </summary>
    public class CustomPanel : Panel
    {
        /// <summary>
        ///カスタムパネルの初期化
        /// </summary>
        public CustomPanel()
        {
            // ダブルバッファリングを有効にする
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();
        }
    }
}