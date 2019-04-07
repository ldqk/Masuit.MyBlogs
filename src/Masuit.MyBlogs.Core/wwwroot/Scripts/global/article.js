$(function () {
	$("#toc").show();
    var toc = $("#toc").tocify({
        selectors: ".ibox-content h3,.ibox-content h4,.ibox-content h5"
    }).data("toc-tocify");
	$(".tocify>.close").on("click", function(e) {
		$(this).parent().hide();
	});		
	SyntaxHighlighter.all();
	SyntaxHighlighter.defaults['toolbar'] = false;
	layui.use('layedit', function () {
		layui.layedit.build('layedit', {
			tool: ["strong", "italic", 'link', "unlink", "face"],
			height: 100
		});
	});
	$("#code-token").on("submit", function(e) {
		e.preventDefault();
		$.post("/post/CheckViewToken", $(this).serialize(), function(data) {
			if (data.Success) {
				window.location.reload();
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
			}
		});
	});
	$(".getcode").on("click", function(e) {
		e.preventDefault();
		$.post("/post/getviewtoken",
			{
				__RequestVerificationToken:$("[name=__RequestVerificationToken]").val(),
				email:$("#email3").val()
			}, function(data) {
			if (data.Success) {
				window.notie.alert({
					type: 1,
					text: "验证码发送成功，请注意查收邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！",
					time: 4
				});
				window.localStorage.setItem("email",$("#email3").val());
				$(".getcode").attr('disabled',true);
				var count=0;
				var timer=setInterval(function() {
					count++;
					$(".getcode").text('重新发送('+(120-count)+')');
					if (count>120) {
						clearInterval(timer);
					}
				},1000);
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
			}
		});
	});
	var user = JSON.parse(localStorage.getItem("user"));
	var email = localStorage.getItem("email");
	if (email) {
		$("[name='Email']").val(email);
		$("#email-token").submit();
	}
	if (user) {
		$("[name='NickName']").val(user.NickName);
		$("[name='Email']").val(user.Email);
		$("[name='QQorWechat']").val(user.QQorWechat);
	}
	bindReplyBtn();//绑定回复按钮事件
	bindVote();//绑定文章投票按钮
	getcomments();//获取评论
	commentVoteBind(); //评论投票
	$("#OperatingSystem").val(platform.os.toString());
    $("#Browser").val(platform.name + " " + platform.version);

	//异步提交评论表单
	$("#comment").on("submit", function(e) {
		e.preventDefault();
		layui.layedit.sync(1);
		if ($("#name").val().trim().length <= 1 || $("#name").val().trim().length > 36) {
			window.notie.alert({
				type: 3,
				text: '再怎么你也应该留个合理的名字吧，非主流的我可不喜欢！',
				time: 4
			});
			return;
		}
		if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test($("#email").val().trim())) {
			window.notie.alert({
				type: 3,
				text: '请输入正确的邮箱格式！',
				time: 4
			});
			return;
		}
		
		if($("#email").val().indexOf("163")>1||$("#email").val().indexOf("126")>1) {
			var _this=this;
			swal({
				title: '邮箱确认',
				text: "检测到您输入的邮箱是网易邮箱，本站的邮件服务器可能会因为您的反垃圾设置而无法将邮件正常发送到您的邮箱，建议使用您的其他邮箱，或者检查反垃圾设置后，再点击确定按钮继续！",
				type: 'warning',
				showCancelButton: true,
				confirmButtonColor: '#3085d6',
				cancelButtonColor: '#d33',
				confirmButtonText: '确定',
				cancelButtonText: '换个邮箱',
				confirmButtonClass: 'btn btn-success btn-lg',
				cancelButtonClass: 'btn btn-danger btn-lg',
				buttonsStyling: false
			}).then(function(isConfirm) {
				if (isConfirm === true) {
					submitComment(_this);
				}
			});
			return;
		}
		submitComment(this);
	});
	
	//表单取消按钮
	$(".btn-cancel").click(function() {
		$(':input', '#reply-form').not(':button,:submit,:reset,:hidden').val('').removeAttr('checked')
			.removeAttr('checked'); //评论成功清空表单
		//Custombox.close();
		layer.closeAll();
		setTimeout(function() {
			$("#reply").css("display", "none");
		}, 500);
	});

	//回复表单的提交
	$("#reply-form").on("submit", function(e) {
		e.preventDefault();
		layui.layedit.sync(window.currentEditor);
		loading();
		if ($("#name2").val().trim().length <= 0 || $("#name").val().trim().length > 36) {
			window.notie.alert({
				type: 3,
				text: "亲，能留个正常点的名字不！",
				time: 4
			});
			loadingDone();
			return;
		}
		if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test($("#email2")
			.val().trim())) {
			window.notie.alert({
				type: 3,
				text: "请输入正确的邮箱格式！",
				time: 4
			});
			loadingDone();
			return;
		}
		localStorage.setItem("user", JSON.stringify($(this).serializeObject()));
		$.post("/comment/put", $(this).serialize(), (data) => {
			loadingDone();
			if (data.Success) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				layer.closeAll();
				setTimeout(function () {
					getcomments();
					$("[id^='LAY_layedit']").contents().find('body').html('');
					$("#reply").css("display", "none");
				}, 500);
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
			}
		});
	});

	$("#donate").on("click", function (e) {
		$.post("/system/getsetting", { name: "Donate" }, function (data) {
			swal({
				title: "支付宝扫一扫付款捐赠！",
				html:"<a href='/donate'>更多方式</a>",
				showCancelButton: true,
				confirmButtonColor: "#DD6B55",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
				showLoaderOnConfirm: true,
				imageUrl: data.Data.Value,
				imageWidth: 400,
				animation: true,
				allowOutsideClick: false
			}).then(function() {

			}, function() {
				swal("您的捐赠将会支持本站做的更好！", null, "error");
			});
		});
	});
});

/**
 * 提交评论
 * @returns {} 
 */
function submitComment(_this) {
	loading();
	localStorage.setItem("user", JSON.stringify($(_this).serializeObject()));
	$.post("/comment/put", $(_this).serialize(), (data) => {
		loadingDone();
		if (data.Success) {
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
			setTimeout(function() {
				getcomments();
				$("[id^='LAY_layedit']").contents().find('body').html('');
			},100);
		} else {
			window.notie.alert({
				type: 3,
				text: data.Message,
				time: 4
			});
		}
	});
}

//评论回复按钮事件
function bindReplyBtn() {
	$(".msg-list article .panel-body a").on("click", function(e) {
		e.preventDefault();
		loadingDone();
		var user = JSON.parse(localStorage.getItem("user"));
		if (user) {
			$("[name='NickName']").val(user.NickName);
			$("[name='Email']").val(user.Email);
			$("[name='QQorWechat']").val(user.QQorWechat);
		}
		var href = $(this).attr("href");
		var uid = href.substring(href.indexOf("uid") + 4);
		$("#uid").val(uid);
		$("#OperatingSystem2").val(platform.os.toString());
		$("#Browser2").val(platform.name + " " + platform.version);
		//Custombox.open({
		//	target: '#modal',
		//	overlayOpacity: 0.1,
		//	speed:10,
		//	zIndex: 100
		//});

		layui.use("layer", function() {
			var layer = layui.layer;
			layer.open({
				type: 1,
				zIndex: 20,
				title: '回复评论',
				area: (window.screen.width > 540 ? 540 : window.screen.width) + 'px',// '340px'], //宽高
				content: $("#reply"),
				end: function() {
					$("#reply").css("display", "none");
				}
			});
		});
		$(".layui-layer").insertBefore($(".layui-layer-shade"));
		window.currentEditor = layui.layedit.build('layedit2', {
			tool: ["strong", "italic", 'link', "unlink", "face"],
			height: 100
		});
	});
}

//绑定评论投票
function commentVoteBind() {
	$(".cmvote").on("click", function(e) {
		$.post("/comment/CommentVote", {
			id: $(this).data("id")
		}, (data) => {
			if (data) {
				if (data.Success) {
					console.log($(this).children("span.count"));
                    $(this).children("span.count").text(parseInt($(this).children("span.count").text())+1);
					$(this).addClass("disabled");
					this.disabled = true;
					window.notie.alert({
						type: 1,
						text: data.Message,
						time: 4
					});
				} else {
					window.notie.alert({
						type: 3,
						text: data.Message,
						time: 4
					});
				}
			}
		});
	});
}

function bindVote() {
    $("#voteup").on("click", function(e) {
        $.post("/post/voteup", {
			id: $("#postId").val()
		}, (data) => {
			if (data) {
				if (data.Success) {
					$(this).children()[1].innerText = parseInt($(this).children()[1].innerText) + 1;
					$(this).addClass("disabled");
					this.disabled = true;
					window.notie.alert({
						type: 1,
						text: data.Message,
						time: 4
					});
				} else {
					window.notie.alert({
						type: 3,
						text: data.Message,
						time: 4
					});
				}
			}
		});
	});
    $("#votedown").on("click", function(e) {
        $.post("/post/votedown", {
			id: $("#postId").val()
		}, (data) => {
			if (data) {
				if (data.Success) {
					$(this).children()[1].innerText = parseInt($(this).children()[1].innerText) + 1;
					$(this).addClass("disabled");
					this.disabled = true;
					window.notie.alert({
						type: 1,
						text: data.Message,
						time: 4
					});
				} else {
					window.notie.alert({
						type: 3,
						text: data.Message,
						time: 4
					});
				}
			}
		});
	});
}