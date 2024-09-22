using System.Net;
using Newtonsoft.Json;
using System.Xml;

namespace Avability2
{
    public static class Globals
    {

        /// <summary>
        /// 程序是否还活着
        /// </summary>
        public static bool Running = true;

        /// <summary>
        /// 详细打印模式，在控制台按F1触发
        /// </summary>
        public static bool Verbose = false;

        /// <summary>
        /// 日区模式，盯日本SKU用
        /// </summary>
        public static bool Japan = false;
        
        /// <summary>
        /// 固定查询商店列表
        /// </summary>
        public static List<string> FixedStoreList = new List<string>{ };

        /// <summary>
        /// 固定查询型号列表
        /// </summary>
        public static List<string> FixedModel = new List<string>{ };

        public static List<string> IgnoreModel = new List<string>{ };

        public static bool ProMax = false;

        public static bool UseTgBot = false;
        public static string TgBotKey = "";
        public static string ChatRoomID = "";
        public static int Interval = 5000;
        public static string apiEndpoint = "https://www.apple.com.cn/shop/fulfillment-messages";
        public static string apiEndpoint_JP = "https://www.apple.com/jp/shop/fulfillment-messages";
        public static string apiEndpoint_Custom = "";
        public static bool UseCustomApi = false;

        public static bool UseDynamicTimer = false;
        public static int DynamicInterval = 5000;
        public static int Min_DynamicInt = 1000;
        public static int Max_DynamicInt = 5000;
        public static int Step_DynamicInt = 100;
        public static bool UseRandomStep = false;

        public static DateTime LastUpdateSuccessTime = DateTime.UtcNow;
        public static DateTime LastExceptionTime = DateTime.UtcNow;

        public static bool LastUpdateSuccess = true;

        static string lastExcp = "";
        public static string LastException { get { return lastExcp; } set { lastExcp = value;TotalException += 1; LastExceptionTime = DateTime.UtcNow; } }

        public static long TotalException = 0;
        public static long TotalRequests = 0;
        public static long TotalStocks = 0;

        // public static bool noBlue = false;
        public static bool showIneligible = false;

        public static string PrintVar()
        {
            return
                $"CurrentStatus:\n" +
                $"Running:{Running}\n" +
                $"Verbose:{Verbose}\n" +
                $"Japan:{Japan}\n" +
                $"ProMax:{ProMax}\n" +

                $"FixedStoreList:{String.Join(',', FixedStoreList)}\n" + 
                $"FixedModelList:{String.Join(',', FixedModel)}\n" +
                $"Interval:{Interval}\n" +
                $"UseDynamicTimer:{UseDynamicTimer}\n" +
                $"DynamicInterval:{DynamicInterval}\n" +
                $"Min_Int:{Min_DynamicInt}\n" +
                $"Max_Int:{Max_DynamicInt}\n" +
                $"Step_Int:{Step_DynamicInt}\n" +
                $"UseRandomStep:{UseRandomStep}\n" +
                $"LastUpdateSuccessTime:{LastUpdateSuccessTime}\n" +
                $"LastUpdateSuccess:{LastUpdateSuccess}\n" +
                $"LastExceptionTime:{LastExceptionTime}\n" + 
                $"LastException:{LastException}\n" +
                $"TotalException:{TotalException}\n" +
                $"TotalRequests:{TotalRequests}\n" +
                $"TotalStocks:{TotalStocks}\n"+
                $"IgnoreList:{string.Join(",",IgnoreModel)}\n" +
                $"UseTgBot:{UseTgBot}\n"+
                $"BotKey:{TgBotKey}\n"+
                $"ChatRoomID{ChatRoomID}";
        }
    }

    public static class Internals
    {
        public static Random rand = new Random(DateTime.UtcNow.Second);
        public static TgBot BotInstance = new TgBot(Globals.TgBotKey, Globals.ChatRoomID);

        //"https://reserve-prime.apple.com/CN/zh_CN/reserve/A/stores.json")
        public static string Request(string Url)
        {
            var data = "";

            try
            {
                if(Globals.Verbose)
                    Console.WriteLine("Request URL:" + Url);
                
                var req = WebRequest.CreateHttp(Url);
                req.Timeout = 5000;
                var resp = req.GetResponse() as HttpWebResponse;

                using (var respReader = new System.IO.StreamReader(resp.GetResponseStream()))
                {
                    data = respReader.ReadToEnd();
                }

                if(Globals.Verbose)
                    Console.WriteLine("Response:" + data);
                    
                return data;
            }
            catch(Exception e)
            {
                Globals.LastException = "Request:" + e.Message;
                return "";
            }

        }

        public static string RequestPost(string Url,string Post)
        {
            var data = "";
            try
            {
                if(Globals.Verbose)
                    Console.WriteLine("Request URL:" + Url);
                
                var req = WebRequest.Create(Url);
                req.Method = "POST";
                req.ContentType = "application/json";
                using (var sw = new StreamWriter(req.GetRequestStream()))
                {
                    sw.WriteLine(Post);
                    sw.Flush();
                    sw.Close();
                }
                var resp = req.GetResponse() as HttpWebResponse;
                using (var respReader = new System.IO.StreamReader(resp.GetResponseStream()))
                {
                    data = respReader.ReadToEnd();
                }

                if(Globals.Verbose)
                    Console.WriteLine("Response:" + data);

                return data;
            }
            catch(Exception e)
            {
                Console.WriteLine("RequestPost Excp: " + e.Message);
                Console.WriteLine("RequestPost Url: " + Url);
                return "";
            }
        }

        public static XmlDocument JsonToXmlDoc(string json) => JsonConvert.DeserializeXmlNode("{\"Root\":" + json + "}");

        #region 纪念曾经抢过的型号
        public static Dictionary<string, string> ModelList_14Pro = new Dictionary<string, string>()
        {
           {"MPXR3CH/A","14 PRO BLK 128" },
           {"MPXY3CH/A","14 PRO SLV 128" },
           {"MQ053CH/A","14 PRO GLD 128" },
           {"MQ0D3CH/A","14 PRO PUR 128" },

           {"MQ0M3CH/A","14 PRO BLK 256" },
           {"MQ0W3CH/A","14 PRO SLV 256" },
           {"MQ143CH/A","14 PRO GLD 256" },
           {"MQ1C3CH/A","14 PRO PUR 256" },

           {"MQ1J3CH/A","14 PRO BLK 512" },
           {"MQ1R3CH/A","14 PRO SLV 512" },
           {"MQ203CH/A","14 PRO GLD 512" },
           {"MQ263CH/A","14 PRO PUR 512" },

           {"MQ2D3CH/A","14 PRO BLK 1TB" },
           {"MQ2K3CH/A","14 PRO SLV 1TB" },
           {"MQ2R3CH/A","14 PRO GLD 1TB" },
           {"MQ2Y3CH/A","14 PRO PUR 1TB" },

           {"MQ833CH/A","14 PM BLK 128" },
           {"MQ843CH/A","14 PM SLV 128" },
           {"MQ853CH/A","14 PM GLD 128" },
           {"MQ863CH/A","14 PM PUR 128" },

           {"MQ873CH/A","14 PM BLK 256" },
           {"MQ883CH/A","14 PM SLV 256" },
           {"MQ893CH/A","14 PM GLD 256" },
           {"MQ8A3CH/A","14 PM PUR 256" },

           {"MQ8D3CH/A","14 PM BLK 512" },
           {"MQ8E3CH/A","14 PM SLV 512" },
           {"MQ8F3CH/A","14 PM GLD 512" },
           {"MQ8G3CH/A","14 PM PUR 512" },

           {"MQ8H3CH/A","14 PM BLK 1TB" },
           {"MQ8J3CH/A","14 PM SLV 1TB" },
           {"MQ8L3CH/A","14 PM GLD 1TB" },
           {"MQ8M3CH/A","14 PM PUR 1TB" }

        };

        public static Dictionary<string, string> ModelList_15Pro = new Dictionary<string, string>()
        {
            {"MTQ63CH/A","15Pro Orig 128G" },
            {"MTQ73CH/A","15Pro Blue 128G" },
            {"MTQ53CH/A","15Pro White 128G" },
            {"MTQ43CH/A","15Pro Black 128G" },

            {"MTQA3CH/A","15Pro Orig 256G" },
            {"MTQC3CH/A","15Pro Blue 256G" },
            {"MTQ93CH/A","15Pro White 256G" },
            {"MTQ83CH/A","15Pro Black 256G" },

            {"MTQF3CH/A","15Pro Orig 512G" },
            {"MTQG3CH/A","15Pro Blue 512G" },
            {"MTQE3CH/A","15Pro White 512G" },
            {"MTQD3CH/A","15Pro Black 512G" },

            {"MTQK3CH/A","15Pro Orig 1TB" },
            {"MTQL3CH/A","15Pro Blue 1TB" },
            {"MTQJ3CH/A","15Pro White 1TB" },
            {"MTQH3CH/A","15Pro Black 1TB" }
        };

        public static Dictionary<string, string> ModelList_15ProMax = new Dictionary<string, string>()
        {
            {"MU2Q3CH/A","15ProMax Orig 256G" },
            {"MU2R3CH/A","15ProMax Blue 256G" },
            {"MU2P3CH/A","15ProMax White 256G" },
            {"MU2N3CH/A","15ProMax Black 256G" },

            {"MU2V3CH/A","15ProMax Orig 512G" },
            {"MU2W3CH/A","15ProMax Blue 512G" },
            {"MU2U3CH/A","15ProMax White 512G" },
            {"MU2T3CH/A","15ProMax Black 512G" },

            {"MU603CH/A","15ProMax Orig 1TB" },
            {"MU613CH/A","15ProMax Blue 1TB" },
            {"MU2Y3CH/A","15ProMax White 1TB" },
            {"MU2X3CH/A","15ProMax Black 1TB" }
        };
        #endregion
        public static Dictionary<string,string> ModelList_16Pro = new Dictionary<string, string>()
        {
            {"MYLQ3CH/A","16Pro Dst 128" },
            {"MYLV3CH/A","16Pro Dst 256" },
            {"MYM23CH/A","16Pro Dst 512" },
            {"MYM73CH/A","16Pro Dst 1TB" },

            {"MYLR3CH/A","16Pro Org 128" },
            {"MYLW3CH/A","16Pro Org 256" },
            {"MYM43CH/A","16Pro Org 512" },
            {"MYM83CH/A","16Pro Org 1TB" },

            {"MYLP3CH/A","16Pro Wht 128" },
            {"MYLU3CH/A","16Pro Wht 256" },
            {"MYLY3CH/A","16Pro Wht 512" },
            {"MYM63CH/A","16Pro Wht 1TB" },

            {"MYLN3CH/A","16Pro Blk 128" },
            {"MYLT3CH/A","16Pro Blk 256" },
            {"MYLX3CH/A","16Pro Blk 512" },
            {"MYM53CH/A","16Pro Blk 1TB" }
        };

        public static Dictionary<string,string> ModelList_16ProMax = new Dictionary<string, string>()
        {
            {"MYTP3CH/A","16ProMax Dst 256" },
            {"MYTW3CH/A","16ProMax Dst 512" },
            {"MYW13CH/A","16ProMax Dst 1TB" },

            {"MYTQ3CH/A","16ProMax Org 256" },
            {"MYTX3CH/A","16ProMax Org 512" },
            {"MYW23CH/A","16ProMax Org 1TB" },

            {"MYTN3CH/A","16ProMax Wht 256" },
            {"MYTT3CH/A","16ProMax Wht 512" },
            {"MYW03CH/A","16ProMax Wht 1TB" },

            {"MYTM3CH/A","16ProMax Blk 256" },
            {"MYTR3CH/A","16ProMax Blk 512" },
            {"MYTY3CH/A","16ProMax Blk 1TB" }
        };

        #region 日版型号
        public static Dictionary<string, string> ModelList_15Pro_J = new Dictionary<string, string>()
        {
            {"MTU93J/A","15Pro Orig 128G" },
            {"MTUA3J/A","15Pro Blue 128G" },
            {"MTU83J/A","15Pro White 128G" },
            {"MTU73J/A","15Pro Black 128G" },

            {"MTUF3J/A","15Pro Orig 256G" },
            {"MTUG3J/A","15Pro Blue 256G" },
            {"MTUD3J/A","15Pro White 256G" },
            {"MTUC3J/A","15Pro Black 256G" },

            {"MTUK3J/A","15Pro Orig 512G" },
            {"MTUL3J/A","15Pro Blue 512G" },
            {"MTUJ3J/A","15Pro White 512G" },
            {"MTUH3J/A","15Pro Black 512G" },

            {"MTUT3J/A","15Pro Orig 1TB" },
            {"MTUU3J/A","15Pro Blue 1TB" },
            {"MTUR3J/A","15Pro White 1TB" },
            {"MTUQ3J/A","15Pro Black 1TB" }
        };

        public static Dictionary<string, string> ModelList_15ProMax_J = new Dictionary<string, string>()
        {
            {"MU6R3J/A","15ProMax Orig 256G" },
            {"MU6T3J/A","15ProMax Blue 256G" },
            {"MU6Q3J/A","15ProMax White 256G" },
            {"MU6P3J/A","15ProMax Black 256G" },

            {"MU6W3J/A","15ProMax Orig 512G" },
            {"MU6X3J/A","15ProMax Blue 512G" },
            {"MU6V3J/A","15ProMax White 512G" },
            {"MU6U3J/A","15ProMax Black 512G" },

            {"MU713J/A","15ProMax Orig 1TB" },
            {"MU723J/A","15ProMax Blue 1TB" },
            {"MU703J/A","15ProMax White 1TB" },
            {"MU6Y3J/A","15ProMax Black 1TB" }
        };

        public static Dictionary<string,string> ModelList_16Pro_J = new Dictionary<string, string>()
        {
            {"MYMX3J/A","16Pro Dst 128" },
            {"MYN23J/A","16Pro Dst 128" },
            {"MYN63J/A","16Pro Dst 512" },
            {"MYNA3J/A","16Pro Dst 1TB" },

            {"MYMY3J/A","16Pro Org 128" },
            {"MYN33J/A","16Pro Org 256" },
            {"MYN73J/A","16Pro Org 512" },
            {"MYNC3J/A","16Pro Org 1TB" },

            {"MYMW3J/A","16Pro Wht 128" },
            {"MYN13J/A","16Pro Wht 256" },
            {"MYN53J/A","16Pro Wht 512" },
            {"MYN93J/A","16Pro Wht 1TB" },

            {"MYMV3J/A","16Pro Blk 128" },
            {"MYN03J/A","16Pro Blk 256" },
            {"MYN43J/A","16Pro Blk 512" },
            {"MYN83J/A","16Pro Blk 1TB" }
        };
        public static Dictionary<string,string> ModelList_16ProMax_J = new Dictionary<string, string>()
        {
            {"MYWJ3J/A","16ProMax Dst 128" },
            {"MYWN3J/A","16ProMax Dst 512" },
            {"MYWT3J/A","16ProMax Dst 1TB" },

            {"MYWK3J/A","16ProMax Org 256" },
            {"MYWP3J/A","16ProMax Org 512" },
            {"MYWU3J/A","16ProMax Org 1TB" },

            {"MYWH3J/A","16ProMax Wht 256" },
            {"MYWM3J/A","16ProMax Wht 512" },
            {"MYWR3J/A","16ProMax Wht 1TB" },

            {"MYWG3J/A","16ProMax Blk 256" },
            {"MYWL3J/A","16ProMax Blk 512" },
            {"MYWQ3J/A","16ProMax Blk 1TB" }
        };
        #endregion

        
        public static Dictionary<string, string> ModelList
        {
            get
            {
                if(Globals.Japan){
                    //JP
                    if(Globals.ProMax) return ModelList_16ProMax_J;
                    else return ModelList_16Pro_J.Concat(ModelList_16ProMax_J).ToDictionary((a) => a.Key, (b) => b.Value);
                }else{
                    //CN
                    if(Globals.ProMax) return ModelList_16ProMax;
                    else return ModelList_16Pro.Concat(ModelList_16ProMax).ToDictionary((a) => a.Key, (b) => b.Value);
                }
            }
        }

        public static void UpdateDynamicInterval()
        {
            if (Globals.UseDynamicTimer)
            {
                if (Globals.UseRandomStep)
                {
                    Globals.Step_DynamicInt = rand.Next(100, 500);
                    if (Globals.Verbose)
                        Console.WriteLine("Update DynamicTimer Step:{0}", Globals.Step_DynamicInt);
                }

                if (Globals.LastUpdateSuccess)
                {
                    Globals.DynamicInterval -= Globals.Step_DynamicInt;
                    if (Globals.DynamicInterval <= Globals.Min_DynamicInt)
                        Globals.DynamicInterval = Globals.Min_DynamicInt;

                    if (Globals.Verbose)
                        Console.WriteLine("Update DynamicTimer Interval:(Dn){0}", Globals.DynamicInterval);
                }
                else
                {

                    Globals.DynamicInterval += Globals.Step_DynamicInt;
                    if (Globals.DynamicInterval >= Globals.Max_DynamicInt)
                        Globals.DynamicInterval = Globals.Max_DynamicInt;

                    if (Globals.Verbose)
                        Console.WriteLine("Update DynamicTimer Interval:(Up){0}", Globals.DynamicInterval);
                }
            }
        }

        public static string HandleArgument(string input)
        {
            var argPart = input.Split(':');
            if (argPart.Length == 2) return argPart[1].Trim();
            return "";
        }

        public static bool TryHandleArgument(string input,string[] expected,out string param, bool haveParam = true)
        {
            param = "";
            foreach(string except in expected)
            {
                if (input.ToLower().StartsWith(except))
                {
                    if (haveParam)
                    {
                        var argPart = input.Split(':');
                        if (argPart.Length == 2)
                        {
                            param = argPart[1].Trim();
                            return true;
                        }
                    }
                    else return true;
                }
            }
            return false;
        }
    }

}

