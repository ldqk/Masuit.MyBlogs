myApp.controller("searchAnalysis", ["$scope", "$http", "NgTableParams", "$timeout", function($scope, $http, NgTableParams, $timeout) {
	var self = this;
	$scope.query = "";
	$scope.currentPage = 1;
	var _timeout;
	$http.get("/search/HotKey").then(function(res) {
		if(res.data.Success) {
			$scope.agg = res.data.Data;
		} else {
			window.notie.alert({
				type:3,
				text:res.data.Message,
				time:4
			});
		}
	});

	$scope.paginationConf = {
		currentPage:1,
		itemsPerPage:10,
		pagesLength:25,
		perPageOptions:[10, 15, 20, 30, 50, 100, 200],
		rememberPerPage:'perPageItems',
		onChange:function() {
			if(_timeout) {
				$timeout.cancel(_timeout);
			}
			_timeout = $timeout(function() {
				self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
				_timeout = null;
			}, 100);
		}
	};
	$scope.search = function() {
		if(_timeout) {
			$timeout.cancel(_timeout);
		}
		_timeout = $timeout(function() {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
			_timeout = null;
		}, 1000);
	}
	this.GetPageData = function(page, size) {
		$http.post(`/search/SearchList?page=${page}&size=${size}&search=${$scope.query}`).then(function(res) {
			if(res.data.TotalCount > 0) {
				$scope.paginationConf.currentPage = page;
				$scope.paginationConf.totalItems = res.data.TotalCount;
				$("#interview").next("div[ng-table-pagination]").remove();
				self.tableParams = new NgTableParams({
					count:50000
				}, {
					filterDelay:0,
					dataset:res.data.Data
				});
			} else {
				window.notie.alert({
					type:3,
					text:res.data.Message,
					time:4
				});
			}
		});
	};
	self.del = function(row) {
		swal({
			title:"确认删除这条记录吗？",
			text:row.Title,
			showCancelButton:true,
			confirmButtonColor:"#DD6B55",
			confirmButtonText:"确定",
			cancelButtonText:"取消",
			showLoaderOnConfirm:true,
			animation:true,
			allowOutsideClick:false
		}).then(function() {
			$scope.request("/search/delete/" + row.Id, null, function(data) {
				window.notie.alert({
					type:1,
					text:data.Message,
					time:4
				});
			});
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}, function() {
		}).catch(swal.noop);
	}
}]);