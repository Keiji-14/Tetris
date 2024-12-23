using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// ランキング画面の管理
    /// </summary>
    public class Ranking : Panel
    {
        /// <summary>タイトル戻るイベント</summary>
        public event Action onTitleBack;
        /// <summary>ランキング最大数</summary>
        private const int maxRankingSize = 5;

        /// <summary>
        /// ランキング画面の初期化
        /// </summary>
        public Ranking()
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

            // スコアデータの読み込み
            ScoreManager.Instance.LoadData();
        }

        /// <summary>
        /// ランキング画面の描画処理
        /// </summary>
        /// <param name="e">描画のイベント</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 背景描画
            BackgroundRenderer.DrawBackground(e.Graphics, ClientSize);

            // ランキング表示
            DrawRanking(e.Graphics);
        }

        /// <summary>
        /// ランキングを描画する
        /// </summary>
        /// <param name="g">描画用のオブジェクト</param>
        private void DrawRanking(Graphics g)
        {
            // タイトルを描画
            using (Font titleFont = new Font("Arial", 36, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.White))
            {
                string titleText = "RANKING";
                SizeF textSize = g.MeasureString(titleText, titleFont);
                PointF titlePosition = new PointF((ClientSize.Width - textSize.Width) / 2, 100);
                g.DrawString(titleText, titleFont, titleBrush, titlePosition);
            }

            // モード名を描画
            using (Font modeFont = new Font("Arial", 24, FontStyle.Bold))
            using (Brush modeBrush = new SolidBrush(Color.White))
            {
                string normalModeText = "Normal";
                string timeAttackModeText = "TimeAttack";

                // NormalとTimeAttackのタイトル位置
                PointF normalModePosition = new PointF(ClientSize.Width / 4 - 50, 200);
                PointF timeAttackModePosition = new PointF(ClientSize.Width * 3 / 4 - 100, 200);

                g.DrawString(normalModeText, modeFont, modeBrush, normalModePosition);
                g.DrawString(timeAttackModeText, modeFont, modeBrush, timeAttackModePosition);
            }

            // ランキングを描画
            using (Font rankingFont = new Font("Arial", 24))
            using (Brush rankingBrush = new SolidBrush(Color.White))
            {
                // Normalランキングの描画位置
                int yPosition = 250;
                int maxRankingWidth = 0;

                // Normalモードのスコア幅を計算
                var normalRanking = ScoreManager.Instance.normalRanking;
                for (int i = 0; i < maxRankingSize; i++)
                {
                    string rankingText = $"{i + 1}. ";
                    // スコアがなければ0を表示
                    int score = i < normalRanking.Count ? normalRanking[i] : 0;
                    rankingText += score.ToString();
                    SizeF textSize = g.MeasureString(rankingText, rankingFont);
                    maxRankingWidth = Math.Max(maxRankingWidth, (int)textSize.Width);

                    g.DrawString(rankingText, rankingFont, rankingBrush, new PointF((ClientSize.Width / 4 - maxRankingWidth) + 150 / 2, yPosition));
                    yPosition += 40;
                }

                // TimeAttackランキングの描画位置
                yPosition = 250;
                maxRankingWidth = 0;

                // TimeAttackモードのスコア幅を計算
                var timeAttackRanking = ScoreManager.Instance.timeAttackRanking;
                for (int i = 0; i < maxRankingSize; i++)
                {
                    string rankingText = $"{i + 1}. ";
                    // スコアがなければ0を表示
                    int score = i < timeAttackRanking.Count ? timeAttackRanking[i] : 0;
                    rankingText += score.ToString();
                    SizeF textSize = g.MeasureString(rankingText, rankingFont);
                    maxRankingWidth = Math.Max(maxRankingWidth, (int)textSize.Width);

                    g.DrawString(rankingText, rankingFont, rankingBrush, new PointF((ClientSize.Width - maxRankingWidth) - 300 / 2, yPosition));
                    yPosition += 40;
                }
            }

            // タイトルに戻るを描画
            using (Font titleBackFont = new Font("Arial", 20, FontStyle.Bold))
            using (Brush titleBackBrush = new SolidBrush(Color.White))
            {
                string titleBackText = "▶ タイトルに戻る";
                SizeF textSize = g.MeasureString(titleBackText, titleBackFont);
                PointF titlePosition = new PointF((ClientSize.Width - textSize.Width) / 2, 500);
                g.DrawString(titleBackText, titleBackFont, titleBackBrush, titlePosition);
            }

        }

        /// <summary>
        /// キー入力イベント
        /// </summary>
        /// <param name="e">キー入力のイベント</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // 決定
            if (e.KeyCode == Keys.Space)
            {
                onTitleBack?.Invoke();
                AudioManager.Instance.PlaySound(AudioManager.Instance.cursorSE);
            }
        }
    }
}