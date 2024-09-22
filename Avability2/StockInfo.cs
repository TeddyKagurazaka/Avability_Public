using System.Xml;

namespace Avability2
{
    public class StockInfo : IDisposable
    {
        StreamWriter sw;

        public delegate void MessageHandler(string message);
        //public MessageHandler onSendingMessage;

        public bool lastRequestSuccess { get; private set; }
        //public string lastRequestData { get; private set; }

        public StockInfo()
        {
            //lastRequestData = "";
            lastRequestSuccess = true;

            sw = new StreamWriter("Stocks2.csv", true);
            sw.AutoFlush = true;
        }

        public void Dispose()
        {
            sw.Flush();
            sw.Close();
        }

        public void LoadStock(string storeID) => GetStocks(storeID, Internals.ModelList.Keys.ToArray());

        public void GetStocks(string storeID, IEnumerable<string> productIDs)
        {
            //https://www.apple.com/jp/shop/fulfillment-messages?pl=true&mts.0=regular&parts.0=MQ9C3J/A&location=530-0001
            var url = (Globals.UseCustomApi ? Globals.apiEndpoint_Custom : (Globals.Japan ? Globals.apiEndpoint_JP : Globals.apiEndpoint)) + "?store=" + storeID;
            int id = 0;
            foreach (var partID in productIDs)
            {
                url += string.Format("&parts.{0}={1}", id, partID);
                id++;
            }

            var resp = Internals.Request(url);
            if (string.IsNullOrEmpty(resp))
            {
                Console.WriteLine("[{1}]Request for {0} fail.(Empty Response)", storeID, DateTime.UtcNow.ToString());
                lastRequestSuccess = false;
                Globals.LastUpdateSuccess = false;
            }
            else if (resp.Contains("Service Unavailable"))
            {
                Console.WriteLine("[{1}]Request for {0} fail(503).", storeID, DateTime.UtcNow.ToString());
                lastRequestSuccess = false;
                Globals.LastUpdateSuccess = false;
            }
            else
            {
                lastRequestSuccess = true;
                //lastRequestData = resp;

                Globals.LastUpdateSuccessTime = DateTime.UtcNow;
                Globals.LastUpdateSuccess = true;
                //Globals.LastRequest = resp;

                ProcessStocks(resp);
            }

            Globals.TotalRequests += 1;
            Internals.UpdateDynamicInterval(lastRequestSuccess);
        }

        void ProcessStocks(string json)
        {
            string stocksText = "";
            try
            {
                var tempJson = Internals.JsonToXmlDoc(json);
                var storesInfo = tempJson.SelectSingleNode("/Root/body/content/pickupMessage/stores") as XmlElement;
                if(storesInfo == null){
                    var errorMessage = tempJson.SelectSingleNode("/Root/body/content/pickupMessage/errorMessage") as XmlElement;
                    Console.WriteLine("[{0}]{1}",DateTime.UtcNow.ToString(),errorMessage.InnerText);
                    return;
                }

                var storeName = storesInfo!.SelectSingleNode("storeName")!.InnerText;
                var storeCity = storesInfo.SelectSingleNode("city").InnerText;

                var partsAvabList =
                    tempJson.SelectSingleNode("/Root/body/content/pickupMessage/stores/partsAvailability");
                if (partsAvabList == null)
                {
                    Console.WriteLine("partsAvabList == null");
                    return;
                }

                foreach (XmlElement partsAvailable in partsAvabList.ChildNodes)
                {
                    var modelNumber = partsAvailable.SelectSingleNode("partNumber").InnerText;
                    var isAvailable = partsAvailable.SelectSingleNode("pickupDisplay").InnerText;
                    var partName = Internals.ModelList.ContainsKey(modelNumber)
                        ? Internals.ModelList[modelNumber]
                        : modelNumber;
                    //Console.WriteLine(string.Format("{0}:{1}", modelNumber, isAvailable));

                    if (isAvailable.ToLower() == "available" ||     //真有货   
                        (Globals.showIneligible && isAvailable.ToLower() == "ineligible"))      //应该是有货 但是不给约
                    {

                            #region 曾经的15蓝色
                            // if(!(Globals.noBlue && partName.Contains("Blue")))
                            // {
                            //     stocksText += string.Format("{0},{1},{2},{3},{4},{5}\n",
                            //     DateTime.UtcNow.ToString(),
                            //     storeCity,
                            //     storeName,
                            //     modelNumber,
                            //     partName,
                            //     isAvailable.ToLower());

                            //     Console.WriteLine("[{0}]{1}/{2} Found Stock:{3}({4})({5})",
                            //         DateTime.UtcNow.ToString(),
                            //         storeCity,
                            //         storeName,
                            //         modelNumber,
                            //         partName,
                            //         isAvailable.ToLower()
                            //     );
                            //     Globals.TotalStocks += 1;
                            // }
                            // else
                            // {

                            //     Console.WriteLine("[{0}]{1}/{2} Found Stock:{3}({4})({5}) But it is blue,ignoring.",
                            //         DateTime.UtcNow.ToString(),
                            //         storeCity,
                            //         storeName,
                            //         modelNumber,
                            //         partName,
                            //         isAvailable.ToLower()
                            //     );
                            //     Globals.TotalStocks += 1;
                            // }
                            #endregion

                            if(Globals.IgnoreModel.Contains(modelNumber))
                            {
                                Console.WriteLine("[{0}]{1}/{2} Found Stock:{3}({4})({5}) But set ignored,ignoring.",
                                    DateTime.UtcNow.ToString(),
                                    storeCity,
                                    storeName,
                                    modelNumber,
                                    partName,
                                    isAvailable.ToLower()
                                );

                                Globals.TotalStocks += 1;
                            }
                            else{
                                stocksText += string.Format("{0},{1},{2},{3},{4},{5}\n",
                                    DateTime.UtcNow.ToString(),
                                    storeCity,
                                    storeName,
                                    modelNumber,
                                    partName,
                                    isAvailable.ToLower());

                                Console.WriteLine("[{0}]{1}/{2} Found Stock:{3}({4})({5})",
                                    DateTime.UtcNow.ToString(),
                                    storeCity,
                                    storeName,
                                    modelNumber,
                                    partName,
                                    isAvailable.ToLower()
                                );
                                Globals.TotalStocks += 1;
                            }
                            
                    }
                    if(Globals.Verbose)
                        Console.WriteLine("[{0} {1} {2}] Checking {3}...", DateTime.UtcNow.ToString(), storeCity,storeName,modelNumber);

                    if (Globals.Verbose)
                        Console.WriteLine("[{0}]{1}/{2} {3}({4}):{5}",
                            DateTime.UtcNow.ToString(),
                            storeCity,
                            storeName,
                            modelNumber,
                            partName,
                            isAvailable
                        );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[{0}]Response handle fail detected.", DateTime.UtcNow.ToString());
                File.WriteAllText(DateTime.UtcNow.Ticks + ".json",
                    string.Format(
                        "Excp: {0}\r\n\r\nTrace:{1}\r\n\r\nJson:\r\n{2}",
                        e.Message,
                        e.StackTrace, json)
                );

                Globals.LastException = "Handle:" + e.Message;
            }
            finally
            {
                if (!string.IsNullOrEmpty(stocksText))
                {
                    if (Globals.UseTgBot)
                    {
                        //用委托可能会塞死 不然还是走独立线程
                        //onSendingMessage(stocksText);
                        //onSendingMessage(stocksText);
                        new Thread(new ThreadStart(() =>
                        {
                           Internals.BotInstance.SendGroupMessage(stocksText);
                           //Internals.SendTgMessage(stocksText);
                        })).Start();
                    }

                    sw.WriteLine(stocksText);
                    sw.Flush();
                }
            }
        }
    }
}