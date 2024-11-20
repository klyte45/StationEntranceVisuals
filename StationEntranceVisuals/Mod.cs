using System.ComponentModel;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using StationEntranceVisuals.Utils;

namespace StationEntranceVisuals
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(StationEntranceVisuals)}.{nameof(Mod)}").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad)); 

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");
            
            var bw = new BackgroundWorker();
            bw.DoWork += CopyFiles;
            bw.RunWorkerAsync();
        }

        private static void CopyFiles(object sender, DoWorkEventArgs e)
        {
            FileUtils.CopyFiles();
        }  

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }
}
