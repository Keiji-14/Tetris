using System;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// キー入力を管理するクラス
    /// </summary>
    public class InputManager
    {
        /// <summary>上キーやWキーを押した時のアクション</summary>
        private readonly Action onInputUp;
        /// <summary>下キーやSキーを押した時のアクション</summary>
        private readonly Action onInputDown;
        /// <summary>左キーやAキーを押した時のアクション</summary>
        private readonly Action onInputLeft;
        /// <summary>右キーやDキーを押した時のアクション</summary>
        private readonly Action onInputRight;
        /// <summary>Rキーを押した時のアクション</summary>
        private readonly Action onInputR;
        /// <summary>Hキーを押した時のアクション</summary>
        private readonly Action onInputH;
        /// <summary>Spaceキーを押した時のアクション</summary>
        private readonly Action onInputSpace;
        /// <summary>Escapeキーを押した時のアクション</summary>
        private readonly Action onInputEscape;
        /// <summary>下キーやSキーを離した時のアクション</summary>
        private readonly Action onReleaseDown;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public InputManager(
            Action onInputUp,
            Action onInputDown,
            Action onInputLeft,
            Action onInputRight,
            Action onInputR,
            Action onInputH,
            Action onInputSpace,
            Action onInputEscape,
            Action onReleaseDown)
        {
            this.onInputUp = onInputUp;
            this.onInputDown = onInputDown;
            this.onInputLeft = onInputLeft;
            this.onInputRight = onInputRight;
            this.onInputR = onInputR;
            this.onInputH = onInputH;
            this.onInputSpace = onInputSpace;
            this.onInputEscape = onInputEscape;
            this.onReleaseDown = onReleaseDown;
        }

        /// <summary>
        /// キー押下時の処理
        /// </summary>
        public void HandleKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    onInputUp?.Invoke();
                    break;
                case Keys.Down:
                case Keys.S:
                    onInputDown?.Invoke();
                    break;
                case Keys.Left:
                case Keys.A:
                    onInputLeft?.Invoke();
                    break;
                case Keys.Right:
                case Keys.D:
                    onInputRight?.Invoke();
                    break;
                case Keys.R:
                    onInputR?.Invoke();
                    break;
                case Keys.H:
                    onInputH?.Invoke();
                    break;
                case Keys.Space:
                    onInputSpace?.Invoke();
                    break;
                case Keys.Escape:
                    onInputEscape?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// キー離脱時の処理
        /// </summary>
        public void HandleKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                onReleaseDown?.Invoke();
            }
        }
    }
}