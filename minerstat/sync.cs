using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Http;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;

namespace minerstat
{
class sync
{
   public static string monitorURL;
   public static string apiResponse;
   public static string apiHardware;
   public static string apiCpu;
   private static readonly HttpClient client = new HttpClient();
   public static PerformanceCounter ramCounter;
   private static WebSocket wclient;
   const string host = "wss://minerstat.com:2096/windows";

   [DllImport("user32")]
   public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

   [DllImport("user32")]
   public static extern void LockWorkStation();

   public class API
   {
      public string token { get; set; }
      public string worker { get; set; }
      public string workerData { get; set; }
      public string minerData { get; set; }
      public string hwData { get; set; }
      public string cpuData { get; set; }
   }

   public class WORKERAPI
   {
      public string miner { get; set; }
      public string version { get; set; }
      public string cpu { get; set; }
      public string cpud { get; set; }
      public string os { get; set; }
      public string space { get; set; }
      public string freemem { get; set; }
      public string localip { get; set; }
      public string remoteip { get; set; }
      public string currentcpu { get; set; }
   }

    async public static void loop(object sender, ElapsedEventArgs exw)
   {
      wclient = new WebSocket(host);



      try
      {
         if (modules.checkNet(false) == false)
         {
            Program.NewMessage("SYNC => Skip: CONNECTION LOST", "ERROR");
            Program.connectionError = true;
         }
         else
         {
            if (modules.IsReach().Equals(false))
            {
               Program.NewMessage("SYNC => Skip: MINERSTAT UNREACHABLE", "ERROR");
               Program.connectionError = true;
            }
            else
            {
               // SET null
               apiResponse             = "";
               apiHardware             = "";
               apiCpu                  = "";
               Program.connectionError = false;


               // CHECK IP
               if (Program.currentIP.Equals("0.0.0.0"))
               {
                  Program.currentIP = modules.GetUserIP();
               }

               // 1) PREPARE THE URL'S if Needed
               switch (mining.minerDefault.ToLower())
               {
               case "cast-xmr":
                  monitorURL = "http://127.0.0.1:7777";
                  break;

               case "xmr-stak":
                  monitorURL = "http://127.0.0.1:2222/api.json";
                  break;

               case "xmrig-amd":
                  monitorURL = "http://127.0.0.1:4028/";
                  break;

               case "xmrig-nvidia":
                  monitorURL = "http://127.0.0.1:4028/";
                  break;

               case "wildrig-multi":
                  monitorURL = "http://127.0.0.1:4028/";
                  break;

               case "trex":
                  monitorURL = "http://127.0.0.1:4068/summary";
                  break;

               case "bminer":
                  monitorURL = "http://127.0.0.1:1880/api/status";
                  break;

               case "lolminer":
                  monitorURL = "http://127.0.0.1:3333/summary";
                  break;

               case "gminer":
                  monitorURL = "http://127.0.0.1:3333/api/v1/status";
                  break;

               case "grinprominer":
                  monitorURL = "http://127.0.0.1:5777/api/status";
                  break;

               case "srbminer":
                  monitorURL = "http://127.0.0.1:21555";
                  break;
               
               case "claymore-zec":
                  monitorURL = "http://127.0.0.1:3333";
                  break;
               }


               // 2) Fetch API's
               if (mining.minerDefault.ToLower().Contains("ccminer") || mining.minerDefault.ToLower().Contains("cryptodredge") || mining.minerDefault.ToLower().Contains("z-enemy")) { modules.getStat(); }
               if (mining.minerDefault.ToLower().Contains("ewbf")) { modules.getStat_ewbf(); }
               if (mining.minerDefault.ToLower().Contains("zm-zec")) { modules.getStat_zm(); }
               if (mining.minerDefault.ToLower().Contains("phoenix-eth") || mining.minerDefault.ToLower().Contains("claymore-eth") || mining.minerDefault.ToLower().Contains("claymore-xmr") || mining.minerDefault.ToLower().Contains("claymore-neoscrypt")) { modules.getStat_claymore(); }
               if (mining.minerDefault.ToLower().Contains("ethminer") || mining.minerDefault.ToLower().Contains("progpowminer") || mining.minerDefault.ToLower().Contains("serominer")) { modules.getStat_ethminer(); }
               if (mining.minerDefault.ToLower().Contains("mkxminer")) { modules.getStat_mkxminer(); }
               if (mining.minerDefault.ToLower().Contains("gateless")) { modules.getStat_sgminer(); }
               if (mining.minerDefault.ToLower().Contains("miniz")) { modules.getStat_miniz(); }
               if (mining.minerDefault.ToLower().Contains("cast-xmr") || mining.minerDefault.ToLower().Contains("xmr-stak") || mining.minerDefault.ToLower().Contains("claymore-zec") || mining.minerDefault.ToLower().Contains("bminer") || mining.minerDefault.ToLower().Contains("trex") || mining.minerDefault.ToLower().Contains("grinprominer") || mining.minerDefault.ToLower().Contains("lolminer") || mining.minerDefault.ToLower().Contains("srbminer") || mining.minerDefault.ToLower().Contains("xmrig-amd") || mining.minerDefault.ToLower().Contains("xmrig-nvidia") || mining.minerDefault.ToLower().Contains("wildrig-multi") || mining.minerDefault.ToLower().Equals("gminer"))
               {
                  string          input;
                  HttpWebRequest  request  = (HttpWebRequest)WebRequest.Create(monitorURL);
                  HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                  StreamReader    sr       = new StreamReader(response.GetResponseStream());
                  input = sr.ReadToEnd();
                  sr.Close();
                  apiResponse = input;
               }
               if (mining.minerDefault.ToLower().Contains("sgminer") || mining.minerDefault.ToLower().Contains("teamredminer")) { modules.getStat_sgminer(); }

               // Hardware Monitor
               modules.getData hwQuery = new modules.getData("http://localhost:" + Program.monitorport + "/", "POST", "");
               apiHardware = hwQuery.GetResponse();

               // CPU Miner's
               if (mining.minerCpu.Equals("True"))
               {
                  switch (mining.cpuDefault.ToLower())
                  {
                  case "xmr-stak-cpu":
                     monitorURL = "HTTP";
                     break;

                  case "cpuminer-opt":
                     monitorURL = "TCP";
                     break;

                  case "xmrig":
                     monitorURL = "HTTP";
                     break;
                  }

                  try
                  {
                     if (monitorURL.Equals("HTTP"))
                     {
                        string          input;
                        HttpWebRequest  request  = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:7887");
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        StreamReader    sr       = new StreamReader(response.GetResponseStream());
                        input = sr.ReadToEnd();
                        sr.Close();
                        apiCpu = input;
                     }
                     else
                     {
                        modules.getStat_cpu();
                     }
                  }
                  catch (Exception ex)
                  {
                     Program.NewMessage("ERROR => CPU API NOT RUNNING", "");
                     watchDog.cpuHealth();
                  }
               }



               // 4) POST

               await Task.Delay(1000);

               try
               {
                  var ramCount = "0";
                  try
                  {
                     ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
                     ramCount   = Convert.ToInt32(ramCounter.NextValue()).ToString();
                  }
                  catch (Exception) { }

                  WORKERAPI WORKERAPI = new WORKERAPI
                  {
                     miner      = mining.minerDefault.ToLower(),
                     version    = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                     cpu        = mining.minerCpu,
                     cpud       = "HASH",
                     currentcpu = mining.cpuDefault.ToLower(),
                     freemem    = ramCount,
                     localip    = modules.GetLocalIPAddress(),
                     remoteip   = Program.currentIP,
                     space      = "" + modules.GetTotalFreeSpace("C") / 1000000,
                     os         = "win"
                  };

                  API API = new API
                  {
                     token      = Program.token,
                     worker     = Program.worker,
                     workerData = JsonConvert.SerializeObject(WORKERAPI),
                     minerData  = apiResponse,
                     hwData     = apiHardware,
                     cpuData    = apiCpu
                  };



                  /*******
                   * WEBSOCKETS
                   **********/

                  //wclient.OnOpen += (ss, ee) => Program.NewMessage("connect: " + host, "");
                  wclient.OnError += (ss, ee) =>
                  {
                     //Program.NewMessage("NOTICE => Socket error, switch back to HTTPS", "");
                     //Program.NewMessage("SOCKET => " + ee.Message, "");
                     if (!ee.Message.Contains("occurred"))
                     {
                        wclient.Close();
                     }
                     var postValue = new Dictionary<string, string>
                     {
                        { "minerData", apiResponse },
                        { "hwData", apiHardware },
                        { "cpuData", apiCpu },
                        { "idle", "false" }
                     };
                     var content = new FormUrlEncodedContent(postValue);
                     postAsync(content, ramCount);
                  };
                  wclient.OnMessage += (ss, ee) =>
                  {
                     var apiError = "";
                     if (apiResponse.Equals(""))
                     {
                        apiError = "(MINER ERROR)";
                     }

                     try
                     {
                        int package = (apiHardware.Length + apiResponse.Length + apiCpu.Length) * sizeof(Char);
                        modules.updateTraffic(package);
                        Program.NewMessage("SYNC (WSS) => API Updated " + apiError + " [ ~ " + (package / 1000) + " KB ]", "INFO");
                     }
                     catch (Exception)
                     {
                        Program.NewMessage("SYNC (WSS) => API Updated " + apiError + " [ ~ 1 KB ]", "INFO");
                        modules.updateTraffic(1);
                     }

                     if (!ee.Data.Equals("null"))
                     {
                        Program.NewMessage("REMOTE COMMAND => " + ee.Data, "");
                        RemoteCommand(ee.Data);
                     }
                     wclient.Close();
                  };
                  //wclient.OnClose += (ss, ee) => Program.NewMessage(host, "");

                  wclient.Connect();
                  File.WriteAllText(@Program.minerstatDir + "/test.json", JsonConvert.SerializeObject(API));

                  wclient.Send(JsonConvert.SerializeObject(API));

                  //Program.NewMessage(JsonConvert.SerializeObject(API), "");

                  if (apiResponse.Equals(""))
                  {
                     Program.NewMessage("ERROR => UNABLE TO FETCH MINER API.", "");
                  }
               }
               catch (Exception ex) { Program.NewMessage("ERROR => " + ex.ToString(), ""); }
            }
         }
      }
      catch (Exception error)
      {
         Program.NewMessage("ERROR => " + error.ToString(), "");
                var ramCount = "0";
                    try
                    {
                       

                        WORKERAPI WORKERAPI = new WORKERAPI
                        {
                            miner = mining.minerDefault.ToLower(),
                            version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                            cpu = mining.minerCpu,
                            cpud = "HASH",
                            currentcpu = mining.cpuDefault.ToLower(),
                            freemem = "1",
                            localip = modules.GetLocalIPAddress(),
                            remoteip = "0.0.0.0",
                            space = "1",
                            os = "win"
                        };

                        API API = new API
                        {
                            token = Program.token,
                            worker = Program.worker,
                            workerData = JsonConvert.SerializeObject(WORKERAPI),
                            minerData = apiResponse,
                            hwData = apiHardware,
                            cpuData = apiCpu
                        };

                    // Hardware Monitor
                    modules.getData hwQuery = new modules.getData("http://localhost:" + Program.monitorport + "/", "POST", "");
                    apiHardware = hwQuery.GetResponse();

                    var postValue = new Dictionary<string, string>
                        {
                            { "minerData", apiResponse },
                            { "hwData", apiHardware },
                            { "cpuData", apiCpu },
                            { "idle", "true" }
                        };
                    var content = new FormUrlEncodedContent(postValue);
                        postAsync(content, ramCount);

                    }
                    catch (Exception) { }
            } 
   }

   async public static void postAsync(FormUrlEncodedContent content, string ramCount)
   {
      try
      {
         var response = await client.PostAsync("https://api.minerstat.com/v2/set_node_config.php?token=" + Program.token + "&worker=" + Program.worker + "&miner=" + mining.minerDefault.ToLower() + "&ver=" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "&cpuu=" + mining.minerCpu + "&cpud=HASH" + "&os=win" + "&algo=&best=&space=" + modules.GetTotalFreeSpace("C") / 1000000 + "&freemem=" + ramCount + "&localip=" + modules.GetLocalIPAddress() + "&remoteip=" + modules.GetUserIP() + "&currentcpu=" + mining.cpuDefault.ToLower(), content);

         var responseString = await response.Content.ReadAsStringAsync();

         try
         {
            int package = (apiHardware.Length + apiResponse.Length + apiCpu.Length) * sizeof(Char);
            modules.updateTraffic(package);
            Program.NewMessage("SYNC (HTTPS) => API Updated  [ ~ " + (package / 1000) + " KB ]", "INFO");
         }
         catch (Exception)
         {
            Program.NewMessage("SYNC (HTTPS) => API Updated  [ ~ 1 KB ]", "INFO");
            modules.updateTraffic(1);
         }

         if (!responseString.Equals(""))
         {
            Program.NewMessage("REMOTE COMMAND => " + responseString, "");
            RemoteCommand(responseString);
         }
      }
      catch (Exception) { }
   }

   async public static void RemoteCommand(string command)
   {
      if (command.Equals("BENCHMARK"))
      {
         mining.killAll();
         Program.watchDogs.Stop();
         Program.syncLoop.Stop();
         Program.NewMessage("BENCHMARK => Starts in 2 second", "");
         await Task.Delay(2000);
                try
                {
                    if (!File.Exists(Program.currentDir + "/benchmark.txt"))
                    {
                        File.WriteAllText(Program.currentDir + "/benchmark.txt", "");
                    }
                } catch (Exception) { }
         BenchMark.Start();
      }

      if (command.Equals("STOP"))
      {
         mining.killAll();
         Program.watchDogs.Stop();
         //Program.syncLoop.Stop();
         Program.NewMessage("MINING => STOP", "");
      }

      if (command.Equals("RESTARTNODE") || command.Equals("RESTART") || command.Equals("START"))
      {
         mining.killAll();
         Program.watchDogs.Stop();
         Program.syncLoop.Stop();
         await Task.Delay(1500);

         mining.Start();
      }

      if (command.Equals("REBOOT"))
      {
         mining.killAll();
         Program.watchDogs.Stop();
         Program.syncLoop.Stop();
         Program.NewMessage("SYSTEM => REBOOT in 1 sec", "");
         await Task.Delay(1500);

         var psi = new ProcessStartInfo("shutdown", "/r /f /t 0");
         psi.CreateNoWindow  = true;
         psi.UseShellExecute = false;
         Process.Start(psi);
      }

      if (command.Equals("SHUTDOWN"))
      {
         mining.killAll();
         Program.watchDogs.Stop();
         Program.syncLoop.Stop();
         Program.NewMessage("SYSTEM => SHUTDOWN in 1 sec", "");
         await Task.Delay(1500);

         var psi = new ProcessStartInfo("shutdown", "/s /f /t 0");
         psi.CreateNoWindow  = true;
         psi.UseShellExecute = false;
         Process.Start(psi);
      }

      if (command.Equals("DOWNLOADWATTS"))
      {
         mining.downloadConfig(Program.token, Program.worker);
      }

      if (command.Equals("RESTARTWATTS"))
      {
         mining.killAll();
         Program.watchDogs.Stop();
         Program.syncLoop.Stop();
         await Task.Delay(1500);

         mining.downloadConfig(Program.token, Program.worker);
         await Task.Delay(1000);

         mining.Start();
      }
   }
}
}
