using System.Linq;
namespace AvabilityTest;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void Test_StoreInfo(){
        var stores = new StoreInfo();

        //你得获得到一个商店列表
        Assert.IsTrue(stores.Update());

        //这个商店列表肯定不能为空
        Assert.AreNotEqual(stores.ReturnStoreID().Length, 0);
    }

    [TestMethod]
    public void Test_ModelInfo_iPhone16(){
        //型号列表不能为空
        Assert.AreNotEqual(Internals.ModelList.Count, 0);
        foreach(var models in Internals.ModelList){
            //查17Pro库存，所以型号名字肯定是17Pro
            Assert.IsTrue(models.Value.StartsWith("17Pro"));
            Console.WriteLine("{0} - {1}",models.Key,models.Value);
        }
    }

    [TestMethod]
    public void Test_GetStock(){
        //查询昆明店iPhone SE3的库存，因为SE3，所以必定成功
        //2025.9.12 现在是iPhone 16e
        using (StockInfo stockHandler = new StockInfo())
        {
            stockHandler.GetStocks("R670", new string[] { "MD2C4CH/A" });

            //请求肯定得成功
            Assert.IsTrue(Globals.LastUpdateSuccess);

            //库存绝对加1
            Assert.AreNotEqual(Globals.TotalStocks, 0);
        }

    }

    // [TestMethod]
    // public void Test_TgBot(){
    //     Assert.AreNotEqual(Globals.TgBotKey,"");
    //     Assert.AreNotEqual(Globals.ChatRoomID,"");

    //     TgBot BotInstance_Test = new TgBot(Globals.TgBotKey, Globals.ChatRoomID);
    //     BotInstance_Test.SendGroupMessage("Testing TgBot");
    // }
}