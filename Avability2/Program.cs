using Avability2;

#region Args
foreach (var argus in args)
{
    var param = "";

    #region Arg_Stores
    if (Globals.FixedStoreList.Count == 0 && Internals.TryHandleArgument(argus, new string[] { "--stores:", "-s:" }, out param, true))
    {
        var storeIDList = param.Split(',');
        if (storeIDList.Length >= 1)
        {
            Globals.FixedStoreList.AddRange(storeIDList);
            Console.WriteLine("Force check mode(Store):" + param);
        }
    }
    #endregion

    #region Arg_Models
    if (Globals.FixedModel.Count == 0 && Internals.TryHandleArgument(argus, new string[] { "--models:", "-m:" }, out param, true))
    {
        var modelIDList = param.Split(',');
        if (modelIDList.Length >= 1)
        {
            Globals.FixedModel.AddRange(modelIDList);
            Console.WriteLine("Force check mode(Model):" + param);
        }
    }
    #endregion

    #region Arg_ProMax
    if (!Globals.ProMax && Internals.TryHandleArgument(argus, new string[] { "--promax", "-pm" }, out param, false))
    {
        Globals.ProMax = true;
        Console.WriteLine("Force check Promax only");
    }
    #endregion

    #region Arg_Timer
    if (Internals.TryHandleArgument(argus, new string[] { "--timer:", "-t:" }, out param, true))
    {
        int timer = 5000;
        if (int.TryParse(param, out timer))
        {
            Console.WriteLine("Set refresh interval:" + timer);
            Globals.Interval = timer;
            Globals.DynamicInterval = timer;
            Globals.Max_DynamicInt = timer;
        }
    }
    #endregion

    #region Arg_Api
    if (!Globals.UseCustomApi && argus.ToLower().StartsWith("--api:"))
    {
        var timerList = argus.Split(':');
        if (timerList.Length == 3) //--api: https:
        {
            if (timerList[1].StartsWith("http"))
            {
                Console.WriteLine("Overriding api endpoint:" + timerList[1].Trim() + ":" + timerList[2].Trim());
                Globals.UseCustomApi = true;
                Globals.apiEndpoint_Custom = timerList[1].Trim() + ":" + timerList[2].Trim();
            }
        }
    }

    if(argus.ToLower().StartsWith("--japan")){
        Globals.Japan = true;
        Console.WriteLine("Enabling Japan Mode.");
    }
    #endregion

    #region Arg_Bot
    if (!Globals.UseTgBot && argus.ToLower().StartsWith("--bot"))
    {
        Globals.TgBotKey = Environment.GetEnvironmentVariable("TgBotKey");
        Globals.ChatRoomID = Environment.GetEnvironmentVariable("TgChatRoomID");
        
        if(string.IsNullOrEmpty(Globals.TgBotKey) || string.IsNullOrEmpty(Globals.ChatRoomID)){
            Console.WriteLine("Please set up TgBotKey and TgChatRoomID in env varible. Or remove --bot if not using Tg Notification.");
            Environment.Exit(-1);
        }else{
            Globals.UseTgBot = true;
            Console.WriteLine("Enabling Telegram Bot.");
        }
    }
    #endregion

    #region Arg_DTimer
    if (!Globals.UseDynamicTimer &&
        Internals.TryHandleArgument(argus, new string[] { "--dynamictimer", "-dtimer" }, out param, false))
    {
        Globals.UseDynamicTimer = true;
        Console.WriteLine("Enabling Dynamic Timer. Min:{0}/Step:{1}/Max:{2}",
            Globals.Min_DynamicInt, Globals.Step_DynamicInt, Globals.Max_DynamicInt);
    }

    if (Internals.TryHandleArgument(argus, new string[] { "--dynamicparam:", "-dparam:" }, out param, true))
    {
        var dprm = param.Split(',');
        if (dprm.Length == 3)
        {
            int minPrm = Globals.Min_DynamicInt;
            int stepPrm = Globals.DynamicInterval;
            int maxPrm = Globals.Max_DynamicInt;
            if (int.TryParse(dprm[0], out minPrm) && int.TryParse(dprm[1], out stepPrm) && int.TryParse(dprm[2], out maxPrm))
            {
                Globals.Min_DynamicInt = minPrm;
                Globals.Step_DynamicInt = stepPrm;
                Globals.Max_DynamicInt = maxPrm;

                Console.WriteLine("Update Dynamic Timer. Min:{0}/Step:{1}/Max:{2}",
                    Globals.Min_DynamicInt, Globals.Step_DynamicInt, Globals.Max_DynamicInt);
            }
        }
    }

    if (!Globals.UseRandomStep &&
        Internals.TryHandleArgument(argus, new string[] { "--dynamicrandom", "-drand" }, out param, false))
    {
        Globals.UseRandomStep = true;
        Console.WriteLine("Enabling Random Stepping for DynamicTimer.");
    }
    #endregion

    #region Other
    // if (argus.ToLower().StartsWith("--noblue"))
    // {
    //     Globals.noBlue = true;
    //     Console.WriteLine("We are now ignoring Blue model.");
    // }
    
    if (argus.ToLower().StartsWith("--ineligible"))
    {
        Globals.showIneligible = true;
        Console.WriteLine("Showing models with ineligible status.");
    }
    #endregion

    #region Help
    if (argus.ToLower() == "--help" || argus.ToLower() == "-h")
    {
        Console.WriteLine("Usage:\n" +
            "--stores/-s:R670,R671\n\tForce check stores only.seperate with ','.\n" +
            "--models/-m:MPWE3CH/A,MPXY3CH/A\n\tForce check modelID stocks only.seperate with ','.\n" +
            // "--noblue\n\t Since nobody wants iPhone 15 Pro Blue,will ignore this model,only when sending notifications.\n" +
            "--ineligible\n\t Showing models with ineligible status, maybe someone just not paid for it.\n" +
            "--promax/-pm\n\tOnly Checks for 14 Pro Max,Not available when -m\n" +
            "--timer:5000/-t:5000\n\tset timer(ms), default 5000ms(5s), will cause api limit if too fast.\n" +
            "--api:\n\tOverride API endpoint,for use with other stores.\n\tmust use -s: and -m: for model/store infos.\n" +
            "--help/-h\n\tthis thing.\n" +
            "\n" +
            "When in App:\n"+
            "F1:Verbose Mode,print all results whenever stocks available or not.\n"+
            "Esc:Gracefully exit(allow .csv write and notification finish.)");
        return;
    }
    #endregion
}
#endregion

if(Globals.UseCustomApi && !(Globals.FixedStoreList.Count == 0 && Globals.FixedModel.Count == 0))
{
    Console.WriteLine("You are using custom Api without defining models(-m:) and/or stores(-s:).\n" +
        "The app will definitely not work when querying models not belongs to that region.");
    return;
}

Task<bool> stockTask = new Task<bool>(new Func<bool>(() =>
{
    while(Globals.Running){
        using (StoreInfo storeInfo = new StoreInfo())
        using (StockInfo stockHandler = new StockInfo()){
            if(Globals.FixedStoreList.Count == 0){
                Console.WriteLine("No Stores,Updating first.");
                if (!storeInfo.Update())
                {
                    Console.WriteLine("No storeInfo,wait for next update.");
                }else{
                    var storeIDs = storeInfo.ReturnStoreID();
                    if(storeIDs.Length <= 0){
                        Console.WriteLine("No stores,wait for next update.");
                    }else{
                        Globals.FixedStoreList.AddRange(storeIDs);
                        Console.WriteLine("Store Loaded:{0}", Globals.FixedStoreList.Count);
                    }
                }
            }
            
            try{
                foreach (var store in Globals.FixedStoreList)
                {
                    if (Globals.Verbose)
                        Console.WriteLine("Requesting " + store);
                    
                    if (Globals.FixedModel.Count > 0)
                        stockHandler.GetStocks(store, Globals.FixedModel);
                    else
                        stockHandler.LoadStock(store);
                    
                    if (!Globals.Running) break;

                    if (Globals.UseDynamicTimer)
                        Thread.Sleep(Globals.DynamicInterval);
                    else
                        Thread.Sleep(Globals.Interval);
                }
            }catch(Exception e){
                //比如说要是你在Tg申请修改StoreList(/japan,/update store)，这个地方都会因为数组变化导致异常，正好中断这一轮的查询
                Console.WriteLine(e.Message);

                if (Globals.UseDynamicTimer)
                    Thread.Sleep(Globals.DynamicInterval);
                else
                    Thread.Sleep(Globals.Interval);
            }
        }
    }

    Console.WriteLine("StoreTask exited by request.");
    return true;
}));

stockTask.Start();
Thread.Sleep(2000);
if (stockTask.IsCompleted)
{
    Console.WriteLine("StockTask Exited with result:{0}", stockTask.Result);
    return;
}
else
{
    if (Globals.UseTgBot)
    {
        Internals.BotInstance.SendGroupMessage(
            string.Format("[{0}]AvabilityV2 online,param:{1}",
                DateTime.UtcNow.ToString(),
                string.Join(" ", args))
        );
    }

    #region Handle Console Input
    while (Globals.Running)
    {
        if(!Environment.UserInteractive || Console.IsInputRedirected)
        {
            Console.WriteLine("You are running with somthing like system daemon.");
            Console.WriteLine("To stop, use systemctl.");
            Thread.Sleep(int.MaxValue);
        }else{
            if(stockTask.IsCompleted){
                Console.WriteLine("stockTask dead.Exiting.");
                break;
            }

            var input = Console.ReadKey();
            switch (input.Key)
            {
                case ConsoleKey.F1:
                    if (!Globals.Verbose)
                    {
                        Console.WriteLine("Enabling Verbose");
                        Globals.Verbose = true;
                    }
                    else
                    {
                        Console.WriteLine("Disabling Verbose");
                        Globals.Verbose = false;
                    }
                    break;
                case ConsoleKey.F2:
                    Console.WriteLine("Current DynamicTimer Status:{0}", Globals.UseDynamicTimer);
                    if (Globals.UseDynamicTimer)
                    {
                        Console.WriteLine("Timer setting: Min:{0}/Step:{1}/Max:{2} Current:{3}",
                            Globals.Min_DynamicInt, Globals.Step_DynamicInt, Globals.Max_DynamicInt, Globals.DynamicInterval);
                    }
                    break;
                case ConsoleKey.F3:
                    Globals.ProMax = !Globals.ProMax;
                    if(Globals.ProMax) Console.WriteLine("Enabling ProMax Mode.");
                    else Console.WriteLine("Disabling ProMax Mode.");
                    break;

                case ConsoleKey.F4:
                    if(Globals.UseCustomApi){
                        Console.WriteLine("You enabled custom api,please disable.");
                        break;
                    }

                    Globals.Japan = !Globals.Japan;
                                               
                    Globals.FixedStoreList.Clear();
                    Globals.FixedModel.Clear();
                    if(Globals.Japan) Console.WriteLine("Enabling Japan Mode.");
                    else Console.WriteLine("Disabling Japan Mode.");
                    break;

                case ConsoleKey.Spacebar:
                    Console.WriteLine(Globals.PrintVar());
                    break;
                case ConsoleKey.Escape:
                    Console.WriteLine("Exiting...");
                    Globals.Running = false;
                    break;
            }
        }
    }
    #endregion

    int attempt = 0;
    while (!stockTask.IsCompleted)
    {
        if(attempt >= 10)
        {
            Environment.Exit(-1);
        }

        Console.WriteLine("Waiting for stockTask Exit.");
        attempt++;
        Thread.Sleep(1000);
    }
}

return;