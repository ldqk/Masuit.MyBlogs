;(function($) {
	$.fn.serializeObject = function() {
		var o = {};
		var a = this.serializeArray();
		$.each(a, function() {
			if (o[this.name]) {
				if (!o[this.name].push) {
					o[this.name] = [o[this.name]];
				}
				o[this.name].push(this.value || '');
			} else {
				o[this.name] = this.value || '';
			}
		});
		return o;
	}
})(jQuery);
$(function() {
	window.ifvisible.blur(function() {
		$("body").animate({
			opacity: 0.5
		}, 100);
	});
	window.ifvisible.wakeup(function(e) {
		$("body").animate({
			opacity: 1
		}, 100);
	});
	popBrowserTips();
	$("img").lazyload({
		effect: "fadeIn", //渐现，show(直接显示),fadeIn(淡入),slideDown(下拉)
		threshold: 180, //预加载，在图片距离屏幕180px时提前载入
		//event: 'click',  // 事件触发时才加载，click(点击),mouseover(鼠标划过),sporty(运动的),默认为scroll（滑动）
		//container: $("#container"), // 指定对某容器中的图片实现效果
		failure_limit: 2 //加载2张可见区域外的图片,lazyload默认在找到第一张不在可见区域里的图片时则不再继续加载,但当HTML容器混乱的时候可能出现可见区域内图片并没加载出来的情况
	});
	$(".demo1").bootstrapNews({
		newsPerPage: 5,
		autoplay: true,
		pauseOnHover: true,
		direction: 'up',
		newsTickerInterval: 2000,
		onToDo: function() {
			//console.log(this);
		}
	});
	$(".notices").bootstrapNews({
		newsPerPage: 4,
		autoplay: true,
		pauseOnHover: true,
		navigation: false,
		direction: 'down',
		newsTickerInterval: 2500,
		onToDo: function() {
			//console.log(this);
		}
	});

	//全局加载动画
	$("a[href]").click(function(e) {
		if ($(this).attr("target") == "_blank") {
			return;
		}
		if ($(this).attr("href").indexOf("#") >= 0 || $(this).attr("href").indexOf("javascript") >= 0) {
			return;
		}
		loading();
		setTimeout(function() {
			loadingDone();
			window.notie.alert({
				type: 4,
				text: "页面加载失败！",
				time: 4
			});
		}, 60000);
	});

	function subscribe() {
		loading();
		$.post("/subscribe/subscribe", $("#subscribe").serialize(), function(data) {
			loadingDone();
			if (data && data.Success) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				$(':input', '#article-form').not(':button,:submit,:reset,:hidden').val('').removeAttr('checked').removeAttr('checked');
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
			}
		});
	}

	//订阅表单验证
	$("#subscribe").on("submit", function(e) {
		e.preventDefault();
		if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test($(".email").val().trim())) {
			window.notie.alert(3, "请输入正确的邮箱格式！", 4);
			return;
		}
		if($(".email").val().trim().indexOf("163")>1||$(".email").val().trim().indexOf("126")>1) {
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
					subscribe();
				}
			});
			return;
		}
		subscribe();
	});
	//new WOW().init();//滚动加载
	var nav=$(".cd-main-header");
	if (document.documentElement.scrollTop || document.body.scrollTop > 0) {
		nav.css("background-color", "white");
	} else {
		nav.css("background-color", "transparent");
	}
	document.onscroll = function() {
		if (document.documentElement.scrollTop || document.body.scrollTop > 10) {
			nav.css({
				"background-color": "white",
				transition: "all 1s ease-in-out"
			});
		} else {
			nav.css({
				"background-color": "transparent",
				transition: "all 1s ease-in-out"
			});
		}
	}

	//搜索建议
	$("#search").bsSuggest({
		allowNoKeyword: false, //是否允许无关键字时请求数据。为 false 则无输入时不执行过滤请求  
		multiWord: true, //以分隔符号分割的多关键字支持  
		separator: ",", //多关键字支持时的分隔符，默认为空格  
		getDataMethod: "url", //获取数据的方式，总是从 URL 获取  
		url: 'https://sp0.baidu.com/5a1Fazu8AA54nxGko9WTAnF6hhy/su?p=3&t=' + (new Date()).getTime() +
			'&wd=', /*优先从url ajax 请求 json 帮助数据，注意最后一个参数为关键字请求参数*/
		jsonp: 'cb', //如果从 url 获取数据，并且需要跨域，则该参数必须设置  
		processData: function(json) { // url 获取数据时，对数据的处理，作为 getData 的回调函数  
			var i, len, data = {
							value: []
						};
			if (!json || !json.s || json.s.length === 0) {
				return false;
			}
			len = json.s.length;
			var jsonStr = "{'value':[";
			for (i = 0; i < len; i++) {
				data.value.push({
					word: json.s[i]
				});
			}
			data.defaults = 'baidu';
			//字符串转化为 js 对象  
			return data;
		}
	});

	var tippy = new Tippy('.tippy-scale', {
		position: 'bottom',
		animation: 'scale',
		arrow: 'true',
		'theme': 'light'
	}); //注册tooltips
	
	$(".btn").on("mousedown", function(e) {
		window.ripplet(e, {
			color: null,
			className: 'rainbow',
			clearingDuration: '3s',
			spreadingDuration: '1s'
		});
	});
	if (!Object.prototype.hasOwnProperty.call(window, 'event')) {
		['mousedown', 'mouseenter', 'onmouseleave'].forEach(function(eventType) {
			window.addEventListener(eventType, function(event) {
				window.event = event;
			}, true);
		});
	}

	var nid = window.localStorage.getItem("notice") || '0';
	window.fetch("/notice/last", {
		credentials: 'include',
		method: 'POST',
		mode: 'cors'
	}).then(function(response) {
		return response.json();
	}).then(function(data) {
		data = data.Data;
		if (nid != data.Id) {
			//公告层
			layer.open({
				title: '网站公告：' + data.Title,
				offset: (window.screen.width > 400 ? "100px" : "40px"),
				area: (window.screen.width > 400 ? 400 : window.screen.width - 10) + 'px',
				shade: 0.6,
				closeBtn: true,
				content: data.Content,
				btn: ["查看详情", '知道了', '哦'],
				btn1: function(layero) {
					window.localStorage.setItem("notice", data.Id);
					window.location.href = "/n/" + data.Id;
					loading();
				},
				btn2: function(index) {
					window.localStorage.setItem("notice", data.Id);
					layer.closeAll();
				},
				btn3: function(index) {
					//return true;
				}
			});
		}
	}).catch(function(e) {
		console.log("Oops, error");
	});
});

//两个全局加载动画
function loading() {
	let r = new Date().getMilliseconds();
	//$(".loading" + (r % 3 + 1)).show();
	$(".loading1").show();
}

function loadingDone() {
	$(".loading1").hide();
	$(".loading2").hide();
	$(".loading3").hide();
}

function GetOperatingSystem(os) {
	if (os) {
		if (os.indexOf("Windows") >= 0) {
			return `<i class="icon-windows8"></i>${os}`;
		} else if (os.indexOf("Mac") >= 0) {
			return `<i class="icon-apple"></i>${os}`;
		} else if (os.indexOf("Chrome") >= 0) {
			return `<i class="icon-chrome"></i>${os}`;
		} else if (os.indexOf("Android") >= 0) {
			return `<i class="icon-android"></i>${os}`;
		} else {
			return `<i class="icon-stats"></i>${os}`;
		}
	} else {
		return `<i class="icon-stats"></i>未知操作系统`;
	}
}

function GetBrowser(browser) {
	if (browser) {
		if (browser.indexOf("Chrome") >= 0) {
			return `<i class="icon-chrome"></i>${browser}`;
		} else if (browser.indexOf("Firefox") >= 0) {
			return `<i class="icon-firefox"></i>${browser}`;
		} else if (browser.indexOf("IE") >= 0) {
			return `<i class="icon-IE"></i>${browser}`;
		} else if (browser.indexOf("Edge") >= 0) {
			return `<i class="icon-edge"></i>${browser}`;
		} else if (browser.indexOf("Opera") >= 0) {
			return `<i class="icon-opera"></i>${browser}`;
		} else if (browser.indexOf("Safari") >= 0) {
			return `<i class="icon-safari"></i>${browser}`;
		} else {
			return `<i class="icon-browser2"></i>${browser}`;
		}
	} else {
		return `<i class="icon-browser2"></i>未知浏览器`;
	}
}

function getFile(obj, inputName) {
	var file_name = $(obj).val();
	console.log(file_name);
	$("input[name='" + inputName + "']").val(file_name);
}

//递归加载评论
//加载父楼层
function loadParentComments(data) {
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
                                <header class="panel-heading">${startfloor}# ${rows[i].IsMaster ? `<i class="icon icon-user"></i>` : ""}${rows[i].NickName}${rows[i].IsMaster ? `(管理员)` : ""}
                                    <span class="pull-right" style="font-size: 10px;">${rows[i].CommentDate}<span class="hidden-sm hidden-xs"> | ${GetOperatingSystem(rows[i].OperatingSystem) + " | " + GetBrowser(rows[i].Browser)}</span></span>
                                </header>
                                <div class="panel-body">
                                    ${rows[i].Content} 
                                    <span class="cmvote btn btn-sm btn-info" data-id="${rows[i].Id}">有用(<span>${rows[i].VoteCount}</span>)</span>
                                    <a class="btn btn-sm btn-info" href="?uid=${rows[i].Id}">回复</a>
                                    ${loadComments(data.rows, Enumerable.From(data.rows).Where(c => c.ParentId === rows[i].Id).OrderBy(c => c.CommentDate).ToArray(), startfloor--)}
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
function loadComments(data, comments, root, depth = 0) {
    var colors = ["info", "success", "primary", "warning", "danger"];
    var floor = 1;
    depth++;
    var html = '';
    Enumerable.From(comments).ForEach((item, index) => {
	    var color = colors[depth%5];
		html += `<article id="${item.Id}" class="panel panel-${color}">
                        <div class="panel-heading">
                            ${depth}-${floor++}# ${item.IsMaster ?`<i class="icon icon-user"></i>`:""}${item.NickName}${item.IsMaster ?`(管理员)`:""}
                            <span class="pull-right" style="font-size: 10px;">${item.CommentDate}<span class="hidden-sm hidden-xs"> | ${GetOperatingSystem(item.OperatingSystem) + " | " + GetBrowser(item.Browser)}</span></span>
                        </div>
                        <div class="panel-body">
                            ${item.Content} 
                            <span class="cmvote btn btn-sm btn-${color}" data-id="${item.Id}">有用(<span>${item.VoteCount}</span>)</span>
                            <a class="btn btn-sm btn-${color}" href="?uid=${item.Id}">回复</a>
                            ${loadComments(data, Enumerable.From(data).Where(c => c.ParentId === item.Id).OrderBy(c => c.CommentDate), root, depth)}
                        </div>
                    </article>`;
    });
    return html;
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
										<a class="btn btn-sm btn-info" href="?uid=${rows[i].Id}">回复</a>
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
							<a class="btn btn-sm btn-${color}" href="?uid=${item.Id}">回复</a>
							${loadMsgs(data,Enumerable.From(data).Where(c => c.ParentId === item.Id).OrderBy(c => c.PostDate),root, depth)}
						</div>
					</article>`;
	});
	return html;
}

function popBrowserTips() {
	if (window.sessionStorage) {
		var deny = window.sessionStorage.getItem("deny") || false;
		if (window.screen.width <= 320 && !deny) {
			swal({
				title: '访问受限制?',
				html: "首先欢迎您的访问，由于检测到您的设备<span style='color:red'>屏幕宽度过小</span>，网站的部分功能可能不会兼容你的设备，但是您<span style='color:red'>可以继续浏览</span>，为确保最佳用户体验，建议使用<span style='color:red'>5寸以上移动设备</span>，或分辨率大于<span style='color:red'>1360 x 768</span>的<span style='color:red'>电脑浏览器</span>访问本站，感谢您的来访和支持！",
				type: 'error',
				showCloseButton: true,
				showCancelButton: true,
				confirmButtonColor: '#3085d6',
				cancelButtonColor: '#d33',
				confirmButtonText: '我知道了',
				cancelButtonText: '哦哦'
			}).then(function(isConfirm) {
				if (isConfirm) {
					window.sessionStorage.setItem("deny", true);
				}
			});
		}
	}
}