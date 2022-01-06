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
	$('body').css({
	   'filter': 'blur(0)',
	   '-webkit-filter' : 'blur(0)',
	   '-moz-filter': 'blur(0)',
	   '-o-filter': 'blur(0)',
	   '-ms-filter': 'blur(0)',
	   "transition": "all 1s ease-in-out"
	});
	loadingDone();

	$("img").lazyload({
		effect: "fadeIn", //渐现，show(直接显示),fadeIn(淡入),slideDown(下拉)
		threshold: 2700, //预加载，在图片距离屏幕180px时提前载入
		//event: 'click',  // 事件触发时才加载，click(点击),mouseover(鼠标划过),sporty(运动的),默认为scroll（滑动）
		//container: $("#container"), // 指定对某容器中的图片实现效果
		failure_limit: 10 //加载2张可见区域外的图片,lazyload默认在找到第一张不在可见区域里的图片时则不再继续加载,但当HTML容器混乱的时候可能出现可见区域内图片并没加载出来的情况
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

	//new WOW().init();//滚动加载
	var nav = $(".cd-main-header");
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
	
	window.fetch("/notice/last").then(function(response) {
		return response.json();
	}).then(function(data) {
		if (!data.Success) {
			return ;
		}
		data = data.Data;
		var nid = [].concat(JSON.parse(window.localStorage.getItem("notice") || '[]'));
		if (nid.indexOf(data.Id)==-1) {
			//公告层
			layer.open({
				title: '网站公告：' + data.Title,
				offset: (window.screen.width > 400 ? "100px" : "40px"),
				area: (window.screen.width > 400 ? 400 : window.screen.width - 10) + 'px',
				shade: 0.6,
				closeBtn: true,
				content: data.Content,
				btn: ["查看详情", '知道了'],
				btn1: function(layero) {
					nid.push(data.Id);
					window.localStorage.setItem("notice", JSON.stringify(nid));
					window.location.href = "/notice/" + data.Id;
					loading();
				},
				btn2: function(index) {
					nid.push(data.Id);
					window.localStorage.setItem("notice", JSON.stringify(nid));
					layer.closeAll();
				}
			});
		}
	}).catch(function(e) {
		console.log("Oops, error");
	});

	setInterval(function() {
		let timestamp = new Date().getTime();
		DotNet.invokeMethodAsync('Masuit.MyBlogs.Core', 'Latency').then(data => {
			$("#ping").text(new Date().getTime()-timestamp);
		});
	}, 2000);

	// 自动重试加载图片
	$('img').on("error",function() {
	   var that=$(this);
	   var retry=that.attr("retry")||0;
	   if(retry>10){
		  return ;
	   }else{
		  retry++;
		  that.attr("retry", retry);//重试次数+1
		  that.attr('src', that.attr("src"));//继续刷新图片
	   }
	});
	$('img').on("abort",function() {
	   var that=$(this);
	   var retry=that.attr("retry")||0;
	   if(retry>10){
		  return ;
	   }else{
		  retry++;
		  that.attr("retry", retry);//重试次数+1
		  that.attr('src', that.attr("src"));//继续刷新图片
	   }
	});
});

//全局加载动画
function loading() {
	$(".loading1").show();
}

function loadingDone() {
	$(".loading1").hide();
}
var clearSelect= "getSelection" in window ? function(){
 window.getSelection().removeAllRanges();
} : function(){
 document.selection.empty();
};

function hackClip() {
	let transfer = document.createElement('input');
	document.body.appendChild(transfer);
	transfer.value = '1';
	transfer.select();
	if (document.execCommand('copy')) {
		document.execCommand('copy');
	}
	document.body.removeChild(transfer);
}

/**禁止复制 */
function CopyrightProtect() {
	setInterval(function() {
		try {
			(function() {}["constructor"]("debugger")());
			$(".article-content").on("keydown",function (e) {
				var currKey = 0, evt = e || window.event;
				currKey = evt.keyCode || evt.which || evt.charCode;
				if (currKey == 123 || (evt.ctrlKey && currKey == 67) || (evt.ctrlKey && currKey == 83) || (evt.ctrlKey && currKey == 85)) { //禁止F12，Ctrl+C，Ctrl+U
					clearSelect();
					evt.cancelBubble = true;
					evt.returnValue = false;
					return false;
				}
			});
			document.onkeydown = function (e) {
				var currKey = 0, evt = e || window.event;
				currKey = evt.keyCode || evt.which || evt.charCode;
				if (currKey == 123 || (evt.ctrlKey && currKey == 65) || (evt.ctrlKey && currKey == 83) || (evt.ctrlKey && currKey == 85) || (evt.ctrlKey && evt.shiftKey) || evt.altKey) {
					clearSelect();
					evt.cancelBubble = true;
					evt.returnValue = false;
					return false;
				}
			}
			document.ondragstart=function (e) {
				e.returnValue = false;
				hackClip();
				return false;
			}
			$(".article-content").on("copy",function (e) {
				e.returnValue = false;
				hackClip();
				return false;
			});
			document.oncontextmenu = function (e) {
				e.returnValue = false;
				hackClip();
				return false;
			}
		} catch (ex) {
			console.error(ex);
		}
	},500);
}

/**禁止编辑器内复制 */
function CopyrightProtect4Editor() {
	setInterval(function() {
		try {
			(function() {}["constructor"]("debugger")());
			document.getElementById("ueditor_0").contentWindow.document.body.onkeydown = function (e) {
				var currKey = 0, evt = e || window.event;
				currKey = evt.keyCode || evt.which || evt.charCode;
				if (currKey == 123 || (evt.ctrlKey && currKey == 67) || (evt.ctrlKey && currKey == 83) || (evt.ctrlKey && currKey == 85) || (evt.ctrlKey && currKey == 88) || (evt.ctrlKey && evt.shiftKey) || evt.altKey) {
					clearSelect();
					evt.cancelBubble = true;
					evt.returnValue = false;
					return false;
				}
			}
			document.getElementById("ueditor_0").contentWindow.document.body.ondragstart = function (e) {
				e.returnValue = false;
				hackClip();
				return false;
			}
			document.getElementById("ueditor_0").contentWindow.document.body.oncopy = function (e) {
				e.returnValue = false;
				hackClip();
				return false;
			}
		} catch (ex) {
			console.error(ex);
		}
	},500);
}
/**禁止全局复制 */
function GlobalCopyrightProtect() {
	setInterval(function() {
		try {
			(function() {}["constructor"]("debugger")());
			$(".article-content").on("keydown",function (e) {
				var currKey = 0, evt = e || window.event;
				currKey = evt.keyCode || evt.which || evt.charCode;
				if (currKey == 123 || (evt.ctrlKey && currKey == 67) || (evt.ctrlKey && currKey == 83)|| (evt.ctrlKey && currKey == 85)) { //禁止F12，Ctrl+C，Ctrl+U
					evt.cancelBubble = true;
					evt.returnValue = false;
					clearSelect();
					return false;
				}
			});
			document.onkeydown = function (e) {
				var currKey = 0, evt = e || window.event;
				currKey = evt.keyCode || evt.which || evt.charCode;
				if (currKey == 123 || (evt.ctrlKey && currKey == 65) || (evt.ctrlKey && currKey == 83) || (evt.ctrlKey && currKey == 85) || (evt.ctrlKey && evt.shiftKey) || evt.altKey) {
					evt.cancelBubble = true;
					evt.returnValue = false;
					clearSelect();
					return false;
				}
			}
			document.ondragstart=function (e) {
				e.returnValue = false;
				hackClip();
				return false;
			}
			$(".article-content").on("copy",function (e) {
				e.returnValue = false;
				hackClip();
				return false;
			});
			document.oncontextmenu = function () {
				event.returnValue = false;
				hackClip();
				return false;
			}
		} catch (ex) {
			console.error(ex);
		}
	},500);
}

function GetOperatingSystem(os) {
	if (os) {
		if (os.indexOf("Windows") >= 0) {
			return '<i class="icon-windows8"></i>'+os;
		} else if (os.indexOf("Mac") >= 0) {
			return '<i class="icon-apple"></i>'+os;
		} else if (os.indexOf("Chrome") >= 0) {
			return '<i class="icon-chrome"></i>'+os;
		} else if (os.indexOf("Android") >= 0) {
			return '<i class="icon-android"></i>'+os;
		} else {
			return '<i class="icon-stats"></i>'+os;
		}
	} else {
		return '<i class="icon-stats"></i>未知操作系统';
	}
}

function GetBrowser(browser) {
	if (browser) {
		if (browser.indexOf("Chrome") >= 0) {
			return '<i class="icon-chrome"></i>'+browser;
		} else if (browser.indexOf("Firefox") >= 0) {
			return '<i class="icon-firefox"></i>'+browser;
		} else if (browser.indexOf("IE") >= 0) {
			return '<i class="icon-IE"></i>'+browser;
		} else if (browser.indexOf("Edge") >= 0) {
			return '<i class="icon-edge"></i>'+browser;
		} else if (browser.indexOf("Opera") >= 0) {
			return '<i class="icon-opera"></i>'+browser;
		} else if (browser.indexOf("Safari") >= 0) {
			return '<i class="icon-safari"></i>'+browser;
		} else {
			return '<i class="icon-browser2"></i>'+browser;
		}
	} else {
		return '<i class="icon-browser2"></i>未知浏览器';
	}
}

function getFile(obj, inputName) {
	$("input[name='" + inputName + "']").val($(obj).val());
}

function post(url, params, callback, error) {
	var formData = new FormData();
	Object.keys(params).forEach(function(key) {
		formData.append(key, params[key]);
	});
	window.fetch(url, {
		credentials: 'include',
		method: 'POST',
		mode: 'cors',
		body: formData
	}).then(function(response) {
		return response.json();
	}).then(function(data) {
		callback(data);
	}).catch(function(e) {
		loadingDone();
		if (error) {
			error(e);
		}
	});
}
function get(url, callback, error) {
	window.fetch(url, {
		credentials: 'include',
		method: 'GET',
		mode: 'cors'
	}).then(function(response) {
		return response.json();
	}).then(function(data) {
		callback(data);
	}).catch(function(e) {
		loadingDone();
		if (error) {
			error(e);
		}
	});
}

async function blockCategory(id,name) {
	await swal({
		title: "确认屏蔽【"+name+"】吗？",
		text: "屏蔽之后将不再收到该分类的相关推送！若需要取消屏蔽，清除本站的浏览器缓存即可。",
		showCancelButton: true,
		confirmButtonColor: "#DD6B55",
		confirmButtonText: "确定",
		cancelButtonText: "取消",
		showLoaderOnConfirm: true,
		animation: true,
		allowOutsideClick: false
	}).then(async function() {
		cookieStore.set({
		  name: "HideCategories",
		  value: id+"%2C"+(await cookieStore.get("HideCategories")||{value:"0"}).value,
		  expires: Date.now() + 24*60*60*365
		}).then(
		  function() {
			swal({
				text: "屏蔽成功",type:"success",
				showConfirmButton: false,
				timer:1500
			}).catch(swal.noop)
		  },
		  function(reason) {
			console.error("It failed: ", reason);
		  }
		);
	}, function() {
	}).catch(swal.noop);
}

/**
 * 鼠标桃心
 */
(function(window, document) {
	var hearts = [];
	window.requestAnimationFrame = (function() {
		return window.requestAnimationFrame ||
			window.webkitRequestAnimationFrame ||
			window.mozRequestAnimationFrame ||
			window.oRequestAnimationFrame ||
			window.msRequestAnimationFrame ||
			function(callback) {
				setTimeout(callback, 1000 / 60);
			}
	})();
	init();

	function init() {
		css(".heart{width: 10px;height: 10px;position: fixed;background: #f00;transform: rotate(45deg);-webkit-transform: rotate(45deg);-moz-transform: rotate(45deg);}.heart:after,.heart:before{content: '';width: inherit;height: inherit;background: inherit;border-radius: 50%;-webkit-border-radius: 50%;-moz-border-radius: 50%;position: absolute;}.heart:after{top: -5px;}.heart:before{left: -5px;}");
		attachEvent();
		gameloop();
	}

	function gameloop() {
		for (var i = 0; i < hearts.length; i++) {
			if (hearts[i].alpha <= 0) {
				document.body.removeChild(hearts[i].el);
				hearts.splice(i, 1);
				continue;
			}
			hearts[i].y--;
			hearts[i].scale += 0.004;
			hearts[i].alpha -= 0.013;
			hearts[i].el.style.cssText =
				"left:" +
				hearts[i].x +
				"px;top:" +
				hearts[i].y +
				"px;opacity:" +
				hearts[i].alpha +
				";transform:scale(" +
				hearts[i].scale +
				"," +
				hearts[i].scale +
				") rotate(45deg);background:" +
				hearts[i].color;
		}
		requestAnimationFrame(gameloop);
	}

	function attachEvent() {
		var old = typeof window.onclick === "function" && window.onclick;
		window.onclick = function(event) {
			old && old();
			createHeart(event);
		}
	}

	function createHeart(event) {
		var d = document.createElement("div");
		d.className = "heart";
		hearts.push({
			el: d,
			x: event.clientX - 5,
			y: event.clientY - 5,
			scale: 1,
			alpha: 1,
			color: randomColor()
		});
		document.body.appendChild(d);
	}

	function css(css) {
		var style = document.createElement("style");
		style.type = "text/css";
		try {
			style.appendChild(document.createTextNode(css));
		} catch (ex) {
			style.styleSheet.cssText = css;
		}
		document.getElementsByTagName('head')[0].appendChild(style);
	}

	function randomColor() {
		return "rgb(" +
			(~~(Math.random() * 255)) +
			"," +
			(~~(Math.random() * 255)) +
			"," +
			(~~(Math.random() * 255)) +
			")";
	}
})(window, document);