using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// ゲームオーバー画面の管理
    /// </summary>
    public class GameOver : Panel
    {
        /// <summary>ゲームのリトライイベント</summary>
        public event Action onRetryGame;
        /// <summary>ランキングイベント</summary>
        public event Action onRanking;
        /// <summary>タイトル戻るイベント</summary>
        public event Action onTitleBack;
        /// <summary>スコアを保持する変数</summary>
        private int score;
        /// <summary>現在選択しているメニューのインデックス</summary>
        private int selectedOption = 0;
        /// <summary>メニューのオプション項目</summary>
        private string[] menuOptions = { "リトライ", "ランキング", "タイトルに戻る" };

        /// <summary>
        /// ゲームオーバー画面の初期化
        /// </summary>
        public GameOver()
        {
            // パネルの設定
            Dock = DockStyle.Fill;
            BackColor = Color.Black;

            // ダブルバッファリングの有効化
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();

            // キー入力イベント
            KeyDown += new KeyEventHandler(OnKeyDown);
            // キー入力を有効化
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
        }

        /// <summary>
        /// スコアを設定する処理
        /// </summary>
        public void SetScore(int score)
        {
            this.score = score;
        }

        /// <summary>
        /// ゲームオーバー画面の描画処理
        /// </summary>
        /// <param name="e">描画のイベント</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 背景描画
            BackgroundRenderer.DrawBackground(e.Graphics, ClientSize);

            // ゲームオーバー表示
            DrawGameOver(e.Graphics);

            // メニュー描画
            DrawMenu(e.Graphics);
        }

        /// <summary>
        /// タイトル文字を描画する
        /// </summary>
        /// <param name="g">描画用のオブジェクト</param>
        private void DrawGameOver(Graphics g)
        {
            using (Font titleFont = new Font("Arial", 36, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.White))
            {
                string titleText = "Game Over";
                SizeF textSize = g.MeasureString(titleText, titleFont);
                PointF titlePosition = new PointF((ClientSize.Width - textSize.Width) / 2, 100);
                g.DrawString(titleText, titleFont, titleBrush, titlePosition);
            }

            // スコアを表示
            using (Font scoreFont = new Font("Arial", 24, FontStyle.Bold))
            using (Brush scoreBrush = new SolidBrush(Color.White))
            {
                string scoreText = "Score: " + score.ToString();
                SizeF scoreSize = g.MeasureString(scoreText, scoreFont);
                PointF scorePosition = new PointF((ClientSize.Width - scoreSize.Width) / 2, 200);
                g.DrawString(scoreText, scoreFont, scoreBrush, scorePosition);
            }
        }

        /// <summary>
        /// メニュー項目を描画する
        /// </summary>
        /// <param name="g">描画用のオブジェクト</param>
        private void DrawMenu(Graphics g)
        {
            using (Font menuFont = new Font("Arial", 18, FontStyle.Bold))
            {
                for (int i = 0; i < menuOptions.Length; i++)
                {
                    // メニュー項目の位置計算
                    string option = menuOptions[i];
                    SizeF textSize = g.MeasureString(option, menuFont);
                    PointF position = new PointF((ClientSize.Width - textSize.Width) / 2, 400 + i * 50);

                    if (i == selectedOption)
                    {
                        // 選択中の項目にはカーソルを表示
                        g.DrawString("▶", new Font("Arial", 24, FontStyle.Bold), Brushes.White, position.X - 30, position.Y);
                    }

                    g.DrawString(option, menuFont, Brushes.White, position);
                }
            }
        }

        /// <summary>
        /// キー入力イベント
        /// </summary>
        /// <param name="e">キー入力のイベント</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // 上下キーで選択肢を移動
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
            {
                selectedOption = (selectedOption - 1 + menuOptions.Length) % menuOptions.Length;
                Invalidate();
                AudioManager.Instance.PlaySound(AudioManager.Instance.cursorSE);
            }
            else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                selectedOption = (selectedOption + 1) % menuOptions.Length;
                Invalidate();
                AudioManager.Instance.PlaySound(AudioManager.Instance.cursorSE);
            }
            // 決定
            else if (e.KeyCode == Keys.Space)
            {
                MenuSelection();
                AudioManager.Instance.PlaySound(AudioManager.Instance.cursorSE);
            }
        }

        /// <summary>
        /// メニュー項目が選択されたときの処理
        /// </summary>
        private void MenuSelection()
        {
            switch (selectedOption)
            {
                case 0:
                    // リトライ
                    onRetryGame?.Invoke();
                    break;
                case 1:
                    // ランキング表示
                    onRanking?.Invoke();
                    break;
                case 2:
                    // タイトルに戻る
                    onTitleBack?.Invoke();
                    break;
                default:
                    break;
            }
        }
    }
}