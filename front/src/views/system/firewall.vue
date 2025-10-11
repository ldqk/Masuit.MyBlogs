<template>
<div class="firewall-page">
  <!-- 顶部操作栏 -->
  <div class="row q-mb-md">
    <div class="col">
      <div class="text-h6">防火墙策略配置</div>
    </div>
    <div class="col text-right">
      <q-btn-group>
        <q-btn color="negative" icon="block" label="编辑全局IP黑名单" @click="editIPBlackList" />
        <q-btn color="warning" icon="warning" label="编辑IP地址段黑名单" @click="editIPRangeBlackList" />
        <q-btn color="positive" icon="security" label="编辑IP白名单" @click="editIPWhiteList" />
        <q-btn color="primary" icon="save" label="保存配置" @click="saveSettings" :loading="saving" />
      </q-btn-group>
    </div>
  </div>
  <!-- 防火墙基础配置 -->
  <q-card flat bordered class="q-mb-md">
    <q-card-section>
      <div class="text-h6">
        <q-toggle v-model="firewallEnabled" label="防火墙状态" color="primary" />
      </div>
      <!-- 访问频率限制 -->
      <div class="row q-mb-md">
        <div class="col">
          <q-input v-model.number="settings.LimitIPFrequency" type="number" dense outlined prefix="单IP访问频次：每" suffix="秒内" />
        </div>
        <div class="col">
          <q-input v-model.number="settings.LimitIPRequestTimes" type="number" dense outlined prefix="最大请求" suffix="次" />
        </div>
        <div class="col">
          <q-input v-model.number="settings.BanIPTimespan" type="number" dense outlined prefix="冻结该IP" suffix="分钟" />
        </div>
        <div class="col">
          <q-input v-model.number="settings.LimitIPInterceptTimes" type="number" dense outlined prefix="拦截次数达到" suffix="次，上报防火墙永久冻结该IP" />
        </div>
      </div>
      <!-- 挑战模式配置 -->
      <div class="row q-mb-md">
        <div class="col">
          <q-select v-model="settings.ChallengeMode" :options="challengeModeOptions" label="挑战模式" dense outlined emit-value map-options />
        </div>
        <div v-if="settings.ChallengeMode === 'CloudflareTurnstileChallenge'" class="col">
          <q-input autogrow v-model="settings.TurnstileClientKey" label="Cloudflare客户端公钥" placeholder="0x4AAAAAAAAu4dr4wC-ZVpPT" dense outlined />
        </div>
        <div v-if="settings.ChallengeMode === 'CloudflareTurnstileChallenge'" class="col">
          <q-input autogrow v-model="settings.TurnstileSecretKey" label="Cloudflare服务端密钥" placeholder="0x4AAAAAAAAu4eDk4a2NvM91-OoOptBYl5Y" dense outlined />
          <div v-if="!settings.TurnstileSecretKey || !settings.TurnstileClientKey" class="text-negative text-caption"> 温馨提示：请先到 <a href="https://dash.cloudflare.com/" target="_blank">Cloudflare Turnstile</a> 生成站点密钥。 </div>
        </div>
        <!-- 挑战规则配置 -->
        <div class="col" v-if="settings.ChallengeMode">
          <q-select v-model="settings.ChallengeRule" :options="challengeRuleOptions" dense outlined emit-value map-options prefix="启用规则:" />
        </div>
        <div v-if="settings.ChallengeRule === 'Region' && settings.ChallengeMode" class="col">
          <q-input v-model="settings.ChallengeRegions" label="地区限制" placeholder="竖线分隔，支持国家、地区、城市、运营商、ASN" dense outlined />
        </div>
        <div v-if="settings.ChallengeRule === 'Region' && settings.ChallengeMode" class="col">
          <q-select v-model="settings.ChallengeRegionLimitMode" :options="regionLimitModeOptions" label="限制模式" dense outlined emit-value map-options />
        </div>
      </div>
      <!-- 地区和网络限制 -->
      <div class="row q-gutter-md q-mb-md">
        <div class="col">
          <q-input autogrow v-model="settings.DenyArea" label="受限制的地区或运营商" placeholder="禁止访问的地区，逗号分隔,如：江苏,苏州,移动,AS2333,DMIT，支持地区、运营商、ASN、机房名称" dense outlined />
        </div>
        <div class="col">
          <q-input autogrow v-model="settings.AllowedArea" label="不受限制的地区或运营商" placeholder="不受访问限制的地区或网络，逗号分隔，支持地区、运营商、ASN、机房名称" dense outlined />
        </div>
      </div>
      <!-- 攻击重定向 -->
      <div class="row q-gutter-md q-mb-md">
        <div class="col">
          <q-input autogrow v-model="settings.AttackRedirects" label="攻击重定向" placeholder="检测到CC攻击时，将请求重定向到指定URL，一行一个" dense outlined />
        </div>
        <div class="col">
          <q-input autogrow v-model="settings.BlockHeaderValues" label="屏蔽固定的请求头值" placeholder="禁止的HeaderValues，竖线分隔" dense outlined />
        </div>
      </div>
      <!-- 其他限制配置 -->
      <div class="row q-gutter-md q-mb-md">
        <div class="col">
          <q-input autogrow v-model="settings.UserAgentBlocked" label="UA标识限制" placeholder="禁止的UserAgent，逗号分隔(MicroMessenger,QQ)" dense outlined />
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 受限制和UA限制提示语 -->
  <q-card flat bordered class="q-mb-md">
    <q-card-section>
      <div class="text-h6 q-mb-md">提示信息配置</div>
      <div class="q-mb-md">
        <div class="text-subtitle1 q-mb-sm">受限制提示语：</div>
        <div ref="accessDenyTipsEditor" class="editor-container" style="height: 200px"></div>
      </div>
      <div class="q-mb-md">
        <div class="text-subtitle1 q-mb-sm">UA限制提示语：</div>
        <div ref="userAgentBlockedMsgEditor" class="editor-container" style="height: 400px"></div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 拦截日志 -->
  <q-card flat bordered class="q-mb-md">
    <q-card-section>
      <div class="text-h6 q-mb-md">
        <q-icon name="security" class="q-mr-sm" /> 拦截日志 <q-chip v-if="interceptCount > 0" color="negative" text-color="white" :label="`累计拦截 ${interceptCount} 次`" class="q-ml-sm" />
      </div>
      <!-- 拦截日志表格 -->
      <div v-if="logs.length === 0" class="text-center q-pa-md text-grey-6"> 暂无拦截日志 </div>
      <div v-else><!-- 操作按钮 -->
        <div class="row q-gutter-md q-mb-md items-center">
          <div class="col-auto">
            <q-btn color="negative" icon="delete" label="清空日志" :loading="clearingLogs">
              <q-popup-proxy transition-show="scale" transition-hide="scale">
                <q-card>
                  <q-card-section class="row items-center">
                    <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                    <div>
                      <div class="text-h6">确认清空拦截日志吗？</div>
                      <div class="text-subtitle2">清空后将无法恢复！</div>
                    </div>
                  </q-card-section>
                  <q-card-actions align="right">
                    <q-btn color="negative" label="确认" v-close-popup @click="clearLogs" />
                    <q-btn color="primary" label="取消" v-close-popup />
                  </q-card-actions>
                </q-card>
              </q-popup-proxy>
            </q-btn>
          </div>
          <div class="col-auto">
            <q-toggle v-model="distinctLogs" label="按IP去重" @update:model-value="toggleDistinct" />
          </div>
          <div class="col-auto">
            <q-btn color="primary" icon="refresh" label="刷新" @click="loadInterceptLogs" :loading="loadingLogs" />
          </div>
          <div class="col-4 text-right">
            <q-input autofocus v-model="searchTerm" dense outlined placeholder="搜索IP、请求URL、来源URL、UserAgent、备注" @keyup.enter="loadInterceptLogs" debounce="100">
              <template #prepend>
                <q-icon name="search" class="cursor-pointer" />
              </template>
              <template #append>
                <q-icon name="close" class="cursor-pointer" v-if="searchTerm" @click="searchTerm = ''" />
              </template>
            </q-input>
          </div>
        </div>
        <!-- 表格 -->
        <vxe-table ref="tableRef" :data="paginatedLogs" stripe border :loading="loadingLogs">
          <vxe-column field="IP" title="IP地址" width="160" sortable>
            <template #default="{ row }">
              <a :href="`/tools/ip/${row.IP}`" target="_blank" class="text-primary text-weight-bold"> {{ row.IP }} </a>
              <q-btn color="positive" icon="security" dense flat size="sm">
                <q-tooltip class="bg-indigo">添加到白名单</q-tooltip>
                <q-popup-proxy transition-show="scale" transition-hide="scale">
                  <q-card>
                    <q-card-section class="row items-center">
                      <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                      <div>
                        <div class="text-h6">确认操作</div>
                        <div class="text-subtitle2"> 此操作将标记该用户IP为白名单，是否继续？ </div>
                      </div>
                    </q-card-section>
                    <q-card-actions align="right">
                      <q-btn flat label="确认" color="negative" v-close-popup @click="addToWhiteList(row.IP)" />
                      <q-btn flat label="取消" color="primary" v-close-popup />
                    </q-card-actions>
                  </q-card>
                </q-popup-proxy>
              </q-btn>
            </template>
          </vxe-column>
          <vxe-column field="Address" title="IP地理位置" width="180"></vxe-column>
          <vxe-column field="RequestUrl" title="请求URL" min-width="200">
            <template #default="{ row }">
              <a :href="row.RequestUrl" target="_blank" class="text-primary">{{ row.RequestUrl }}</a>
            </template>
          </vxe-column>
          <vxe-column field="Referer" title="来源URL" width="200">
            <template #default="{ row }">
              <a :href="row.Referer" target="_blank" class="text-primary">{{ row.Referer }}</a>
            </template>
          </vxe-column>
          <vxe-column field="UserAgent" title="UserAgent" width="200"></vxe-column>
          <vxe-column field="Time" title="拦截时间" width="120" sortable>
            <template #default="{ row }"> {{ formatDate(row.Time) }} </template>
          </vxe-column>
          <vxe-column field="Remark" title="备注" width="120"></vxe-column>
          <vxe-column width="40" align="center">
            <template #default="{ row }">
              <q-btn color="info" icon="visibility" dense flat size="sm" @click="viewHeaders(row.Headers)">
                <q-tooltip class="bg-green">查看请求头</q-tooltip>
              </q-btn>
            </template>
          </vxe-column>
        </vxe-table>
        <!-- 分页控制栏 -->
        <div class="row items-center justify-between q-mb-md">
          <div class="col-auto">
            <span class="text-body2 text-grey-7"> 共 {{ filteredLogs.length }} 条记录，当前显示第 {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, filteredLogs.length) }} 条 </span>
          </div>
          <div class="col-auto justify-center q-mt-md">
            <q-pagination v-model="currentPage" :max="totalPages" :max-pages="6" direction-links boundary-links color="primary" @update:model-value="onPageChange" />
          </div>
          <div class="col-auto">
            <q-select v-model="pageSize" :options="pageSizeOptions" dense outlined label="每页显示" @update:model-value="onPageSizeChange" emit-value map-options />
          </div>
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 拦截次数排行榜 -->
  <q-card v-if="ranking.length > 0" flat bordered>
    <q-card-section>
      <div class="text-h6 q-mb-md">
        <q-icon name="leaderboard" class="q-mr-sm" /> 拦截次数排行榜
      </div>
      <vxe-table :data="ranking" stripe border>
        <vxe-column field="index" title="序号" width="80" align="center">
          <template #default="{ $rowIndex }">
            <q-badge :color="$rowIndex + 1 <= 3 ? 'negative' : 'grey'" :label="$rowIndex + 1" />
          </template>
        </vxe-column>
        <vxe-column field="Key" title="IP地址">
          <template #default="{ row }">
            <a :href="`https://ipinfo.io/${row.Key}`" target="_blank" class="text-primary text-weight-bold"> {{ row.Key }} </a>
            <q-btn color="negative" icon="block" dense flat size="sm" class="q-ml-sm">
              <q-tooltip class="bg-red">添加到黑名单</q-tooltip>
              <q-popup-proxy transition-show="scale" transition-hide="scale">
                <q-card>
                  <q-card-section class="row items-center">
                    <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                    <div>
                      <div class="text-h6">确认操作</div>
                      <div class="text-subtitle2"> 此操作将标记该用户IP为黑名单，是否继续？ </div>
                    </div>
                  </q-card-section>
                  <q-card-actions align="right">
                    <q-btn flat label="确认" color="negative" v-close-popup @click="addToBlackList(row.Key)" />
                    <q-btn flat label="取消" color="primary" v-close-popup />
                  </q-card-actions>
                </q-card>
              </q-popup-proxy>
            </q-btn>
          </template>
        </vxe-column>
        <vxe-column field="Count" title="拦截次数" sortable align="center">
          <template #default="{ row }">
            <q-chip color="negative" text-color="white" :label="row.Count" />
          </template>
        </vxe-column>
        <vxe-column field="Address" title="地理位置"></vxe-column>
        <vxe-column field="Start" title="拦截开始时间" width="160" sortable>
          <template #default="{ row }"> {{ formatDateTime(row.Start) }} </template>
        </vxe-column>
        <vxe-column field="End" title="拦截截止时间" width="160" sortable>
          <template #default="{ row }"> {{ formatDateTime(row.End) }} </template>
        </vxe-column>
        <vxe-column field="Continue" title="持续时长"></vxe-column>
      </vxe-table>
    </q-card-section>
  </q-card>
  <!-- IP列表编辑对话框 -->
  <q-dialog v-model="showIPListDialog">
    <q-card style="width: 60vw">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">{{ currentListTitle }}</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="closeIPListDialog" />
      </q-card-section>
      <q-card-section class="q-pt-none">
        <q-input autogrow v-model="currentIPList" placeholder="请输入IP地址" :hint="currentListType === 'IP地址段黑名单'
          ? '每行一条地址段，起始地址和结束地址用空格分隔开，其余信息也用空格分隔开'
          : '多个IP之间用英文逗号分隔'
          " />
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" @click="closeIPListDialog" />
        <q-btn color="primary" label="保存" @click="saveIPList" :loading="savingIPList" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <!-- 请求头详情对话框 -->
  <q-dialog v-model="showHeadersDialog">
    <q-card style="min-width: 600px">
      <q-card-section>
        <div class="text-h6">请求头详情</div>
      </q-card-section>
      <q-card-section>
        <pre class="headers-content">{{ currentHeaders }}</pre>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn color="negative" label="关闭" @click="showHeadersDialog = false" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <q-dialog v-model="updateFirewallStatus">
    <q-card>
      <q-card-section>
        <div class="text-h6">确认关闭WAF防火墙吗？</div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn color="negative" label="确认" @click="settings.FirewallEnabled = 'false'" v-close-popup />
      </q-card-actions>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, computed } from "vue";
import { toast } from "vue3-toastify";
import api from "@/axios/AxiosConfig";
import dayjs from "dayjs";
import { VxeColumnPropTypes } from "vxe-table";
import loadCodeMirror from "../../utils/codeMirrorLoader";

// 定义接口类型
interface Settings {
  FirewallEnabled?: string;
  LimitIPFrequency?: number;
  LimitIPRequestTimes?: number;
  BanIPTimespan?: number;
  LimitIPInterceptTimes?: number;
  ChallengeMode?: string;
  TurnstileClientKey?: string;
  TurnstileSecretKey?: string;
  ChallengeRule?: string;
  ChallengeRegions?: string;
  ChallengeRegionLimitMode?: string;
  DenyArea?: string;
  AllowedArea?: string;
  AttackRedirects?: string;
  AccessDenyTips?: string;
  UserAgentBlocked?: string;
  UserAgentBlockedMsg?: string;
  BlockHeaderValues?: string;
  [key: string]: any;
}

interface InterceptLog {
  IP: string;
  Address: string;
  RequestUrl: string;
  Referer: string;
  UserAgent: string;
  Time: string;
  Remark: string;
  Headers: string;
}

interface RankingItem {
  Key: string;
  Count: number;
  Address: string;
  Start: string;
  End: string;
  Continue: string;
}

interface ApiResponse {
  Success: boolean;
  Message: string;
  Data?: any;
}

// 响应式数据
const settings = ref<Settings>({});
const logs = ref<InterceptLog[]>([]);
const ranking = ref<RankingItem[]>([]);
const interceptCount = ref(0);

const saving = ref(false);
const loadingLogs = ref(false);
const clearingLogs = ref(false);
const distinctLogs = ref(false);

// 分页相关数据
const currentPage = ref(1);
const pageSize = ref(10);
const pageSizeOptions = [
  { label: "10条/页", value: 10 },
  { label: "20条/页", value: 20 },
  { label: "50条/页", value: 50 },
  { label: "100条/页", value: 100 },
  { label: "200条/页", value: 200 },
];

// IP列表管理
const showIPListDialog = ref(false);
const currentListType = ref("");
const currentListTitle = ref("");
const currentIPList = ref<string>("");
const savingIPList = ref(false);

// 请求头详情
const showHeadersDialog = ref(false);
const currentHeaders = ref("");

// 编辑器引用
const accessDenyTipsEditor = ref();
const userAgentBlockedMsgEditor = ref();
let accessDenyTipsCodeMirrorEditor: any = null;
let userAgentBlockedMsgCodeMirrorEditor: any = null;

const destroyEditors = () => {
  const disposeEditor = (editorRef: any, containerRef: any) => {
    if (editorRef) {
      const wrapper = editorRef.getWrapperElement?.();
      if (wrapper && wrapper.parentNode) {
        wrapper.parentNode.removeChild(wrapper);
      }
    }

    if (containerRef?.value) {
      containerRef.value.innerHTML = '';
    }
  };

  disposeEditor(accessDenyTipsCodeMirrorEditor, accessDenyTipsEditor);
  disposeEditor(userAgentBlockedMsgCodeMirrorEditor, userAgentBlockedMsgEditor);

  accessDenyTipsCodeMirrorEditor = null;
  userAgentBlockedMsgCodeMirrorEditor = null;
};

// 选项配置
const challengeModeOptions = [
  { label: "无", value: "" },
  { label: "JS挑战", value: "JSChallenge" },
  { label: "验证码挑战", value: "CaptchaChallenge" },
  { label: "CloudflareTurnstile", value: "CloudflareTurnstileChallenge" },
];

const challengeRuleOptions = [
  { label: "所有地区", value: "All" },
  { label: "在以下地区：", value: "Region" },
];

const regionLimitModeOptions = [
  { label: "以内", value: "1" },
  { label: "以外", value: "2" },
];
const updateFirewallStatus = ref(false);
// 计算属性
const firewallEnabled = computed({
  get: () => settings.value.FirewallEnabled === "true",
  set: (val: boolean) => {
    if (val) {
      settings.value.FirewallEnabled = "true";
    } else {
      updateFirewallStatus.value = true;
    }
  },
});

// 分页计算属性
const filteredLogs = computed(() => {
  const filtered = logs.value.filter((x) => x.IP?.includes(searchTerm.value) || x.RequestUrl?.includes(searchTerm.value) || x.Referer?.includes(searchTerm.value) || x.UserAgent?.includes(searchTerm.value) || x.Remark?.includes(searchTerm.value));
  if (distinctLogs.value) {
    // 按IP去重
    return filtered.reduce((acc: InterceptLog[], current: InterceptLog) => {
      const exists = acc.find((item) => item.IP === current.IP);
      if (!exists) {
        acc.push(current);
      }
      return acc;
    }, []);
  }
  return filtered;
});

const totalPages = computed(() => {
  return Math.ceil(filteredLogs.value.length / pageSize.value);
});

const searchTerm = ref("");
const paginatedLogs = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  const end = start + pageSize.value;
  return filteredLogs.value.slice(start, end);
});

// 加载系统设置
const loadSettings = async () => {
  try {
    const response = (await api.get("/system/getsettings")) as ApiResponse;
    if (response?.Success && response.Data) {
      const settingsObj: Settings = {};
      response.Data.forEach((item: any) => {
        settingsObj[item.Name] = item.Value;
      });
      settings.value = settingsObj;

      // 初始化编辑器内容
      await nextTick();
      if (accessDenyTipsCodeMirrorEditor) {
        accessDenyTipsCodeMirrorEditor.setValue(settings.value.AccessDenyTips || "");
      }
      if (userAgentBlockedMsgCodeMirrorEditor) {
        userAgentBlockedMsgCodeMirrorEditor.setValue(
          settings.value.UserAgentBlockedMsg || ""
        );
      }
    }
  } catch (error) {
    toast.error("获取系统设置失败", { autoClose: 2000, position: "top-center" });
    console.error("Error loading settings:", error);
  }
};

// 加载拦截日志
const loadInterceptLogs = async () => {
  loadingLogs.value = true;
  try {
    const response = (await api.get("/system/InterceptLog")) as ApiResponse;
    if (response?.Success && response.Data) {
      logs.value = response.Data.list || [];
      interceptCount.value = response.Data.interceptCount || 0;
      ranking.value = response.Data.ranking || [];
    }
  } catch (error) {
    toast.error("加载拦截日志失败", { autoClose: 2000, position: "top-center" });
    console.error("Error loading logs:", error);
  } finally {
    loadingLogs.value = false;
  }
};

// 清空拦截日志
const clearLogs = async () => {
  clearingLogs.value = true;
  const response = (await api.get("/system/clearInterceptLog")) as ApiResponse;
  if (response?.Success) {
    toast.success("拦截日志已清空", { autoClose: 2000, position: "top-center" });
    loadInterceptLogs();
  } else {
    toast.error(response?.Message || "清空失败", {
      autoClose: 2000,
      position: "top-center",
    });
  }
  clearingLogs.value = false;
};

// 分页方法
const onPageChange = (page: number) => {
  currentPage.value = page;
};

const onPageSizeChange = (newPageSize: number) => {
  pageSize.value = newPageSize;
  currentPage.value = 1; // 重置到第一页
};

// 切换去重显示
const toggleDistinct = () => {
  // 重置到第一页
  currentPage.value = 1;
  // 计算属性会自动处理去重逻辑
};

// 保存防火墙设置
const saveSettings = async () => {
  saving.value = true;
  try {
    // 获取编辑器内容
    if (accessDenyTipsCodeMirrorEditor) {
      settings.value.AccessDenyTips = accessDenyTipsCodeMirrorEditor.getValue();
    }
    if (userAgentBlockedMsgCodeMirrorEditor) {
      settings.value.UserAgentBlockedMsg = userAgentBlockedMsgCodeMirrorEditor.getValue();
    }

    const response = (await api.post(
      "/system/save",
      Object.keys(settings.value).map((key) => {
        return { Name: key, Value: settings.value[key] };
      })
    )) as ApiResponse;

    if (response?.Success) {
      toast.success(response.Message || "保存成功", {
        autoClose: 2000,
        position: "top-center",
      });
    } else {
      toast.error(response?.Message || "保存失败", {
        autoClose: 2000,
        position: "top-center",
      });
    }
  } catch (error) {
    toast.error("保存失败", { autoClose: 2000, position: "top-center" });
    console.error("Error saving settings:", error);
  } finally {
    saving.value = false;
  }
};

// 编辑IP黑名单
const editIPBlackList = async () => {
  try {
    const response = (await api.get("/system/IpBlackList")) as ApiResponse;
    if (response?.Success && response.Data) {
      currentListType.value = "IP黑名单";
      currentListTitle.value = "编辑全局IP黑名单";
      currentIPList.value = response.Data;
      showIPListDialog.value = true;
    }
  } catch (error) {
    toast.error("获取IP黑名单失败", { autoClose: 2000, position: "top-center" });
    console.error("Error loading IP blacklist:", error);
  }
};

// 编辑IP地址段黑名单
const editIPRangeBlackList = async () => {
  try {
    const response = (await api.get("/system/GetIPRangeBlackList")) as ApiResponse;
    if (response?.Success) {
      currentListType.value = "IP地址段黑名单";
      currentListTitle.value = "编辑IP地址段黑名单";
      currentIPList.value = response.Data;
      showIPListDialog.value = true;
    }
  } catch (error) {
    toast.error("获取IP地址段黑名单失败", { autoClose: 2000, position: "top-center" });
    console.error("Error loading IP range blacklist:", error);
  }
};

// 编辑IP白名单
const editIPWhiteList = async () => {
  try {
    const response = (await api.get("/system/IpWhiteList")) as ApiResponse;
    if (response?.Success) {
      currentListType.value = "IP白名单";
      currentListTitle.value = "编辑IP白名单";
      currentIPList.value = response.Data;
      showIPListDialog.value = true;
    }
  } catch (error) {
    toast.error("获取IP白名单失败", { autoClose: 2000, position: "top-center" });
    console.error("Error loading IP whitelist:", error);
  }
};

// 保存IP列表
const saveIPList = async () => {
  savingIPList.value = true;
  try {
    let endpoint = "";
    if (currentListType.value === "IP黑名单") {
      endpoint = "/system/SetIpBlackList";
    } else if (currentListType.value === "IP地址段黑名单") {
      endpoint = "/system/SetIPRangeBlackList";
    } else if (currentListType.value === "IP白名单") {
      endpoint = "/system/SetIpWhiteList";
    }

    const response = (await api.post(endpoint, { content: currentIPList.value })) as ApiResponse;
    if (response?.Success) {
      toast.success("保存成功", { autoClose: 2000, position: "top-center" });
      closeIPListDialog();
    } else {
      toast.error(response?.Message || "保存失败", {
        autoClose: 2000,
        position: "top-center",
      });
    }
  } catch (error) {
    toast.error("保存失败", { autoClose: 2000, position: "top-center" });
    console.error("Error saving IP list:", error);
  } finally {
    savingIPList.value = false;
  }
};

// 关闭IP列表对话框
const closeIPListDialog = () => {
  showIPListDialog.value = false;
  currentListType.value = "";
  currentListTitle.value = "";
  currentIPList.value = "";
};

// 添加到白名单
const addToWhiteList = async (ip: string) => {
  const data = (await api.post(`/system/AddToWhiteList`, { ip })) as ApiResponse;
  toast.success(data?.Message || "已加入白名单", {
    autoClose: 2000,
    position: "top-center",
  });
};

// 加入黑名单
const addToBlackList = async (ip: string) => {
  const data = (await api.post(`/system/AddToBlackList`, { ip })) as ApiResponse;
  toast.success(data?.Message || "已加入黑名单", {
    autoClose: 2000,
    position: "top-center",
  });
};

// 查看请求头详情
const viewHeaders = (headers: string) => {
  try {
    currentHeaders.value = JSON.stringify(JSON.parse(headers), null, 2);
  } catch {
    currentHeaders.value = headers;
  }
  showHeadersDialog.value = true;
};

// 日期格式化
const formatDate = (dateStr: string): string => {
  return dayjs(dateStr).format("MM-DD HH:mm");
};

const formatDateTime = (dateStr: string): string => {
  return dayjs(dateStr).format("YYYY-MM-DD HH:mm:ss");
};

// 初始化CodeMirror编辑器
const initEditors = async () => {
  await nextTick();

  const CodeMirror = (window as any).CodeMirror;
  if (!CodeMirror) {
    return;
  }

  destroyEditors();

  if (accessDenyTipsEditor.value) {
    accessDenyTipsCodeMirrorEditor = CodeMirror(accessDenyTipsEditor.value, {
      value: settings.value.AccessDenyTips || "",
      lineNumbers: true,
      mode: 'htmlmixed'
    });
  }

  if (userAgentBlockedMsgEditor.value) {
    userAgentBlockedMsgCodeMirrorEditor = CodeMirror(userAgentBlockedMsgEditor.value, {
      value: settings.value.UserAgentBlockedMsg || "",
      lineNumbers: true,
      mode: 'htmlmixed'
    });
  }
};

// 生命周期钩子
onMounted(async () => {
  await loadSettings();
  await loadInterceptLogs();
  try {
    await loadCodeMirror();
    // 延迟初始化编辑器
    setTimeout(() => {
      initEditors();
    }, 100);
  } catch (error) {
    toast.error("编辑器初始化失败", { autoClose: 2000, position: "top-center" });
    console.error("Error loading CodeMirror assets:", error);
  }
});

onUnmounted(() => {
  destroyEditors();
});
</script>
<style scoped lang="scss">
.firewall-page {
  padding: 20px;
}

.editor-container {
  border: 1px solid #ddd;
  border-radius: 4px;
}

:deep(.CodeMirror) {
  height: 100%;
}

:deep(.CodeMirror-scroll) {
  height: 100%;
}

.headers-content {
  background-color: #f5f5f5;
  padding: 16px;
  border-radius: 4px;
  overflow-x: auto;
  white-space: pre-wrap;
  word-wrap: break-word;
  max-height: 400px;
}

// 响应式设计
@media (max-width: 768px) {
  .firewall-page {
    padding: 10px;
  }

  .q-btn-group {
    flex-direction: column;

    .q-btn {
      margin-bottom: 8px;
    }
  }
}
</style>
