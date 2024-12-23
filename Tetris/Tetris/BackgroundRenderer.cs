using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// 背景を描画する処理
    /// </summary>
    public class BackgroundRenderer
    {
        /// <summary>背景画像</summary>
        private static Image backgroundImg;

        /// <summary>
        /// 背景画像を読み込む
        /// </summary>
        static BackgroundRenderer()
        {
            try
            {
                // 背景画像の読み込み（共通で使用）
                backgroundImg = ImageLoader.LoadImage("background.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show("背景画像が見つかりません: " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 背景を描画する処理
        /// </summary>
        /// <param name="g">描画用のオブジェクト</param>
        /// <param name="clientSize">描画のサイズ</param>
        public static void DrawBackground(Graphics g, Size clientSize)
        {
            if (backgroundImg != null)
            {
                // フォームサイズに合わせて背景画像を描画
                g.DrawImage(backgroundImg, 0, 0, clientSize.Width, clientSize.Height);
            }
        }
    }
}