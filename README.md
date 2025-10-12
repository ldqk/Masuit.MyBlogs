### Masuit.MyBlogs
<a href="https://gitee.com/masuit/Masuit.MyBlogs"><img src="https://gitee.com/static/images/logo-black.svg" height="32"></a> <a href="https://github.com/ldqk/Masuit.MyBlogs"><img src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/95/Font_Awesome_5_brands_github.svg/54px-Font_Awesome_5_brands_github.svg.png" height="32"><img src="https://upload.wikimedia.org/wikipedia/commons/thumb/2/29/GitHub_logo_2013.svg/128px-GitHub_logo_2013.svg.png" height="32"></a>  

个人博客站项目源码，高性能高安全性低占用的博客系统，这也许是我写过的性能最高的web项目了。**仅3MB的代码量！** 目前日均处理请求数80-600w次，同时在线活跃用户数60-600人，**数据量累计已达到数百万条**，数据库+Redis+网站主程序同时运行在一台4核8GB的机器上，浏览器页面请求秒级响应，CPU平均使用率控制在10%左右，内存占用控制在400MB左右。
![任务管理器](https://img11.360buyimg.com/ddimg/jfs/t1/170269/23/18655/93697/6076eb8fE82d545e7/78f0815f7311cd49.png)
![image](https://user-images.githubusercontent.com/20254980/206615289-fa975ddc-4534-47f8-9d80-d8ab637b0157.png)

![image](https://user-images.githubusercontent.com/20254980/129124476-88a324ac-cfd2-4e9b-8fb9-e84e12d04051.png)

### 演示站点
测试站点1：[https://masuit.org](https://masuit.org)，测试站点2：[https://masuit.com](https://masuit.com)，测试站点3：[https://ldqk.xyz](https://ldqk.xyz)

[![LICENSE](https://img.shields.io/badge/license-Anti%20996-blue.svg)](https://github.com/996icu/996.ICU/blob/master/LICENSE) ![codeSize](https://img.shields.io/github/languages/code-size/ldqk/Masuit.MyBlogs.svg) ![language](https://img.shields.io/github/languages/top/ldqk/Masuit.MyBlogs.svg)

### 请注意：
一旦使用本开源项目以及引用了本项目或包含本项目代码的公司因为违反劳动法（包括但不限定非法裁员、超时用工、雇佣童工等）在任何法律诉讼中败诉的，一经发现，本项目作者有权利追讨本项目的使用费（**公司工商注册信息认缴金额的2-5倍作为本项目的授权费**），或者直接不允许使用任何包含本项目的源代码！任何性质的`外包公司`或`996公司`需要使用本类库，请联系作者进行商业授权！其他企业或个人可随意使用不受限。996那叫用人，也是废人。8小时工作制才可以让你有时间自我提升，将来有竞争力。反对996，人人有责！

## Star趋势  
<img src="https://starchart.cc/ldqk/Masuit.MyBlogs.svg">    

### 项目主要技术栈
.NET 9  
ASP.NET Core MVC + vue3 + Blazor Server  
Entity Framework Core 9 (Npgsql Provider)  
Masuit.Tools  
Masuit.LuceneEFCore.SearchEngine  
Hangfire  
FreeRedis + EFCoreSecondLevelCacheInterceptor  
YARP Reverse Proxy  
Vue 3 + Quasar + Pinia + Vue Router 4  
### 开发环境
操作系统：Windows 11 24H2  
IDE：Visual Studio 2022 v17.14 (或更高版本)  
数据库：PostgreSQL 16.x  
Redis：Redis 8.x (Windows 版或兼容发行版)  
Node.js：20.x LTS (前端构建)  
运行时：必须是 .NET 9 
### 当前运行环境
操作系统：Windows Server 2019  
数据库：PostgreSQL 18.x  
Redis：Redis 8.x  
运行时：.NET 9 + HTTP/3 (QUIC)  
服务器配置：4核+8GB+6Gbps  
承载流量：单日请求量平均600w左右，单日带宽1TB左右  
`请勿使用阿里云、百度云等活动超卖机运行本程序，否则卡出翔！！！`  
`如何判断服务器商是否有超卖：给你的服务器跑个分，如果跑分接近于网络上该处理器公布的分数，则不是超卖的机器，计算公式：总分/核心数进行比较，由于是虚拟机，如果单独比较单核跑分，没有参考意义`
### 基础设施要求
||最低配置|推荐配置|豪华配置|至尊配置|
| --------   | -----:   | :----: | :----: | :----: |
|CPU|1核|2核|2核|4核|
|内存|1GB|2GB|4GB|8GB|
|带宽|1Mbps|1Mbps|5Mbps|1000Mbps+|
|数据库|pgsql 9|pgsql 14|pgsql 15+|pgsql 16+|
|缓存组件|Redis 3.2+|Redis 5.0+|Redis 5.0+|Redis 7.0+|
|备注|玩玩而已|几个人同时访问|几百个人同时访问，单日请求量600w以下|单日请求量600w以上|
### 主要功能
#### 服务器性能监控
可直接在线实时监控服务器的运行状态，包括CPU、网络带宽、磁盘使用率、内存占用等情况，百分位统计和图表统计，可记录最近一天的服务器健康状态，通过websocket进行数据的推送，仅支持Windows，且需要Windows安装最新的更新。
![image](https://user-images.githubusercontent.com/20254980/127088294-89c63e04-399c-45a1-ae47-5b55ea86a05d.png)

#### 文章管理
- 包含文章审核、文章合并、文章列表的增删查改、分类管理、专题管理；
- 文章审核：当用户在前台页进行投稿后，会进入审核状态，审核通过后，才会在前台页的文章列表中展示出来。
- 文章合并：当用户在前台页进行了文章的编辑后，会创建出文章的合并请求，当后台管理进行相应的合并操作后，前台用户的修改才会正式生效，可以直接合并、编辑并合并和拒绝合并，拒绝时，修改人会收到相应的邮件通知。
- 文章操作：可对文章进行修改、新增、置顶、临时删除(下架)、还原、永久删除、禁止评论等操作，编辑后的文章会生成历史版本。文章支持模板变量。
- 分类管理：对文章的分类进行增删查改和文章的移动等操作，与文章的关系：一对多。
- 专题管理：对文章的专题进行管理，与文章的关系：多对多。
- 快速分享：首页快速分享栏目的管理。
<img width="1918" height="1878" alt="image" src="https://github.com/user-attachments/assets/8ade4235-5caa-4317-982e-cf0e725e1685" />
<img width="1906" height="2001" alt="image" src="https://github.com/user-attachments/assets/869c22a7-bb1a-4946-9e82-45175c27debf" />
<img width="1781" height="1851" alt="image" src="https://github.com/user-attachments/assets/c4d73160-89b9-4dfe-a12b-fbde98991c70" />
<img width="1923" height="1114" alt="image" src="https://github.com/user-attachments/assets/aa072465-52d8-462b-8f52-8d4f2c8ce4d8" />
<img width="1781" height="1851" alt="image" src="https://github.com/user-attachments/assets/7a4b33e3-926b-4afc-9a6a-a2eaaf33638d" />
<img width="1921" height="1453" alt="image" src="https://github.com/user-attachments/assets/9dd9790b-9b4a-42a4-b24c-b737e308dd46" />

#### 评论和留言管理
对前台用户提交的留言和评论进行审核，当前台用户提交的内容可能包含有敏感词时，会进入人工审核，审核成功才会在前台页中展示。
#### 消息通知
站内消息包含评论、留言、投稿、文章合并等通知。
#### 公告管理
对网站的公告进行增删查改管理。支持定时上下架发布。
<img width="1781" height="1851" alt="image" src="https://github.com/user-attachments/assets/2830e082-0b9a-446c-ae34-c05953ab0921" />

#### 杂项页管理
一些通用的页面管理，可自由灵活的创建静态页面。
<img width="1921" height="944" alt="image" src="https://github.com/user-attachments/assets/ffd0c02f-97c6-46df-80b1-55fa0315441f" />

#### 系统设置
- 包含系统的全局设置、防火墙管理、网站运行日志记录、友链管理、邮件模板的管理。
- 全局设置：网站的一些基本配置和SEO相关操作等；
- 防火墙：对网站的所有请求进行全局流量的拦截，让规则内的请求阻止掉，支持黑名单、白名单、IP地址段、国家或地区、关键词审查等规则；
- 模板变量：针对文章内容的通用内容生成，变量只能添加不能删除。
<img width="1781" height="1851" alt="image" src="https://github.com/user-attachments/assets/ee8f7458-38e4-4006-9257-99774570c9f9" />
<img width="1781" height="1851" alt="image" src="https://github.com/user-attachments/assets/f44852ef-7f78-425a-a76d-ce8a7263e0c5" />
<img width="1923" height="1999" alt="image" src="https://github.com/user-attachments/assets/a25755e6-d0e5-4e47-a641-cdfab763a9e1" />
<img width="1921" height="1162" alt="image" src="https://github.com/user-attachments/assets/277654e9-54e1-4c5e-9e9d-8e881cfffd60" />
<img width="1916" height="1143" alt="image" src="https://github.com/user-attachments/assets/f16c70fb-97f7-4ad4-af94-08225935213b" />
<img width="1923" height="1482" alt="image" src="https://github.com/user-attachments/assets/46ac35cb-2758-4fad-8da1-524766b77dc7" />

#### 广告管理
主动式的广告投放管理，支持竞价排名，支持在banner、边栏、页内、列表内的广告展示，竞价或权重的高低决定广告出现的概率。支持按地区进行投放。
<img width="1924" height="1726" alt="image" src="https://github.com/user-attachments/assets/697f185f-230f-455d-bcc2-8676095fc97b" />
<img width="1923" height="1735" alt="image" src="https://github.com/user-attachments/assets/02332ff1-b441-4974-8920-c405daccf8e3" />

#### 赞助管理
对网站打赏进行增删查改操作，自动掩码。
<img width="1924" height="1023" alt="image" src="https://github.com/user-attachments/assets/878e9149-e3cd-4fcc-8119-4c5bd62353c9" />

#### 搜索统计
当前台用户每Session周期内的关键词搜索，不重复的关键词将会被记录，用于热词统计，仅记录最近一个月内的所有搜索关键词，用于统计当月、7天以及当天的搜索热词。
<img width="1912" height="1985" alt="image" src="https://github.com/user-attachments/assets/aa189d4c-06fd-4cc3-8974-2b8349176366" />

#### 任务管理
hangfire的可视化管理页面
#### 文件管理
服务器文件的在线管理，支持浏览、预览、压缩、解压缩、创建文件夹、上传、下载、打包下载等文件的基本操作。
![image](https://user-images.githubusercontent.com/20254980/127089568-5d3bcef6-5ad7-4f44-b30d-b7253be2d3fb.png)

### 项目架构
- 项目采用单体架构，方便部署和配置，传统的MVC模式，ASP.NET Core MVC+EF Core的简单架构。  
- Controller→Service→Repository→DbContext  

### 项目文件夹定义：
App_Data：存放网站的一些常规数据，以文本的形式存在，这类数据不需要频繁更新的。  
┠─cert文件夹：存放https证书  
┠─ban.txt：敏感词库  
┠─CustomKeywords.txt：搜索分词词库  
┠─denyip.txt：IP地址黑名单  
┠─DenyIPRange.txt：IP地址段黑名单  
┠─GeoLite2-City.mmdb：MaxMind地址库  
┠─ip2region.db：ip2region地址库  
┠─mod.txt：审查词库  
┠─whitelist.txt：IP地址白名单  
Common：基础公共帮助类；  
Configs：项目的一些配置对象  
Controllers：控制器  
Extensions：一些扩展类或一些项目的扩展功能，比如hangfire、ueditor、中间件、拦截器等；  
Infrastructure：数据访问基础设施，包含Repository和Services，相当于老项目的DAL和BLL；  
Models：存放一些实体类或DTO；  
Views：razor视图  
wwwroot：项目的所有静态资源；  
### 核心功能点技术实现
#### 后端技术栈：
依赖注入容器：.NET 内置容器 + Autofac（批量注入与属性注入）；  
静态映射：Riok.Mapperly 4.x；  
缓存体系：FreeRedis 管理热点数据，EFCoreSecondLevelCacheInterceptor 提供 EF Core 二级缓存；  
定时任务：Hangfire 1.8 统一调度友链回链、文章定时发布、访客统计、索引刷新等任务；  
实时通信：Blazor Server + SignalR WebSocket 推送服务器健康状态；  
硬件检测：Masuit.Tools 封装的硬件检测能力；  
协议支持：Kestrel + HTTP/3 (QUIC) + 自动 HTTPS/反向代理（YARP）；  
全文检索：Masuit.LuceneEFCore.SearchEngine 基于 Lucene.NET 4.8 实现全文检索；  
中文分词：结巴分词结合本地词库实现精准分词；  
断点续传与压缩：Masuit.Tools 提供 resumable download、7z/zip 压缩能力；  
Html 字符串操作：htmldiff.net-core 用于版本对比，HtmlAgilityPack 提取 DOM，HtmlSanitizer 提供表单防 XSS；  
图床：支持 gitee、github、gitlab 多图床上传及本地/外部存储切换；  
拦截器：授权、请求、异常、URL 重写、防火墙等中间件构建纵深防护体系；  
请求来源审计：MaxMind + IP2Region + 本地数据库联合判定来访地区；  
RSS：WilderMinds.RssSyndication 输出站点 RSS；  
EF 批量扩展：Z.EntityFramework.Plus 提供高性能批处理；  
文档转换：OpenXml + Mammoth 将上传的 Word 文档转换为 HTML；  
在线文件管理：Angular FileManager + 定制 API 支持在线管理服务器文件。  

#### 前端技术栈
- Vue 3 + TypeScript + Quasar 2 搭建后台管理界面与前台公共组件；  
- Pinia 3 负责全局状态管理，Vue Router 4 实现权限路由；  
- Axios + 自定义拦截器统一请求入口，支持代理与鉴权；  
- VXE Table / VXE UI 提供高性能表格、表单与数据可视化组件；  
- dayjs 进行国际化时间处理，内置相对时间与时区扩展；  
- @kangc/v-md-editor + vue-ueditor-wrap 提供 Markdown/富文本编辑体验；  
- animate.css、lottie-web、Quasar Notify 等组件丰富交互与动效；  
- 构建工具链基于 Vue CLI 5 + webpack，支持按需加载与 gzip/zip 产物清理。  
#### 性能和安全相关
- hangfire实现分布式任务调度；
- Z.EntityFramework.Plus实现数据访问层的高性能数据库批量操作；
- Lucene.NET实现高性能站内检索；
- 通过url的敏感词检查过滤恶意流量；
- 限制客户端的请求频次；
- 表单的AntiForgeryToken防止恶意提交；
- ip2region+MaxMind地址库实现请求来源审查；
- 用户信息采用端到端RSA非对称加密进行数据传输；
### 项目部署
以Windows系统为例，Linux系统请自行折腾。
#### 1.安装基础设施：
1. 安装 .NET 9 SDK/运行时：[https://dotnet.microsoft.com/zh-cn/download](https://dotnet.microsoft.com/zh-cn/download)
2. 安装pgsql：[pgsql 绿色版](https://masuit.org/2160)
3. 安装 Redis 8.x（Windows 版或兼容发行版）：[redis for windows绿色版](https://masuit.org/130)
4. （可选）安装 Node.js 20.x LTS，用于前端项目构建与调试
#### 2.生成网站应用
#### 方式一：编译源代码：
编译需要将[Masuit.Tools](https://github.com/ldqk/Masuit.Tools)项目和[Masuit.LuceneEFCore.SearchEngine](https://github.com/ldqk/Masuit.LuceneEFCore.SearchEngine)项目也一起clone下来，和本项目平级目录存放，才能正常编译，否则，将[Masuit.Tools](https://github.com/ldqk/Masuit.Tools)项目和[Masuit.LuceneEFCore.SearchEngine](https://github.com/ldqk/Masuit.LuceneEFCore.SearchEngine)项目移除，通过nuget安装也是可以的。  
![](https://git.imweb.io/ldqk/imgbed/raw/master/20191019/6370710386639200004363431.png)  
#### 方式二：下载编译好的现成的二进制文件
前往[Release](https://github.com/ldqk/Masuit.MyBlogs/releases)下载最新的压缩包解压即可。
#### 3.还原数据库脚本
创建数据库，名称随意，如：myblogs，然后前往[Release](https://github.com/ldqk/Masuit.MyBlogs/releases)或仓库目录 [`database/postgres`](database/postgres) 下载最新的 PostgreSQL 脚本/备份文件，执行 `psql -U postgres -d myblogs -f xxx.sql` 还原。   
如需迁移到其他数据库，可先还原到 PostgreSQL，再使用 [Full Convert](https://masuit.org/2163) 或自定义脚本迁移到目标数据库类型。
#### 4.修改配置文件：
主要需要配置的是以下内容，其他配置均为可选项，不配置则表示不启用；
![image](https://user-images.githubusercontent.com/20254980/169738528-ba0cc1a4-cb19-4e9d-b6cd-2f146a633c35.png)  
如果你使用了CDN，需要配置TrueClientIPHeader选项为真实IP请求转发头，如cloudflare的叫CF-Connecting-IP。
如果Redis不在本机，需要在配置文件中的Redis节下配置，固定为Redis，值的格式：127.0.0.1:6379,allowadmin=true，若未正确配置，将按默认值“127.0.0.1:6379,allowadmin=true,abortConnect=false”。  
如需为 HttpClient 添加代理或反向代理转发，请根据环境调整 `HttpClientProxy` 与 `ReverseProxy` 节点。  
其他配置请参考appsettings.json的注释按需配置即可。  
#### 5.启动网站
配置好环境和配置文件后，可直接通过dotnet Masuit.MyBlogs.Core.dll命令或直接双击Masuit.MyBlogs.Core.exe运行，也可以通过nssm挂在为Windows服务运行，或者你也可以尝试在Linux下部署。  
#### 6.前端管理界面构建
1. 进入前端目录：`cd front`
2. 安装依赖：`npm install`（可按需使用 `pnpm`/`yarn`）
3. 开发模式：`npm run dev`，默认端口 `http://localhost:8868`
4. 生产构建：`npm run build`，产物会自动输出到 `src/Masuit.MyBlogs.Core/wwwroot/dashboard`

#### 其他方式部署
IIS：部署时必须将应用程序池的标识设置为LocalSystem，否则无法监控服务器硬件，同时需要安装.NET Core Hosting运行时环境，IIS程序池改为无托管代码。  
![](https://git.imweb.io/ldqk/imgbed/raw/master/5ccbf30b6a083.jpg)  
docker/Linux：自行爬文。  
#### 有偿代部署服务
请联系：admin@masuit.com

### 后台管理：
https://127.0.0.1:5001/dashboard
- 初始用户名：masuit  
- 初始密码：123abc@#$
`若密码不对，可在debug模式下进入后台【用户管理】下重置密码`

### 推荐项目
基于EntityFrameworkCore和Lucene.NET实现的全文检索搜索引擎：[Masuit.LuceneEFCore.SearchEngine](https://github.com/ldqk/Masuit.LuceneEFCore.SearchEngine "Masuit.LuceneEFCore.SearchEngine")

.NET万能框架工具库：[Masuit.Tools](https://github.com/ldqk/Masuit.Tools)
