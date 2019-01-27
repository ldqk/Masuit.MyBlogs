myApp.controller("contact", ["$scope", "$http", "NgTableParams", function ($scope, $http, NgTableParams) {
	window.hub.stop();
	var self = this;
	var source = [];
	$scope.loading();
	$scope.paginationConf = {
		currentPage: $scope.currentPage ? $scope.currentPage : 1,
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
		$http.post("/contact/getpagedata", {
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
			source = angular.copy(res.data.Data);
			$scope.loadingDone();
		});
	};
	self.del = function (row) {
		swal({
			title: "确认删除这条联系方式吗？",
			text: row.Title,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function () {
			$scope.request("/contact/delete", {
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
	self.cancel = function (row, rowForm) {
		var originalRow = resetRow(row, rowForm);
		angular.extend(row, originalRow);
	};
	self.save = function (row, rowForm) {
		swal({
			title: "确认修改" + row.Name + "地址为：",
			text: row.Url,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function () {
			$scope.request("/contact/edit", row, function (data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				var originalRow = resetRow(row, rowForm);
				angular.extend(originalRow, row);
			});
		}, function () {
		}).catch(swal.noop);
	};
	function resetRow(row, rowForm) {
		row.isEditing = false;
		rowForm.$setPristine();
		self.tableTracker.untrack(row);
		return _.findWhere(source, function (r) {
			return r.id === row.id;
		});
	}
	$scope.add= function() {
		swal({
			title: '添加联系方式',
			html:
			'<input id="title" class="swal2-input" autofocus placeholder="联系方式">' +
			'<input id="url" class="swal2-input" placeholder="地址">',
			showCloseButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false,
			preConfirm: function () {
				return new Promise(function (resolve) {
					$scope.request("/contact/add", {
						Title: $('#title').val(),
						Url: $('#url').val()
					}, function (res) {
						resolve(res);
					});
				});
			}
		}).then(function (result) {
			if (result) {
				if (result.Success) {
					swal(result.Message, "", "success");
					self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
				} else {
					swal(result.Message, "", "error");
				}
			}
		}).catch(swal.noop);
	}
	$scope.loadingDone();
}]);