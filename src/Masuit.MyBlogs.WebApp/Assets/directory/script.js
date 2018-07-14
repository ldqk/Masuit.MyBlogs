;
$(function() {
	/*
    * 仿百度百科右侧导航代码 - 页面目录结构导航 v0.01
    * 只写了两级，无限级别也可以，是逻辑上的级别，html结构全是同一级别
    * 滑标动画用的css3过渡动画，不支持的浏览器就没动画效果了
    * 和百度百科比起来还是比较弱，没有实现右边也可以滚动的功能
    */
	function DirectoryNav($h, config) {
		this.opts = $.extend(true, {
			scrollThreshold: 0.1, //滚动检测阀值 0.5在浏览器窗口中间部位
			scrollSpeed: 700, //滚动到指定位置的动画时间
			scrollTopBorder: 500, //滚动条距离顶部多少的时候显示导航，如果为0，则一直显示
			easing: 'swing', //不解释
			delayDetection: 200, //延时检测，避免滚动的时候检测过于频繁
			scrollChange: function() {}
		}, config);
		this.$win = $(window);
		this.$h = $h;
		this.$pageNavList = "";
		this.$pageNavListLis = "";
		this.$curTag = "";
		this.$pageNavListLiH = "";
		this.offArr = [];
		this.curIndex = 0;
		this.scrollIng = false;
		this.init();
	}

	DirectoryNav.prototype = {
		init: function() {
			this.make();
			this.setArr();
			this.bindEvent();
		},
		make: function() {
			//生成导航目录结构,这是根据需求自己生成的。如果你直接在页面中输出一个结构那也挺好不用 搞js
			$("body").append(
				'<div class="directory-nav" id="directoryNav"><ul></ul><span class="cur-tag"></span><span class="c-top"></span><span class="c-bottom"></span><span class="line"></span></div>>');
			var $hs = this.$h,
				$directoryNav = $("#directoryNav"),
				temp = [],
				index1 = 0,
				index2 = 0;
			$hs.each(function(index) {
				var $this = $(this),
					text = $this.text();
				if (this.tagName.toLowerCase() == 'h2') {
					index1++;
					if (index1 % 2 == 0) {
						index2 = 0;
					}
					temp.push('<li class="l1"><span class="c-dot"></span>' + index1 + '. <a class="l1-text">' + text + '</a></li>');
				} else {
					index2++;
					temp.push('<li class="l2">' + index1 + '.' + index2 + ' <a class="l2-text">' + text + '</a></li>');

				}
			});
			$directoryNav.find("ul").html(temp.join(""));

			//设置变量
			this.$pageNavList = $directoryNav;
			this.$pageNavListLis = this.$pageNavList.find("li");
			this.$curTag = this.$pageNavList.find(".cur-tag");
			this.$pageNavListLiH = this.$pageNavListLis.eq(0).height();

			if (!this.opts.scrollTopBorder) {
				this.$pageNavList.show();
			}
		},
		setArr: function() {
			var This = this;
			this.$h.each(function() {
				var $this = $(this),
					offT = Math.round($this.offset().top);
				This.offArr.push(offT);
			});
		},
		posTag: function(top) {
			this.$curTag.css({
				top: top + 'px'
			});
		},
		ifPos: function(st) {
			var offArr = this.offArr;
			//console.log(st);
			var windowHeight = Math.round(this.$win.height() * this.opts.scrollThreshold);
			for (var i = 0; i < offArr.length; i++) {
				if ((offArr[i] - windowHeight) < st) {
					var $curLi = this.$pageNavListLis.eq(i),
						tagTop = $curLi.position().top;
					$curLi.addClass("cur").siblings("li").removeClass("cur");
					this.curIndex = i;
					this.posTag(tagTop + this.$pageNavListLiH * 0.5);
					//this.curIndex = this.$pageNavListLis.filter(".cur").index();
					this.opts.scrollChange.call(this);
				}
			}
		},
		bindEvent: function() {
			var This = this,
				show = false,
				timer = 0;
			this.$win.on("scroll", function() {
				var $this = $(this);
				clearTimeout(timer);
				timer = setTimeout(function() {
					This.scrollIng = true;
					if ($this.scrollTop() > This.opts.scrollTopBorder) {
						if (!This.$pageNavListLiH) {
							This.$pageNavListLiH = This.$pageNavListLis.eq(0).height();
						}
						if (!show) {
							This.$pageNavList.fadeIn();
							show = true;
						}
						This.ifPos($(this).scrollTop());
					} else {
						if (show) {
							This.$pageNavList.fadeOut();
							show = false;
						}
					}
				}, This.opts.delayDetection);
			});

			this.$pageNavList.on("click", "li", function() {
				var $this = $(this),
					index = $this.index();
				This.scrollTo(This.offArr[index]);
			})
		},
		scrollTo: function(offset, callback) {
			var This = this;
			$('html,body').animate({
				scrollTop: offset
			}, this.opts.scrollSpeed, this.opts.easing, function() {
				This.scrollIng = false;
				//修正弹两次回调 蛋疼
				callback && this.tagName.toLowerCase() == 'body' && callback();
			});
		}
	};

	if (window.screen.width > 960) {
		//实例化
		var directoryNav = new DirectoryNav($(".ibox-content h2,.ibox-content h3,.ibox-content h4,.ibox-content h5"), {
			scrollTopBorder: 0 //滚动条距离顶部多少的时候显示导航，如果为0，则一直显示
		});
	}
});