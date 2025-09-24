// 设置UEditor资源路径
window.UEDITOR_HOME_URL = '/UEditorPlus/';
window.UEDITOR_CORS_URL = '/UEditorPlus/';
const { createApp, ref, onMounted, watch, computed } = Vue;
createApp({
  setup() {
    const post = ref({});
    const categories = ref([]);
    const disableGetcode = ref(false);
    const codeMsg = ref("获取验证码");
    return {
      post,
      categories,
      disableGetcode,
      codeMsg
    };
  },
  methods: {
    flattenCategories(categories, parentName = '') {
      const result = []

      for (const category of categories) {
        // 构建当前分类的完整名称
        const fullName = parentName ? `${parentName}/${category.Name}` : category.Name

        // 添加当前分类到结果中
        result.push({
          ...category,
          Name: fullName,
          OriginalName: category.Name // 保存原始名称
        })

        // 递归处理子分类
        if (category.Children && category.Children.length > 0) {
          const childCategories = this.flattenCategories(category.Children, fullName)
          result.push(...childCategories)
        }
      }

      return result
    },
    async getCategories() {
      const data = await axios.get("/category/getcategories").then(function (response) {
        return response.data;
      });
      if (!data.Success) {
        return;
      }

      this.categories = this.flattenCategories(data.Data.sort((a, b) => (b.Id == 1) - (a.Id == 1)));
    },
    submit() {
      if (this.post.Title.trim().length <= 2 || this.post.Title.trim().length > 128) {
        VxeUI.modal.notification({
          content: '文章标题必须在2到128个字符以内！',
          status: 'error',
        });
        return;
      }
      if (this.post.Author.trim().length <= 1 || this.post.Author.trim().length > 24) {
        VxeUI.modal.notification({
          content: '昵称不能少于2个字符或超过24个字符！',
          status: 'error',
        });
        return;
      }
      if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(this.post.Email.trim())) {
        VxeUI.modal.notification({
          content: '请输入正确的邮箱格式！',
          status: 'error',
        });
        return;
      }
      if (this.post.Content.length < 20 || this.post.Content.length > 1000000) {
        VxeUI.modal.notification({
          content: '文章内容过短或者超长，请修改后再提交！',
          status: 'error',
        });
        loadingDone();
        return;
      }
      axios.create({
        headers: {
          'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
      }).post("/Post/Publish", this.post).then(res => {
        const data = res.data;
        if (data.Success) {
          VxeUI.modal.notification({
            content: data.Message,
            status: 'success',
          });
          clearInterval(window.interval);
          localStorage.removeItem("write-post-draft");
        } else {
          VxeUI.modal.notification({
            content: data.Message,
            status: 'error',
          });
        }
      });
    },
    async getcode(email) {
      VxeUI.modal.notification({
        content: '正在发送验证码，请稍候...',
        status: 'info',
      });
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
        VxeUI.modal.notification({
          content: '验证码发送成功，请注意查收邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！',
          status: 'success',
        });
        localStorage.setItem("user", JSON.stringify({ NickName: this.post.Author, Email: this.post.Email }));
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
        VxeUI.modal.notification({
          content: data.Message,
          status: 'error',
        });
        this.disableGetcode = false;
      }
    },
    search() {
      window.open("/s?wd=" + this.post.Title);
    }
  },
  created() {
    if (window.UE) {
      window.ue = UE.getEditor('editor', {
        initialFrameWidth: null
      });
    }
    this.getCategories();
    var user = JSON.parse(localStorage.getItem("user"));
    if (user) {
      this.post.Author = user.NickName;
      this.post.Email = user.Email;
    }
    //检查草稿
    if (localStorage.getItem("write-post-draft")) {
      VxeUI.modal.confirm({
        title: '草稿箱',
        content: '检查到上次有未提交的草稿，是否加载？',
        mask: false,
        lockView: false
      }).then(type => {
        if (type == 'confirm') {
          this.post = JSON.parse(localStorage.getItem("write-post-draft"));
          ue.setContent(this.post.Content);
          window.interval = setInterval(() => {
            this.post.Content = ue.getContent();
            localStorage.setItem("write-post-draft", JSON.stringify(this.post));
          }, 5000);
        } else {
          window.interval = setInterval(() => {
            this.post.Content = ue.getContent();
            localStorage.setItem("write-post-draft", JSON.stringify(this.post));
          }, 5000);
        }
      });
    } else {
      window.interval = setInterval(() => {
        this.post.Content = ue.getContent();
        localStorage.setItem("write-post-draft", JSON.stringify(this.post));
      }, 5000);
    }
  },
}).use(VxeUI).mount('#publishApp');
// }