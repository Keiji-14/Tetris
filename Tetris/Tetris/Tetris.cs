using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// テトリスゲーム画面の管理
    /// </summary>
    public class Tetris : Panel
    {
        #region 変数関連
        // イベント関連
        /// <summary>リトライイベント</summary>
        public event Action onRetryGame;
        /// <summary>タイトルに戻るイベント</summary>
        public event Action onTitleBack;
        /// <summary>ゲームオーバーイベント</summary>
        public event Action onGameOver;

        // フラグ: ゲーム開始状態を管理
        /// <summary>リトライイベント</summary>
        private bool isGameRunning = false;
        /// <summary>リトライイベント</summary>
        private bool isReadyState = false;
        /// <summary>リトライイベント</summary>
        private bool isGoState = false;

        // ゲームの基本設定
        /// <summary>壁を含む盤面の縦マス</summary>
        private const int rows = 21;
        /// <summary>底を含む盤面の横マス</summary>
        private const int columns = 12;
        /// <summary>セルのサイズ</summary>
        private const int cellSize = 25;
        /// <summary>初期の落下速度</summary>
        private const int baseSpeed = 800;
        /// <summary>落下速度の最大値</summary>
        private const int maxSpeed = 300;
        /// <summary>レベルごとに減少する速度 </summary>
        private const int speedIncrement = 50;
        /// <summary>レベルアップに必要なスコア</summary>
        private const int scorePerLevel = 1000;
        /// <summaryタイムアタックモードの制限時間</summary>
        private const int timeAttackTimer = 180;

        // ゲームの盤面
        /// <summary>ゲームの盤面</summary>
        private int[,] grid;
        /// <summary>壁や底の画像</summary>
        private Image blockImg;
        /// <summary>盤面の各セルに表示する画像</summary>
        private Image[,] gridImgs;

        // ゲーム進行関連のタイマー
        /// <summary>落下の間隔</summary>
        private Timer dropTimer;
        /// <summary>ゲーム時間を計測するタイマー</summary>
        private Timer gameTimeTimer;

        // テトリミノ関連
        /// <summary>現在のテトリミノ</summary>
        private Tetromino currentTetromino;
        /// <summary>次のテトリミノ</summary>
        private Tetromino nextTetromino;
        /// <summary>ホールド状態のテトリミノ</summary>
        private Tetromino holdTetromino = null;

        // その他
        /// <summary>ポーズ画面</summary>
        private Pause pause;
        /// <summary>次のテトリミノを表示するパネル</summary>
        private Panel nextTetrominoPanel;
        /// <summary>テトリミノのホールド状態を表示するパネル</summary>
        private Panel holdTetrominoPanel;
        /// <summary>キー入力を管理するクラス</summary>
        private InputManager inputManager;
        #endregion

        #region 初期化関連
        /// <summary>s
        /// ゲーム画面の初期化
        /// </summary>
        public Tetris()
        {
            // ウィンドウの設定
            ClientSize = new Size(800, 600);
            BackColor = Color.Black;

            // ダブルバッファリングの有効化
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();

            // 壁ブロック画像の読み込み
            try
            {
                blockImg = ImageLoader.LoadImage("block.png");
            }
            catch (Exception ex)
            {
                // 画像がない場合、プログラムを終了
                MessageBox.Show("画像が見つかりません: " + ex.Message);
                Environment.Exit(1);
            }

            // ゲームの初期化
            InitializeGame();
            // ゲーム開始演出を表示
            ShowStartAnimation();
        }

        /// <summary>
        /// ゲームの初期化
        /// </summary>
        private void InitializeGame()
        {
            // ゲームの状態を初期化
            GameDataManager.Instance.InitializeGameData();

            // ラベルの初期化
            InitializeLabel();
            // 盤面データの初期化
            InitializeGrid();
            // テトリミノの初期化
            InitializeTeromino();
            // キー入力の初期化
            InitializeInputKey();

            // 落下用のタイマーを設定
            dropTimer = new Timer();
            // 最初は800msごとに落下
            dropTimer.Interval = baseSpeed;
            dropTimer.Tick += new EventHandler(OnDrop);

            // ポーズ機能を初期化
            pause = new Pause(this);
            pause.onResumeGame += ResumeGame;
            pause.onRetryGame += () => onRetryGame?.Invoke();
            pause.onTitleBack += () => onTitleBack?.Invoke();
            pause.ResumeGame();

            // ランキングの読み込み
            ScoreManager.Instance.LoadData();

            switch (GameDataManager.Instance.gameMode)
            {
                case GameMode.Normal:
                    // ノーマルモードのハイスコア表示の更新
                    UpdateHighScoreDisplay(ScoreManager.Instance.normalHighScore);
                    break;
                case GameMode.TimeAttack:
                    // タイムアタックモードのハイスコア表示の更新
                    UpdateHighScoreDisplay(ScoreManager.Instance.timeAttackHighScore);
                    break;
            }
        }

        /// <summary>
        /// キー入力を初期化
        /// </summary>
        private void InitializeInputKey()
        {
            // キー入力イベント
            KeyDown += new KeyEventHandler(OnKeyDown);
            KeyUp += new KeyEventHandler(OnKeyUp);
            // キー入力を有効化
            SetStyle(ControlStyles.Selectable, true);
            // フォーカスを有効にする
            TabStop = true;

            // InputManager の初期化
            inputManager = new InputManager(
                onInputUp: () => { },
                onInputLeft: () =>
                {
                    if (!pause.IsPaused() && CanMoveLeft())
                        currentTetromino.col--;
                },
                onInputRight: () =>
                {
                    if (!pause.IsPaused() && CanMoveRight())
                        currentTetromino.col++;
                },
                onInputDown: () => { dropTimer.Interval = 50; },
                onInputR: RotateTetromino,
                onInputH: HoldTetromino,
                onInputSpace: () =>
                {
                    while (CanMoveDown())
                    {
                        currentTetromino.row++;
                    }
                    GameDataManager.Instance.UpdateScore(10);
                    UpdateScoreDisplay();
                    PlaceTetrominoOnGrid();
                    RemoveLine();
                    SpawnTetromino();
                },
                onInputEscape: () =>
                {
                    pause.PauseGame();
                    StopTimer();
                    AudioManager.Instance.PlaySound(AudioManager.Instance.cursorSE);
                },
                onReleaseDown: () => { dropTimer.Interval = 500; }
            );
        }

        /// <summary>
        /// ラベルを初期化
        /// </summary>
        private void InitializeLabel()
        {
            // スコア表示用のラベルを作成
            Label scoreLabel = new Label
            {
                Name = "ScoreLabel",
                Text = "Score: 0",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(ClientSize.Width - 230, 450),
                Size = new Size(200, 30)
            };

            // ハイスコア表示用のラベルを作成
            Label highScoreLabel = new Label
            {
                Name = "HighScoreLabel",
                Text = "High Score: 0",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(ClientSize.Width - 230, 400),
                Size = new Size(200, 30)
            };

            // 消したライン数表示用のラベルを作成
            Label removeLineLabel = new Label
            {
                Name = "RemoveLineLabel",
                Text = "Lines: 0",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(ClientSize.Width - 230, 350),
                Size = new Size(200, 30)
            };

            // プレイ時間表示用のラベルを作成
            Label playTimerLabel = new Label
            {
                Name = "PlayTimerLabel",
                Text = "Time: 00:00",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(ClientSize.Width - 230, 300),
                Size = new Size(200, 30)
            };

            // プレイ時間表示用のラベルを作成
            Label gameModeLabel = new Label
            {
                Name = "GameModeLabel",
                Text = "Mode: " + GameDataManager.Instance.gameMode,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(ClientSize.Width - 230, 250),
                Size = new Size(200, 30)
            };

            // ラベルをフォームに追加
            Controls.Add(scoreLabel);
            Controls.Add(highScoreLabel);
            Controls.Add(removeLineLabel);
            Controls.Add(playTimerLabel);
            Controls.Add(gameModeLabel);
        }

        /// <summary>
        /// ゲームの盤面を初期化
        /// </summary>
        private void InitializeGrid()
        {
            // 初期化
            grid = new int[rows, columns];
            gridImgs = new Image[rows, columns];

            // 盤面を空で初期化（0は空、1はブロック）
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // 左右の壁と最下段にブロックを配置
                    if (col == 0 || col == columns - 1 || row == rows - 1)
                    {
                        // 壁や底の部分
                        grid[row, col] = 1;
                        gridImgs[row, col] = blockImg;
                    }
                    else
                    {
                        // 壁や底以外の部分
                        grid[row, col] = 0;
                        gridImgs[row, col] = null;
                    }
                }
            }
        }

        /// <summary>
        /// テトリミノのを初期化
        /// </summary>
        private void InitializeTeromino()
        {
            // 次のテトリミノを表示するパネル作成
            nextTetrominoPanel = new Panel
            {
                Size = new Size(120, 120),
                Location = new Point(ClientSize.Width - 220, 50),
                BackColor = Color.Black,
            };
            // パネルに「NEXT」を描画する
            nextTetrominoPanel.Paint += NextTetrominoPanel;

            // ホールド状態のテトリミノを表示するパネル作成
            holdTetrominoPanel = new Panel
            {
                Size = new Size(120, 120),
                Location = new Point(ClientSize.Width - 700, 50),
                BackColor = Color.Black,
            };
            // パネルに「Next」を描画する
            holdTetrominoPanel.Paint += HoldTetrominoPanel;

            // パネルをフォームに追加
            Controls.Add(nextTetrominoPanel);
            Controls.Add(holdTetrominoPanel);
        }

        /// <summary>
        /// ゲームを開始する
        /// </summary>
        private void StartGame()
        {
            switch (GameDataManager.Instance.gameMode)
            {
                case GameMode.Normal:
                    // プレイ時間計測タイマーを開始
                    StartPlayTimeTimer();
                    break;
                case GameMode.TimeAttack:
                    // タイムアタック用のタイマー開始
                    StartTimeAttackTimer();
                    break;
            }

            // 最初のテトリミノと次のテトリミノを生成
            currentTetromino = TetrominoGenerator.CreateRandomTetromino();
            nextTetromino = TetrominoGenerator.CreateRandomTetromino();
            UpdateNextTetrominoPreview();

            // テトリミノ落下タイマーを開始
            dropTimer.Start();
        }
        #endregion

        #region 描画関連
        /// <summary>
        ///　初期ゲーム画面の描画処理
        /// </summary>
        /// <param name="e">描画のイベント</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 背景描画
            BackgroundRenderer.DrawBackground(e.Graphics, ClientSize);
            // 描画用のオブジェクトを取得
            Graphics g = e.Graphics;

            // 画面中央へのオフセット計算
            int offsetX = (ClientSize.Width - columns * cellSize) / 2;
            int offsetY = (ClientSize.Height - rows * cellSize) / 2;

            // 盤面の描画
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // 各セルの位置を計算
                    int x = offsetX + col * cellSize;
                    int y = offsetY + row * cellSize;

                    if (gridImgs[row, col] != null)
                    {
                        g.DrawImage(gridImgs[row, col], x, y, cellSize, cellSize);
                    }
                    else if (grid[row, col] == 1)
                    {
                        // 壁や底の部分
                        g.DrawImage(blockImg, x, y, cellSize, cellSize);
                    }

                    // セルの枠線を描画
                    g.DrawRectangle(Pens.Black, x, y, cellSize, cellSize);
                }
            }

            // ゲーム開始演出を描画
            if (isReadyState)
            {
                DrawCenteredText(g, "READY", Color.White, 48);
            }
            else if (isGoState)
            {
                DrawCenteredText(g, "GO!", Color.Red, 48);
                AudioManager.Instance.PlaySound(AudioManager.Instance.startSE);
            }
            else if (isGameRunning)
            {
                // 通常のゲーム描画処理
                DrawGame(g, offsetX, offsetY);
            }
        }

        /// <summary>
        /// 中央にテキストを描画
        /// </summary>
        private void DrawCenteredText(Graphics g, string text, Color color, int fontSize)
        {
            using (Font font = new Font("Arial", fontSize, FontStyle.Bold))
            using (Brush brush = new SolidBrush(color))
            {
                SizeF textSize = g.MeasureString(text, font);
                float x = (ClientSize.Width - textSize.Width) / 2;
                float y = (ClientSize.Height - textSize.Height) / 2;
                g.DrawString(text, font, brush, x, y);
            }
        }

        /// <summary>
        /// ゲーム描画処理
        /// </summary>
        /// <param name="g">描画用のオブジェクト</param>
        /// <param name="offsetX">描画位置のXオフセット</param>
        /// <param name="offsetY">描画位置のYオフセット</param>
        private void DrawGame(Graphics g, int offsetX, int offsetY)
        {
            // 現在のテトリミノの描画
            DrawTetromino(g, offsetX, offsetY);

            // 落下予測地点を描画
            int predictionRow = GetDropPredictionRow();
            DrawPredictedTetromino(g, offsetX, offsetY, predictionRow);
        }

        /// <summary>
        /// テトリミノを描画する処理
        /// </summary>
        /// <param name="g">描画用のオブジェクト</param>
        /// <param name="offsetX">描画位置のXオフセット</param>
        /// <param name="offsetY">描画位置のYオフセット</param>
        private void DrawTetromino(Graphics g, int offsetX, int offsetY)
        {
            // 現在のテトリミノの形状に基づいて描画
            for (int row = 0; row < currentTetromino.shape.GetLength(0); row++)
            {
                for (int col = 0; col < currentTetromino.shape.GetLength(1); col++)
                {
                    if (currentTetromino.shape[row, col] == 1)
                    {
                        int x = offsetX + (currentTetromino.col + col) * cellSize;
                        int y = offsetY + (currentTetromino.row + row) * cellSize;

                        // テトリミノの画像を指定した位置に描画
                        g.DrawImage(currentTetromino.blockImg, x, y, cellSize, cellSize);
                    }
                }
            }
        }

        /// <summary>
        /// 落下予測地点にテトリミノを描画
        /// </summary>
        /// <param name="g">描画用のオブジェクト</param>
        /// <param name="offsetX">描画位置のXオフセット</param>
        /// <param name="offsetY">描画位置のYオフセット</param>
        /// <param name="predictionRow">落下予測地点の行</param>
        private void DrawPredictedTetromino(Graphics g, int offsetX, int offsetY, int predictionRow)
        {
            for (int row = 0; row < currentTetromino.shape.GetLength(0); row++)
            {
                for (int col = 0; col < currentTetromino.shape.GetLength(1); col++)
                {
                    if (currentTetromino.shape[row, col] == 1)
                    {
                        int x = offsetX + (currentTetromino.col + col) * cellSize;
                        int y = offsetY + (predictionRow + row) * cellSize;

                        // 半透明の色で描画
                        using (Brush brush = new SolidBrush(Color.FromArgb(100, Color.Gray)))
                        {
                            g.FillRectangle(brush, x, y, cellSize, cellSize);
                        }

                        // 枠線を描画
                        g.DrawRectangle(Pens.Gray, x, y, cellSize, cellSize);
                    }
                }
            }
        }

        /// <summary>
        /// 次のテトリミノのパネル上に文字を描画
        /// </summary>
        /// -<param name="e">描画のイベント</param>
        private void NextTetrominoPanel(object sender, PaintEventArgs e)
        {
            using (Font font = new Font("Arial", 14, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.White))
            {
                string text = "NEXT";
                SizeF textSize = e.Graphics.MeasureString(text, font);
                PointF textPosition = new PointF((nextTetrominoPanel.Width - textSize.Width) / 2, 5);
                e.Graphics.DrawString(text, font, brush, textPosition);
            }
        }

        /// <summary>
        /// ホールドのパネル上に文字を描画
        /// </summary>
        /// -<param name="e">描画のイベント</param>
        private void HoldTetrominoPanel(object sender, PaintEventArgs e)
        {
            using (Font font = new Font("Arial", 14, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.White))
            {
                string text = "HOLD";
                SizeF textSize = e.Graphics.MeasureString(text, font);
                PointF textPosition = new PointF((nextTetrominoPanel.Width - textSize.Width) / 2, 5);
                e.Graphics.DrawString(text, font, brush, textPosition);
            }
        }


        /// <summary>
        /// ゲーム開始演出を非同期で表示
        /// </summary>
        private async void ShowStartAnimation()
        {
            // 「Ready」を表示
            isReadyState = true;
            Invalidate();
            await Task.Delay(2000);

            // 「GO」を表示
            isReadyState = false;
            isGoState = true;
            Invalidate();
            await Task.Delay(1000);

            // ゲームを開始
            isGoState = false;
            isGameRunning = true;
            Invalidate();

            StartGame();
        }

        /// <summary>
        /// ゲーム終了を画面に表示
        /// </summary>
        private void ShowFinishUI()
        {
            // ラベルを作成
            Label finishLabel = new Label
            {
                Text = "Finish",
                Font = new Font("Arial", 48, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            finishLabel.Location = new Point(
                (ClientSize.Width - finishLabel.PreferredWidth) / 2,
                (ClientSize.Height - finishLabel.PreferredHeight) / 2);

            // ラベルをフォームに追加
            Controls.Add(finishLabel);
            finishLabel.BringToFront();
        }
        #endregion

        #region ゲーム時間関連
        /// <summary>
        /// プレイ時間の計測を開始
        /// </summary>
        private void StartPlayTimeTimer()
        {
            // Timer の初期化
            gameTimeTimer = new Timer
            {
                // 1秒ごとに計測
                Interval = 1000
            };
            gameTimeTimer.Tick += (sender, e) =>
            {
                // 1秒加算
                GameDataManager.Instance.UpdateGameTimer(1);
                UpdatePlayTimeLabel();
            };
            gameTimeTimer.Start();
        }

        /// <summary>
        /// プレイ時間ラベルの更新
        /// </summary>
        private void UpdatePlayTimeLabel()
        {
            Label playTimerLabel = Controls.Find("PlayTimerLabel", true)[0] as Label;
            if (playTimerLabel != null)
            {
                int minutes = GameDataManager.Instance.elapsedSeconds / 60;
                int seconds = GameDataManager.Instance.elapsedSeconds % 60;
                playTimerLabel.Text = $"Time: {minutes:D2}:{seconds:D2}";
            }
        }

        /// <summary>
        /// タイムアタックのタイマー処理
        /// </summary>
        private void StartTimeAttackTimer()
        {
            gameTimeTimer = new Timer();
            gameTimeTimer.Interval = 1000; // 1秒ごとにカウント
            gameTimeTimer.Tick += (sender, e) =>
            {
                // 1秒減算
                GameDataManager.Instance.UpdateGameTimer(-1);
                UpdatePlayTimeLabel();

                if (GameDataManager.Instance.elapsedSeconds <= 0)
                {
                    EndTimeAttack();
                    gameTimeTimer.Stop();
                }
            };
            GameDataManager.Instance.SetGameTimer(timeAttackTimer);
            gameTimeTimer.Start();
        }

        /// <summary>
        /// ゲーム時間を停止させる処理
        /// </summary>
        private void StopTimer()
        {
            dropTimer.Stop();
            gameTimeTimer.Stop();
        }
        #endregion

        #region ライン消し関連
        /// <summary>
        /// ラインを消す処理
        /// </summary>
        private void RemoveLine()
        {
            // 消したラインの数をカウント
            int linesCleared = 0;

            // 最下行はチェックしない
            for (int row = rows - 2; row >= 0; row--)
            {
                bool isFull = true;

                // 行がすべてブロックで埋まっているか確認
                for (int col = 0; col < columns; col++)
                {
                    if (grid[row, col] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }
                if (isFull)
                {
                    // ラインを削除し、上の行を下にシフト
                    for (int r = row; r > 0; r--)
                    {
                        for (int c = 1; c < columns - 1; c++)
                        {
                            grid[r, c] = grid[r - 1, c];
                            gridImgs[r, c] = gridImgs[r - 1, c];
                        }
                    }

                    // 最上行を空にする(壁は除外)
                    for (int col = 1; col < columns - 1; col++)
                    {
                        grid[0, col] = 0;
                        gridImgs[0, col] = null;
                    }

                    // ラインが消えた数をカウント
                    linesCleared++;
                    // インデックスを1行戻す
                    row++;
                }
            }

            if (linesCleared > 0)
            {
                // 消したライン数をカウント
                GameDataManager.Instance.UpdateRemovedLines(linesCleared);
                AudioManager.Instance.PlaySound(AudioManager.Instance.lineClearSE);

                UpdateRemoveLineDisplay();
                // 消したライン数に基づいてスコアを更新
                UpdateScore(linesCleared);
            }
        }

        /// <summary>
        /// 消したライン数を表示する更新する
        /// </summary>
        private void UpdateRemoveLineDisplay()
        {
            Label removeLineLabel = (Label)Controls.Find("RemoveLineLabel", true)[0];
            removeLineLabel.Text = $"Lines: {GameDataManager.Instance.removedLines}";
        }
        #endregion

        #region スコア関連
        /// <summary>
        /// スコアを更新する処理
        /// </summary>
        /// <param name="linesCleared">消したライン数</param>
        private void UpdateScore(int linesCleared)
        {
            // ラインが消えた数に応じてスコアを増加
            // 1ライン消した場合
            if (linesCleared == 1)
            {
                GameDataManager.Instance.UpdateScore(100);
            }
            // 2ライン消した場合 (100x2 + 50)
            else if (linesCleared == 2)
            {
                GameDataManager.Instance.UpdateScore(250);
            }
            // 3ライン消した場合 (100x3 + 150)
            else if (linesCleared == 3)
            {
                GameDataManager.Instance.UpdateScore(450);
            }
            // 4ライン消した場合 (100x4 + 300)
            else if (linesCleared == 4)
            {
                GameDataManager.Instance.UpdateScore(700);
            }

            // スコア表示を更新
            UpdateScoreDisplay();
        }

        /// <summary>
        /// スコア表示を更新する処理
        /// </summary>
        private void UpdateScoreDisplay()
        {
            Label scoreLabel = (Label)Controls.Find("ScoreLabel", true)[0];
            scoreLabel.Text = "Score: " + GameDataManager.Instance.score.ToString();

            // 落下速度を更新
            UpdateSpeedBasedOnScore();
        }

        /// <summary>
        /// ハイスコア表示を更新する
        /// </summary>
        /// <param name="highScore">ハイスコア</param>
        private void UpdateHighScoreDisplay(int highScore)
        {
            Label highScoreLabel = (Label)Controls.Find("HighScoreLabel", true)[0];
            highScoreLabel.Text = "High Score: " + highScore.ToString();
        }

        /// <summary>
        /// スコアに基づいて落下速度を調整
        /// </summary>
        private void UpdateSpeedBasedOnScore()
        {
            // レベルを計算
            int newLevel = GameDataManager.Instance.score / scorePerLevel;

            // レベルが上がった場合のみ速度を調整
            if (newLevel > GameDataManager.Instance.currentLevel)
            {
                GameDataManager.Instance.UpdateLevel(newLevel);

                // 落下速度を更新
                int newSpeed = Math.Max(baseSpeed - GameDataManager.Instance.currentLevel * speedIncrement, maxSpeed);
                GameDataManager.Instance.UpdateSpeed(newSpeed);
                dropTimer.Interval = GameDataManager.Instance.currentSpeed;
            }
        }
        #endregion

        #region キー入力関連        
        /// <summary>
        /// キーボードが押された時の処理
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            inputManager.HandleKeyDown(e);
            Invalidate();
        }

        /// <summary>
        /// キーボードから離れた時の処理
        /// </summary>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            inputManager.HandleKeyUp(e);
        }
        #endregion

        #region テトリミノ関連
        /// <summary>
        /// テトリミノが1マス下に移動できるかを確認する処理
        /// </summary>
        /// <param name="e">イベント</param>
        private void OnDrop(object sender, EventArgs e)
        {
            if (CanMoveDown())
            {
                // テトリミノを一つ下に移動
                currentTetromino.row++;
            }
            else
            {
                // 位置に配置時にスコアを10加算
                GameDataManager.Instance.UpdateScore(10);
                UpdateScoreDisplay();
                PlaceTetrominoOnGrid();
                RemoveLine();
                SpawnTetromino();
            }

            Invalidate();
        }

        /// <summary>
        /// テトリミノを盤面に配置する処理
        /// </summary>
        private void PlaceTetrominoOnGrid()
        {
            // テトリミノの行数
            int shapeRows = currentTetromino.shape.GetLength(0);
            // テトリミノの列数
            int shapeCols = currentTetromino.shape.GetLength(1);

            // 盤面に配置
            for (int row = 0; row < shapeRows; row++)
            {
                for (int col = 0; col < shapeCols; col++)
                {
                    if (currentTetromino.shape[row, col] == 1)
                    {
                        int gridRow = currentTetromino.row + row;
                        int gridCol = currentTetromino.col + col;

                        grid[gridRow, gridCol] = 1;
                        gridImgs[gridRow, gridCol] = currentTetromino.blockImg;
                    }
                }
            }

            AudioManager.Instance.PlaySound(AudioManager.Instance.placeTetrominoSE);
        }

        /// <summary>
        /// テトリミノが1マス下に移動できるかを判定する処理
        /// </summary>
        private bool CanMoveDown()
        {
            int shapeRows = currentTetromino.shape.GetLength(0);
            int shapeCols = currentTetromino.shape.GetLength(1);

            // 下方向への移動が可能か確認
            for (int row = 0; row < shapeRows; row++)
            {
                for (int col = 0; col < shapeCols; col++)
                {
                    if (currentTetromino.shape[row, col] == 1)
                    {
                        int newRow = currentTetromino.row + row + 1;
                        int newCol = currentTetromino.col + col;

                        // 盤面の範囲外または既存のブロックに当たった場合
                        if (newRow >= rows || grid[newRow, newCol] == 1)
                        {
                            // 移動不可能な場合
                            return false;
                        }
                    }
                }
            }
            // 移動可能な場合
            return true;
        }

        /// <summary>
        /// 左に移動できるかの判定する処理
        /// </summary>
        private bool CanMoveLeft()
        {
            // テトリミノの行数
            int shapeRows = currentTetromino.shape.GetLength(0);
            // テトリミノの列数
            int shapeCols = currentTetromino.shape.GetLength(1);

            for (int row = 0; row < shapeRows; row++)
            {
                for (int col = 0; col < shapeCols; col++)
                {
                    if (currentTetromino.shape[row, col] == 1)
                    {
                        int newRow = currentTetromino.row + row;
                        int newCol = currentTetromino.col + col - 1;

                        if (newCol < 0 || grid[newRow, newCol] == 1)
                        {
                            // 左に移動不可能
                            return false;
                        }
                    }
                }
            }
            // 左に移動可能
            return true;
        }

        /// <summary>
        /// 右に移動できるかの判定する処理
        /// </summary>
        private bool CanMoveRight()
        {
            // テトリミノの行数
            int shapeRows = currentTetromino.shape.GetLength(0);
            // テトリミノの列数
            int shapeCols = currentTetromino.shape.GetLength(1);

            for (int row = 0; row < shapeRows; row++)
            {
                for (int col = 0; col < shapeCols; col++)
                {
                    if (currentTetromino.shape[row, col] == 1)
                    {
                        int newRow = currentTetromino.row + row;
                        int newCol = currentTetromino.col + col + 1;

                        if (newCol >= columns || grid[newRow, newCol] == 1)
                        {
                            // 右に移動不可能
                            return false;
                        }
                    }
                }
            }
            // 右に移動可能
            return true;
        }

        /// <summary>
        /// テトリミノを回転させる
        /// </summary>
        private void RotateTetromino()
        {
            // 現在のテトリミノの形状を回転
            int[,] rotatedShape = RotateMatrix(currentTetromino.shape);

            // 回転後に他のブロックや壁と衝突しないか、盤面内に収まるかチェック
            if (CanRotate(rotatedShape, out int adjustedRow, out int adjustedCol))
            {
                // 衝突しなければ形状を更新
                currentTetromino.shape = rotatedShape;
                currentTetromino.row = adjustedRow;
                currentTetromino.col = adjustedCol;
            }

            Invalidate();

            AudioManager.Instance.PlaySound(AudioManager.Instance.rotateSE);
        }

        /// <summary>
        /// 行列を90度回転させる
        /// </summary>
        /// <param name="matrix">回転させる行列</param>
        private int[,] RotateMatrix(int[,] matrix)
        {
            // 行列の行数
            int rows = matrix.GetLength(0);
            // 行列の列数
            int cols = matrix.GetLength(1);
            // 回転後の行列（列と行を入れ替え）
            int[,] rotatedMatrix = new int[cols, rows];

            // 90度時計回りに回転させる
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // 回転後の位置に値をセット
                    rotatedMatrix[col, rows - row - 1] = matrix[row, col];
                }
            }
            // 回転後の行列を返す
            return rotatedMatrix;
        }

        /// <summary>
        /// 回転後の衝突チェックと壁蹴り処理
        /// </summary>
        /// <param name="rotatedShape">回転後のテトリミノの形状</param>
        /// <param name="adjustedRow">衝突を回避するために調整された行の</param>
        /// <param name="adjustedCol">衝突を回避するために調整された列の</param>
        private bool CanRotate(int[,] rotatedShape, out int adjustedRow, out int adjustedCol)
        {
            int[] rowOffsets = { 0, -1, 1, 0 };
            int[] colOffsets = { 0, 0, 0, -1, 1 };

            for (int rowShift = 0; rowShift < rowOffsets.Length; rowShift++)
            {
                for (int colShift = 0; colShift < colOffsets.Length; colShift++)
                {
                    int newRow = currentTetromino.row + rowOffsets[rowShift];
                    int newCol = currentTetromino.col + colOffsets[colShift];

                    // 新しい位置にテトリミノを移動できるか判定する
                    if (CanMoveTo(rotatedShape, newRow, newCol))
                    {
                        adjustedRow = newRow;
                        adjustedCol = newCol;
                        // 回転後に移動可能
                        return true;
                    }
                }
            }

            // 回転不可の場合
            adjustedRow = currentTetromino.row;
            adjustedCol = currentTetromino.col;
            return false;
        }

        /// <summary>
        /// 指定された形状が移動可能かを確認する処理
        /// </summary>
        /// <param name="shape">確認する形状</param>
        /// <param name="targetRow">目標行</param>
        /// <param name="targetCol">目標列</param>
        private bool CanMoveTo(int[,] shape, int targetRow, int targetCol)
        {
            int shapeRows = shape.GetLength(0);
            int shapeCols = shape.GetLength(1);

            // 形状を確認
            for (int row = 0; row < shapeRows; row++)
            {
                for (int col = 0; col < shapeCols; col++)
                {
                    if (shape[row, col] == 1)
                    {
                        int newRow = targetRow + row;
                        int newCol = targetCol + col;

                        // 盤面外か他のブロックと衝突した場合、移動不可
                        if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= columns || grid[newRow, newCol] == 1)
                        {
                            // 移動不可能な場合
                            return false;
                        }
                    }
                }
            }
            // 移動可能の場合
            return true;
        }

        /// <summary>
        /// テトリミノの落下予測地点を計算する
        /// </summary>
        /// <returns>予測地点の行</returns>
        private int GetDropPredictionRow()
        {
            int predictionRow = currentTetromino.row;

            while (true)
            {
                predictionRow++;
                if (!CanMoveTo(currentTetromino.shape, predictionRow, currentTetromino.col))
                {
                    predictionRow--;
                    break;
                }
            }

            return predictionRow;
        }

        /// <summary>
        /// テトリミノをホールドする処理
        /// </summary>
        private void HoldTetromino()
        {
            if (holdTetromino == null)
            {
                // 初めてホールドする場合、現在のテトリミノをホールドエリアに保存
                holdTetromino = currentTetromino;
                // 新しいテトリミノを生成
                currentTetromino = nextTetromino;
                nextTetromino = TetrominoGenerator.CreateRandomTetromino();

                // 次のテトリミノを更新
                UpdateNextTetrominoPreview();
            }
            else
            {
                // 既にホールドされている場合は現在のテトリミノと交換
                Tetromino switchTetromino = holdTetromino;
                holdTetromino = currentTetromino;
                currentTetromino = switchTetromino;
            }

            AudioManager.Instance.PlaySound(AudioManager.Instance.holdSE);

            UpdateHeldTetrominoPreview();
            Invalidate();
        }

        /// <summary>
        /// 現在のテトリミノを更新し、次のテトリミノを生成
        /// </summary>
        private void SpawnTetromino()
        {
            currentTetromino = nextTetromino;
            nextTetromino = TetrominoGenerator.CreateRandomTetromino();

            // プレビューを更新
            UpdateNextTetrominoPreview();

            // ゲームオーバー判定
            if (CheckGameOver() && !GameDataManager.Instance.isGameOver)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// 次に生成されるテトリミノを表示
        /// </summary>
        private void UpdateNextTetrominoPreview()
        {
            Bitmap previewBitmap = new Bitmap(nextTetrominoPanel.Width, nextTetrominoPanel.Height);

            using (Graphics g = Graphics.FromImage(previewBitmap))
            {
                g.Clear(Color.Black);

                int cellSize = 20; // プレビュー用のセルサイズ
                int offsetX = (nextTetrominoPanel.Width - nextTetromino.shape.GetLength(1) * cellSize) / 2;
                int offsetY = (nextTetrominoPanel.Height - nextTetromino.shape.GetLength(0) * cellSize) / 2;

                for (int row = 0; row < nextTetromino.shape.GetLength(0); row++)
                {
                    for (int col = 0; col < nextTetromino.shape.GetLength(1); col++)
                    {
                        if (nextTetromino.shape[row, col] == 1)
                        {
                            int x = offsetX + col * cellSize;
                            int y = offsetY + row * cellSize;
                            g.DrawImage(nextTetromino.blockImg, x, y + 10, cellSize, cellSize);
                        }
                    }
                }
            }

            // プレビューをパネルに反映
            nextTetrominoPanel.BackgroundImage = previewBitmap;
        }

        /// <summary>
        /// ホールドされたテトリミノを表示
        /// </summary>
        private void UpdateHeldTetrominoPreview()
        {
            // ホールドされていない場合は何もしない
            if (holdTetromino == null)
                return;

            Bitmap previewBitmap = new Bitmap(holdTetrominoPanel.Width, holdTetrominoPanel.Height);

            using (Graphics g = Graphics.FromImage(previewBitmap))
            {
                g.Clear(Color.Black);

                int cellSize = 20;
                int offsetX = (holdTetrominoPanel.Width - holdTetromino.shape.GetLength(1) * cellSize) / 2;
                int offsetY = (holdTetrominoPanel.Height - holdTetromino.shape.GetLength(0) * cellSize) / 2;

                // テトリミノの形状に基づいて描画
                for (int row = 0; row < holdTetromino.shape.GetLength(0); row++)
                {
                    for (int col = 0; col < holdTetromino.shape.GetLength(1); col++)
                    {
                        if (holdTetromino.shape[row, col] == 1)
                        {
                            int x = offsetX + col * cellSize;
                            int y = offsetY + row * cellSize;
                            g.DrawImage(holdTetromino.blockImg, x, y + 10, cellSize, cellSize);
                        }
                    }
                }
            }

            // プレビューをパネルに反映
            holdTetrominoPanel.BackgroundImage = previewBitmap;
        }
        #endregion

        #region ポーズやゲームオーバー関連
        /// <summary>
        /// ポーズ状態を解除する処理
        /// </summary>
        private void ResumeGame()
        {
            dropTimer.Start();
            gameTimeTimer.Start();
        }

        /// <summary>
        /// ゲームオーバー処理
        /// </summary>
        private async void TriggerGameOver()
        {
            // ゲームオーバーフラグを立てる
            GameDataManager.Instance.GameOver();
            // ランキング情報を更新
            ScoreManager.Instance.UpdateScoreData(GameDataManager.Instance.score);
            AudioManager.Instance.PlaySound(AudioManager.Instance.finishSE);


            // 操作とタイマーを停止
            StopTimer();
            DisableInput();
            // 「Finish」のUIを表示
            ShowFinishUI();

            // 3秒待機
            await Task.Delay(3000);

            // ゲームオーバー画面に遷移
            onGameOver?.Invoke();
        }

        /// <summary>
        /// ゲームオーバー時に操作を無効化
        /// </summary>
        private void DisableInput()
        {
            // イベントや入力処理を停止
            KeyDown -= OnKeyDown;
            KeyUp -= OnKeyUp;
        }

        /// <summary>
        /// ゲームオーバーかどうかを判定する処理
        /// </summary>
        private bool CheckGameOver()
        {
            int[,] shape = currentTetromino.shape;
            int shapeRows = shape.GetLength(0);
            int shapeCols = shape.GetLength(1);

            // 現在のテトリミノの位置で盤面上部に衝突するか確認
            for (int row = 0; row < shapeRows; row++)
            {
                for (int col = 0; col < shapeCols; col++)
                {
                    if (shape[row, col] == 1)
                    {
                        int boardRow = currentTetromino.row + row;
                        int boardCol = currentTetromino.col + col;

                        // 盤面上部に衝突した場合、ゲームオーバー
                        if (boardRow >= 0 && grid[boardRow, boardCol] == 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// タイムアタック終了時の処理
        /// </summary>
        private void EndTimeAttack()
        {
            // ゲームオーバー
            if (!GameDataManager.Instance.isGameOver)
            {
                TriggerGameOver();
            }
        }
        #endregion
    }
}