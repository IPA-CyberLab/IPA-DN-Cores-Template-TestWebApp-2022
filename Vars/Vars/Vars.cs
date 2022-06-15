// TestWebApp Source Code
// 
// License: The Apache License, Version 2.0
// https://www.apache.org/licenses/LICENSE-2.0
// 
// Copyright (c) IPA CyberLab of Industrial Cyber Security Center.
// Copyright (c) NTT-East Impossible Telecom Mission Group.
// Copyright (c) Daiyuu Nobori.
// Copyright (c) SoftEther VPN Project, University of Tsukuba, Japan.
// Copyright (c) SoftEther Corporation.
// Copyright (c) all contributors on IPA-DN-Ultra Library and SoftEther VPN Project in GitHub.
// 
// All Rights Reserved.

// この Vars.cs ファイルは、シン・テレワークシステムを設置するユーザー側で書き換えて動作をカスタマイズするためのファイルです。

#pragma warning disable CA2235 // Mark all non-serializable fields
#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

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
using System.Security.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Https;

using IPA.Cores.Basic;
using IPA.Cores.Helper.Basic;
using static IPA.Cores.Globals.Basic;

using IPA.Cores.Web;
using IPA.Cores.Helper.Web;
using static IPA.Cores.Globals.Web;

using IPA.Cores.Codes;
using IPA.Cores.Helper.Codes;
using static IPA.Cores.Globals.Codes;
using IPA.App.TestWebApp;

namespace IPA.App.AppVars
{
    public static partial class AppVarsGlobal
    {
        // 定数
        public static readonly Copenhagen<int> Web_MaxBodySizeForUsers = 1 * 1024 * 1024;
        public static readonly Copenhagen<int> Web_MaxConcurrentKestrelConnectionsForUsers = 1000 * Math.Max(Environment.ProcessorCount, 1);

        // 初期化
        public static void InitMain()
        {
            // 許容する TLS のバージョンを設定するには、以下の行をコメントアウトして設定を変更すること。
            //CoresConfig.SslSettings.DefaultSslProtocolVersionsAsClient.TrySet(SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13);
            //CoresConfig.SslSettings.DefaultSslProtocolVersionsAsServer.TrySet(SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13);

            // HTTPS Web サーバーの証明書マネージャ (CertVault) の初期設定
            // ※ この設定を変更する前に、一度でも ThinWebClient を起動した場合は、
            //    初回起動時に設定ファイル「ThinWebClientApp/Local/App_IPA.App.ThinWebClientApp/Config/CertVault/settings.json」が
            //    自動生成されている。
            //    その後にこの Vars.cs ファイルの内容を書き換えても、
            //    「ThinWebClientApp/Local/App_IPA.App.ThinWebClientApp/Config/CertVault/settings.json」
            //    ファイルの内容には適用されない。
            //    このような場合には、
            //    一度 ThinWebClient を終了し、
            //    「ThinWebClientApp/Local/App_IPA.App.ThinWebClientApp/Config/CertVault/settings.json」
            //    を削除してから再度 ThinWebClient を起動すると、以下の内容が適用される。

            // true に設定すると、Let's Encrypt を使用して証明書を自動取得・更新するように試みるようになる。
            // Let's Encrypt を使用する場合は true、使用しない場合は false に設定すること。
            // 通常は、Let's Encrypt を使用せず、証明書を別に管理し、
            // 静的証明書ファイル (ThinWebClientApp/Local/App_IPA.App.ThinWebClientApp/Config/CertVault/StaticCerts/default.pfx) を設置しメンテナンスすることを推奨する。
            CoresConfig.CertVaultSettings.DefaultUseAcme.TrySetValue(false);

            CoresConfig.CertVaultSettings.DefaultNonAcmeEnableAutoGenerateSubjectNameCert.TrySetValue(false);   // これは、false を設定することを推奨する。
        }

        // 証明書データを保持するクラス
        public static partial class Certs
        {
            // Web API の HTTPS サーバー証明書と秘密鍵 (PKCS#12 形式)
            // このサンプルコードでは、ソースコードツリー中の
            // Vars/VarResources/VarResources/Certs/WebApiStaticCert.pfx
            // ファイルを Web サーバーの HTTPS サーバー証明書と秘密鍵として取り扱っています。
            // この HTTPS サーバー証明書と秘密鍵の置換は、以下のファイル名を変更するか、または、上記のファイルに使用したい証明書データファイルを上書きすることで可能です。
            static readonly Singleton<PalX509Certificate> WebApiStaticCert_Singleton =
                new Singleton<PalX509Certificate>(() => new PalX509Certificate(new FilePath(AppGlobal.AppRes,
                    "Certs/DefaultWebApiStaticCert.pfx")));

            public static PalX509Certificate WebApiStaticCert => WebApiStaticCert_Singleton;

        }


        // Web サーバーの設定
        public static void InitalizeWebServerConfig(HttpServerOptions opt)
        {
            // false にすると robots.txt ファイルを設置しなくなります。
            opt.DenyRobots = false;

            // true にすると HTTP ポートへのアクセス時に自動的に HTTPS ポートにリダイレクトするようになります。
            // 適切な SSL サーバー証明書が利用されていない場合、Web ブラウザで証明書エラーが発生します。
            opt.AutomaticRedirectToHttpsIfPossible = false;
        }
    }

    // TestWebApp の動作をカスタマイズするためのクラスです。
    public class MyTestWebAppHook : TestWebAppService.HookBase
    {
    }


    /// --- 以下のコードは変更しないでください ---
    /// 
    // 内部ヘルパー
    public static class _TestWebAppLibHelper
    {
        public static readonly string TestWebAppThisSourceCodeFileName = Dbg.GetCallerSourceCodeFilePath();
    }

    // 内部ヘルパー
    public static partial class AppGlobal
    {
        public static ResourceFileSystem AppRes => Res.Codes;

        public static partial class Res
        {
            public static readonly ResourceFileSystem Codes = ResourceFileSystem.CreateOrGet(
                new AssemblyWithSourceInfo(typeof(Res), new SourceCodePathAndMarkerFileName(_TestWebAppLibHelper.TestWebAppThisSourceCodeFileName, "app_resource_root")));
        }
    }
}

