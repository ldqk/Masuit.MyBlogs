
const { createApp, ref, onMounted, watch, computed, defineComponent, nextTick } = Vue;
const { createDiscreteApi } = naive;
const MessageReplies = defineComponent({
  name: 'MessageReplies',
  props: {
    msg: { type: Array, required: true },
    depth: { type: Number, default: 0 },
    isAdmin: { type: Boolean, default: false }
  },
  emits: ['pass', 'del', 'bounce-email', 'reply-msg'],
  methods: {
    pass(id) { this.$emit('pass', id); },
    del(id) { this.$emit('del', id); },
    bounceEmail(email) { this.$emit('bounce-email', email); },
    replyMsg(item) { this.$emit('reply-msg', item); },
    GetOperatingSystem(os) { return window.GetOperatingSystem(os); },
    GetBrowser(browser) { return window.GetBrowser(browser); },
    getColor(depth) {
      const colors = ['info', 'success', 'primary', 'warning', 'danger'];
      return colors[(depth + 1) % 5];
    }
  },
  computed: {
    sortedMsg() {
      return this.msg.slice().sort((x, y) => x.Id - y.Id);
    }
  },
  template: `
    <div>
      <article v-for="(item, idx) in sortedMsg" :key="item.Id" class="panel" :class="'panel-' + getColor(depth)">
        <div class="panel-heading">
          {{depth + 1}}-{{idx + 1}}# 
          <i v-if="item.IsMaster" class="icon icon-user"></i>
          {{item.NickName}}
          <span v-if="item.IsMaster">(管理员)</span> | {{item.PostDate}}
          <span class="pull-right hidden-sm hidden-xs" style="font-size: 10px;">
            <span v-if="isAdmin">
              <a v-if="item.Status==4" class="label label-success" @click="pass(item.Id)">通过</a> |
              <a class="label label-danger" @click="del(item.Id)">删除</a> |
            </span>
            <span class="hidden-sm hidden-xs" v-html="GetOperatingSystem(item.OperatingSystem) + ' | ' + GetBrowser(item.Browser)"></span>
          </span>
        </div>
        <div class="panel-body line-height24">
          <div v-html="item.Content"></div>
          <a class="reply" :class="'label label-' + getColor(depth)" @click="replyMsg(item)">
            <i class="icon-comment"></i>
          </a>
          <div v-if="isAdmin">
            <div class="margin-top10"></div>
            <div class="pull-left">
              <span class="label label-success">{{item.IP}}</span>
              <span class="label label-primary">{{item.Location}}</span>
            </div>
            <div class="pull-right">
              <span class="label label-success" @click="bounceEmail(item.Email)">{{item.Email}}</span>
            </div>
          </div>
          <br/>
          <message-replies v-if="item.Children && item.Children.length"
            :msg="item.Children"
            :depth="depth + 1"
            :is-admin="isAdmin"
            @bounce-email="bounceEmail"
            @reply-msg="replyMsg"
            @pass="$emit('pass', $event)"
            @del="$emit('del', $event)">
          </message-replies>
        </div>
      </article>
    </div>
  `
});

const ParentMessages = defineComponent({
  name: 'ParentMessages',
  props: { data: Object, isAdmin: { type: Boolean, default: false } },
  components: { MessageReplies },
  emits: ['reply-msg', 'getmsgs'],
  computed: {
    startfloor() {
      if (!this.data) return 1;
      const page = Math.min(Math.max(this.data.page, 1), Math.ceil(this.data.total / this.data.size));
      return this.data.parentTotal - (page - 1) * this.data.size;
    }
  },
  methods: {
    pass(id) {
      axios.post("/msg/pass/" + id).then((res) => {
        window.message.info(res.data.Message);
        this.$emit('getmsgs');
      });
    },
    del(id) {
      dialog.warning({
        title: '删除留言',
        content: '确认删除这条留言吗？',
        positiveText: '确定',
        negativeText: '取消',
        draggable: true,
        onPositiveClick: () => {
          axios.post("/msg/delete/" + id).then((res) => {
            message.success(res.data.Message);
            this.$emit('getmsgs', null);
          });
        }
      })
    },
    replyMsg(item) { this.$emit('reply-msg', item); },
    bounceEmail(email) {
      dialog.warning({
        title: '添加邮箱黑名单',
        content: '确认将此邮箱添加到黑名单吗？',
        positiveText: '确定',
        negativeText: '取消',
        draggable: true,
        onPositiveClick: () => {
          axios.post("/system/BounceEmail", { email: email }).then(() => {
            message.success('邮箱添加到黑名单成功');
          }).catch(() => {
            message.error('操作失败，请稍候再试');
          });
        }
      })
    },
    GetOperatingSystem(os) { return window.GetOperatingSystem(os); },
    GetBrowser(browser) { return window.GetBrowser(browser); }
  },
  template: `
    <ul v-if="data && data.rows" class="animated fadeInRight media-list wow">
      <li v-for="(row, idx) in data.rows" :key="row.Id" class="msg-list media animated fadeInRight" :id="row.Id">
        <div class="media-body">
          <article class="panel panel-info">
            <header class="panel-heading">
              {{startfloor - idx}}# 
              <i v-if="row.IsMaster" class="icon icon-user"></i>
              {{row.NickName}}<span v-if="row.IsMaster">(管理员)</span>
              | {{row.PostDate}}
              <span class="pull-right" style="font-size: 10px;">
                <span v-if="isAdmin">
                  <a v-if="row.Status==4" class="label label-success" @click="pass(row.Id)">通过</a> |
                  <a class="label label-danger" @click="del(row.Id)">删除</a> |
                </span>
                <span class="hidden-sm hidden-xs" v-html="GetOperatingSystem(row.OperatingSystem) + ' | ' + GetBrowser(row.Browser)"></span>
              </span>
            </header>
            <div class="panel-body line-height24">
              <div v-html="row.Content"></div>
              <a class="reply label label-info" @click="replyMsg(row)">
                <i class="icon-comment"></i>
              </a>
              <div v-if="isAdmin">
                <div class="margin-top10"></div>
                <div class="pull-left">
                  <span class="label label-success">{{row.IP}}</span>
                  <span class="label label-primary">{{row.Location}}</span>
                </div>
                <div class="pull-right">
                  <span class="label label-success" @click="bounceEmail(row.Email)">{{row.Email}}</span>
                </div>
                <br/>
              </div>
              <message-replies
                v-if="row.Children && row.Children.length"
                :msg="row.Children"
                :depth="0"
                :is-admin="isAdmin"
                @pass="pass"
                @bounce-email="bounceEmail"
                @reply-msg="replyMsg"
                @del="del"></message-replies>
            </div>
          </article>
        </div>
      </li>
    </ul>
  `
});

createApp({
  components: { ParentMessages },
  setup() {
    const user = window.defaultUser ? window.defaultUser() : { NickName: '', Email: '', Agree: false };
    const msg = ref({
      Content: '',
      Id: 0,
      NickName: user.NickName,
      Email: user.Email,
      ParentId: null,
      Agree: user.Agree,
      OperatingSystem: DeviceInfo.OS.toString(),
      Browser: DeviceInfo.browserInfo.Name + " " + DeviceInfo.browserInfo.Version
    });
    const reply = ref({
      for: null,
      Content: '',
      Id: 0,
      NickName: user.NickName,
      Email: user.Email,
      ParentId: null,
      Agree: user.Agree,
      OperatingSystem: DeviceInfo.OS.toString(),
      Browser: DeviceInfo.browserInfo.Name + " " + DeviceInfo.browserInfo.Version
    });
    const disableGetcode = ref(false);
    const codeMsg = ref("获取验证码");
    const list = ref([]);
    const cid = new URLSearchParams(window.location.search).get("cid") || 0;
    const pageConfig = ref({
      page: 1,
      size: 10,
      total: 0
    });
    return {
      msg,
      reply,
      disableGetcode,
      codeMsg,
      list,
      cid,
      pageConfig
    };
  },
  data() {
    return {
      showPopup: false,
      showModel: false
    };
  },
  methods: {
    submit(item) {
      if (item.NickName.trim().length <= 0 || item.NickName.trim().length > 24) {
        message.error('昵称要求2-24个字符！');
        loadingDone();
        return;
      }
      if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(item.Email.trim())) {
        message.error('请输入正确的邮箱格式！');
        loadingDone();
        return;
      }
      if (item.Content.trim().length <= 2 || item.Content.trim().length > 1000) {
        message.error('内容过短或者超长，请输入有效的留言内容！');
        loadingDone();
        return;
      }
      if (item.Email.indexOf("163") > 1 || item.Email.indexOf("126") > 1) {
        dialog.warning({
          title: '邮箱确认',
          content: '检测到您输入的邮箱是网易邮箱，本站的邮件服务器可能会因为您的反垃圾设置而无法将邮件正常发送到您的邮箱，建议使用您的其他邮箱，或者检查反垃圾设置后，再点击确定按钮继续！',
          positiveText: '确定',
          negativeText: '取消',
          draggable: true,
          onPositiveClick: () => {
            this.postMessage(item);
          }
        });
        return;
      }
      this.postMessage(item);
    },
    postMessage(item) {
      axios.create({
        headers: {
          'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
      }).post("/Msg/submit", item).then((response) => {
        loadingDone();
        const data = response.data;
        if (data && data.Success) {
          window.notie.alert({
            type: 1,
            text: data.Message,
            time: 4
          });
          this.getmsgs();
        } else {
          window.notie.alert({
            type: 3,
            text: data.Message,
            time: 4
          });
        }
      });
    },
    getmsgs() {
      axios.get(`/msg/getmsgs?page=${this.pageConfig.page}&size=${this.pageConfig.size}&cid=${this.cid}`).then((response) => {
        this.list = response.data.Data;
        this.pageConfig.total = this.list.total;
      });
    },
    async getcode(email) {
      message.info('正在发送验证码，请稍候...');
      const data = await axios.create({
        headers: {
          'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
      }).post("/validate/sendcode", {
        //__RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value,
        email: email
      }).then(res => res.data);
      if (data.Success) {
        this.disableGetcode = true;
        message.success('验证码发送成功，请注意查收邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！');
        localStorage.setItem("user", JSON.stringify({ NickName: this.reply.NickName || this.msg.NickName, Email: this.reply.Email || this.msg.Email }));
        var count = 0;
        var timer = setInterval(() => {
          count++;
          this.codeMsg = '重新发送(' + (120 - count) + ')';
          if (count > 120) {
            clearInterval(timer);
            this.disableGetcode = false;
            this.codeMsg = '重新发送';
          }
        }, 1000);
      } else {
        message.error(data.Message);
        this.disableGetcode = false;
      }
    },
    replyMsg(item) {
      this.reply.ParentId = item.Id;
      this.reply.for = item;
      this.showPopup = true;
    }
  },
  watch: {
    'pageConfig.page'(newVal, oldVal) {
      if (newVal !== oldVal) {
        this.getmsgs();
      }
    },
    'pageConfig.size'(newVal, oldVal) {
      if (newVal !== oldVal) {
        this.getmsgs();
      }
    },
    showPopup(newVal) {
      if (newVal) {
        nextTick(() => {
          window.ue2 = UE.getEditor('editor2', {
            //这里可以选择自己需要的工具按钮名称,此处仅选择如下五个
            toolbars: [['source', //源代码
              'removeformat', //清除格式
              'bold', //加粗
              'italic', //斜体
              'underline', //下划线
              'strikethrough', //删除线
              'blockquote', //引用
              'pasteplain', //纯文本粘贴模式
              'fontsize', //字号
              'paragraph', //段落格式
              'forecolor', //字体颜色
              'backcolor', //背景色
              'insertcode', //代码语言
              'horizontal', //分隔线
              'justifyleft', //居左对齐
              'justifyright', //居右对齐
              'justifycenter', //居中对齐
              'link', //超链接
              'unlink', //取消链接
              'emotion', //表情
              'simpleupload', //单图上传
              'insertorderedlist', //有序列表
              'insertunorderedlist', //无序列表
            ]],
            initialFrameWidth: null,
            //默认的编辑区域高度
            initialFrameHeight: 200
          });
          ue2.addListener('contentChange', () => {
            this.reply.Content = ue2.getContent();
          });
        });
      } else {
        if (window.ue2) {
          window.ue2.destroy();
        }
      }
    }
  },
  mounted() {
    const { message, dialog } = createDiscreteApi(["message", "dialog"]);
    message.info('欢迎使用留言板，期待你的留言！');
    window.message = message;
    window.dialog = dialog;
    this.getmsgs();
    if (window.UE) {
      window.ue = UE.getEditor('editor', {
        //这里可以选择自己需要的工具按钮名称,此处仅选择如下五个
        toolbars: [['source', //源代码
          'removeformat', //清除格式
          'bold', //加粗
          'italic', //斜体
          'underline', //下划线
          'strikethrough', //删除线
          'blockquote', //引用
          'pasteplain', //纯文本粘贴模式
          'fontsize', //字号
          'paragraph', //段落格式
          'forecolor', //字体颜色
          'backcolor', //背景色
          'insertcode', //代码语言
          'horizontal', //分隔线
          'justifyleft', //居左对齐
          'justifyright', //居右对齐
          'justifycenter', //居中对齐
          'link', //超链接
          'unlink', //取消链接
          'emotion', //表情
          'simpleupload', //单图上传
          'insertorderedlist', //有序列表
          'insertunorderedlist', //无序列表
        ]],
        initialFrameWidth: null,
        //默认的编辑区域高度
        initialFrameHeight: 200
      });
      ue.addListener('contentChange', () => {
        this.msg.Content = ue.getContent();
      });
    }
  }
}).use(naive).mount('#msgApp');