angular.module('myApp', ["tm.pagination"]).filter('htmlString', ['$sce', function($sce) {
	return function(text) {
		return $sce.trustAsHtml(text);
	};
}]).controller("home", ["$scope", "$http", "$timeout", function ($scope, $http, $timeout) {
	$scope.Issue = { Level: 0 };
	var user = JSON.parse(localStorage.getItem("user"));
	if (user) {
		$scope.Issue.Name=(user.NickName);
		$scope.Issue.Email = (user.Email);
	}
	if (window.UE) {
		if ($scope.ue) {
		} else {
			$scope.ue = UE.getEditor('editor', { toolbars: [[
					'removeformat', //清除格式
					'fontsize', //字号
					'emotion', //表情
					'link', //超链接
					'paragraph', //段落格式
					'simpleupload', //单图上传
					'justifyleft', //居左对齐
					'justifyright', //居右对齐
					'justifycenter', //居中对齐
					'forecolor', //字体颜色
					'lineheight', //行间距
				]], zIndex: 1000000 , initialFrameWidth: null });
		}
	}
	$http.post("/passport/getuserinfo", null).then(function(res) {
		if (res.data.Success) {
			$scope.user = res.data.Data;
		}
	});
	$http.post("/bug/getbuglevels", null).then(function(res) {
		if (res.data.Success) {
			$scope.BugLevels = res.data.Data;
		}
	});
	$scope.paginationConf = {
		currentPage: 1,
		itemsPerPage: 10,
		pagesLength: 15,
		perPageOptions: [10, 15, 20, 30, 50, 100],
		rememberPerPage: 'perPageItems',
		onChange: function() {
			$scope.GetPageData();
		}
	};
	$scope.GetPageData = function() {
		$http.post("/bug/pagedata", {
			page: $scope.paginationConf.currentPage,
			size: $scope.paginationConf.itemsPerPage,
			kw: $scope.kw
		}).then(function(res) {
			$scope.paginationConf.totalItems = res.data.TotalCount;
			$("div[ng-table-pagination]").remove();
			$scope.Issues = res.data.Data;
			window.loadingDone();
		});
	}
	$scope.create = function () {
		layer.open({
			type: 1,
			zIndex: 20,
			title: '创建一个问题',
			area: (window.screen.width > 540 ? 540 : window.screen.width) + 'px', // '340px'], //宽高
			content: $("#bug-form"),
			end: function () {
				$("#bug-form").css("display", "none");
			}
		});
		$(".layui-layer").insertBefore($(".layui-layer-shade"));
	}
	$scope.cancel = function() {
		layer.closeAll();
		setTimeout(function() {
			$("#bug-form").css("display", "none");
		}, 500);
	}
	$scope.submit = function(issue) {
		window.loading();
		issue.Description = $scope.ue.getContent();
		if (issue.Name && issue.Name.trim().length > 17) {
			window.notie.alert({
				type: 4,
				text: "名字不能为空或者超出16个字符！",
				time: 4
			});
			loadingDone();
			return;
		}
		if (issue.Email&&!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(
			issue.Email.trim())) {
			window.notie.alert({
				type: 3,
				text: "请输入正确的邮箱格式！",
				time: 4
			});
			loadingDone();
			return;
		}
		if (issue.Title&&!issue.Title.trim()) {
			window.notie.alert({
				type: 3,
				text: "主题不能为空！",
				time: 4
			});
			loadingDone();
			return;
		}
		if (issue.Link && !issue.Link.trim()) {
			window.notie.alert({
				type: 3,
				text: "链接不能为空！",
				time: 4
			});
			loadingDone();
			return;
		}
		if (issue.Description&&!issue.Description.trim()) {
			window.notie.alert({
				type: 3,
				text: "问题描述不能为空！",
				time: 4
			});
			loadingDone();
			return;
		}
		issue["__RequestVerificationToken"] = $('[name="__RequestVerificationToken"]').val();
		$http.post("/bug/submit", issue, {
			'Content-Type': 'application/x-www-form-urlencoded'
		}).then(function(res) {
			var data = res.data;
			if (data.Success) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				$scope.Issue = {Level:0};
				$scope.GetPageData();
				$scope.cancel();
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
			}
			loadingDone();
		}, function() {
			window.notie.alert({
				type: 3,
				text: '服务请求失败！',
				time: 4
			});
			loadingDone();
		});
	}
	var _timeout;
	$scope.search = function(kw) {
		if (_timeout) {
			$timeout.cancel(_timeout);
		}
		_timeout = $timeout(function() {
			$scope.kw = kw;
			$scope.GetPageData();
			_timeout = null;
		}, 500);
	}
	$scope.handle = function(id) {
		layer.prompt({
			title: '输入反馈留言',
			formType: 2
		}, function(text, index) {
			layer.close(index);
			window.loading();
			$http.post("/bug/handle", {
				id,
				text
			}).then(function(res) {
				var data = res.data;
				if (data.Success) {
					layer.msg(data.Message);
					$scope.GetPageData();
					$scope.cancel();
				} else {
					$scope.loadingDone();
				}
			}, function() {
				window.notie.alert({
					type: 3,
					text: '服务请求失败！',
					time: 4
				});
				$scope.loadingDone();
			});
		});
	}
	$scope.delete = function(id) {
		layer.confirm('确认删除这个问题？', {
			btn: ['确定', '取消']
		}, function() {
			$http.post("/bug/delete", {
				id
			}).then(function(res) {
				var data = res.data;
				if (data.Success) {
					layer.msg(data.Message);
					$scope.GetPageData();
					$scope.cancel();
				} else {
					$scope.loadingDone();
				}
			}, function() {
				window.notie.alert({
					type: 3,
					text: '服务请求失败！',
					time: 4
				});
				$scope.loadingDone();
			});
		}, function() {
		});
	}
}]);