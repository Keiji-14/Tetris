using System;
using System.IO;
using System.Media;
using System.Windows.Forms;
using WMPLib;

namespace Tetris
{
    /// <summary>
    /// オーディオを管理するクラス
    /// </summary>
    public class AudioManager
    {
        /// <summary>オーディオを管理するインスタンス</summary>
        private static AudioManager instance = null;
        /// <summary>BGMを管理するWindows Media Playerインスタンス</summary>
        private static WindowsMediaPlayer bgmPlayer;
        /// <summary>カーソル切り替えの効果音</summary>
        public string cursorSE { get; private set; } = "CursorSE.wav";
        /// <summary>ライン消去の効果音</summary>
        public string lineClearSE { get; private set; } = "LineClearSE.wav";
        /// <summary>テトリミノ回転の効果音</summary>
        public string rotateSE { get; private set; } = "RotateSE.wav";
        /// <summary>テトリミノをホールド時の効果音</summary>
        public string holdSE { get; private set; } = "HoldSE.wav";
        /// <summary>テトリミノ配置の効果音</summary>
        public string placeTetrominoSE { get; private set; } = "PlaceSE.wav";
        /// <summary>ゲーム開始時の効果音</summary>
        public string startSE { get; private set; } = "StartSE.wav";
        /// <summary>ゲーム終了時の効果音</summary>
        public string finishSE { get; private set; } = "FinishSE.wav";
        /// <summary>BGM</summary>
        public string bgm { get; private set; } = "BGM.wav";

        /// <summary>
        /// ゲームの状態のシングルトン化
        /// </summary>
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AudioManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// 効果音を再生する
        /// </summary>
        /// <param name="soundFilePath">再生する音源ファイルのパス</param>
        public void PlaySound(string soundFilePath)
        {
            // 実行ファイルのディレクトリを取得
            string exeDirectory = Application.StartupPath;

            // 音源フォルダのパスを作成
            string audiosDirectory = Path.Combine(exeDirectory, "Audio");

            // 音源ファイルのパスを作成
            string audioPath = Path.Combine(audiosDirectory, soundFilePath);

            if (File.Exists(audioPath))
            {
                try
                {
                    using (SoundPlayer player = new SoundPlayer(audioPath)) // 正しいパスを渡す
                    {
                        player.Play();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("音声の再生に失敗しました: " + ex.Message);
                }
            }
            else
            {
                // ファイルが存在しない場合、例外をスロー
                throw new FileNotFoundException($"音源ファイル '{audioPath}' が見つかりません。");
            }
        }

        /// <summary>
        /// BGMを再生する
        /// </summary>
        /// <param name="bgmFilePath">再生する音源のファイルパス</param>
        public void PlayBGM(string soundFilePath)
        {
            // 実行ファイルのディレクトリを取得
            string exeDirectory = Application.StartupPath;

            // 音源フォルダのパスを作成
            string audiosDirectory = Path.Combine(exeDirectory, "Audio");

            // 音源ファイルのパスを作成
            string audioPath = Path.Combine(audiosDirectory, soundFilePath);

            if (File.Exists(audioPath))
            {
                // 初期化
                if (bgmPlayer == null)
                {
                    bgmPlayer = new WindowsMediaPlayer();
                    // ループ設定
                    bgmPlayer.settings.setMode("loop", true);
                }

                // 音源ファイルのパスを設定
                bgmPlayer.URL = audioPath;
                // 音量を30%に設定
                bgmPlayer.settings.volume = 20;

                bgmPlayer.controls.play();
            }
            else
            {
                MessageBox.Show($"音源ファイルが見つかりません: {audioPath}");
            }
        }

        /// <summary>
        /// BGMを停止する
        /// </summary>
        public void StopBGM()
        {
            bgmPlayer?.controls.stop();
        }
    }
}