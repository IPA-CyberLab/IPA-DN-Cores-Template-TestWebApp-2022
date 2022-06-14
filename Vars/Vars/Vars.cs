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

namespace IPA.App.AppVars
{
    public static partial class TestWebAppVars
    {
        // 初期化
        public static void InitMain()
        {
            // 許容する TLS のバージョンを設定するには、以下の行をコメントアウトして設定を変更すること。
            //CoresConfig.SslSettings.DefaultSslProtocolVersionsAsClient.TrySet(SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13);
            //CoresConfig.SslSettings.DefaultSslProtocolVersionsAsServer.TrySet(SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13);
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

        // TestWebApp 固有の設定クラス
        public static partial class TestWebAppVarsConfig
        {
            public static void InitMain()
            {
            }
        }
    }

    // TestWebApp の動作をカスタマイズするためのクラスです。
    public class MyTestWebAppHook : TestWebAppHookBase
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
    public static partial class TestWebAppGlobal
    {
        public static ResourceFileSystem TestWebAppRes => Res.Codes;

        public static partial class Res
        {
            public static readonly ResourceFileSystem Codes = ResourceFileSystem.CreateOrGet(
                new AssemblyWithSourceInfo(typeof(Res), new SourceCodePathAndMarkerFileName(_TestWebAppLibHelper.TestWebAppThisSourceCodeFileName, "app_resource_root")));
        }
    }
}

