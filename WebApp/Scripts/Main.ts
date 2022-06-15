// --- CSS ---
require("./Main.scss");

// --- Polyfill ---
require("./submodules/IPA-DN-WebNeko/Scripts/Polyfill/polyfill.ts");

// --- HTML parts ---
import "core-js/es/promise";
import "@fortawesome/fontawesome-free/js/all";
import "prismjs";
import "prismjs/components/prism-json";
import "prismjs/components/prism-bash";
import "prismjs/plugins/line-numbers/prism-line-numbers";
import "prismjs/plugins/autolinker/prism-autolinker";
import "prismjs/plugins/command-line/prism-command-line";
import "prismjs/plugins/normalize-whitespace/prism-normalize-whitespace";
import "buefy";

// --- Imports ---
import { default as Guacamole } from "./submodules/IPA-DN-WebNeko/Libraries/guacamole-common-js-1.3.0/guacamole-common";
import { Util } from "./submodules/IPA-DN-WebNeko/Scripts/Common/Base/Util";
import { Str } from "./submodules/IPA-DN-WebNeko/Scripts/Common/Base/Str";
import { GuaComfortableKeyboard, GuaConnectedKeyboard, GuaKeyCodes, GuaUtil, GuaStates, GuaConsts, GuaResizeManager } from "./submodules/IPA-DN-WebNeko/Scripts/Misc/GuaUtil/GuaUtil";
import { ClipboardUtil, Html } from "./submodules/IPA-DN-WebNeko/Scripts/Common/Base/Html";
import { Secure } from "./submodules/IPA-DN-WebNeko/Scripts/Common/Base/Secure";
import { Task } from "./submodules/IPA-DN-WebNeko/Scripts/Common/Base/Task";
import { Axios, Vue, Buefy } from "./submodules/IPA-DN-WebNeko/Scripts/Imports";
import { Time } from "./submodules/IPA-DN-WebNeko/Scripts/Common/Base/Time";
import { PageContext, StrTable, StrTableUtil } from "./submodules/IPA-DN-WebNeko/Scripts/Common/Base/Cores";

// --- Common Init ---
Vue.use(Buefy);

let Page: PageContext;
let Stb: StrTable;

// ページを初期化 (各ページで共通)
export function Cores_InitJavaScriptWithPageContext(contextObjStr: string): void
{
    const contextObj = Util.JsonToObject(Str.JavaScriptSafeStrDecode(contextObjStr));

    Page = new PageContext(contextObj);
    Stb = StrTableUtil.LoadStrTable(Page.LanguageKey);
}



