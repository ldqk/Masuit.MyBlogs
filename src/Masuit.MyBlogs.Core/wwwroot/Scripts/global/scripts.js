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
	//$('.header').css({
	//   'filter': 'unset',
	//   '-webkit-filter' : 'unset',
	//   '-moz-filter': 'unset',
	//   '-o-filter': 'unset',
	//   '-ms-filter': 'unset',
	//   "transition": "all 0.1s ease-in-out"
	//});
	loadingDone();
	
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