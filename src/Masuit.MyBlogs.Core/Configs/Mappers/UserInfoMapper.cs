using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class UserInfoMapper
{
    public static partial UserInfoDto ToDto(this UserInfo userInfo);

    [MapperIgnoreTarget(nameof(UserInfo.Id))]
    [MapperIgnoreTarget(nameof(UserInfo.Password))]
    [MapperIgnoreTarget(nameof(UserInfo.SaltKey))]
    public static partial UserInfo ToUserInfo(this UserInfoDto dto);

    [MapperIgnoreTarget(nameof(UserInfo.Id))]
    [MapperIgnoreTarget(nameof(UserInfo.Password))]
    [MapperIgnoreTarget(nameof(UserInfo.SaltKey))]
    public static partial void Update(this UserInfoDto cmd, UserInfo userInfo);

    public static partial IQueryable<UserInfoDto> ProjectDto(this IQueryable<UserInfo> q);
}