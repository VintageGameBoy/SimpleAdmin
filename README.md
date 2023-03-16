<div align="left"><h1 align="center">SimpleAdmin</a></h1></div>
<div align="center"><h4 align="center">✨没有花里胡哨，只有简单、稳定、高效✨</h4></div>


### 如果您觉得有帮助，请点右上角 "Star" 支持一下谢谢

## 🎨框架介绍🎨
💥SimpleAdmin一个小而美的通用业务型后台管理系统。前端基于小诺2.0版本二次开发,采用Vue3+Vite+Vuex+JS,并在此基础上增加更人性化功能,后端采用.NET6/7+Furion,ORM采用Sqlsugar+单例模式,基于Redis的分布式缓存，采用RBAC+多机构的权限管理模式，实现全网最灵活的接口级别数据权限控制，集成国密加解密，导入导出、导入导出、批量修改等常用插件。后端代码注释覆盖率>90%，并配备手摸手教学文档，非常适合二次开发。将日常开发中的业务场景和框架紧密结合，并坚持以人为本,以业务为中心，做到开箱即用,代码简洁、易扩展，注释详细，文档齐全，让你的开发少走弯路。

## 🍕选择SimpleAdmin的N个理由🍕

#### 📕前端采用Vue3+JavaScript
本系统是基于前端基于小诺2.0版本二次开发,采用Vue3+Vite+Vuex+JS,并在此基础上增加更人性化功能。市面上主流的Vue3框架大部分都是采用`TypeScript`开发，虽然是官方推荐，但是也增加了一些学习成本，如果您之前使用的事vue2+js的开发方式，那么再使用本系统就非常的好上手，也不用担心vue2打包之后第一次加载巨慢的问题。


#### 📗后台基于Furion脚手架
后端基于Furion脚手架搭建。Furion是目前.NET最好用的后端框架之一,有着全网最详细的使用文档和说明，作为新手或二次开发也能很快的上手，你想要的功能基本都能在Furion中找到。而且只要提issues作者基本上都能快速解答，没有后顾之忧。
##### 🍎 Furion框架特点
- 全新面貌：基于 `.NET5/6/7+` 平台，没有历史包袱
- 极少依赖：框架只依赖两个第三方包
- 极易入门：只需要一个 `Inject()` 即可完成配置
- 极速开发：内置丰富的企业应用开发功能
- 极其灵活：轻松面对多变复杂的需求
- 极易维护：采用独特的架构思想，只为长久维护设计
- 完整文档：提供完善的开发文档
- **跨全平台：支持所有主流操作系统及 .NET 全部项目类型**

文档地址:[https://dotnetchina.gitee.io/furion](https://dotnetchina.gitee.io/furion)

源码地址:[https://gitee.com/dotnetchina/Furion](https://gitee.com/dotnetchina/Furion)

#### 🍭ORM基于Sqlsugar
SqlSugar是一款老牌.NET开源ORM框架，由果糖大数据科技团队维护和更新 ，开箱即用
最易上手的ORM框架，本系统也是基于Sqlsugar单例模式+CodeFirst+仓储的结构，无需担心作用域问题，直接爽撸！

文档地址:[https://www.donet5.com/Home/Doc](https://www.donet5.com/Home/Doc)

源码地址:[https://gitee.com/dotnetchina/SqlSugar](https://gitee.com/dotnetchina/SqlSugar)

#### 📘基于Redis的分布式缓存
本系统使用了大量的缓存操作，一些基础配置和用户权限信息都放在了缓存中，用户首次登录后，下次再登录接口耗时实测`10-30ms`。
<img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/redis1.png"/>
<img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/redis2.png"/>

Redis客户端使用的是我基于[NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis)二次封装的[SimpleRedis](https://gitee.com/zxzyjs/SimpleRedis.git)。简化了注入操作，更方便使用。

##### 🍎NewLife.Redis特性
* 在ZTO大数据实时计算广泛应用，200多个Redis实例稳定工作一年多，每天处理近1亿包裹数据，日均调用量80亿次
* 低延迟，Get/Set操作平均耗时200~600us（含往返网络通信）
* 大吞吐，自带连接池，最大支持1000并发 
* 高性能，支持二进制序列化

#### 🧱接口级别的数据权限范围
作为一个<font color="red">后台管理系统</font>，一个<font color="red">权限管理框架</font>,权限设计作为一个管理系统的灵魂，是一个系统好不好用的关键。本系统是采用RBAC+多机构的权限管理模式，并实现全网最灵活的接口级别数据权限控制，可以指定某个角色的某个接口的数据权限范围，非常的灵活。除此之外还支持给用户单独授权，让用户脱离所有角色之外，实现灵活再灵活的数据权限。
<img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/接口数据权限.png"/>

在系统中，获取数据权限非常简单，只需一个方法搞定。
```cs
//获取数据范围
var dataScope = await _sysUserService.GetLoginUserApiDataScope();
```

#### 🍢支持多种主键类型
系统默认采用`long`类型的`雪花ID`作为系统主键，但是在交流过程中发现，部分小伙伴使用`string`类型或者`guid`作为系统主键，还有的是部分老系统用的是string类型，但是simpleadmin确是`long`类型，所以根据这个业务场景，本框架也支持`String`类型的系统主键，只需要拉取对应的`stringid`分支即可，完全不需要另外去修改代码，并且也能和master分支进行最新的同步。

#### 📞支持Mqtt/Signalr的即时通讯
作为前后端分离项目,前后端交互是一个非常重要的功能。目前主流框架都是通过Socket实现，本系统自然也是实现了基于<font color="red">Signalr</font>的前后端交互，并在此基础上实现了基于<font color="red">MQTT</font>的前后端交互功能，MQTT相比socket业务场景更多更灵活，在物联网方向有着非常多的应用。.NET应用在工业物联网方向也是有很多的岗位，学习MQTT还是非常有必要的。

MQTT客户端使用的是我基于[NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT)二次封装的[SimpleMQTT](https://gitee.com/zxzyjs/SimpleMQTT.git)。采用单例模式，可以动态的添加和删除mqtt客户端，非常好用。

文档地址：[https://www.cnblogs.com/huguodong/p/16991947.html](https://www.cnblogs.com/huguodong/p/16991947.html)

#### 🔧代码生成器
大部分项目里，其实有很多代码是重复的，几乎每个模块都有 CRUD 增删改查的功能，而这些功能的实现代码往往是大同小异的。如果这些功能都要自己去手写，非常无聊枯燥，浪费时间且效率很低，还可能会写错。代码生成功能通过选择数据库表，完成单表的增删改查管理功能，可以生成包括前端、后端、和SQL语句，支持预览的同时还支持ZIP压缩包和直接生成到项目两种模式，解放你的双手，大大减少了重复代码的编写,无需复制提升开发效率。
<img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/代码生成.png"/>

#### 🎺常用功能组件化
在系统一些常用的功能直接封装成vue组件，如批量导入，批量导出，批量删除，批量编辑等,只需按照规范传入相应参数就可以很好的实现功能，减少了很多不必要的前端代码。最典型的就是批量导入组件，只需传入api接口,通过后端接口自动生成表头，并实现字段级别的错误信息提示和预览，非常好用。
<img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/16.png"/>

#### 📖最详细的注释和使用文档
作为一个开源项目，如何让使用者看懂代码是非常重要的，很多作者会忽视这一点，导致我们阅读其代码的时候很难理解其为什么要这么写,如果没有详细的文档，想要二次开发需要花精力去研究源码。而本系统完全不用担心这个问题，后端源码注释覆盖率超过<font color="#dd0000">90%</font><br />,每一个方法,每一步都有详细的解释和说明，
并且推出了手摸手教学文档，目的就是让使用者能够轻易上手，就算不是自己的代码，根据注释和文档也能轻易看懂和二次开发,不用每天在群里问一些非常基础的问题，节约了大量的开发时间。
<img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/wendang.png"/>

#### 📖关于系统的问题第一时间解决
如果您在使用过程中遇到问题，或者发现bug，可以直接发issues描述您的问题，只要是系统bug第一时间修复。或者您对当前系统有什么建议或意见，也欢迎您提出，只要是符合大部分业务场景的都会考虑支持。我们也有和谐内部的qq/微信交流群，没有像其他系统qq群一样每天都是一堆启动报错问题，页面错误问题，有的只是愉快的吹水及系统功能的讨论和优化方案，因个人精力有限，暂不对外开发，需要获取前端授权之后方可加入，因为我发现适当的增加门槛对群的氛围很有帮助，可以减少很多没有意义的提问。

## 📺效果图📺
<table>
    <tr>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/1.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/2.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/3.png"/></td>
    </tr>
      <tr>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/4.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/5.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/6.png"/></td>
    </tr>
      <tr>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/7.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/8.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/9.png"/></td>
    </tr>
      <tr>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/10.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/11.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/12.png"/></td>
    </tr>
      <tr>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/13.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/14.png"/></td>
       <td><img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/15.png"/></td>
    </tr>

</table>

## 🚑快速启动🚑
#### 🚀启动前端

如果没有安装 Node.js 16，下载地址：[https://nodejs.org](https://nodejs.org)

```
npm install
```
```
npm run dev
```
#### 🛩️启动后端
编辑`Core.Development.json`文件,配置Redis地址和数据库地址,设置`SimpleAdmin.Web.Entry`为启动项目，直接启动项目即可。

<img src="https://gitee.com/zxzyjs/SimpleAdmin/raw/master/doc/Image/后端启动.png"/>

## 🎞️演示地址
1. 地址1：[http://admin.zxzyjs.com](http://admin.zxzyjs.com)
2. 地址2：[http://121.43.166.188:12345](http://121.43.166.188:12345)
3. 账号：superAdmin
4. 密码：123456

## 分支说明

- master 
正式稳定版本，具体版本升级内容看更新标签

- dev 
开发的分支（代码可能随时会推，不保证运行和使用）

- stringid
字符串类型主键分支,系统主键用string类型的使用此分支

## 🎈相关连接🎈

更新日志：[点击查看](https://gitee.com/zxzyjs/SimpleAdmin/commits/master)

文档地址：[https://www.cnblogs.com/huguodong/p/17021233.html](https://www.cnblogs.com/huguodong/p/17021233.html)

常见问题合集：[https://www.cnblogs.com/huguodong/p/17021241.html](https://www.cnblogs.com/huguodong/p/17021241.html)

## 🔖友情链接🔖
- 👉 Furion：[https://dotnetchina.gitee.io/furion](https://dotnetchina.gitee.io/furion)
-  👉 SqlSugar：[https://www.donet5.com/Doc/1/1180](https://www.donet5.com/Doc/1/1180)
-  👉 NewLife：[https://www.newlifex.com/](https://www.newlifex.com/)
-  👉 Snowy：[https://xiaonuo.vip/doc](https://xiaonuo.vip/doc)
-  👉 IdGenerator：[https://github.com/yitter/idgenerator](https://github.com/yitter/idgenerator)
-  👉 Masuit.Tools：[https://gitee.com/masuit/Masuit.Tools](https://gitee.com/masuit/Masuit.Tools)
-  👉 Emqx：[https://www.emqx.com/zh](https://www.emqx.com/zh)
  
## 💾版权声明💾

- 后端源码完全免费开源商用。
- 前端源码尊重小诺开源协议，不能作为开源竞品，但你任然可以用于个人项目等接私活或企业项目脚手架使用，为防止侵权，如需获取前端源码授权请[加我](https://qm.qq.com/cgi-bin/qm/qr?k=of373DKaD1xCdNfz8DskOwauCguHfMrS&noverify=0&personal_qrcode_source=4)获取授权，或者直接咸鱼搜simpleadmin
- 不能以任何形式用于非法为目的的行为。
- 任何基于本软件而产生的一切法律纠纷和责任，均于作者无关。
