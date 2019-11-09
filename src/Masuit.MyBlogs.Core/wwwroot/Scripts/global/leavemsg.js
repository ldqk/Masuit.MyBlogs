$(function() {
    layui.use('layedit', function() {
        layui.layedit.build('layedit', {
			tool: ["strong", "italic", 'link', "unlink", "face"],
			height: 150
        });
    });
	$("#OperatingSystem").val(platform.os.toString());
	$("#Browser").val(platform.name + " " + platform.version);
	getmsgs();
	var user = JSON.parse(localStorage.getItem("user"));
	if (user) {
		$("[name='NickName']").val(user.NickName);
		$("[name='Email']").val(user.Email);
		$("[name='QQorWechat']").val(user.QQorWechat);
	}
    //异步提交留言表单开始
    $("#msg-form").on("submit", function(e) {
        e.preventDefault();
        layui.layedit.sync(1);
        if ($("#name").val().trim().length <= 0 || $("#name").val().trim().length > 36) {
	        window.notie.alert({
		        type: 3,
		        text: '昵称要求2-36个字符！',
		        time: 4
	        });
            loadingDone();
            return;
        }
        if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test($("#email").val().trim())) {
	        window.notie.alert({
		        type: 3,
                text: '请输入正确的邮箱格式！',
		        time: 4
	        });
            loadingDone();
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
        if ($("#layedit").val().trim().length <= 2 || $("#layedit").val().trim().length > 1000) {
	        window.notie.alert({
		        type: 3,
                text: '内容过短或者超长，请输入有效的留言内容！',
		        time: 4
	        });
            loadingDone();
            return;
		}
		submitComment(this);
    });
    //异步提交留言表单结束
	
    //表单取消按钮
    $(".btn-cancel").click(function() {
        $(':input', '#reply-form').not(':button,:submit,:reset,:hidden').val('').removeAttr('checked').removeAttr('checked'); //评论成功清空表单
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
        if ($("#name2").val().trim().length <= 0 || $("#name").val().trim().length > 36) {
	        window.notie.alert({
		        type: 3,
                text: "昵称要求2-36个字符！",
		        time: 4
	        });
            loadingDone();
            return;
        }
        if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test($("#email2").val().trim())) {
	        window.notie.alert({
		        type: 3,
                text: "请输入正确的邮箱格式！",
		        time: 4
	        });
            loadingDone();
            return;
        }
		localStorage.setItem("user", JSON.stringify($(this).serializeObject()));
		$.post("/Msg/Put", $(this).serialize(), (data) => {
            loadingDone();
            if (data && data.Success) {
		        window.notie.alert({
			        type: 1,
			        text: data.Message,
			        time: 4
		        });
				layer.closeAll();
					setTimeout(function() {
					getmsgs();
					$("#reply").css("display", "none");
					$("[id^=LAY_layedit]").contents().find('body').html('');
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
});
/**
 * 提交留言
 * @returns {} 
 */
function submitComment(_this) {
    loading();
	localStorage.setItem("user", JSON.stringify($(_this).serializeObject()));
    $.post("/Msg/Put", $(_this).serialize(), (data) => {
        loadingDone();
        if (data && data.Success) {
			window.notie.alert({
				type: 1,
				text:data.Message,
				time: 4
			});
			setTimeout(function() {
				getmsgs();
				$("[id^=LAY_layedit]").contents().find('body').html('');
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
	$(".msg-list article .panel-body a").on("click", function (e) {
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
		layui.use("layer", function() {
			var layer = layui.layer;
			layer.open({
				type: 1,
				zIndex:20,
				title: '回复留言',
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

//递归加载留言
//加载父楼层
function loadParentMsgs(data) {
	loading();
	var html = '';
	if (data) {
		var rows = Enumerable.From(data.rows).Where(c => c.ParentId === 0).ToArray();
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
									<header class="panel-heading">${startfloor}# ${rows[i].IsMaster? `<i class="icon icon-user"></i>` : ""}${rows[i].NickName}${rows  [i].IsMaster ? `(管理员)` : ""}
										<span class="pull-right" style="font-size: 10px;">${rows[i].PostDate}<span class="hidden-sm hidden-xs"> | ${GetOperatingSystem(rows[i].OperatingSystem) + " | " + GetBrowser(rows[i].Browser)}</span></span>
									</header>
									<div class="panel-body">
										${rows[i].Content}
										<a class="label label-info" href="?uid=${rows[i].Id}"><i class="icon-comment"></i></a>
										${loadMsgs(data.rows,Enumerable.From(data.rows).Where(c => c.ParentId === rows[i].Id).OrderBy(c => c.PostDate).ToArray(), startfloor--)}
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
function loadMsgs(data, msg, root, depth = 0) {
	var colors = ["info", "success", "primary", "warning", "danger"];
	var floor = 1;
	depth++;
	var html = '';
	Enumerable.From(msg).ForEach((item, index) => {
		var color = colors[depth % 5];
		html += `<article id="${item.Id}" class="panel panel-${color}">
						<div class="panel-heading">
							${depth}-${floor++}# ${item.IsMaster ? `<i class="icon icon-user"></i>` : ""}${item.NickName}${item.IsMaster ? `(管理员)` : ""}<span class="pull-right" style="font-size: 10px;">${item.PostDate}<span class="hidden-sm hidden-xs"> | ${GetOperatingSystem(item.OperatingSystem) + " | " + GetBrowser(item.Browser)}</span>
							</span>
						</div>
						<div class="panel-body">
							${item.Content}
							<a class="label label-${color}" href="?uid=${item.Id}"><i class="icon-comment"></i></a>
							${loadMsgs(data,Enumerable.From(data).Where(c => c.ParentId === item.Id).OrderBy(c => c.PostDate),root, depth)}
						</div>
					</article>`;
	});
	return html;
}