﻿using CefSharp;
using CefSharp.WinForms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace minerstat {
static class Program {
   // minerstat sync
   public static string lastupdate;
   public static int totalByte;
   public static int totalTraffic;
   public static string suffix;
   public static int watchDogFailover;
   public static int watchDogFailoverCpu;

   // Open hardware monitor
   public static int monitorport;

   // minerstat Auth
   public static string loginjson;
   public static string token;
   public static string worker;

   // minerstat Direcories
   public static string currentDir, currentIP;
   public static string tempDir;
   public static string minerstatDir;

   // minerstat Display Console
   public static List<string> Message = new List<string> ();
   public static void update(string last)
   {
      lastupdate = last;
   }

   // Internet Speed
   public static System.Net.WebClient wcc;
   public static DateTime dt1c;
   public static byte[] datac;
   public static DateTime dt2c;
   public static double connectionspeed;
   public static Boolean connectionError;
   public static Nullable<bool> prevConnectionError;

   // Timers
   public static System.Timers.Timer watchDogs;
   public static System.Timers.Timer syncLoop;
   public static System.Timers.Timer crashLoop;
   public static System.Timers.Timer offlineLoop;
   public static System.Timers.Timer bufferProtection;
   public static Boolean SyncStatus;

   // Resources
   static string lib, browser, locales, res;

   // Start on Windows Protection
   public static int StartDelay;
   public static Boolean StartDelayOver;

   [STAThread]
   static void Main(string[] args)
   {
      if (args.Length == 0)
      {
         MessageBox.Show("ERROR => Please, Start with minerstat.exe");
         Application.Exit();
      }
      else
      {
         // Assigning file paths to varialbles
         lib     = Program.currentDir + @"resources\libcef.dll";
         browser = Program.currentDir + @"resources\CefSharp.BrowserSubprocess.exe";
         locales = Program.currentDir + @"resources\locales\";
         //res = Program.currentDir + @"resources\";

         var  libraryLoader = new CefLibraryHandle(lib);
         bool isValid       = !libraryLoader.IsInvalid;
         libraryLoader.Dispose();

         var settings = new CefSettings();
         settings.BrowserSubprocessPath = browser;
         settings.LocalesDirPath        = locales;
         settings.ResourcesDirPath      = res;
         settings.SetOffScreenRenderingBestPerformanceArgs();
         settings.WindowlessRenderingEnabled = true;
         Cef.Initialize(settings);

         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);

         // Edit registry
         // Disables Windows Error messages like: Ethdcrminer64 stopped working ..
         // minerstat Watchdog will be able to restart the process without a notice
         // NOTICE: if not works, Click on /misc/minerfix.reg
         try
         {
            var rk = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\Windows Error Reporting");
            rk.CreateSubKey("DontShowUI", RegistryKeyPermissionCheck.Default);
            rk.SetValue("DontShowUI", 1);
         }
         catch (Exception) { }

         // SET Global Varibles
         //currentDir = System.Environment.CurrentDirectory;
         currentDir     = AppDomain.CurrentDomain.BaseDirectory;
         tempDir        = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
         minerstatDir   = tempDir + "/minerstat";
         suffix         = "byte";
         totalTraffic   = 0;
         StartDelayOver = true;
         currentIP      = "0.0.0.0";

         // Open hardware monitor
         Random random = new Random();
         monitorport = random.Next(8600, 8700);

         // Initalize Watchdog
         watchDogs           = new System.Timers.Timer(TimeSpan.FromSeconds(25).TotalMilliseconds); // set the time (25 sec in this case)
         watchDogs.AutoReset = true;
         watchDogs.Elapsed  += new System.Timers.ElapsedEventHandler(watchDog.health);
         watchDogFailover    = 0;

         // Initalize Syncing
         syncLoop           = new System.Timers.Timer(TimeSpan.FromSeconds(20).TotalMilliseconds); // set the time (19 sec in this case)
         syncLoop.AutoReset = true;
         syncLoop.Elapsed  += new System.Timers.ElapsedEventHandler(sync.loop);

         // Double Crash Protection
         crashLoop           = new System.Timers.Timer(TimeSpan.FromSeconds(1800).TotalMilliseconds); // set the time (30 min in this case)
         crashLoop.AutoReset = true;
         crashLoop.Elapsed  += new System.Timers.ElapsedEventHandler(crash);

         // Offline Events
         prevConnectionError   = null;
         offlineLoop           = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds); // set the time (10 sec in this case)
         offlineLoop.AutoReset = true;
         offlineLoop.Elapsed  += new System.Timers.ElapsedEventHandler(offline.protect);
         offlineLoop.Start();

         // Buffer Protection
         // Windows like to close the application on "long run" without Exception or any notice
         bufferProtection           = new System.Timers.Timer(TimeSpan.FromSeconds(14400).TotalMilliseconds); // set the time (4 hours in this case)
         bufferProtection.AutoReset = true;
         bufferProtection.Elapsed  += new System.Timers.ElapsedEventHandler(buffer.Protection);
         bufferProtection.Start();

         // Check update folder
         if (Directory.Exists(currentDir + "/update/"))
         {
            if (File.Exists(currentDir + "/update/minerstat.exe"))
            {
               try
               {
                  File.Delete("minerstat.exe");
                  File.Copy("update/minerstat.exe", "minerstat.exe");

                  Directory.Delete(currentDir + "/update/", true);
               }
               catch (Exception ex) {  }
            }
         }

         // Start on Windows Double miner start Protection
         if (args.Contains("startWithWindows"))
         {
            StartDelayOver = false;
            StartDelay     = 10000;
         }

         // RUN UX
         SyncStatus = false;
         Application.Run(new Form1());
      }
   }

   public static void crash(object sender, ElapsedEventArgs exw)
   {
   }

   public static void NewMessage(string text, string type)
   {
      try {
         if (Message.Count >= 15)
         {
            Message.RemoveAt(0);
         }

         String hourMinute;

         hourMinute = DateTime.Now.ToString("HH:mm:ss");

         if (type.Equals("WARNING"))
         {
            Message.Add(("<span>[" + hourMinute + "] ") + text + "</span> <br>");
         }
         else
         {
            if (type.Equals("ERROR"))
            {
               Message.Add(("<span class='error'>[" + hourMinute + "] ") + text + "</span> <br>");
            }
            else
            {
               Message.Add(("[" + hourMinute + "] ") + text + " <br>");
            }
         }
      }
      catch (Exception) {
      }
   }
}
}
