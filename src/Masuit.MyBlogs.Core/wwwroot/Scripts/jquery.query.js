(function(root, factory) {
	//amd
	if (typeof define === 'function' && define.amd) {
		define(['$'], factory);
	} else if (typeof exports === 'object') { //umd
		module.exports = factory();
	} else {
		root.Query = factory(window.Zepto || window.jQuery || $);
	}
})(this, function($) {
	var Query = {
		getQuery: function(name, type, win) {
			var reg = new RegExp("(^|&|#)" + name + "=([^&]*)(&|$|#)", "i");
			win = win || window;
			var Url = win.location.href;
			var u, g, StrBack = '';
			if (type == "#") {
				u = Url.split("#");
			} else {
				u = Url.split("?");
			}
			if (u.length == 1) {
				g = '';
			} else {
				g = u[1];
			}
			if (g != '') {
				gg = g.split(/&|#/);
				var MaxI = gg.length;
				str = arguments[0] + "=";
				for (i = 0; i < MaxI; i++) {
					if (gg[i].indexOf(str) == 0) {
						StrBack = gg[i].replace(str, "");
						break;
					}
				}
			}
			return decodeURI(StrBack);
		},
		getForm: function(form) {
			var result = {},
				tempObj = {};
			$(form).find('*[name]').each(function(i, v) {
				var nameSpace,
					name = $(v).attr('name'),
					val = $.trim($(v).val()),
					tempArr = [];
				if (name == '' || $(v).hasClass('getvalued')) {
					return;
				}

				if ($(v).data('type') == "money") {
					val = val.replace(/\,/gi, '');
				}

				//处理radio add by yhx  2014-06-18
				if ($(v).attr("type") == "radio") {
					var tempradioVal = null;
					$("input[name='" + name + "']:radio").each(function() {
						if ($(this).is(":checked"))
							tempradioVal = $.trim($(this).val());
					});
					if (tempradioVal) {
						val = tempradioVal;
					} else {
						val = "";
					}
				}


				if ($(v).attr("type") == "checkbox") {
					var tempradioVal = [];
					$("input[name='" + name + "']:checkbox").each(function() {
						if ($(this).is(":checked"))
							tempradioVal.push($.trim($(this).val()));
					});
					if (tempradioVal.length) {
						val = tempradioVal.join(',');
					} else {
						val = "";
					}
				}

				if ($(v).attr('listvalue')) {
					if (!result[$(v).attr('listvalue')]) {
						result[$(v).attr('listvalue')] = [];
						$("input[listvalue='" + $(v).attr('listvalue') + "']").each(function() {
							if ($(this).val() != "") {
								var name = $(this).attr('name');
								var obj = {};
								if ($(this).data('type') == "json") {
									obj[name] = JSON.parse($(this).val());
								} else {
									obj[name] = $.trim($(this).val());
								}
								if ($(this).attr("paramquest")) {
									var o = JSON.parse($(this).attr("paramquest"));
									obj = $.extend(obj, o);
								}
								result[$(v).attr('listvalue')].push(obj);
								$(this).addClass('getvalued');
							}
						});
					}
				}

				if ($(v).attr('arrayvalue')) {
					if (!result[$(v).attr('arrayvalue')]) {
						result[$(v).attr('arrayvalue')] = [];
						$("input[arrayvalue='" + $(v).attr('arrayvalue') + "']").each(function() {
							if ($(this).val() != "") {
								var obj = {};
								if ($(this).data('type') == "json") {
									obj = JSON.parse($(this).val());
								} else {
									obj = $.trim($(this).val());
								}
								if ($(this).attr("paramquest")) {
									var o = JSON.parse($(this).attr("paramquest"));
									obj = $.extend(obj, o);
								}
								result[$(v).attr('arrayvalue')].push(obj);
							}
						});
					}
				}
				if (name == '' || $(v).hasClass('getvalued')) {
					return;
				}
				//构建参数
				if (name.match(/\./)) {
					tempArr = name.split('.');
					nameSpace = tempArr[0];
					if (tempArr.length == 3) {
						tempObj[tempArr[1]] = tempObj[tempArr[1]] || {};
						tempObj[tempArr[1]][tempArr[2]] = val;
					} else {
						if ($(v).data('type') == "json") {
							tempObj[tempArr[1]] = JSON.parse(val);
							if ($(v).attr("paramquest")) {
								var o = JSON.parse($(v).attr("paramquest"));
								tempObj[tempArr[1]] = $.extend(tempObj[tempArr[1]], o);
							}
						} else {
							tempObj[tempArr[1]] = val;
						}
					}
					if (!result[nameSpace]) {
						result[nameSpace] = tempObj;
					} else {
						result[nameSpace] = $.extend({}, result[nameSpace], tempObj);
					}
				} else {
					result[name] = val;
				}

			});
			var obj = {};
			for (var o in result) {
				var v = result[o];
				if (typeof v == "object") {
					obj[o] = JSON.stringify(v);
				} else {
					obj[o] = result[o]
				}
			}
			$('.getvalued').removeClass('getvalued');
			return obj;
		},
		setHash: function(obj) {
			var str = '';
			obj = $.extend(this.getHash(), obj)
			var arr = [];
			for (var v in obj) {
				if(obj[v]!=''){
					arr.push(v + '=' + encodeURIComponent(obj[v]));
				}
			}
			str+=arr.join('&');
			location.hash = str;
			return this;
		},
		getHash: function(name) {
			if (typeof name === "string") {
				return this.getQuery(name, "#");
			} else {
				var obj = {};
				var hash = location.hash;
				if(hash.length>0){
					hash = hash.substr(1);
					var hashArr = hash.split('&');
					for (var i = 0, l = hashArr.length; i < l; i++) {
						var a = hashArr[i].split('=');
						if (a.length > 0) {
							obj[a[0]] = decodeURI(a[1]) || '';
						}
					}
				}
				return obj;
			}
		}
	};
	return Query;
});