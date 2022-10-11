myApp.controller("comment", ["$scope", "$http", "NgTableParams", function ($scope, $http, NgTableParams) {
	var self = this;
	$scope.paginationConf = {
		currentPage: $scope.currentPage ? $scope.currentPage : 1,
		//totalItems: $scope.total,
		itemsPerPage: 10,
		pagesLength: 25,
		perPageOptions: [10, 15, 20, 30, 50, 100, 200],
		rememberPerPage: 'perPageItems',
		onChange: function () {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	};
	this.GetPageData = function (page, size) {
		$http.post(`/comment/GetPendingComments?page=${page}&size=${size}`).then(function (res) {
			$scope.paginationConf.currentPage = page;
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
			title: "确认删除这条留言吗？",
			text: row.Title,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function () {
			$scope.request("/comment/delete/" + row.Id,null, function (data) {
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
	self.pass = function (row) {
		$scope.request("/comment/pass", row, function (data) {
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
			self.stats = [];
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		});
	}
}]);