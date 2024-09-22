# Avability_Public

**大爷，该抢果子啦！**

# Currently Supported

- 查 大陆/日本 商店 果子库存
- 查 指定型号/指定商店 果子库存
- 通过自定义API 查全球果子库存
- Telegram 机器人支持

# Usage

- dotnet run Avability2 *-这里写你想要的参数-*
程序运行后会输出所有现货库存，并记录到Stocks2.csv文件内

# Advanced Usage(e.g. Telegram Bot)

- 准备好 *Telegram Bot API Key* 和一个 *Chatroom ID*(用于发送推送).
- 设置环境变量 **TgBotKey** 为你的API Key， **TgChatRoomID** 为你的Chatroom ID
- dotnet run Avability2 --bot *-这里写你想要的参数-*

# Params:

- --stores:/-s: 
  
  提供指定店铺的ID(如 R670(昆明))，只查询该店的库存，以逗号分割。

  (仅限该区所属商店，默认为中国，见下面)
- --models:/-m: 
  
  提供指定SKU的ID(如 MMWX3CH/A(iPhone SE 3))，只查询该产品库存，以逗号分割。

  (在已知SKU型号的情况下可以查询苹果店在售所有商品的库存，仅限该区商店有售商品，区域默认为中国，见下面)
- --promax/-pm 
  
  只查询ProMax库存，Bot已自带Pro/ProMax的SKU，无需再指定查询型号。
- --timer:/t: 
  
  指定查询间隔（以毫秒为单位），默认为5秒。

  (查询过快会导致服务器429，并影响同局域网下设备查询库存和结账，建议不要低于5秒)
- --api: 
  自己设置API，如果你知道其他服的 shop/fulfillment-messages 地址，配合-s: 和-m: 可查询其他地区库存

  (由于不能跨区查库存，设定此选项必须同时设定-s:和-m:，否则无法查询) 
  
  **(使用自定义api时，不再支持 切换Pro/ProMax 以及 日本/中国 API 功能。)**
- --japan 
  
  查询日本库存，Bot已自带查询日本库存的API和SKU，无需再次指定。
- --bot
  
  启动Telegram Bot通知功能，需要自己在环境变量里指定API Key和Chatroom ID，否则不启动。(参照上面)
- --dynamictimer/-dtimer 
  
  激活动态间隔 (在遇到服务器429时会自动放宽查询间隔)
- --dynamicparam:/-dparam: 
  
  设置动态间隔，格式为 最小间隔,间距间隔,最大间隔 以逗号分隔，单位为毫秒

  (根据设置的间隔，在遇到429时Bot会自动增加间距间隔直到达到最大间隔，在成功请求时自动减小间隔直到最小间隔)
- --dynamicrandom/-drand 
  
  间距间隔采用随机间隔（随机100到500毫秒）
- --ineligible 
  
  在提示"不可在 该直营店 取货"时也发送通知，有时候可能只是蓝精灵排满

# Params(On Console):
程序启动之后，在控制台窗口可以进行以下操作:
- F1: Verbose模式，打印所有请求/回传/查询结果(包括无货)
- F2: 显示动态间隔情况
- F3: 切换ProMax模式/普通模式
- F4: 切换日服模式/国服模式，切换会强制清空指定型号/指定商店，并在下次更新时重新获取商店信息，
- 空格: 打印当前状态
- Esc: 退出程序

# Params(Using Telegram Bot:)
程序启动以后，可以对Bot私聊以下命令:

- /status: 
  
  打印当前状态

- /update store/model *param* 
  
  更新指定查询的商店/型号，若param为clear则清除指定，等同-s:/-m:。

- /ignore clear/add *param* 
  
  指定忽略的型号，这部分型号只记录库存并不发消息通知，以逗号分隔。

  clear时需要任意输入作为param，否则不识别。

- /japan 
  
  切换日服模式/国服模式，切换会强制清空指定型号/指定商店，并在下次更新时重新获取商店信息

- /promax 
  
  切换ProMax模式/普通模式

