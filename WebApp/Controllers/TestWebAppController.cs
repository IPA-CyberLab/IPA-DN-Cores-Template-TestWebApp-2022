using System;
using System.Buffers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Net.WebSockets;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using IPA.Cores.Basic;
using IPA.Cores.Helper.Basic;
using static IPA.Cores.Globals.Basic;

using IPA.Cores.Codes;
using IPA.Cores.Helper.Codes;
using static IPA.Cores.Globals.Codes;

using IPA.Cores.Web;
using IPA.Cores.Helper.Web;
using static IPA.Cores.Globals.Web;

using IPA.App.TestWebApp;



namespace IPA.App.TestWebApp;

public class WebAppControllerBase : Controller
{
    public PageContext Page { get; }
    public IHadbBasedServicePoint Service { get; }

    public StrTableLanguage Language => Page.CurrentLanguage;
    public StrTable StrTable => Page.Stb;

    static Once DumpStrTableJsonOnceFlag;
    static CriticalSection<TestWebAppController> DumpStrTableJsonOnceFlagLock = new CriticalSection<TestWebAppController>();

    public WebAppControllerBase(IHadbBasedServicePoint service, PageContext page)
    {
        this.Service = service;
        this.Page = page;

        this.Page.SetLanguageList(service.LanguageList);

        // 文字列 JSON をダンプする
        lock (DumpStrTableJsonOnceFlagLock)
        {
            if (DumpStrTableJsonOnceFlag.IsFirstCall())
            {
                this.Page.DumpStrTableJson(Env.AppRootDir._CombinePath("wwwroot", "js", "strtable.js"));
            }
        }

        this.Page.SetLanguageByHttpString("ja");
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        try
        {
            this.Page.StartLanguageSelection(this.HttpContext);
        }
        finally
        {
            base.OnActionExecuting(context);
        }
    }

    protected AspNetCookieOptions GetCookieOption() => new AspNetCookieOptions(domain: this.Service.AdminForm_GetCurrentDynamicConfig()!.Service_CookieDomainName);
}

public class TestWebAppController : WebAppControllerBase
{
    public new TestWebAppService Service => (TestWebAppService)base.Service;

    public TestWebAppController(TestWebAppService service, PageContext page) : base(service, page)
    {
    }

    public class ModelEmpty
    {
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> IndexAsync()
    {
        await TaskCompleted;
        return View(new ModelEmpty());
    }
}

