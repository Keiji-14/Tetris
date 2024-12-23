using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// 画像を読み込む処理
    /// </summary>
    public class ImageLoader
    {
        /// <summary>
        /// 指定された名前の画像を読み込む処理
        /// </summary>
        /// <param name="imageName">読み込む画像ファイルの名前</param>
        public static Image LoadImage(string imageName)
        {
            // 実行ファイルのディレクトリを取得
            string exeDirectory = Application.StartupPath;

            // 画像フォルダのパスを作成
            string imagesDirectory = Path.Combine(exeDirectory, "Image");

            // 画像ファイルのパスを作成
            string imagePath = Path.Combine(imagesDirectory, imageName);

            if (File.Exists(imagePath))
            {
                // ファイルが存在する場合、画像を読み込んで返す
                return Image.FromFile(imagePath);
            }
            else
            {
                // ファイルが存在しない場合、例外をスロー
                throw new FileNotFoundException($"画像ファイル '{imageName}' が見つかりません。");
            }
        }
    }
}