$(function () {
	$("#toc").show();
	var toc = $("#toc").tocify({
		selectors: ".ibox-content h3,.ibox-content h4,.ibox-content h5"
	}).data("toc-tocify");
	$(".tocify>.close").on("click", function(e) {
		$(this).parent().hide();
	});		
	$('article img').click(function(){
		window.open($(this).attr("src"));
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
		window.post("/post/CheckViewToken", $(this).serializeObject(), function(data) {
			if (data.Success) {
				window.location.reload();
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
			}
		}, function() {
			window.notie.alert({
				type: 3,
				text: "请求失败，请稍候再试！",
				time: 4
			});
		});
	});
	$(".getcode").on("click", function(e) {
		e.preventDefault();
		layer.tips('正在发送token，请稍候...', '.getcode', {
			tips: [1, '#3595CC'],
			time: 30000
		});
		$(".getcode").attr('disabled', true);
		window.post("/post/getviewtoken", {
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
				var count=0;
				var timer=setInterval(function() {
					count++;
					$(".getcode").text('重新发送('+(120-count)+')');
					if (count>120) {
						clearInterval(timer);
						$(".getcode").attr('disabled', false);
						$(".getcode").text('重新发送');
					}
				},1000);
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
				$(".getcode").attr('disabled', false);
			}
		}, function() {
			window.notie.alert({
				type: 3,
				text: "请求失败，请稍候再试！",
				time: 4
			});
		});
	});
	
$("#getcode").on("click", function (e) {
	e.preventDefault();
	layer.tips('正在发送验证码，请稍候...', '#getcode', {
		tips: [1, '#3595CC'],
		time: 30000
	});
	$("#getcode").attr('disabled', true);
	$.post("/validate/sendcode", {
		__RequestVerificationToken: $("[name=__RequestVerificationToken]").val(),
		email: $("#email").val()
	}, function (data) {
		if (data.Success) {
			layer.tips('验证码发送成功，请注意查收邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！', '#getcode', {
				tips: [1, '#3595CC'],
				time: 5000
			});
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
				time: 5000
			});
			$("#getcode").attr('disabled', false);
		}
	});
});
$("#getcode-reply").on("click", function (e) {
	e.preventDefault();
	layer.tips('正在发送验证码，请稍候...', '#getcode-reply', {
		tips: [1, '#3595CC'],
		time: 30000
	});
	$("#getcode-reply").attr('disabled', true);
	$.post("/validate/sendcode", {
		__RequestVerificationToken: $("[name=__RequestVerificationToken]").val(),
		email: $("#email2").val()
	}, function (data) {
		if (data.Success) {
			layer.tips('验证码发送成功，请注意查收邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！', '#getcode-reply', {
				tips: [1, '#3595CC'],
				time: 5000
			});
			$("#getcode-reply").attr('disabled', true);
			var count = 0;
			var timer = setInterval(function () {
				count++;
				$("#getcode-reply").text('重新发送(' + (120 - count) + ')');
				if (count > 120) {
					clearInterval(timer);
					$("#getcode-reply").attr('disabled', false);
					$("#getcode-reply").text('重新发送');
				}
			}, 1000);
		} else {
			layer.tips(data.Message, '#getcode-reply', {
				tips: [1, '#3595CC'],
				time: 5000
			});
			$("#getcode-reply").attr('disabled', false);
		}
	});
});

	bindReplyBtn();//绑定回复按钮事件
	bindVote();//绑定文章投票按钮
	window.getcomments();//获取评论
	commentVoteBind(); //评论投票
	$("#OperatingSystem").val(DeviceInfo.OS.toString());
	$("#Browser").val(DeviceInfo.browserInfo.Name+" "+DeviceInfo.browserInfo.Version);

	//异步提交评论表单
	$("#comment").on("submit", function(e) {
		e.preventDefault();
		layui.layedit.sync(1);
		if ($("#name").val().trim().length <= 1 || $("#name").val().trim().length > 24) {
			window.notie.alert({
				type: 3,
				text: '昵称要求2-24个字符！',
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
		var formData = $("#reply-form").serializeObject();
		if (formData["NickName"].trim().length <= 0 ||formData["NickName"].trim().length > 24) {
			window.notie.alert({
				type: 3,
				text: "昵称要求2-24个字符！",
				time: 4
			});
			loadingDone();
			return;
		}

		if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(formData["Email"].trim())) {
			window.notie.alert({
				type: 3,
				text: "请输入正确的邮箱格式！",
				time: 4
			});
			loadingDone();
			return;
		}
		
		window.post("/comment/submit", formData, function(data) {
			loadingDone();
			if (data.Success) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				layer.closeAll();
				setTimeout(function () {
					window.getcomments();
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
		}, function() {
			window.notie.alert({
				type: 3,
				text: "请求失败，请稍候再试！",
				time: 4
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
	window.post("/comment/submit", $(_this).serializeObject(), function(data) {
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
	}, function() {
		window.notie.alert({
			type: 3,
			text: "请求失败，请稍候再试！",
			time: 4
		});
	});
}

//评论回复按钮事件
function bindReplyBtn() {
	$(".msg-list article .panel-body a").on("click", function(e) {
		e.preventDefault();
		loadingDone();
		var href = $(this).attr("href");
		var uid = href.substring(href.indexOf("uid") + 4);
		$("#uid").val(uid);
		$("#OperatingSystem2").val(DeviceInfo.OS.toString());
		$("#Browser2").val(DeviceInfo.browserInfo.Name+" "+DeviceInfo.browserInfo.Version);
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
		window.post("/comment/CommentVote", { id: $(this).data("id") }, function(data) {
			if (data) {
				if (data.Success) {
					console.log($(this).children("span.count"));
					$(this).children("span.count").text(parseInt($(this).children("span.count").text())+1);
					$(this).addClass("disabled");
					this.disabled = true;
					window.notie.alert({ type: 1, text: data.Message, time: 4 });
				} else {
					window.notie.alert({ type: 3, text: data.Message, time: 4 });
				}
			}
		});
	});
}

function bindVote() {
	$("#voteup").on("click", function(e) {
		window.post("/post/voteup", { id: $("#postId").val() }, function(data) {
			if (data) {
				if (data.Success) {
					$("#voteup span").text(parseInt($("#voteup span").text()) + 1);
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
		}, function() {
			window.notie.alert({
				type: 3,
				text: "请求失败，请稍候再试！",
				time: 4
			});
		});
	});
	$("#votedown").on("click", function(e) {
		window.post("/post/votedown", { id: $("#postId").val() }, function(data) {
			if (data) {
				if (data.Success) {
					$("#votedown span").text(parseInt($("#votedown span").text()) + 1);
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
		}, function() {
			window.notie.alert({
				type: 3,
				text: "请求失败，请稍候再试！",
				time: 4
			});
		});
	});
}

//递归加载评论
//加载父楼层
function loadParentComments(data) {
	loading();
	var html = '';
	if (data) {
		var rows = data.rows;
		var page = data.page;
		var size = data.size;
		var maxPage = Math.ceil(data.total / size);
		page = page > maxPage ? maxPage : page;
		page = page < 1 ? 1 : page;
		var startfloor = data.parentTotal - (page - 1) * size;
		for (let i = 0; i < rows.length; i++) {
			html += `<li class="msg-list media animated fadeInRight" id='${rows[i].Id}'>
						<div class="media-body">
							<article class="panel panel-info">
								<header class="panel-heading ${rows[i].IsMaster ? "text-red" : ""} ${rows[i].IsAuthor ? "text-bold" : ""}">${startfloor--}# ${rows[i].IsMaster ? `<i class="icon icon-user"></i>` : ""}${rows[i].NickName}${rows[i].IsMaster ? `(管理员)` : ""} | ${rows[i].CommentDate}
									<span class="pull-right hidden-sm hidden-xs" style="font-size: 10px;">${GetOperatingSystem(rows[i].OperatingSystem) + " | " + GetBrowser(rows[i].Browser)}</span>
								</header>
								<div class="panel-body">
									${rows[i].Content} 
									<span class="cmvote label label-info" data-id="${rows[i].Id}"><i class="icon-thumbsup"></i>(<span>${rows[i].VoteCount}</span>)</span>
									<a class="label label-info" href="?uid=${rows[i].Id}"><i class="icon-comment"></i></a>
									${loadComments(rows[i].Children)}
								</div>
							</article>
						</div>
					</li>`;
		}
	}
	loadingDone();
	return html;
}

//加载子楼层
function loadComments(comments, depth = 0) {
	comments.sort(function(x, y) {
		return x.Id - y.Id
	});

	var colors = ["info", "success", "primary", "warning", "danger"];
	var floor = 1;
	depth++;
	var html = '';
	for (let item of comments) {
		var color = colors[depth%5];
		html += `<article id="${item.Id}" class="panel panel-${color}">
						<div class="panel-heading ${item.IsMaster ? "text-red" : ""} ${item.IsAuthor ? "text-bold" : ""}">
							${depth}-${floor++}# ${item.IsMaster ?`<i class="icon icon-user"></i>`:""}${item.NickName}${item.IsMaster ?`(管理员)`:""} | ${item.CommentDate}
							<span class="pull-right hidden-sm hidden-xs" style="font-size: 10px;">${GetOperatingSystem(item.OperatingSystem) + " | " + GetBrowser(item.Browser)}</span>
						</div>
						<div class="panel-body">
							${item.Content} 
							<span class="cmvote label label-${color}" data-id="${item.Id}"><i class="icon-thumbsup"></i>(<span>${item.VoteCount}</span>)</span>
							<a class="label label-${color}" href="?uid=${item.Id}"><i class="icon-comment"></i></a>
							${loadComments(item.Children, depth)}
						</div>
					</article>`;
	}
	return html;
}