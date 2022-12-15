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
    let value = Cookies.get("HideCategories")||"0";
    if (value.split(",").indexOf(id+"")>-1) {
        await swal({
		    title: "确认移除屏蔽【"+name+"】吗？",
		    text: "移除屏蔽之后可能会出现一些引起不适的内容，请谨慎操作，确认关闭吗？",
		    showCancelButton: true,
		    confirmButtonColor: "#DD6B55",
		    confirmButtonText: "确定",
		    cancelButtonText: "取消",
		    showLoaderOnConfirm: true,
		    animation: true,
		    allowOutsideClick: false
	    }).then(async function() {
		    Cookies.set("HideCategories",value.split(",").filter(function(item){return item!=id}).join(","),{ expires: 365 });
			swal({
				text: "取消屏蔽成功",type:"success",
				showConfirmButton: false,
				timer:1500
			}).catch(swal.noop);
        }, function() {
	    }).catch(swal.noop);
    } else {
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
		    Cookies.set("HideCategories",id+","+value, {expires: 365 });
            swal({
				text: "屏蔽成功",type:"success",
				showConfirmButton: false,
				timer:1500
			}).catch(swal.noop);
	    }, function() {
	    }).catch(swal.noop);
    }
}

async function disableSafemode() {
	await swal({
		title: "确认关闭安全模式吗？",
		text: "关闭安全模式后可能会出现一些引起不适的内容，请谨慎操作，确认关闭吗？",
		showCancelButton: true,
		confirmButtonColor: "#DD6B55",
		confirmButtonText: "确定",
		cancelButtonText: "取消",
		showLoaderOnConfirm: true,
		animation: true,
		allowOutsideClick: false
	}).then(async function() {
		Cookies.set("Nsfw",0,{ expires: 3650 });
        location.reload();
	}, function() {
	}).catch(swal.noop);
}

async function enableSafemode() {
	if (localStorage.getItem("DefaultSafeMode")==1) {
		return ;
	}
	
	Cookies.set("Nsfw",1,{ expires: 3650 });
    localStorage.setItem("DefaultSafeMode",1);
}

/*默认安全模式*/
;$(function() {
    if(Cookies.get("Nsfw")!="0"){
        $("body").append("<a style='position:fixed;left:0;bottom:0;color:black;z-index:10;text-shadow: 0px 0px 1px #000;'>安全模式</a>");
    }
});