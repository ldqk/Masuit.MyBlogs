using Castle.Core.Internal;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Svg;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Masuit.MyBlogs.Core.Controllers
{
    public class ActivityController : Controller
    {
        [HttpGet("sharecode")]
        public IActionResult GetCode(string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                throw new NotFoundException("联系方式不能为空");
            }

            var enc = q.AESEncrypt();
            Response.Cookies.Append("ShareCode", enc, new CookieOptions()
            {
                Expires = DateTime.Now.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
            return Content(enc);
        }

        public IActionResult ViewCount(string email)
        {
            return Ok(RedisHelper.SMembers("Share:" + email).Length);
        }

        public ActionResult Users(string type = "json", int count = 5)
        {
            var keys = RedisHelper.Keys("Share:*").ToDictionary(s => s[6..].MaskEmail(), s => RedisHelper.SMembers(s).Length).OrderByDescending(p => p.Value).ToList();
            switch (type)
            {
                case "svg":
                    var svg = new SvgDocument()
                    {
                        Height = keys.Count * 19
                    };
                    var svgText = new SvgText();
                    for (var i = 0; i < keys.Count; i++)
                    {
                        var s = keys[i];
                        svgText.Children.Add(new SvgTextSpan()
                        {
                            Text = s.Key,
                            Fill = new SvgColourServer(s.Value >= count ? Color.Red : Color.Black),
                            FontWeight = SvgFontWeight.Bold,
                            Y = new SvgUnitCollection()
                            {
                                new SvgUnit(18*(i+1))
                            },
                            X = new SvgUnitCollection()
                            {
                                new SvgUnit(0)
                            },
                            FontSize = 16
                        });
                    }

                    svg.Children.Add(svgText);
                    using (var stream = new MemoryStream())
                    {
                        svg.Write(stream);
                        return File(stream.ToArray(), ContentType.Svg);
                    }
                default:
                    return Json(keys);
            }
        }

        public ActionResult Count(int? count)
        {
            var keys = RedisHelper.Keys("Share:*");
            if (count.HasValue)
            {
                keys = keys.FindAll(s => RedisHelper.SMembers(s).Length >= count);
            }

            using var stream = new MemoryStream();
            new SvgDocument()
            {
                Height = 20,
                Width = 13 * keys.Length.ToString().Length,
                Children =
                {
                    new SvgText(keys.Length.ToString())
                    {
                        Fill = new SvgColourServer(Color.Red),
                        FontWeight = SvgFontWeight.Bold,
                        Y = new SvgUnitCollection()
                        {
                            new SvgUnit(18)
                        },
                        FontSize = 20
                    }
                }
            }.Write(stream);
            return File(stream.ToArray(), ContentType.Svg);
        }
    }
}