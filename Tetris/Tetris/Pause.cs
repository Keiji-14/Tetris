using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// ポーズ画面の管理
    /// </summary>
    class Pause
    {
        /// <summary>ゲーム再開イベント</summary>
        public event Action onResumeGame;
        /// <summary>リトライイベント</summary>
        public event Action onRetryGame;
        /// <summary>タイトルに戻るイベント</summary>
        public event Action onTitleBack;
        /// <summary>ポーズかどうかの判定</summary>
        private bool isPaused;
        /// <summary>現在選択しているメニューのインデックス</summary>
        private int selectedOption = 0;
        /// <summary>メニューのリスト</summary>
        private string[] menuOptions = { "ゲームに戻る", "リトライ", "タイトルに戻る" };
        /// <summary>ポーズのパネル</summary>
        private Panel pausePanel;

        /// <summary>
        /// ポーズ画面の初期化
        /// </summary>
        /// <param name="parentPanel">親のゲーム画面パネル</param>
        public Pause(Panel parentPanel)
        {
            // ポーズ画面のパネルを作成
            pausePanel = new CustomPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(128, 0, 0, 0),
                Visible = false,
            };

            // 親パネルにポーズパネルを追加
            parentPanel.Controls.Add(pausePanel);
            parentPanel.Controls.SetChildIndex(pausePanel, 0);

            // キー入力イベントの設定
            pausePanel.KeyDown += new KeyEventHandler(OnKeyDown);
            pausePanel.TabStop = true;

            // Paintイベントをサブスクライブ
            pausePanel.Paint += (sender, e) => OnPaint(e);
        }

        /// <summary>
        /// ゲームをポーズ状態にする
        /// </summary>

        public void PauseGame()
        {
            if (!isPaused)
            {
                // ポーズ状態に設定
                isPaused = true;
                pausePanel.Visible = true;
                // フォーカスをポーズ画面に設定
                pausePanel.Focus();
                pausePanel.Invalidate();
            }
        }

        /// <summary>
        /// ゲームを再開する
        /// </summary>
        public void ResumeGame()
        {
            if (isPaused)
            {
                // ポーズ状態を解除
                isPaused = false;
                onResumeGame?.Invoke();
                pausePanel.Visible = false;
                // フォーカスをゲーム画面に戻す
                pausePanel.Parent.Focus();
            }           
        }

        /// <summary>
        /// ポーズ状態を返す
        /// </summary>
        public bool IsPaused()
        {
            return isPaused;
        }

        /// <summary>
        /// ポーズ画面の描画処理
        /// </summary>
        /// <param name="e">描画のイベント</param>
        public void OnPaint(PaintEventArgs e)
        {
            // メニュー項目を描画
            DrawMenu(e.Graphics);
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
                    PointF position = new PointF((pausePanel.ClientSize.Width - textSize.Width) / 2, 220 + i * 50);

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
                pausePanel.Invalidate();
                AudioManager.Instance.PlaySound(AudioManager.Instance.cursorSE);
            }
            else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                selectedOption = (selectedOption + 1) % menuOptions.Length;
                pausePanel.Invalidate();
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
                    // ゲームに戻る
                    ResumeGame();
                    break;
                case 1:
                    // リトライ
                    onRetryGame?.Invoke();
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