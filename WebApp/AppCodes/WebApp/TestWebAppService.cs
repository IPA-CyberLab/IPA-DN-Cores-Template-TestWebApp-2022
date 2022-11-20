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


// 定数的な設定一覧
public static partial class TestWebAppServiceConfig
{
    public static partial class TestWebAppServiceSettings
    {
        public static readonly Copenhagen<int> SampleInt = 10;
    }
}

// TestWebApp サービスのスタートアップパラメータ
public partial class TestWebAppServiceStartupParam : HadbBasedServiceStartupParam
{
    public TestWebAppServiceStartupParam(string hiveDataName = "TestWebAppService", string hadbSystemName = "TestWebAppService")
    {
        this.HiveDataName = hiveDataName;
        this.HadbSystemName = hadbSystemName;
    }
}

// TestWebApp サービスの実装本体
public partial class TestWebAppService : HadbBasedServiceBase<TestWebAppService.MemDb, TestWebAppService.DynConfig, TestWebAppService.HiveSettings, TestWebAppService.HookBase>, TestWebAppService.IRpc
{
    //// 1. 設定関係の定義
    // --- Dynamic Config の定義 (Web アプリの設定画面で管理者が容易に編集可能な設定項目。設定内容はデータベースに保存され、複数の Web アプリインスタンス間で共有される。) ---
    public class DynConfig : HadbBasedServiceDynConfig
    {
        public string SampleTestConfigStr = "";

        protected override void NormalizeImpl()
        {
            if (this.SampleTestConfigStr._IsEmpty()) this.SampleTestConfigStr = "Test123";
        }
    }

    // --- 初期の DynamicConfig の生成 ---
    protected override DynConfig CreateInitialDynamicConfigImpl()
    {
        return new DynConfig();
    }

    // --- Hive Settings の定義 (Web アプリに関する基礎的な設定項目。設定内容は Web アプリの実行可能ディレクトリ以下に JSON ファイルとして保存され、インスタンスごとに異なる値とすることができる。) ---
    public class HiveSettings : HadbBasedServiceHiveSettingsBase
    {
        public string SampleTestSettingStr = "";

        public override void NormalizeImpl()
        {
            if (this.SampleTestSettingStr._IsEmpty()) this.SampleTestSettingStr = "Test456";
        }
    }


    //// 2. データ構造の定義
    // --- MemDb およびデータ構造一覧の定義 (MemDb とは、データベースと常に同期していて、かつ、Web アプリの動作中のインスタンス内のメモリ上に全データが保持されるデータ構造の定義である。) ---
    // まず MemDb の定義をする。ここでは、本 Web アプリが有するデータ構造の型の一覧を登録する。
    public class MemDb : HadbBasedServiceMemDb
    {
        protected override void AddDefinedUserDataTypesImpl(List<Type> ret)
        {
            ret.Add(typeof(ElementA));
        }

        protected override void AddDefinedUserLogTypesImpl(List<Type> ret)
        {
        }
    }

    // 次に MemDb 上に登場するデータ構造体を具体的に定義する。
    public class ElementA : HadbData
    {
        public string Id = "";
        public string Str = "";

        public override void Normalize()
        {
            this.Id = this.Id._NormalizeKey(true);
            this.Str = this.Str._NonNull();
        }

        public override HadbKeys GetKeys() => new HadbKeys(this.Id);
        public override HadbLabels GetLabels() => new HadbLabels(this.Str);
        public static ElementA _Sample => new ElementA
        {
            Id = "ntteast",
            Str = "NTT 東日本",
        };
    }


    //// 3. RPC インターフェイスの定義
    // Web アプリが内外に対して公開する RPC 関数のインターフェイス定義。
    [RpcInterface]
    public partial interface IRpc : IHadbBasedServiceRpcBase
    {
        [RpcMethodHelp("テスト関数。パラメータで int 型で指定された値を文字列に変換し、Hello という文字列を前置して返却します。RPC を呼び出すためのテストコードを実際に記述する際のテストとして便利です。", "Hello 123")]
        public Task<string> Test_HelloWorld([RpcParamHelp("テスト入力整数値", 123)] int i);
    }


    //// 4. フック関係
    // Web アプリの挙動の挙動を変更可能なフック (この HookBase クラスのメソッドは virtual として、デフォルトの挙動をここで定義する。これにより、開発者は override 関数を定義し、挙動を変更することができるようになるのである。)
    public partial class HookBase : HadbBasedServiceHookBase
    {
    }


    //// 5. 内部的に使用するオブジェクト一覧
    // ここで、ライブラリのインスタンスなどの内部的に使用するオブジェクトの定義をすること。
    // ここで定義したオブジェクトの一覧は、コンストラクタで初期化するか、StartImpl() で初期化するべきである。
    // また、CleanupImplAsync で解放するべきである。


    //// 6. スタートアップおよびクリーンアップなどのライフタイム管理関係
    // コンストラクタ
    public TestWebAppService(TestWebAppServiceStartupParam startupParam, HookBase hook) : base(startupParam, hook)
    {
        try
        {
            // ここで必要な初期化を実施すること (コンストラクタの実行コンテキストで実行される)
        }
        catch
        {
            this._DisposeSafe();
            throw;
        }
    }

    // クリーンアップ (不要なリソースの解放)
    protected override async Task CleanupImplAsync(Exception? ex)
    {
        try
        {
            // ここで必要な解放を実施すること
        }
        finally
        {
            await base.CleanupImplAsync(ex);
        }
    }

    // HADB サービス動作開始直前に呼ばれる初期化実装メソッド
    protected override void StartImpl()
    {
        // ここで必要な初期化を実施すること (コンストラクタの実行コンテキストではなく、HADB サービスが動作を開始する直前に実行される)
        // 必要なリソースは、ここで確保すること (例: ライブラリの初期化、インスタンスの作成等)
        // 注意: ここで確保したリソースは、下記の StopImplAsync で必ず解放するようにすること。解放しなければ、リソースリークが発生する。

        // ここで HADB イベント登録を行なうこと
        this.HadbEventListenerList.RegisterCallback(async (caller, type, state, param) =>
        {
            switch (type)
            {
                case HadbEventType.DynamicConfigChanged:
                    // HADB の DynamicConfig の内容が初期化または変更された場合、このイベントが呼ばれる。
                    Event_DynamicConfigReloaded();
                    break;

                case HadbEventType.ReloadDataFull:
                case HadbEventType.ReloadDataPartially:
                    Event_DataReloaded(type == HadbEventType.ReloadDataFull);
                    break;
            }
            await Task.CompletedTask;
        });
    }

    // HADB サービス動作停止直後に呼ばれるリソース解放実装メソッド
    protected override async Task StopImplAsync(Exception? ex)
    {
        // StartImpl で確保されたリソースは、ここで必ず解放すること。解放しなければ、リソースリークが発生する。

        await Task.CompletedTask; // await がないという警告除け (削除して構わない)
    }

    // HADB の DynamicConfig の内容が初期化され、または変更された場合に必ず実行されるメソッド
    void Event_DynamicConfigReloaded()
    {
        // HADB の DynamicConfig の内容が初期化され、または変更された場合に必ず実行したい処理をここに記述する
        var config = this.Hadb.CurrentDynamicConfig;
    }

    // HADB のデータの一部または全部がリロードされた都度必ず実行されるメソッド
    void Event_DataReloaded(bool full)
    {
        // HADB のデータの一部または全部がリロードされた都度必ず実行したい処理をここに記述する
    }

    // 本 Web アプリの健全性チェック URL が呼び出された場合に起動されるメソッド (健全であると回答する場合は Ok、非健全であると回答する場合は例外を返す)
    protected override async Task<OkOrExeption> HealthCheckImplAsync(CancellationToken cancel)
    {
        await Task.CompletedTask;

        this.Hadb.CheckIfReady(true);

        return new OkOrExeption();
    }


    //// 7. サービスとしてクライアントに提供するメソッドの実際の実装
    public Task<string> Test_HelloWorld(int i) => $"Hello {i}"._TaskResult();


}


