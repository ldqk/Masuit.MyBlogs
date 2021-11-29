myApp.controller("values", ["$scope", "$http", "NgTableParams", function ($scope, $http, NgTableParams) {
	UEDITOR_CONFIG.autoHeightEnabled=false;
	UEDITOR_CONFIG.initialFrameHeight=320;
	var self = this;
	self.data = {};
	this.load = function() {
		$http.get("/values/list").then(function(res) {
			self.tableParams = new NgTableParams({}, {
				filterDelay: 0,
				dataset:res.data.Data
			});
		});
	}
	self.load();
	$scope.closeAll = function() {
		layer.closeAll();
		setTimeout(function() {
			$("#modal").css("display", "none");
		}, 500);
	}
	$scope.submit = function (values) {
		$scope.request("/values", values, function (data) {
			swal(data.Message, null, 'info');
			$scope.values = {};
			$scope.closeAll();
			self.load();
		});
	}
	self.edit = function (row) {
		layer.open({
			type: 1,
			zIndex: 8,
			title: '修改变量',
			area: (window.screen.width > 800 ? 800 : window.screen.width) + 'px',
			content: $("#modal"),
			success: function(layero, index) {
				$scope.values = row;
			},
			end: function() {
				$("#modal").css("display", "none");
			}
		});
	}
	self.add = function() {
		layer.open({
			type: 1,
			zIndex: 8,
			title: '添加变量',
			area: (window.screen.width > 800 ? 800 : window.screen.width) + 'px',
			content: $("#modal"),
			success: function(layero, index) {
				$scope.values = {};
			},
			end: function() {
				$("#modal").css("display", "none");
			}
		});
	}
}]);