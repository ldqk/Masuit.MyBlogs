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
    popBrowserTips();
    $("img").lazyload({
        effect: "fadeIn", //渐现，show(直接显示),fadeIn(淡入),slideDown(下拉)
        threshold: 2700, //预加载，在图片距离屏幕180px时提前载入
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

    //搜索建议
    $("#search").bsSuggest({
        allowNoKeyword: false, //是否允许无关键字时请求数据。为 false 则无输入时不执行过滤请求  
        multiWord: true, //以分隔符号分割的多关键字支持  
        separator: ",", //多关键字支持时的分隔符，默认为空格  
        getDataMethod: "url", //获取数据的方式，总是从 URL 获取  
        url: 'https://sp0.baidu.com/5a1Fazu8AA54nxGko9WTAnF6hhy/su?p=3&t=' + (new Date()).getTime() + '&wd=', /*优先从url ajax 请求 json 帮助数据，注意最后一个参数为关键字请求参数*/
        jsonp: 'cb', //如果从 url 获取数据，并且需要跨域，则该参数必须设置  
        processData: function(json) { // url 获取数据时，对数据的处理，作为 getData 的回调函数  
            var i, data = {
                    value: []
                };
            if (!json || !json.s || json.s.length === 0) {
                return false;
            }
            var len = json.s.length;
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
            document.ondragstart=function () {
                return false;
            }
            $(".article-content").on("copy",function (e) {
                e.returnValue = false;
                return false;
            });
            document.oncontextmenu = function (e) {
                e.returnValue = false;
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
                return false;
            }
            document.getElementById("ueditor_0").contentWindow.document.body.oncopy = function (e) {
                e.returnValue = false;
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
            document.ondragstart=function () {
                return false;
            }
            $(".article-content").on("copy",function (e) {
                e.returnValue = false;
                return false;
            });
            document.oncontextmenu = function () {
                event.returnValue = false;
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

function popBrowserTips() {
    if (window.sessionStorage) {
        var deny = window.sessionStorage.getItem("deny") || false;
        if (window.screen.width <= 320 && !deny) {
            swal({
                title: '访问受限制?',
                html: "由于检测到您的设备<span style='color:red'>屏幕宽度过小</span>，网站的部分功能可能不会兼容你的设备，但是您<span style='color:red'>可以继续浏览</span>，为确保最佳用户体验，建议使用<span style='color:red'>5寸以上移动设备</span>，或分辨率大于<span style='color:red'>1360 x 768</span>的<span style='color:red'>电脑浏览器</span>访问本站，感谢您的来访和支持！",
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