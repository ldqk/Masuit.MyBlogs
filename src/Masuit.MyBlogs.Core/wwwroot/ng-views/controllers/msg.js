myApp.controller("msg", ["$scope", "$http", "NgTableParams", function ($scope, $http, NgTableParams) {
	var self = this;
	$scope.currentPage = 1;
	$scope.paginationConf = {
		currentPage: $scope.currentPage ||1,
		itemsPerPage: 10,
		pagesLength: 25,
		perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
		rememberPerPage: 'perPageItems',
		onChange: function () {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	};
	this.GetPageData = function(page, size) {
		$http.get(`/msg/GetPendingMsgs?page=${page}&size=${size}`).then(function(res) {
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
	self.del = function(row) {
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
		}).then(function() {
			$scope.request("/msg/delete/" + row.Id, null, function(data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
			});
			_.remove(self.tableParams.settings().dataset, function(item) {
				return row === item;
			});
			self.tableParams.reload().then(function(data) {
				if (data.length === 0 && self.tableParams.total() > 0) {
					self.tableParams.page(self.tableParams.page() - 1);
					self.tableParams.reload();
				}
			});
		}, function() {
		}).catch(swal.noop);
	}
	self.pass = function(row) {
		$scope.request("/msg/pass", row, function(data) {
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
myApp.controller("msgs", ["$scope", "$http", function ($scope, $http) {
	var self = this;
	$scope.currentPage = 1;
	$scope.paginationConf = {
		currentPage: $scope.currentPage || 1,
		itemsPerPage: 10,
		pagesLength: 25,
		perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
		rememberPerPage: 'perPageItems',
		onChange: function () {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	};
	this.GetPageData = function (page, size) {
		$http.get(`/msg/GetInternalMsgs?page=${page}&size=${size}`).then(function (res) {
			$scope.paginationConf.totalItems = res.data.TotalCount;
			$scope.Messages = res.data.Data;
		});
	};
	$scope.MarkRead= function() {
		var id = _.max($scope.Messages, m => m.Id).Id;
		$http.post("/msg/MarkRead/"+id).then(function(res) {
			var data = res.data;
			if (data.Success) {
				self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
			}
		});
	}
	$scope.toggleRead= function(id,checked) {
		if (checked) {
			$http.post("/msg/read/"+id);
		} else {
			$http.post("/msg/unread/"+id);
		}
	}
	$scope.clearMsgs = function() {
		swal({
			title: '确定清除已读消息?',
			text: "即将彻底清除已读消息，不可恢复!",
			showCancelButton: true,
			confirmButtonColor: '#3085d6',
			cancelButtonColor: '#d33',
			confirmButtonText: '确定!',
			cancelButtonText: '取消',
			preConfirm: function() {
				return new Promise(function(resolve) {
					$http.post("/msg/ClearMsgs").then(function(res) {
						var data = res.data;
						resolve(data);
					});
				});
			},
			allowOutsideClick: false
		}).then(function (data) {
			if (data.Success) {
				self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
			}
		}).catch(swal.noop);
	}
}]);