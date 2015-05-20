﻿using System;
using System.Web.Mvc;

namespace SDNUOJ.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class InfoController : AdminBaseController
    {
        /// <summary>
        /// 提示信息页面
        /// </summary>
        /// <returns>操作后的结果</returns>
        public ActionResult Index(String c, String s, String b)
        {
            ViewBag.Description = c;
            ViewBag.Style = s;
            ViewBag.BackUrl = b;

            return View();
        }
    }
}