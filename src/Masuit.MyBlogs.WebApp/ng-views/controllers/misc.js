myApp.controller("miscAdd", ["$scope", "$http", "$location", "$timeout", function ($scope, $http, $location, $timeout) {
	window.hub.disconnect();
	$scope.misc = {};
	$scope.loading();
	$scope.misc.Id = $location.search()['id'];
	if ($scope.misc.Id) {
		$scope.request("/misc/get", { id: $scope.misc.Id }, function (res) {
			$scope.misc = res.Data;
		});
	}
	//异步提交表单开始
	$scope.submit = function (misc) {
		$scope.loading();
		var url = "/misc/write";
		if (misc.Id) {
			url = "/misc/edit";
		} else {
			misc= {
				Title:misc.Title,
				Content:misc.Content
			};
		}
		$http.post(url, misc).then(function (res) {
			var data = res.data;
			$scope.loadingDone();
			if (data.Success) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				$scope.misc.Content = "";
				$scope.misc.Title = "";
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
	$scope.loadingDone();
}]);
myApp.controller("miscList", ["$scope", "$http", "NgTableParams", function ($scope, $http, NgTableParams) {
	window.hub.disconnect();
	var self = this;
	$scope.loading();
	$scope.paginationConf = {
		currentPage: $scope.currentPage ? $scope.currentPage : 1,
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
		$scope.loading();
		$http.post("/misc/getpagedata", {
			page,
			size
		}).then(function (res) {
			$scope.paginationConf.currentPage = page;
			$scope.paginationConf.totalItems = res.data.TotalCount;
			$("div[ng-table-pagination]").remove();
			self.tableParams = new NgTableParams({
				count: 50000
			}, {
					filterDelay: 0,
					dataset: res.data.Data
				});
			$scope.loadingDone();
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
			$scope.request("/misc/delete", {
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
	$scope.loadingDone();

}]);