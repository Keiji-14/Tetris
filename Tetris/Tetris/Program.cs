using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// アプリケーションのエントリーポイント
    /// </summary>
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン
        /// </summary>
        static void Main()
        {
            // アセンブリ解決イベントの設定
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            // アプリケーションの起動とフォーム表示
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // メインフォームを起動
            Application.Run(new MainForm());
        }

        /// <summary>
        /// アセンブリ解決時に呼ばれるイベントハンドラ
        /// </summary>
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // 解決するアセンブリの名前を取得
            string assemblyName = new AssemblyName(args.Name).Name + ".dll";

            // Dependencies フォルダのパスを取得
            string dependenciesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies");

            // アセンブリのフルパスを作成
            string assemblyPath = Path.Combine(dependenciesPath, assemblyName);

            // アセンブリが存在する場合にロード
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }

            // アセンブリが見つからない場合は null を返す
            return null;
        }
    }
}