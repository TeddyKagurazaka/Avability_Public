using System.Xml;

namespace Avability2
{
    public class TgBot
    {
        public class MessageModel
        {
            public string chat_id = "";
            public string text = "";
            public string parse_mode = "HTML";

            public MessageModel(string ChatID,string Text) { 
                chat_id = ChatID;
                text = Text;
            }
        }

        string apiKey = "";
        string globalChatRoomID = "";

        string botApi = "https://api.telegram.org/bot";

        string msgApi { get => botApi + apiKey + "/sendMessage"; }

        string updateApi { get => botApi + apiKey + "/getUpdates"; }
        int updateId = 0;
        Task handleMsgTask;



        public TgBot(string key, string GlobalChatRoomID)
        {
            apiKey = key;
            globalChatRoomID = GlobalChatRoomID;
            if (Globals.UseTgBot)
            {
                handleMsgTask = new Task(new Action(HandleIncomingMessage));
                handleMsgTask.Start();
            }
            else { handleMsgTask = new Task(() => { }); }
        }

        public void SendTgMessage(string content, string chatID = "")
        {
            var resp = Internals.RequestPost(
                    msgApi,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new MessageModel(chatID,content))
                );

            if (string.IsNullOrEmpty(resp))
                Console.WriteLine("Tg send message fail.");

        }

        public void SendGroupMessage(string content) => SendTgMessage(content,globalChatRoomID);

        public void HandleIncomingMessage()
        {
            var req = "";
            while (Globals.Running)
            {
                try
                {
                    req = Internals.Request(updateApi + "?offset=" + updateId.ToString());
                    if (!string.IsNullOrEmpty(req))
                    {
                        var resp = Internals.JsonToXmlDoc(req);
                        foreach (XmlElement statuses in resp.SelectNodes("/Root/result"))
                        {
                            var newIDNode = statuses.SelectSingleNode("update_id");
                            if (newIDNode != null)
                            {
                                if (int.TryParse(newIDNode.InnerText, out updateId))
                                {
                                    updateId += 1;
                                }
                                else Console.WriteLine("Update updateID failed.");
                            }

                            //绕过群组信息
                            var msgNode = statuses.SelectSingleNode("message");
                            if (msgNode != null)
                            {
                                //发现个人信息
                                var chatId = msgNode.SelectSingleNode("chat/id").InnerText;
                                var chatText = msgNode.SelectSingleNode("text").InnerText.Split(' ');
                                if(chatText.Length >= 1){
                                    var cmd = chatText[0];
                                    switch(cmd.ToLower()){
                                        case "/status":
                                            SendTgMessage(Globals.PrintVar(), chatId);
                                            break;
                                            
                                        case "/update":
                                            if(chatText.Length >= 3){
                                                var prm1 = chatText[1];
                                                var prm2 = chatText[2].Split(',');
                                                var prmValid = true;
                                                if(prm2.Length == 0){
                                                    SendTgMessage("param not valid.",chatId);
                                                    break;
                                                }

                                                if(Globals.UseCustomApi){
                                                    SendTgMessage("You enabled custom api,this option doesn't work.",chatId);
                                                    break;
                                                }

                                                switch(prm1.ToLower()){
                                                    case "store":
                                                        if(prm2[0].ToLower() == "clear"){
                                                            Globals.FixedStoreList.Clear();
                                                            //Globals.UseFixedModel = false;
                                                            SendTgMessage("StoreList Cleared,will request store list on next update.",chatId);
                                                            break;
                                                        }

                                                        foreach(var param2 in prm2){
                                                            if(!param2.StartsWith("R"))
                                                            {
                                                                SendTgMessage(param2 + " not start with R,really?",chatId);
                                                                prmValid = false;
                                                            }
                                                        }

                                                        if(prmValid){
                                                            SendTgMessage("StoreList updated to:" + string.Join(",",prm2), chatId);
                                                            Globals.FixedStoreList.Clear();
                                                            Globals.FixedStoreList.AddRange(prm2);
                                                            //Globals.UseFixedStore = true;
                                                        }
                                                        break;
                                                    case "model":
                                                        if(prm2[0].ToLower() == "clear"){
                                                            lock(Globals.FixedModel){
                                                                Globals.FixedModel.Clear();
                                                            }
                                                            //Globals.UseFixedModel = false;
                                                            SendTgMessage("ModelList Cleared.",chatId);
                                                            break;
                                                        }

                                                        if(prmValid){
                                                            SendTgMessage("ModelList updated to:" + string.Join(",",prm2), chatId);
                                                            lock(Globals.FixedModel){
                                                                Globals.FixedModel.Clear();
                                                                Globals.FixedModel.AddRange(prm2);
                                                            }
                                                            //Globals.UseFixedModel = true;
                                                        }
                                                        break;
                                                    default:
                                                        SendTgMessage("what??", chatId);
                                                        break;
                                                }
                                            }
                                            else SendTgMessage("blah??", chatId);

                                            break;
                                        case "/ignore":
                                            if(chatText.Length >= 3){
                                                var prm1 = chatText[1];
                                                var prm2 = chatText[2].Split(',');

                                                switch(prm1.ToLower()){
                                                    case "clear":
                                                        Globals.IgnoreModel.Clear();
                                                        SendTgMessage("Ignore list cleared.",chatId);
                                                        break;
                                                    case "add":
                                                        if(prm2.Length > 0){
                                                            Globals.IgnoreModel.AddRange(prm2);
                                                            SendTgMessage("added ignore list:" + string.Join(",",prm2),chatId);
                                                        }else SendTgMessage("no Ignore List sent:" + prm2,chatId);
                                                        break;
                                                }

                                            }else
                                                SendTgMessage("blah??", chatId);

                                            break;
                                        case "/japan":
                                            if(Globals.UseCustomApi){
                                                SendTgMessage("You enabled custom api,this option doesn't work.",chatId);
                                                break;
                                            }

                                            Globals.Japan = !Globals.Japan;

                                            Globals.FixedStoreList.Clear();
                                            Globals.FixedModel.Clear();

                                            if(Globals.Japan) SendTgMessage("Changed to Japan mode.",chatId);
                                            else SendTgMessage("Changed to China mode.",chatId);

                                            break;
                                        case "/promax":
                                            Globals.ProMax = !Globals.ProMax;

                                            if(Globals.ProMax) SendTgMessage("Changed to ProMax Mode.",chatId);
                                            else SendTgMessage("Changed to Pro Mode.",chatId);

                                            break;
                                        default:
                                                SendTgMessage("blah?", chatId);
                                            break;
                                    }
                                }
                                else SendTgMessage("blah.", chatId);
                            }
                        }
                    }
                }catch(Exception e)
                {
                    Console.WriteLine("Tgbot Exception:" + e.ToString());
                    Globals.LastException = e.ToString();
                }
                Task.Delay(5000);
            }
        }
    }
}

