(function(root, factory) {
	//amd
	if (typeof define === 'function' && define.amd) {
		define(['$', 'query'], factory);
	} else if (typeof exports === 'object') { //umd
		module.exports = factory();
	} else {
		root.Paging = factory(window.Zepto || window.jQuery || $, Query);
	}
})(this, function($, Query) {
	$.fn.Paging = function(settings) {
		var arr = [];
		$(this).each(function() {
			var options = $.extend({
				target: $(this)
			}, settings);
			var lz = new Paging();
			lz.init(options);
			arr.push(lz);
		});
		return arr;
	};

	function Paging() {
		var rnd = Math.random().toString().replace('.', '');
		this.id = 'Paging_' + rnd;
	}
	Paging.prototype = {
		init: function(settings) {
			this.settings = $.extend({
				callback: null,
				pagesize: 10,
				current: 1,
				prevTpl: "上一页",
				nextTpl: "下一页",
				firstTpl: "首页",
				lastTpl: "末页",
				ellipseTpl: "...",
				toolbar: false,
				hash:true,
				pageSizeList: [5, 10, 15, 20]
			}, settings);
			this.target = $(this.settings.target);
			this.container = $('<div id="' + this.id + '" class="ui-paging-container"/>');
			this.target.append(this.container);
			this.render(this.settings);
			this.format();
			this.bindEvent();
		},
		render: function(ops) {
			this.count = ops.count || this.settings.count;
			this.pagesize = ops.pagesize || this.settings.pagesize;
			this.current = ops.current || this.settings.current;
			this.pagecount = Math.ceil(this.count / this.pagesize);
			this.format();
		},
		bindEvent: function() {
			var _this = this;
			this.container.on('click', 'li.js-page-action,li.ui-pager', function(e) {
				if ($(this).hasClass('ui-pager-disabled') || $(this).hasClass('focus')) {
					return false;
				}
				if ($(this).hasClass('js-page-action')) {
					if ($(this).hasClass('js-page-first')) {
						_this.current = 1;
					}
					if ($(this).hasClass('js-page-prev')) {
						_this.current = Math.max(1, _this.current - 1);
					}
					if ($(this).hasClass('js-page-next')) {
						_this.current = Math.min(_this.pagecount, _this.current + 1);
					}
					if ($(this).hasClass('js-page-last')) {
						_this.current = _this.pagecount;
					}
				} else if ($(this).data('page')) {
					_this.current = parseInt($(this).data('page'));
				}
				_this.go();
			});
			/*
			$(window).on('hashchange',function(){
				var page=  parseInt(Query.getHash('page'));
				if(_this.current !=page){
					_this.go(page||1);
				}
			})
			 */
		},
		go: function(p) {
			var _this = this;
			this.current = p || this.current;
			this.current = Math.max(1, _this.current);
			this.current = Math.min(this.current, _this.pagecount);
			this.format();
			if(this.settings.hash){
				Query.setHash({
					page:this.current
				});
			}
			this.settings.callback && this.settings.callback(this.current, this.pagesize, this.pagecount);
		},
		changePagesize: function(ps) {
			this.render({
				pagesize: ps
			});
		},
		format: function() {
			var html = '<ul>'
			html += '<li class="js-page-first js-page-action ui-pager" >' + this.settings.firstTpl + '</li>';
			html += '<li class="js-page-prev js-page-action ui-pager">' + this.settings.prevTpl + '</li>';
			if (this.pagecount > 6) {
				html += '<li data-page="1" class="ui-pager">1</li>';
				if (this.current <= 2) {
					html += '<li data-page="2" class="ui-pager">2</li>';
					html += '<li data-page="3" class="ui-pager">3</li>';
					html += '<li class="ui-paging-ellipse">' + this.settings.ellipseTpl + '</li>';
				} else
				if (this.current > 2 && this.current <= this.pagecount - 2) {
					html += '<li>' + this.settings.ellipseTpl + '</li>';
					html += '<li data-page="' + (this.current - 1) + '" class="ui-pager">' + (this.current - 1) + '</li>';
					html += '<li data-page="' + this.current + '" class="ui-pager">' + this.current + '</li>';
					html += '<li data-page="' + (this.current + 1) + '" class="ui-pager">' + (this.current + 1) + '</li>';
					html += '<li class="ui-paging-ellipse" class="ui-pager">' + this.settings.ellipseTpl + '</li>';
				} else {
					html += '<li class="ui-paging-ellipse" >' + this.settings.ellipseTpl + '</li>';
					for (var i = this.pagecount - 2; i < this.pagecount; i++) {
						html += '<li data-page="' + i + '" class="ui-pager">' + i + '</li>'
					}
				}
				html += '<li data-page="' + this.pagecount + '" class="ui-pager">' + this.pagecount + '</li>';
			} else {
				for (var i = 1; i <= this.pagecount; i++) {
					html += '<li data-page="' + i + '" class="ui-pager">' + i + '</li>'
				}
			}
			html += '<li class="js-page-next js-page-action ui-pager">' + this.settings.nextTpl + '</li>';
			html += '<li class="js-page-last js-page-action ui-pager">' + this.settings.lastTpl + '</li>';
			html += '</ul>';
			this.container.html(html);
			if (this.current == 1) {
				$('.js-page-prev', this.container).addClass('ui-pager-disabled');
				$('.js-page-first', this.container).addClass('ui-pager-disabled');
			}
			if (this.current == this.pagecount) {
				$('.js-page-next', this.container).addClass('ui-pager-disabled');
				$('.js-page-last', this.container).addClass('ui-pager-disabled');
			}
			this.container.find('li[data-page="' + this.current + '"]').addClass('focus').siblings().removeClass('focus');
			if (this.settings.toolbar) {
				this.bindToolbar();
			}
		},
		bindToolbar: function() {
			var _this = this;
			var html = $('<li class="ui-paging-toolbar"><select class="ui-select-pagesize"></select><input type="text" class="ui-paging-count"/><a href="javascript:void(0)">跳转</a></li>');
			var sel = $('.ui-select-pagesize', html);
			var str = '';
			for (var i = 0, l = this.settings.pageSizeList.length; i < l; i++) {
				str += '<option value="' + this.settings.pageSizeList[i] + '">' + this.settings.pageSizeList[i] + '条/页</option>';
			}
			sel.html(str);
			sel.val(this.pagesize);
			$('input', html).val(this.current);
			$('input', html).click(function() {
				$(this).select();
			}).keydown(function(e) {
				if (e.keyCode == 13) {
					var current = parseInt($(this).val()) || 1;
					_this.go(current);
				}
			});
			$('a', html).click(function() {
				var current = parseInt($(this).prev().val()) || 1;
				_this.go(current);
			});
			sel.change(function() {
				_this.changePagesize($(this).val());
			});
			this.container.children('ul').append(html);
		}
	}
	return Paging;
});