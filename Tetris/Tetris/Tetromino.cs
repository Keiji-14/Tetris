using System;
using System.Drawing;

namespace Tetris
{
    /// <summary>
    /// テトリミノの管理
    /// </summary>
    public class Tetromino
    {
        /// <summary>テトリミノの形状</summary>
        public int[,] shape;
        /// <summary>現在の行位置</summary>
        public int row;
        /// <summary>現在の列位置</summary>
        public int col;
        /// <summary>テトリミノの画像</summary>
        public Image blockImg;

        /// <summary>
        /// テトリミノの初期化
        /// </summary>
        /// <param name="shape">テトリミノの形状</param>
        /// <param name="row">行位置</param>
        /// <param name="col">列位置</param>
        /// <param name="imagePath">ブロック画像のファイルパス</param>
        public Tetromino(int[,] shape, int row, int col, string imagePath)
        {
            this.shape = shape;
            this.row = row;
            this.col = col;
            blockImg = ImageLoader.LoadImage(imagePath);
        }
    }

    /// <summary>
    /// テトリミノを生成する処理
    /// </summary>
    public class TetrominoGenerator
    {
        /// <summary>ランダム生成に使用するインスタンス</summary>
        private static Random random = new Random();
        // <summary>各テトリミノの画像ファイル名リスト</summary>
        private static readonly string[] blockImgs = {
            "I_block.png", "O_block.png", "T_block.png", "L_block.png", "J_block.png", "S_block.png", "Z_block.png"
        };
        /// <summary>前回生成したテトリミノの種類</summary>
        private static int? previousTetrominoType = null;

        /// <summary>
        /// ランダムなテトリミノを生成する
        /// </summary>
        public static Tetromino CreateRandomTetromino()
        {
            int tetrominoType;

            // ランダム生成で、前回と異なるテトリミノを選ぶ
            do
            {
                tetrominoType = random.Next(0, blockImgs.Length);
            } while (previousTetrominoType.HasValue && tetrominoType == previousTetrominoType.Value);

            // 現在の種類を記録
            previousTetrominoType = tetrominoType;
            int[,] shape = null;

            switch (tetrominoType)
            {
                case 0: // Iブロック
                    shape = new int[,]
                    {
                        { 0, 0, 0, 0 },
                        { 1, 1, 1, 1 },
                        { 0, 0, 0, 0 },
                        { 0, 0, 0, 0 }
                    };
                    break;

                case 1: // Oブロック
                    shape = new int[,]
                    {
                        { 0, 0, 0, 0 },
                        { 0, 1, 1, 0 },
                        { 0, 1, 1, 0 },
                        { 0, 0, 0, 0 }
                    };
                    break;

                case 2: // Tブロック
                    shape = new int[,]
                    {
                        { 0, 0, 0 },
                        { 1, 1, 1 },
                        { 0, 1, 0 }
                    };
                    break;

                case 3: // Lブロック
                    shape = new int[,]
                    {
                        { 0, 0, 0 },
                        { 1, 1, 1 },
                        { 0, 0, 1 }
                    };
                    break;

                case 4: // Jブロック
                    shape = new int[,]
                    {
                        { 0, 0, 0 },
                        { 1, 1, 1 },
                        { 1, 0, 0 }
                    };
                    break;

                case 5: // Sブロック
                    shape = new int[,]
                    {
                        { 0, 0, 0 },
                        { 0, 1, 1 },
                        { 1, 1, 0 }
                    };
                    break;

                case 6: // Zブロック
                    shape = new int[,]
                    {
                        { 0, 0, 0 },
                        { 1, 1, 0 },
                        { 0, 1, 1 }
                    };
                    break;
            }

            // グリッド上での初期位置を計算
            int initialRow = shape.GetLength(0) == 4 ? 0 : -1;
            int initialCol = shape.GetLength(1) == 4 ? 3 : 5;

            // テトリミノを生成して返す
            return new Tetromino(shape, initialRow, initialCol, blockImgs[tetrominoType]);
        }
    }
}
