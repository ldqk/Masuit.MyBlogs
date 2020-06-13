myApp.controller("noticeAdd", ["$scope", "$http", "$location", "$timeout", function ($scope, $http, $location, $timeout) {
	window.hub.stop();
	$scope.notice = {};
	$scope.notice.Id = $location.search()['id']||0;
	if ($scope.notice.Id) {
		$scope.request("/notice/get", { id: $scope.notice.Id }, function (res) {
			$scope.notice = res.Data;
		});
	}
	//异步提交表单开始
	$scope.submit = function (notice) {
		var url = "/notice/write";
		if (notice.Id) {
			url = "/notice/edit";
		}
		$http.post(url, notice).then(function (res) {
			var data = res.data;
			if (data.Success) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				$scope.notice.Content = "";
				$scope.notice.Title = "";
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
			}
		});
	}
	//异步提交表单结束
}]);
myApp.controller("noticeList", ["$scope", "$http", "NgTableParams", function ($scope, $http, NgTableParams) {
	window.hub.stop();
	var self = this;
	$scope.paginationConf = {
		currentPage: 1,
		//totalItems: $scope.total,
		itemsPerPage: 10,
		pagesLength: 25,
		perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
		rememberPerPage: 'perPageItems',
		onChange: function () {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	};
	this.GetPageData = function (page, size) {
		$http.post("/notice/getpagedata", {
			page,
			size
		}).then(function (res) {
			$scope.paginationConf.totalItems = res.data.TotalCount;
			$("div[ng-table-pagination]").remove();
			self.tableParams = new NgTableParams({
				count: 50000
			}, {
					filterDelay: 0,
					dataset: res.data.Data
				});
		});
	};
	self.del = function (row) {
		swal({
			title: "确认删除这条公告吗？",
			text: row.Title,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function () {
			$scope.request("/notice/delete", {
				id: row.Id
			}, function (data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
			});
			_.remove(self.tableParams.settings().dataset, function (item) {
				return row === item;
			});
			self.tableParams.reload().then(function (data) {
				if (data.length === 0 && self.tableParams.total() > 0) {
					self.tableParams.page(self.tableParams.page() - 1);
					self.tableParams.reload();
				}
			});
		}, function () {
		}).catch(swal.noop);
	}
}]);