window.onload = function () {
    if (window.UE) {
		window.ue = UE.getEditor('editor', {
			initialFrameWidth: null
		});
	}
    
    var user = JSON.parse(localStorage.getItem("user"));
    if (user) {
        $("[name='Author']").val(user.NickName);
        $("[name='Email']").val(user.Email);
    }

	$('.ui.dropdown.tags').dropdown({
		allowAdditions: true,
		maxSelections: 5,
		message: {
			maxSelections: '最多选择 {maxCount} 项',
			noResults: '无搜索结果！'
		}
	});
	$('.ui.dropdown.category').dropdown({
		message: {
			maxSelections: '最多选择 {maxCount} 项',
			noResults: '无搜索结果！'
		}
	});
	$('.ui.dropdown.category').dropdown("set selected" , 1);

    //异步提交表单开始
    $("#submit").on("click", function (e) {
        e.preventDefault();
        loading();
        if ($("#article").val().trim().length <= 2 || $("#article").val().trim().length > 128) {
	        window.notie.alert({
		        type: 3,
                text: '文章标题必须在2到128个字符以内！',
		        time: 4
	        });
            loadingDone();
            return;
        }
        if ($("#Author").val().trim().length <= 1 || $("#Author").val().trim().length > 24) {
	        window.notie.alert({
		        type: 3,
                text: '昵称不能少于2个字符或超过36个字符！',
		        time: 4
	        });
            loadingDone();
            return;
        }
        if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test($("#Email").val().trim())) {
	        window.notie.alert({
		        type: 3,
                text: '请输入正确的邮箱格式！',
		        time: 4
	        });
            loadingDone();
            return;
        }
        if (ue.getContent().length < 200 || ue.getContent().length > 1000000) {
	        window.notie.alert({
		        type: 3,
                text: '文章内容过短或者超长的，我都认为你是在制造垃圾！',
		        time: 4
	        });
            loadingDone();
            return;
        }
		window.post("/Post/Publish", $("#article-form").serializeObject(), (data) => {
	        loadingDone();
            if (data.Success) {
	            window.notie.alert({
		            type: 1,
		            text: data.Message,
		            time: 4
	            });
	            $(':input', '#article-form').not(':button,:submit,:reset,:hidden').val('').removeAttr('checked').removeAttr('checked'); //评论成功清空表单
	            ue.setContent("");
				clearInterval(window.interval);
            } else {
				window.notie.alert({
		            type: 3,
		            text: data.Message,
		            time: 4
	            });
            }
        });
    });
    //异步提交表单结束
    
    $("#getcode").on("click", function (e) {
        e.preventDefault();
        window.post("/validate/sendcode", {
            __RequestVerificationToken: $("[name=__RequestVerificationToken]").val(),
            email: $("#Email").val()
        }, function (data) {
            if (data.Success) {
                layer.tips('验证码发送成功，请注意查收邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！', '#getcode', {
                  tips: [1, '#3595CC'],
                  time: 4000
                });
                user.NickName = $("[name='NickName']").val();
                user.Email = $("[name='Email']").val();
                localStorage.setItem("user", JSON.stringify(user));
                $("#getcode").attr('disabled', true);
                var count = 0;
                var timer = setInterval(function () {
                    count++;
                    $("#getcode").text('重新发送(' + (120 - count) + ')');
                    if (count > 120) {
                        clearInterval(timer);
                        $("#getcode").attr('disabled', false);
                        $("#getcode").text('重新发送');
                    }
                }, 1000);
            } else {
                layer.tips(data.Message, '#getcode', {
                  tips: [1, '#3595CC'],
                  time: 4000
                });
            }
        });
    });
	
	//检查草稿
	if (localStorage.getItem("write-post-draft")) {
		notie.confirm({
			text: "检查到上次有未提交的草稿，是否加载？",
			submitText: "确定", 
			cancelText: "取消",
			position: "bottom", 
			submitCallback: function () {
				var post=JSON.parse(localStorage.getItem("write-post-draft"));
				$("#article").val(post.Title);
				ue.setContent(post.Content);
				$('.ui.dropdown.category').dropdown('set selected', [post.CategoryId]);
				$('.ui.dropdown.tags').dropdown('set selected', post.Label.split(","));
                window.interval = setInterval(function () {
		            localStorage.setItem("write-post-draft",JSON.stringify($("#article-form").serializeObject()));
	            },5000);
			},
            cancelCallback: function() {
                window.interval = setInterval(function () {
		            localStorage.setItem("write-post-draft",JSON.stringify($("#article-form").serializeObject()));
	            },5000);
            }
		});	
	} else {
        window.interval = setInterval(function () {
		    localStorage.setItem("write-post-draft",JSON.stringify($("#article-form").serializeObject()));
	    },5000);
    }
};