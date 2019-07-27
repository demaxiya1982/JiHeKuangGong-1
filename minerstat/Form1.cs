﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;


namespace minerstat
{
public partial class Form1 : Form
{
   public ChromiumWebBrowser chromeBrowser;
   DropShadow ds = new DropShadow();


   public Form1()
   {
      InitializeComponent();

      try
      {
         if (File.Exists(@Program.currentDir + "/asset/user.json"))
         {
            string json = File.ReadAllText(@Program.currentDir + "/asset/user.json");
            Program.loginjson = json;

            var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);
            Program.token  = (string)jObject["token"];
            Program.worker = (string)jObject["worker"];

            if (!Directory.Exists(Program.minerstatDir))
            {
               Directory.CreateDirectory(Program.minerstatDir);
            }

            if (!File.Exists(@Program.minerstatDir + "/user.json"))
            {
               File.WriteAllText(@Program.minerstatDir + "/user.json", json);
            }
         }
         else
         {
            try
            {
               if (File.Exists(@Program.minerstatDir + "/user.json"))
               {
                  string json = File.ReadAllText(@Program.minerstatDir + "/user.json");
                  Program.loginjson = json;

                  var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                  Program.token  = (string)jObject["token"];
                  Program.worker = (string)jObject["worker"];

                  if (!File.Exists(@Program.currentDir + "/asset/user.json"))
                  {
                     File.WriteAllText(@Program.currentDir + "/asset/user.json", json);
                  }
               }
            }
            catch (Exception issue)
            {
               File.Delete(@Program.minerstatDir + "/user.json");
               Application.Restart();
            }
         }
      }
      catch (Exception err)
      {
         File.Delete(@Program.currentDir + "/asset/user.json");
         Application.Restart();
      }

      InitializeChromium();
      chromeBrowser.RegisterJsObject("doFrame", new mainFrame(chromeBrowser, this));
      chromeBrowser.RegisterJsObject("mining", new mining(this));
      this.Resize          += new EventHandler(Form1_Resize);
      this.LocationChanged += new EventHandler(Form1_Resize);

      bunifuDragControl1.TargetControl = chromeBrowser;
   }

   void Form1_Shown()
   {
      Rectangle rc = this.Bounds;

      rc.Inflate(2, 2);
      ds.Bounds = rc;
      ds.Show();
      this.BringToFront();
   }

   void Form1_Resize(object sender, EventArgs e)
   {
      ds.Visible = (this.WindowState == FormWindowState.Normal);
      if (ds.Visible)
      {
         Rectangle rc = this.Bounds;
         rc.Inflate(2, 2);
         ds.Bounds = rc;
      }
      this.BringToFront();
   }

   private void frameLoad(object sender, EventArgs e)
   {
      hardwareMonitor.jsonserver();
      //chromeBrowser.ShowDevTools();
      if (File.Exists("update.zip"))
      {
         try
         {
            File.Delete("update.zip");
         }
         catch (Exception) { }
      }
   }

   public void InitializeChromium()
   {
      CefSettings settings = new CefSettings();

      // Note that if you get an error or a white screen, you may be doing something wrong !
      // Try to load a local file that you're sure that exists and give the complete path instead to test
      // for example, replace page with a direct path instead :
      // String page = @"C:\Users\SDkCarlos\Desktop\afolder\index.html";

      String page = string.Format(@"{0}\asset\index.html", Application.StartupPath);

      //String page = @"C:\Users\SDkCarlos\Desktop\artyom-HOMEPAGE\index.html";

      if (!File.Exists(page))
      {
         MessageBox.Show("Error The html file doesn't exists : " + page);
      }

      // Initialize cef with the provided settings
      //Cef.Initialize(settings);
      // Create a browser component
      chromeBrowser = new ChromiumWebBrowser(page);

      // Add it to the form and fill it to the form window.
      this.Controls.Add(chromeBrowser);
      chromeBrowser.Dock = DockStyle.Fill;
      CefSharpSettings.LegacyJavascriptBindingEnabled = true;

      // Allow the use of local resources in the browser
      BrowserSettings browserSettings = new BrowserSettings();
      browserSettings.FileAccessFromFileUrls      = CefState.Enabled;
      browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
      chromeBrowser.BrowserSettings = browserSettings;

      Form1_Shown();
   }

   private void frameClosing(object sender, FormClosingEventArgs e)
   {
      //  Cef.Shutdown();
   }

   private void frameClick(object sender, MouseEventArgs e)
   {
   }
}

public class DropShadow : Form
{
   public DropShadow()
   {
      this.Opacity         = 0.1;
      this.BackColor       = Color.Gray;
      this.ShowInTaskbar   = false;
      this.FormBorderStyle = FormBorderStyle.None;
      this.StartPosition   = FormStartPosition.Manual;
   }

   private const int WS_EX_TRANSPARENT = 0x20;
   private const int WS_EX_NOACTIVATE  = 0x8000000;
   protected override System.Windows.Forms.CreateParams CreateParams
   {
      get
      {
         CreateParams cp = base.CreateParams;
         cp.ExStyle = cp.ExStyle | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
         return cp;
      }
   }
}
}
