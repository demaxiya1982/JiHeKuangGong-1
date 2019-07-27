using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace minerstat
{
class BenchMark
{
   public static System.Timers.Timer syncLoop;
   private static Form1 _instanceMainForm = null;
   public BenchMark(Form1 mainForm)
   {
      _instanceMainForm = mainForm;
   }

   public static string JSON;
   public static string B_ID;
   public static string B_HASH;
   public static string B_DURATION;
   public static string B_CLIENT;
   public static string B_CONFIG;

   async public static void Start()
   {
      modules.getData nodeConfig = new modules.getData("https://api.minerstat.com/v2/benchmark/" + Program.token + "/" + Program.worker, "POST", "");
      JSON = nodeConfig.GetResponse();

      mining.benchmark = "YES";

      dynamic B_JSON = JsonConvert.DeserializeObject(JSON);
      foreach (var B_ITEM in B_JSON)
      {
         if (mining.benchmark.Equals("NO")) { break; }

         int delay = 70000;

         B_ID       = B_ITEM.id;
         B_HASH     = B_ITEM.hash;
         B_DURATION = B_ITEM.duration;
         B_CLIENT   = B_ITEM.client;
         B_CONFIG   = B_ITEM.config;

         mining.minerDefault = B_CLIENT.ToLower();

         Program.NewMessage("BENCHMARK => " + B_CLIENT + " / " + B_HASH, "");

         if (B_DURATION.Equals("slow")) { delay = 120000; }
         if (B_DURATION.Equals("medium")) { delay = 70000; }
         if (B_DURATION.Equals("fast")) { delay = 45000; }

         int delaysec = (delay / 1000) - 2;

         mining.Start();

         while (mining.minerStarted == "NO")
         {
            if (mining.minerStarted.Equals("YES")) { break; }
            try {
               Program.watchDogs.Stop();
               Program.syncLoop.Stop();
            }
            catch (Exception) { }
            await Task.Delay(1000);
         }

         //Program.watchDogs.Start();
         Program.syncLoop.Start();

         syncLoop           = new System.Timers.Timer(TimeSpan.FromSeconds(delaysec).TotalMilliseconds);
         syncLoop.AutoReset = true;
         syncLoop.Elapsed  += new System.Timers.ElapsedEventHandler(sync.loop);
         syncLoop.Start();

         await Task.Delay(delay);

         await Task.Delay(2000);

         mining.killAll();

         await Task.Delay(2000);

         Program.NewMessage("https://api.minerstat.com/v2/benchmark/result/" + Program.token + "/" + Program.worker + "/" + B_ID + "/" + B_HASH, "");

         modules.getData minersVersion = new modules.getData("https://api.minerstat.com/v2/benchmark/result/" + Program.token + "/" + Program.worker + "/" + B_ID + "/" + B_HASH, "POST", "");
         string          version       = minersVersion.GetResponse();

         Program.NewMessage("BENCHMARK => " + version.ToString(), "");

         syncLoop.Stop();
      }

      // END
      mining.benchmark = "NO";
      Program.NewMessage("BENCHMARK => Finished", "");
      Program.NewMessage("BENCHMARK => Restarting node", "");
      mining.killAll();
      Program.watchDogs.Stop();
      Program.syncLoop.Stop();
      await Task.Delay(1500);

      mining.Start();
   }
}
}
