using Masuit.Tools;
using System.ComponentModel.DataAnnotations;

namespace Models.Validation
{
    /// <summary>
    /// 网址格式验证
    /// </summary>
    public class IsWebUrlAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                ErrorMessage = "网址不能为空！";
                return false;
            }

            var email = value as string;
            if (email.Length < 11)
            {
                ErrorMessage = "您输入的网址格式不正确！";
                return false;
            }

            if (email.Length > 256)
            {
                ErrorMessage = "网址长度最大允许255个字符！";
                return false;
            }
            if (email.MatchUrl())
            {
                return true;
            }
            ErrorMessage = "您输入的网址格式不正确！";
            return false;
        }
    }
}