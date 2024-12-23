namespace Tetris
{
    /// <summary>
    /// ゲームモードの種類
    /// </summary>
    public enum GameMode
    {
        Normal,
        TimeAttack
    }

    /// <summary>
    /// ゲームの状態を管理するクラス
    /// </summary>
    public class GameDataManager
    {
        /// <summary>ゲームの状態のインスタンス</summary>
        private static GameDataManager instance = null;
        /// <summary>スコアを保持する変数</summary>
        public int score { get; private set; }
        /// <summary>現在の落下スピード</summary>
        public int currentSpeed { get; private set; }
        /// <summary>現在レベル</summary>
        public int currentLevel { get; private set; }
        /// <summary>ラインを消した本数</summary>
        public int removedLines { get; private set; }
        /// <summary>秒単位の経過時間</summary>
        public int elapsedSeconds { get; private set; }
        /// <summary>ゲームオーバーかどうかを判定するフラグ</summary>
        public bool isGameOver { get; private set; }
        /// <summary>選択したのゲームモード</summary>
        public GameMode gameMode { get; private set; }

        /// <summary>
        /// ゲームの状態のシングルトン化
        /// </summary>
        public static GameDataManager Instance
        {
            get
            { 
                if (instance == null)
                {
                    instance = new GameDataManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// ゲームの状態を初期化
        /// </summary>
        public void InitializeGameData()
        {
            score = 0;
            currentSpeed = 0;
            currentLevel = 0;
            removedLines = 0;
            elapsedSeconds = 0;
            isGameOver = false;
        }

        /// <summary>
        /// ゲームモードを設定
        /// </summary>
        /// <param name="setGameMode">設定するゲームモード</param>
        public void SetGameMode(GameMode setGameMode)
        {
            gameMode = setGameMode;
        }

        /// <summary>
        /// ゲーム時間を設定
        /// </summary>
        /// <param name="setTimer">設定するゲーム時間</param>
        public void SetGameTimer(int setTimer)
        {
            elapsedSeconds = setTimer;
        }

        /// <summary>
        /// ゲーム時間を更新する
        /// </summary>
        /// <param name="getScore">更新するゲーム時間</param>
        public void UpdateGameTimer(int updateTime)
        {
            elapsedSeconds += updateTime;
        }

        /// <summary>
        /// ゲーム中のスコアを更新する
        /// </summary>
        /// <param name="getScore">獲得したスコア</param>
        public void UpdateScore(int getScore)
        {
            score += getScore;
        }

        /// <summary>
        /// ゲーム中の消したライン数を更新する
        /// </summary>
        /// <param name="linesCleared">消したライン数</param>
        public void UpdateRemovedLines(int linesCleared)
        {
            removedLines += linesCleared;
        }

        /// <summary>
        /// ゲーム中のレベルを更新する
        /// </summary>
        /// <param name="updateLevel">更新後のレベル</param>
        public void UpdateLevel(int updateLevel)
        {
            currentLevel = updateLevel;
        }

        /// <summary>
        /// ゲーム中のテトリミノの落下速度を更新する
        /// </summary>
        /// <param name="updateSpeed">更新後のスピード</param>
        public void UpdateSpeed(int updateSpeed)
        {
            currentSpeed = updateSpeed;
        }

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        public void GameOver()
        {
            isGameOver = true;
        }
    }
}