// 设置UEditor资源路径
window.UEDITOR_HOME_URL = '/UEditorPlus/';
window.UEDITOR_CORS_URL = '/UEditorPlus/';
const { createApp, ref, onMounted, watch, computed } = Vue;
const { createDiscreteApi } = naive;
const { message, dialog } = createDiscreteApi(["message", "dialog"]);
window.message = message;
window.dialog = dialog;
createApp({
    setup() {
        const post = ref({ CategoryId: 1 });
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
                message.error('文章标题必须在2到128个字符以内！');
                return;
            }
            if (this.post.Author.trim().length <= 1 || this.post.Author.trim().length > 24) {
                message.error('昵称不能少于2个字符或超过24个字符！');
                return;
            }
            if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(this.post.Email.trim())) {
                message.error('请输入正确的邮箱格式！');
                return;
            }
            if (this.post.Content.length < 20 || this.post.Content.length > 1000000) {
                message.error('文章内容过短或者超长，请修改后再提交！');
                return;
            }
            axios.create({
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            }).post("/Post/Publish", this.post).then(res => {
                const data = res.data;
                if (data.Success) {
                    dialog.success({ title: '投递成功', content: data.Message })
                    clearInterval(window.interval);
                    localStorage.removeItem("write-post-draft");
                    this.post = { CategoryId: 1 };
                    ue.setContent('');
                } else {
                    message.error(data.Message);
                }
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
                message.error(data.Message);
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
                initialFrameWidth: null,
                initialFrameHeight: document.body.offsetHeight - 200
            });
            ue.addListener('contentChange', () => {
                this.post.Content = ue.getContent();
            });
        }
        this.getCategories();
        var user = JSON.parse(localStorage.getItem("user"));
        if (user) {
            this.post.Author = user.NickName;
            this.post.Email = user.Email;
        }
        //检查草稿
        const post = JSON.parse(localStorage.getItem("write-post-draft"));
        if (post && post.Content) {
            dialog.warning({
                title: '草稿箱',
                content: '检查到上次有未提交的草稿，是否加载？',
                positiveText: '确定',
                negativeText: '取消',
                draggable: true,
                onPositiveClick: () => {
                    this.post = post;
                    ue.setContent(this.post.Content);
                    window.interval = setInterval(() => {
                        localStorage.setItem("write-post-draft", JSON.stringify(this.post));
                    }, 5000);
                },
                onNegativeClick: () => {
                    window.interval = setInterval(() => {
                        localStorage.setItem("write-post-draft", JSON.stringify(this.post));
                    }, 5000);
                }
            });
        } else {
            window.interval = setInterval(() => {
                localStorage.setItem("write-post-draft", JSON.stringify(this.post));
            }, 5000);
        }
    },
}).use(naive).mount('#publishApp');
// }