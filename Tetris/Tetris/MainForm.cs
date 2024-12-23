using System;
using System.Timers;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// テトリスゲームのメインフォーム
    /// アプリケーション全体の画面遷移を管理
    /// </summary>
    public class MainForm : Form
    {
        /// <summary>タイトル画面</summary>
        private Title titleScreen;
        /// <summary>モード選択画面</summary>
        private ModeSelect modeSelectScreen;
        /// <summary>ゲーム画面</summary>
        private Tetris gameScreen;
        /// <summary>ゲームオーバー画面</summary>
        private GameOver gameOverScreen;
        /// <summary>ランキング画面</summary>
        private Ranking rankingScreen;
        /// <summary>ゲームタイマー</summary>
        private System.Timers.Timer gameTimer;

        /// <summary>
        /// フォームの初期設定
        /// </summary>
        public MainForm()
        {
            // フォームの初期設定
            Text = "Tetris Game";
            ClientSize = new System.Drawing.Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;

            gameTimer = new System.Timers.Timer(16); // 約60FPS (1000ms / 60 = 約16ms)
            gameTimer.Elapsed += GameLoop;
            gameTimer.Start();

            // タイトル画面を表示
            ShowTitleScreen();
            // BGMを再生する
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgm);
        }

        /// <summary>
        /// タイトル画面を表示する処理
        /// </summary>
        private void ShowTitleScreen()
        {
            // 既存の画面をクリア
            Controls.Clear();

            // タイトル画面を生成
            titleScreen = new Title();
            // イベントを追加
            titleScreen.OnModeSelect += ShowModeSelectScreen;
            titleScreen.OnRanking += ShowRankingScreen;
            titleScreen.OnExitGame += ExitGame;
            Controls.Add(titleScreen);
            // タイトル画面にフォーカスを設定
            titleScreen.Focus();
        }

        /// <summary>
        /// モード選択画面を表示する処理
        /// </summary>
        private void ShowModeSelectScreen()
        {
            // 既存の画面をクリア
            Controls.Clear();

            // モード選択画面を生成
            modeSelectScreen = new ModeSelect();
            // イベントを追加
            modeSelectScreen.onNormalMode += ShowGameScreen;
            modeSelectScreen.onTimeAttackMode += ShowGameScreen;
            modeSelectScreen.onTitleBack += ShowTitleScreen;
            Controls.Add(modeSelectScreen);

            // モード選択画面にフォーカスを設定
            modeSelectScreen.Focus();
        }

        /// <summary>
        /// ゲーム画面を表示する処理
        /// </summary>
        private void ShowGameScreen()
        {
            // 既存の画面をクリア
            Controls.Clear();

            // ゲーム画面を生成
            gameScreen = new Tetris();
            // イベントを追加
            gameScreen.onRetryGame += ShowGameScreen;
            gameScreen.onTitleBack += ShowTitleScreen;
            gameScreen.onGameOver += ShowGameOverScreen;
            Controls.Add(gameScreen);

            // ゲーム画面にフォーカスを設定
            gameScreen.Focus();
        }

        /// <summary>
        /// ゲームオーバー画面を表示する処理
        /// </summary>
        private void ShowGameOverScreen()
        {
            // 既存の画面をクリア
            Controls.Clear();

            // ゲームオーバー画面を生成
            gameOverScreen = new GameOver();
            // Tetrisのスコアを渡す
            gameOverScreen.SetScore(GameDataManager.Instance.score);
            // イベントを追加
            gameOverScreen.onRetryGame += ShowGameScreen;
            gameOverScreen.onRanking += ShowRankingScreen;
            gameOverScreen.onTitleBack += ShowTitleScreen;
            Controls.Add(gameOverScreen);

            // ゲームオーバー画面にフォーカスを設定
            gameOverScreen.Focus();
        }

        /// <summary>
        /// ランキング画面を表示する処理
        /// </summary>
        private void ShowRankingScreen()
        {
            // 既存の画面をクリア
            Controls.Clear();
            // ランキング画面を生成
            rankingScreen = new Ranking();
            // イベントを追加
            rankingScreen.onTitleBack += ShowTitleScreen;
            Controls.Add(rankingScreen);

            // ランキング画面にフォーカスを設定
            rankingScreen.Focus();
        }

        private void GameLoop(object sender, ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                // フォーム全体の再描画
                Invalidate();
            }));
        }

        /// <summary>
        /// アプリケーションを終了する
        /// </summary>
        private void ExitGame()
        {
            Application.Exit();
        }
    }
}