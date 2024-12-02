using System;
using System.IO;
using Colossal.PSI.Environment;

namespace StationEntranceVisuals.Utils;

public abstract class FileUtils
{
     
     private static readonly string WePath = Path.Combine(
          EnvPath.kUserDataPath,
          "ModsData",
          "Klyte45Mods",
          "WriteEverywhere"
     );
     
     private static readonly string WeImagesDirectory = Path.Combine(
          WePath,
          "imageAtlases",
          "_StationEntranceVisuals"
     );
     
     private static readonly string WeLayoutsDirectory = Path.Combine(
          WePath,
          "layouts",
          "_StationEntranceVisuals"
     );
     
     private static readonly string OldWeImagesDirectory = Path.Combine(
          WePath,
          "imageAtlases",
          "StationEntranceVisuals"
     );
     
     private static readonly string OldWeLayoutsDirectory = Path.Combine(
          WePath,
          "layouts",
          "StationEntranceVisuals"
     );
     
     public static void DeleteOldFiles()
     {
          try
          {
               DeleteIfExists(OldWeImagesDirectory);
               DeleteIfExists(OldWeLayoutsDirectory);
               DeleteIfExists(WeImagesDirectory);
               DeleteIfExists(WeLayoutsDirectory);
          }
          catch (Exception e)
          {
               Mod.log.Info(e.Message);
          }
     }
     
     private static void DeleteIfExists(string path)
     {
          var folderExists = Directory.Exists(path);
          if (folderExists)
               Directory.Delete(path, true);
     }
}