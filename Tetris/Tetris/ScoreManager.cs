using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// スコアを管理するクラス
    /// </summary>
    public class ScoreManager
    {
        /// <summary>ゲームの状態のインスタンス</summary>
        private static ScoreManager instance = null;
        /// <summary>ランキング最大数</summary>
        private const int maxRankingSize = 5;
        /// <summary>スコア情報のファイルパス</summary>
        private const string saveFilePath = "score.json";
        /// <summary>ノーマルモードのハイスコア</summary>
        public int normalHighScore { get; private set; }
        /// <summary>ノーマルモードのランキング</summary>
        public List<int> normalRanking { get; private set; } = new List<int>();
        /// <summary>タイムアタックモードのハイスコア</summary>
        public int timeAttackHighScore { get; private set; }
        /// <summary>タイムアタックモードのランキング</summary>
        public List<int> timeAttackRanking { get; private set; } = new List<int>();

        /// <summary>
        /// JSONデータに使用するクラス
        /// </summary>
        private class SaveData
        {
            public int NormalHighScore { get; set; }
            public List<int> NormalRanking { get; set; }
            public int TimeAttackHighScore { get; set; }
            public List<int> TimeAttackRanking { get; set; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ScoreManager()
        {
            LoadData();
        }

        /// <summary>
        /// スコアの状態のシングルトン化
        /// </summary>
        public static ScoreManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScoreManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// スコア情報を更新する
        /// </summary>
        public void UpdateScoreData(int score)
        {
            switch (GameDataManager.Instance.gameMode)
            {
                case GameMode.Normal:
                    // ノーマルモードのランキングにスコアを追加してソート
                    normalRanking.Add(score);
                    normalRanking = normalRanking.OrderByDescending(s => s).Take(maxRankingSize).ToList();
                    // ノーマルモードのハイスコアの更新
                    normalHighScore = normalRanking.First();
                    break;
                case GameMode.TimeAttack:
                    // ノーマルモードのランキングにスコアを追加してソート
                    timeAttackRanking.Add(score);
                    timeAttackRanking = timeAttackRanking.OrderByDescending(s => s).Take(maxRankingSize).ToList();
                    // ノーマルモードのハイスコアの更新
                    timeAttackHighScore = timeAttackRanking.First();
                    break;
            }

            // データを保存
            SaveScoreData();
        }

        /// <summary>
        /// スコア情報を保存する
        /// </summary>
        private void SaveScoreData()
        {
            try
            {
                var saveData = new SaveData
                {
                    NormalHighScore = normalHighScore,
                    NormalRanking = normalRanking,
                    TimeAttackHighScore = timeAttackHighScore,
                    TimeAttackRanking = timeAttackRanking
                };
                string json = JsonSerializer.Serialize(saveData);
                File.WriteAllText(saveFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("スコア情報の保存に失敗しました: " + ex.Message);
            }
        }

        /// <summary>
        /// ランキングを読み込む
        /// </summary>
        public void LoadData()
        {
            if (File.Exists(saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    var loadData = JsonSerializer.Deserialize<SaveData>(json);
                    // 読み込んだデータを設定
                    normalHighScore = loadData.NormalHighScore;
                    normalRanking = loadData.NormalRanking ?? new List<int>();
                    timeAttackHighScore = loadData.TimeAttackHighScore;
                    timeAttackRanking = loadData.TimeAttackRanking ?? new List<int>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ランキングデータの読み込みに失敗しました: " + ex.Message);
                    InitializeDefaultScores();
                }
            }
            else
            {
                InitializeDefaultScores();
            }
        }

        /// <summary>
        /// ランキングの初期化
        /// </summary>
        public void ResetRanking()
        {
            InitializeDefaultScores();
            SaveScoreData();
        }

        /// <summary>
        /// スコア情報を初期化する
        /// </summary>
        private void InitializeDefaultScores()
        {
            normalHighScore = 0;
            normalRanking = new List<int>();
            timeAttackHighScore = 0;
            timeAttackRanking = new List<int>();
        }
    }
}