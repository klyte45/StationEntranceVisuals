using System;
using System.IO;
using System.Linq;
using Colossal.PSI.Environment;

namespace StationEntranceVisuals.Utils;

public static class FileUtils
{

     private static readonly string ModId = "94028";
     
     private static readonly string ModName = "StationEntranceVisuals";
     
     private static readonly string ModFilesVersion = "3";

     private static readonly string VersionFileName = "version.txt";
     
     private static readonly string LocalModPath = Path.Combine(
          EnvPath.kCacheDataPath,
          "Mods",
          "mods_subscribed"
     );
     
     private static readonly string DebugModPath = Path.Combine(
          EnvPath.kLocalModsPath
     );
     
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
     
     public static void CopyFiles()
     {
          try
          {
               // Creating folder for images
               CreateIfMissing(WeImagesDirectory);
               
               // Creating folder for layouts
               CreateIfMissing(WeLayoutsDirectory);

               // Delete old folder name
               DeleteIfExists(OldWeImagesDirectory);
               DeleteIfExists(OldWeLayoutsDirectory);
               
               var directories = Directory.GetDirectories(LocalModPath, ModId + "_*");
               string modPath;
               if (directories.Length > 0)
               {
                    modPath = directories.OrderByDescending(item => item.Split('_').Last()).First();
               }
               else
               {
                    modPath = Path.Combine(
                         DebugModPath,
                         ModName
                    );
               }
               
               var createdImageVersion = CreateVersion(WeImagesDirectory, ModFilesVersion);
               
               var createdLayoutVersion = CreateVersion(WeLayoutsDirectory, ModFilesVersion);

               var localImagesDirectory = Path.Combine(
                    modPath,
                    "weImageAtlases"
               );
               
               var localLayoutsDirectory = Path.Combine(
                    modPath,
                    "weLayouts"
               );

               if (createdImageVersion)
               {
                    ClearFolder(WeImagesDirectory);
                    var imagesPaths = Directory.GetFiles(localImagesDirectory);
                    foreach (var imageFile in imagesPaths)
                    {
                         var weFile = Path.Combine(WeImagesDirectory, Path.GetFileName(imageFile));
                         File.Copy(imageFile, weFile, true);
                    }
               }

               if (createdLayoutVersion)
               {
                    ClearFolder(WeLayoutsDirectory);
                    var layoutsPaths = Directory.GetFiles(localLayoutsDirectory);
                    foreach (var layoutFile in layoutsPaths)
                    {
                         var weFile = Path.Combine(WeLayoutsDirectory, Path.GetFileName(layoutFile));
                         File.Copy(layoutFile, weFile, true);
                    }  
               }
          }
          catch (Exception e)
          {
               Mod.log.Info(e.Message);
          }
     }

     private static bool CreateVersion(string path, string version)
     {
          var file = Path.Combine(path, VersionFileName);
          if (File.Exists(file))
          {
               var text = File.ReadAllText(file);
               if (text != version)
               {
                    WriteVersion(file, version);
               }
               return text != version;
          }
          else
          {
               WriteVersion(file, version);
               return true;
          }
     }

     private static void WriteVersion(string file, string version)
     {
          try
          {
               File.WriteAllText(file, version);
          }
          catch (Exception e)
          {
               Mod.log.Error(e, $"Failed to write to version file");
          }
     }
     
     private static void ClearFolder(string folderName)
     {
          foreach (var file in Directory.GetFiles(folderName))
          {
               if (!file.Contains(VersionFileName))
               {
                    File.Delete(file);
               }
          }
     }
     
     private static void CreateIfMissing(string path)
     {
          var folderExists = Directory.Exists(path);
          if (!folderExists)
               Directory.CreateDirectory(path);
     }
     
     private static void DeleteIfExists(string path)
     {
          var folderExists = Directory.Exists(path);
          if (folderExists)
               Directory.Delete(path, true);
     }
}