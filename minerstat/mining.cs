﻿//using MSI.Afterburner;
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
using System.Runtime.ExceptionServices;
using System.Security;

namespace minerstat
{
    class mining
    {
        public static string configJSON;
        public static string minerConfig;
        public static string cpuConfig;
        public static string minerDefault;
        public static string cpuDefault;
        public static string minerType;
        public static string minerOverclock;
        public static string minerCpu,
        remoteVersion;
        public static string benchmark = "NO";
        public static string minerStarted = "NO";
        private static Form1 _instanceMainForm = null;
        private static string filePath;
        private static string cpuConfigFile;
        private static string cpuVersion;
        private static WebClient wc = new WebClient();
        private static string github_version_file = "https://raw.githubusercontent.com/minerstat/minerstat-windows/master/versionStable.txt";

        // public static HardwareMonitor mahm = new HardwareMonitor();
        // EXPLODE
        public static string[] explode(string separator, string source)
        {
            return source.Split(new string[] {
                separator
            },
            StringSplitOptions.None);
        }

        public mining(Form1 mainForm)
        {
            _instanceMainForm = mainForm;
        }

        async public static void killAll()
        {
            for (int i = 0; i < 2; i++)
            {
                // STOP TIMERS
                Program.watchDogs.Stop();
                Program.syncLoop.Stop();

                if (Process.GetProcessesByName("powershell").Length > 0)
                {
                    try
                    {
                        System.Diagnostics.Process.Start("taskkill", "/F /IM powershell.exe /T");
                        System.Diagnostics.Process.Start("taskkill", "/F /IM " + getProcessName().ToLower() + ".exe /T");
                    }
                    catch (Exception)
                    {
                        Program.NewMessage("Unable to close running miner", "ERROR");
                    }
                }

                await Task.Delay(700);
            }
        }

        public static string getCPUProcess()
        {
            string process = "unknown";

            switch (mining.cpuDefault.ToLower())
            {
                case "xmr-stak-cpu":
                    process = "xmr-stak-cpu";
                    break;

                case "cpuminer-opt":
                    process = "cpuminer-celeron";
                    break;

                case "xmrig":
                    process = "xmrig";
                    break;
            }

            return process;
        }

        public static string getProcessName()
        {
            string process = "unknown";
            string gpuType = "amd";

            if (minerType.Equals("nvidia"))
            {
                gpuType = "cuda";
            }

            switch (minerDefault.ToLower())
            {
                case "phoenix-eth":
                    process = "phoenixminer";
                    break;

                case "claymore-neoscrypt":
                    process = "neoscryptminer";
                    break;

                case "ccminer-tpruvot":
                    process = "ccminer-80-x64";
                    break;

                case "cast-xmr":
                    process = "cast_xmr-vega";
                    break;

                case "xmr-stak":
                    process = "xmr-stak";
                    break;

                case "ccminer-alexis":
                    process = "ccmineralexis78";
                    break;

                case "ccminer-x16r":
                    process = "ccminer";
                    break;

                case "bminer":
                    process = "bminer";
                    break;

                case "ccminer-krnlx":
                    process = "ccminer";
                    break;

                case "ethminer":
                    process = "ethminer";
                    break;

                case "serominer":
                    process = "serominer-" + gpuType;
                    break;

                case "claymore-xmr":
                    process = "nsgpucnminer";
                    break;

                case "claymore-eth":
                    process = "ethdcrminer64";
                    break;

                case "claymore-zec":
                    process = "zecminer64";
                    break;

                case "optiminer-zec":
                    process = "optiminer";
                    break;

                case "sgminer-pasc":
                    process = "sgminer";
                    break;

                case "gatelessgate":
                    process = "gatelessgate";
                    break;

                case "sgminer-gm":
                    process = "sgminer";
                    break;

                case "ewbf-zec":
                    process = "miner";
                    break;

                case "ewbf-zhash":
                    process = "miner";
                    break;

                case "trex":
                    process = "t-rex";
                    break;

                case "zm-zec":
                    process = "zm";
                    break;

                case "z-enemy":
                    process = "z-enemy";
                    break;

                case "cryptodredge":
                    process = "cryptodredge";
                    break;

                case "lolminer":
                    process = "lolminer";
                    break;

                case "srbminer":
                    process = "srbminer-cn";
                    break;

                case "progpowminer":
                    process = "progpowminer-" + gpuType;
                    break;

                case "xmrig-amd":
                    process = "xmrig-amd";
                    break;

                case "xmrig-nvidia":
                    process = "xmrig-nvidia";
                    break;

                case "wildrig-multi":
                    process = "wildrig";
                    break;

                case "mkxminer":
                    process = "mkxminer";
                    break;

                case "gminer":
                    process = "miner";
                    break;

                case "grinprominer":
                    process = "GrinProMiner";
                    break;

                case "teamredminer":
                    process = "teamredminer";
                    break;

                case "miniz":
                    process = "miniz";
                    break;
            }

            return process;
        }

        public static Boolean CheckforUpdates()
        {
            try
            {
                var localVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                wc.DownloadFile(new Uri(github_version_file), @Program.currentDir + "/NetVersion.txt");
                remoteVersion = File.ReadAllText(@Program.currentDir + "/NetVersion.txt");
                File.Delete(@Program.currentDir + "/NetVersion.txt");

                if (remoteVersion.Trim() == localVersion.Trim())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void StartUpdate()
        {
            if (!File.Exists(@Program.currentDir + "/minerstat.exe"))
            {
                Program.NewMessage("Main program file doesn't exist, try reinstalling or update the app.", "");
                Application.Exit();
            }
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C choice /C Y /N /D Y /T 0 & cd " + Program.currentDir + " & start minerstat.exe";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Exit();
        }

        async public static void Start()
        {
            minerCpu = "false";
            minerStarted = "NO";
            Program.SyncStatus = false;

            if (Program.StartDelayOver.Equals(false))
            {
                Program.SyncStatus = false;
                Program.NewMessage("INFO => STARTED WITH WINDOWS", "INFO");
                Program.NewMessage("INFO => Programmed mining start delay: " + Program.StartDelay + "ms", "INFO");
                await Task.Delay(Program.StartDelay);

                Program.StartDelayOver = true;
                Program.SyncStatus = true;
            }

            if (CheckforUpdates().Equals(true))
            {
                StartUpdate();
                Application.Exit();
            }

            _instanceMainForm.Invoke((MethodInvoker)delegate {
                _instanceMainForm.TopMost = true;
            });

            try
            {
                if (File.Exists(@Program.minerstatDir + "/user.json"))
                {
                    string json = File.ReadAllText(@Program.minerstatDir + "/user.json");
                    Program.loginjson = json;

                    var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                    Program.token = (string)jObject["token"];
                    Program.worker = (string)jObject["worker"];
                }

                downloadConfig(Program.token, Program.worker);

                if (!Directory.Exists(@Program.currentDir + "/clients"))
                {
                    Directory.CreateDirectory(@Program.currentDir + "/clients");
                }

                // Delete pending remote commands
                modules.getData response = new modules.getData("https://api.minerstat.com/v2/get_command_only.php?token=" + Program.token + "&worker=" + Program.worker, "POST", "");
                string responseString = response.GetResponse();

                if (!responseString.Equals(""))
                {
                    Program.NewMessage("PENDING COMMAND REMOVED  => " + responseString, "");
                }

                modules.getData minersVersion = new modules.getData("https://static-ssl.minerstat.farm/miners/windows/version.json", "POST", "");
                string version = minersVersion.GetResponse();

                var vObject = Newtonsoft.Json.Linq.JObject.Parse(version);
                var minerVersion = (string)vObject[minerDefault.ToLower()];
                cpuVersion = (string)vObject[cpuDefault.ToLower()];

                // main MINER
                if (!Directory.Exists(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/"))
                {
                    Directory.CreateDirectory(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/");
                }

                string localMinerVersion;

                if (File.Exists(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/minerVersion.txt"))
                {
                    localMinerVersion = File.ReadAllText(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/minerVersion.txt");
                }
                else
                {
                    localMinerVersion = "0";
                }

                if (!File.Exists(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/minerUpdated.txt") && !File.Exists(Program.minerstatDir + "/buffer.txt") || !localMinerVersion.Equals(minerVersion) && !File.Exists(Program.minerstatDir + "/buffer.txt"))
                {
                    // ECHO  WORK IN PROGRESS
                    Program.NewMessage("INFO => Download new version of: " + minerDefault.ToLower(), "");

                    if (!localMinerVersion.Equals(minerVersion) && (localMinerVersion != "0"))
                    {
                        try
                        {
                            // DELETE ALL FILES
                            System.IO.DirectoryInfo di = new DirectoryInfo(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/");

                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (DirectoryInfo dir in di.GetDirectories())
                            {
                                dir.Delete(true);
                            }

                            await Task.Delay(1000);

                            Directory.Delete(Program.currentDir + "/clients/" + minerDefault.ToLower(), true);

                            await Task.Delay(1000);

                            if (!Directory.Exists(Program.currentDir + "/clients/" + minerDefault.ToLower()))
                            {
                                Directory.CreateDirectory(Program.currentDir + "/clients/" + minerDefault.ToLower());
                            }
                        }
                        catch (Exception) { }
                    }

                    await Task.Delay(500);

                    if (!Directory.Exists(Program.currentDir + "/clients/" + minerDefault.ToLower()))
                    {
                        Directory.CreateDirectory(Program.currentDir + "/clients/" + minerDefault.ToLower());
                    }

                    Downloader.minerVersion = minerVersion;

                    // DOWNLOAD FRESH PACKAGE
                    await Task.Delay(6500);

                    Downloader.downloadFile(minerDefault.ToLower() + ".zip", minerDefault.ToLower(), "main");
                    Program.SyncStatus = false;
                }
                else
                {
                    await Task.Delay(1500);

                    Program.NewMessage("CONFIG => Default miner: " + minerDefault, "INFO");
                    Program.NewMessage("CONFIG => Worker type: " + minerType, "INFO");
                    Program.NewMessage("CONFIG => CPU Mining: " + minerCpu, "INFO");
                    Program.NewMessage(minerDefault.ToUpper() + " => " + minerConfig, "INFO");
                    // Start miner
                    Program.NewMessage("NODE => Waiting for the next sync..", "INFO");

                    Program.SyncStatus = true;
                    startMiner(true, false);

                    // Start watchDog
                    Program.watchDogs.Start();

                    // Start Crash Protection
                    //Program.crashLoop.Start();
                    // Start SYNC & Remote Command
                    Program.syncLoop.Start();

                    // ETHPILL
                    await Task.Delay(1500);

                    try
                    {
                        if (minerType.ToLower().Equals("nvidia"))
                        {
                            // Hardware Monitor
                            // ONLY FOR: 1080, 1080Ti, Titan XP
                            modules.getData hwQuery = new modules.getData("http://localhost:" + Program.monitorport + "/", "POST", "");
                            string HardwareLog = hwQuery.GetResponse();

                            if (HardwareLog.ToString().ToLower().Contains("1080") || HardwareLog.ToString().ToLower().Contains("1080 ti") || HardwareLog.ToString().ToLower().Contains("titan xp"))
                            {
                                //Program.NewMessage("ETHPill => Compatible device(s) detected", "INFO");
                                if (Process.GetProcessesByName("OhGodAnETHlargementPill-r2").Length == 0)
                                {
                                    // Does your ETHlargement not work as expected? You're likely running an older memory revision.
                                    // With the use of --revA, you can specify which device should be fed our senior solution.If, for example, GPU 0, 3 and 4 aren't the young studs you thought they were, feed them with the following commands:
                                    // OhGodAnETHlargementPill-r2.exe --revA 0,3,4 at ~/minerstat-windows/mist/OhGodAnETHlargementPill-r2-args.txt
                                    //string pillPath = '"' + Program.currentDir + "/mist/" + '"';
                                    string pillArgs = "OhGodAnETHlargementPill-r2.exe";

                                    try
                                    {
                                        pillArgs = File.ReadAllText(Program.currentDir + "/mist/OhGodAnETHlargementPill-r2-args.txt").Split(new[] {
                                            '\r',
                                            '\n'
                                        }).FirstOrDefault();
                                    }
                                    catch (Exception)
                                    {
                                        pillArgs = "OhGodAnETHlargementPill-r2.exe";
                                    }

                                    Process.Start("CMD.exe", "/c " + Program.currentDir + "/mist/" + pillArgs);
                                    Program.NewMessage("ETHPill => Starting in a new window", "INFO");
                                }
                                else
                                {
                                    Program.NewMessage("ETHPill => Already running", "INFO");
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                Program.NewMessage(ex.ToString(), "");
                if (ex.ToString().ToLower().Contains("json") || ex.ToString().ToLower().Contains("urlencoded"))
                {
                    Program.watchDogs.Stop();
                    Program.syncLoop.Stop();
                    Program.offlineLoop.Stop();
                    mining.killAll();

                    await Task.Delay(2000);

                    Program.offlineLoop.Start();
                    Start();
                }
            }
        }

        async public static void startMiner(Boolean m1, Boolean m2)
        {
            // Defender - Add exclusion folder programmatically
            Process.Start("C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe ", "Add-MpPreference -ExclusionPath '" + Program.currentDir + "'");

            if (!File.Exists(Program.minerstatDir + "/buffer.txt"))
            {
                if (File.Exists(Program.currentDir + "/" + minerDefault.ToLower() + ".zip"))
                {
                    try
                    {
                        File.Delete(Program.currentDir + "/" + minerDefault.ToLower() + ".zip");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("ERROR => File .zip removal error");
                    }
                }

                _instanceMainForm.Invoke((MethodInvoker)delegate {
                    _instanceMainForm.TopMost = true;
                });

                if (Process.GetProcessesByName(mining.getProcessName()).Length == 0)
                {
                    if (m1.Equals(true) && m2.Equals(false))
                    {
                        Program.watchDogs.Stop();
                        Program.syncLoop.Stop();
                        //Program.crashLoop.Stop();
                        if (minerCpu.Equals("True"))
                        {
                            if (!Directory.Exists(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/"))
                            {
                                Directory.CreateDirectory(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/");
                            }

                            string cpuMinerVersion;

                            if (File.Exists(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/minerVersion.txt"))
                            {
                                cpuMinerVersion = File.ReadAllText(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/minerVersion.txt");
                            }
                            else
                            {
                                cpuMinerVersion = "0";
                            }

                            if (!File.Exists(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/minerUpdated.txt") || !cpuMinerVersion.Equals(cpuVersion))
                            {
                                // DELETE ALL FILES
                                System.IO.DirectoryInfo di = new DirectoryInfo(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/");

                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                foreach (DirectoryInfo dir in di.GetDirectories())
                                {
                                    dir.Delete(true);
                                }

                                Downloader.minerVersion = cpuVersion;

                                // DOWNLOAD FRESH PACKAGE
                                Program.SyncStatus = false;
                                Downloader.downloadFile(cpuDefault.ToLower() + ".zip", cpuDefault.ToLower(), "cpu");
                            }
                            else
                            {
                                await Task.Delay(1500);

                                startMiner(false, true);
                                Program.SyncStatus = true;
                            }
                        }

                        Program.watchDogs.Start();
                        Program.syncLoop.Start();

                        //Program.crashLoop.Start();
                    }

                    if (m1.Equals(true))
                    {
                        string folderPath = '"' + Program.currentDir + "/clients/" + minerDefault.ToLower() + "/" + '"';
                        Process.Start("C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe ", @"set-location '" + folderPath + "'; " + "./start.bat; pause");

                        minerStarted = "YES";
                    }
                }

                if (Process.GetProcessesByName(getCPUProcess()).Length == 0)
                {
                    if (minerCpu.Equals("True") && m2.Equals(true))
                    {
                        string folderPath = '"' + Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/" + '"';

                        switch (mining.cpuDefault.ToLower())
                        {
                            case "xmr-stak-cpu":
                                filePath = "xmr-stak-cpu.exe";
                                break;

                            case "cpuminer-opt":
                                filePath = "start.bat";
                                break;

                            case "xmrig":
                                filePath = "xmrig.exe";
                                break;
                        }

                        Program.NewMessage(cpuDefault.ToUpper() + " => " + cpuConfig, "INFO");
                        Process.Start("C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe ", @"set-location '" + folderPath + "'; " + "./" + filePath + ";");
                    }
                }
                await Task.Delay(2000);

                Program.SyncStatus = true;

                _instanceMainForm.Invoke((MethodInvoker)delegate {
                    _instanceMainForm.TopMost = false;
                });
            }
            else
            {
                File.Delete(@Program.minerstatDir + "/buffer.txt");
            }
        }


        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static void downloadConfig(string token, string worker)
        {
            try
            {
                modules.getData nodeConfig = new modules.getData("https://api.minerstat.com/v2/node/gpu/" + token + "/" + worker, "POST", "");
                configJSON = nodeConfig.GetResponse();

                var jObject = Newtonsoft.Json.Linq.JObject.Parse(configJSON);
                minerDefault = (string)jObject["default"];
                cpuDefault = (string)jObject["cpuDefault"];
                minerType = (string)jObject["type"];
                minerOverclock = JsonConvert.SerializeObject(jObject["overclock"]);
                minerCpu = (string)jObject["cpu"];

                if (benchmark.Equals("NO"))
                {
                    modules.getData configRequest = new modules.getData("https://api.minerstat.com/v2/conf/gpu/" + token + "/" + worker + "/" + minerDefault.ToLower(), "POST", "");
                    minerConfig = configRequest.GetResponse();
                }
                else
                {
                    minerDefault = BenchMark.B_CLIENT.ToLower();
                    minerConfig = BenchMark.B_CONFIG;
                }

                string fileExtension = "start.bat";

                if (minerDefault.Contains("claymore"))
                {
                    fileExtension = "config.txt";
                }
                if (minerDefault.Contains("phoenix"))
                {
                    fileExtension = "config.txt";
                }
                if (minerDefault.Contains("sgminer"))
                {
                    fileExtension = "sgminer.conf";
                }
                if (minerDefault.Contains("xmr-stak"))
                {
                    fileExtension = "pools.txt";
                }
                if (minerDefault.Contains("trex"))
                {
                    fileExtension = "config.json";
                }
                if (minerDefault.Contains("lolminer"))
                {
                    fileExtension = "user_config.json";
                }

                string folderPath = Program.currentDir + "/clients/" + minerDefault + "/" + fileExtension;
                File.WriteAllText(@folderPath, minerConfig);

                if (minerCpu.Equals("True"))
                {
                    modules.getData cpuRequest = new modules.getData("https://api.minerstat.com/v2/conf/gpu/" + token + "/" + worker + "/" + cpuDefault.ToLower(), "POST", "");
                    cpuConfig = cpuRequest.GetResponse();

                    switch (cpuDefault.ToLower())
                    {
                        case "xmr-stak-cpu":
                            cpuConfigFile = "config.txt";
                            break;

                        case "cpuminer-opt":
                            cpuConfigFile = "start.bat";
                            break;

                        case "xmrig":
                            cpuConfigFile = "config.json";
                            break;
                    }

                    string folderPathCpu = Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/" + cpuConfigFile;
                    File.WriteAllText(@folderPathCpu, cpuConfig);
                }

                if (!JsonConvert.SerializeObject(jObject["overclock"]).Equals(""))
                {
                    if (Process.GetProcessesByName("msiafterburner").Length > 0)
                    {
                        try
                        {
                            Program.NewMessage("CLOCKTUNE => " + JsonConvert.SerializeObject(jObject["overclock"]), "WARNING");
                        }
                        catch (Exception clockTuneJson)
                        {
                            //MessageBox.Show(clockTuneJson.ToString());
                        }

                        try
                        {
                            var mObject = Newtonsoft.Json.Linq.JObject.Parse(JsonConvert.SerializeObject(jObject["overclock"]));
                            var coreclock = (string)mObject["coreclock"];
                            var memoryclock = (string)mObject["memoryclock"];
                            var powerlimit = (string)mObject["powerlimit"];
                            var fan = (string)mObject["fan"];

                            if (coreclock.Contains(" ") || memoryclock.Contains(" ") || powerlimit.Contains(" ") || fan.Contains(" "))
                            {
                                string[] coreArray = explode(" ", coreclock);
                                string[] memoryArray = explode(" ", memoryclock);
                                string[] fanArray = explode(" ", fan);
                                string[] powerArray = explode(" ", powerlimit);

                                for (int i = 0; i <= 16; i++)
                                {
                                    coreclock = coreArray[i];
                                    memoryclock = memoryArray[i];
                                    powerlimit = powerArray[i];
                                    fan = fanArray[i];

                                    if (coreclock.Equals("skip"))
                                    {
                                        coreclock = "9999";
                                    }
                                    if (memoryclock.Equals("skip"))
                                    {
                                        memoryclock = "9999";
                                    }
                                    if (powerlimit.Equals("skip"))
                                    {
                                        powerlimit = "9999";
                                    }
                                    if (fan.Equals("skip"))
                                    {
                                        fan = "9999";
                                    }

                                    if (!string.IsNullOrWhiteSpace(coreclock))
                                    {
                                        if (!coreclock.Equals("9999") && !memoryclock.Equals("9999") && !powerlimit.Equals("9999") && !fan.Equals("9999"))
                                        {
                                            clocktune.Advanced(minerType, Convert.ToInt32(powerlimit), Convert.ToInt32(coreclock), Convert.ToInt32(fan), Convert.ToInt32(memoryclock), i);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (coreclock.Equals("skip"))
                                {
                                    coreclock = "9999";
                                }
                                if (memoryclock.Equals("skip"))
                                {
                                    memoryclock = "9999";
                                }
                                if (powerlimit.Equals("skip"))
                                {
                                    powerlimit = "9999";
                                }
                                if (fan.Equals("skip"))
                                {
                                    fan = "9999";
                                }
                                if (!coreclock.Equals("9999") && !memoryclock.Equals("9999") && !powerlimit.Equals("9999") && !fan.Equals("9999"))
                                {
                                    clocktune.Manual(minerType, Convert.ToInt32(powerlimit), Convert.ToInt32(coreclock), Convert.ToInt32(fan), Convert.ToInt32(memoryclock));
                                }
                            }
                        }
                        catch (Exception clockTuneErrorII)
                        {
                            Program.NewMessage("Use afterburner 4.5.0 version to use ClockTune.", "ERROR");
                            Program.NewMessage("Use afterburner 4.5.0 version to use ClockTune.", "ERROR");
                        }
                    }
                    else
                    {
                        Program.NewMessage("WARNING => MSIAfterburner.exe is not running..", "INFO");
                        Program.NewMessage("WARNING => Install & Run - MSI Afterburner to enable overclocking", "WARNING");
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
    }
}
