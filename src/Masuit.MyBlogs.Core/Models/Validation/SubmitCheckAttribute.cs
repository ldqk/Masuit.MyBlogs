using Masuit.Tools.Html;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Models.Validation
{
    /// <summary>
    /// 检查提交的内容
    /// </summary>
    public class SubmitCheckAttribute : ValidationAttribute
    {
        private bool checkLength;
        private bool checkContent;
        private int MaxLength { get; set; } = 500;
        private int MinLength { get; set; } = 2;

        /// <summary>
        /// 检查提交的内容
        /// </summary>
        /// <param name="checkLength">是否检查内容长度</param>
        /// <param name="checkContent">是否检查内容包含禁用词</param>
        public SubmitCheckAttribute(bool checkLength = true, bool checkContent = true)
        {
            this.checkContent = checkContent;
            this.checkLength = checkLength;
        }

        /// <summary>
        /// 检查提交的内容
        /// </summary>
        /// <param name="maxLength">最大长度</param>
        /// <param name="checkContent">是否检查内容包含禁用词</param>
        public SubmitCheckAttribute(int maxLength, bool checkContent = true) : this(true, checkContent)
        {
            MaxLength = maxLength;
        }

        /// <summary>
        /// 检查提交的内容
        /// </summary>
        /// <param name="minLength">最小长度</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="checkContent">是否检查内容包含禁用词</param>
        public SubmitCheckAttribute(int minLength, int maxLength, bool checkContent = true) : this(maxLength, checkContent)
        {
            MinLength = minLength;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                ErrorMessage = $"请输入有效的内容！提交的内容不能为空！";
                return false;
            }
            string content = (value as string).RemoveHtmlTag().Trim();
            if (checkLength)
            {
                if (string.IsNullOrEmpty(content) || content.Length < 2)
                {
                    ErrorMessage = $"请输入有效的内容！内容要求{MinLength} - {MaxLength}个字符(不包含表情)，而您输入的内容包含{content.Length}个字符！";
                    return false;
                }

                if (content.Length < MinLength)
                {
                    ErrorMessage = $"内容至少要求{MinLength}个字符！";
                    return false;
                }
                if (content.Length > MaxLength)
                {
                    ErrorMessage = $"内容最多允许{MaxLength}个字符！";
                    return false;
                }
            }

            if (checkContent && Regex.Match(content, File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "ban.txt"))).Length > 0)
            {
                ErrorMessage = "您提交的内容包含有非法的词汇，被禁止发表，请检查您要提交的内容！";
                return false;
            }
            return true;
        }
    }
}