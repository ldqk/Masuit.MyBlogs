# 安全审计摘要

审计日期：2026-10-28

本摘要记录对生产代码路径的静态分析，不包含仅在 DEBUG 条件下编译的代码。审计时工作区存在未提交的源码修改；结论和行号基于当时的工作区，不保证与本提交中其他文件的版本完全一致。

本轮按要求排除已知的友链 URL 抓取 SSRF、匿名图片抓取 SSRF、任意 TLS 证书信任和任意来源转发头信任问题。未对运行中的站点执行写操作或重放访问凭据；以下发现均为静态分析结果，未经线上利用验证。由于没有完整的逐文件审阅记录，本摘要不表示已覆盖全部源码或排除其他漏洞。

## 发现概览

| # | 严重程度 | 位置 | 问题 | 置信度 |
|---|---|---|---|---|
| 1 | 高 | [ProtectContent.cshtml](src/Masuit.MyBlogs.Core/Views/Post/ProtectContent.cshtml#L21) | 沿用仓库默认密钥时，受保护内容访问凭据可能被伪造 | 9/10 |
| 2 | 中 | [ProtectContent.cshtml](src/Masuit.MyBlogs.Core/Views/Post/ProtectContent.cshtml#L21) | 删除授权用户后，已有访问凭据仍可重放 | 10/10 |
| 3 | 中 | [ProtectContent.cshtml](src/Masuit.MyBlogs.Core/Views/Post/ProtectContent.cshtml#L51) | 伪造评论 Cookie 可绕过「评论可见」条件 | 10/10 |

## 1. 默认密钥可能导致受保护内容访问凭据被伪造

**前提：** 部署仍使用仓库中公开的默认 `EncryptKey`。实际部署是否沿用默认值尚未验证。

**数据流：** [Startup.cs](src/Masuit.MyBlogs.Core/Startup.cs#L57) 加载配置密钥。[ProtectContent.cshtml](src/Masuit.MyBlogs.Core/Views/Post/ProtectContent.cshtml#L21) 验证「授权可见」内容时，比较请求中的邮箱和基于该密钥派生的 `PostAccessToken`，通过后输出受保护正文。[BaseController.cs](src/Masuit.MyBlogs.Core/Controllers/BaseController.cs#L44) 使用同一密钥验证 `FullAccessToken`，该令牌可影响地区限制的权限判断。

**影响：** 在上述前提下，能够获得公开默认密钥的匿名用户可能自行计算访问令牌，绕过邮箱登记和验证码，读取授权可见内容或绕过文章地区限制。

**修复建议：**

- 不使用仓库默认密钥；部署时通过受保护的配置来源提供独立、随机的密钥，缺失或仍为默认值时拒绝启动。
- 轮换已使用的默认密钥及相关访问凭据；为不同用途分离密钥。
- 使用有服务端有效期和撤销机制的随机访问票据，并在读取时检查对应用户的当前授权状态。

## 2. 撤销用户授权后访问凭据仍有效

**前提：** 攻击者曾获得有效的授权可见内容访问凭据，且之后被管理员删除或撤销授权。

**数据流：** [PostController.cs](src/Masuit.MyBlogs.Core/Controllers/PostController.cs#L420) 在获取验证码前检查邮箱是否登记，验证后签发会话和 Cookie。[UserController.cs](src/Masuit.MyBlogs.Core/Controllers/UserController.cs#L125) 删除用户时没有撤销已有凭据。[ProtectContent.cshtml](src/Masuit.MyBlogs.Core/Views/Post/ProtectContent.cshtml#L21) 渲染时只检查会话是否非空，或 Cookie 是否与密钥派生值匹配，没有重新检查用户授权或服务端票据有效期。

**影响：** 被撤销授权的用户可能继续读取「授权可见」内容。Cookie 的浏览器端过期时间本身不能阻止手工重放；会话分支也需要独立的撤销和到期检查。

**修复建议：** 将票据绑定到服务端用户和授权状态，按请求检查服务端有效期、权限及撤销状态；删除或撤销用户时使其已有票据和会话失效。

## 3. 客户端 Cookie 可绕过「评论可见」条件

**前提：** 匿名访客能够访问相应文章，且文章将受保护内容设置为「评论可见」。

**数据流：** [CommentController.cs](src/Masuit.MyBlogs.Core/Controllers/CommentController.cs#L136) 在评论成功后写入固定值的 `Comment_<文章ID>` Cookie。[ProtectContent.cshtml](src/Masuit.MyBlogs.Core/Views/Post/ProtectContent.cshtml#L51) 只判断该 Cookie 是否非空，随后输出受保护正文。

**影响：** 访客可以自行设置非空 Cookie，无须提交评论或完成评论流程即可读取该模式下的内容。此发现不直接影响密码可见和授权可见模式。

**修复建议：** 在服务端记录并验证文章与访客的评论授权状态，或使用绑定文章、访客和有效期的签名票据；读取受保护正文时验证其真实性和有效期。
