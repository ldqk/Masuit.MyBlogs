myApp.controller("mergelist", ["$scope", "$http", "NgTableParams", "$timeout", function ($scope, $http, NgTableParams, $timeout) {
	window.hub.stop();
	var self = this;
	$scope.loading();
	$scope.kw = "";
	$scope.orderby = 1;
	$scope.paginationConf = {
		currentPage: $scope.currentPage ? $scope.currentPage : 1,
		itemsPerPage: 10,
		pagesLength: 25,
		perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
		rememberPerPage: 'perPageItems',
		onChange: function() {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	};
	this.GetPageData = function(page, size) {
		$scope.loading();
		$http.get("/merge?page="+page+"&size="+size+"&kw="+$scope.kw).then(function(res) {
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
	self.pass = function(row) {
        swal({
			title: "确认直接合并这篇文章吗？",
			text: row.Title,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/merge/"+row.Id, null, function(data) {
			    window.notie.alert({
				    type: 1,
				    text: data.Message,
				    time: 4
			    });
			    self.stats = [];
			    self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		    });
		}, function() {
		}).catch(swal.noop);
    }

	self.reject = function(row) {
		swal({
			title: "拒绝合并理由：",
            input: 'textarea',
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false,
            inputValidator: function(value) {
            return new Promise(function(resolve, reject) {
              if (value) {
                resolve();
              } else {
                reject('请填写拒绝理由!');
              }
            });
          }
		}).then(function(reason) {
            $scope.request("/merge/reject/"+row.Id, null, function(data) {
			    window.notie.alert({
				    type: 1,
				    text: data.Message,
				    time: 4
			    });
			    self.stats = [];
			    self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		    });
		}, function() {
		}).catch(swal.noop);
    }

	self.block = function(row) {
		swal({
			title: "确认标记为恶意修改吗？",
			text: row.Title,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
            $scope.request("/merge/block/"+row.Id, null, function(data) {
			    window.notie.alert({
				    type: 1,
				    text: data.Message,
				    time: 4
			    });
			    self.stats = [];
			    self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		    });
		}, function() {
		}).catch(swal.noop);
    }

	var _timeout;
	$scope.search = function (kw) {
		if (_timeout) {
			$timeout.cancel(_timeout);
		}
		_timeout = $timeout(function () {
			$scope.kw = kw;
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage, $scope.kw);
			_timeout = null;
		}, 500);
	}
	$scope.loadingDone();
}]);
myApp.controller("mergecompare", ["$scope", "$http", "$timeout","$location", function ($scope, $http, $timeout,$location) {
	window.hub.stop();
	clearInterval(window.interval);
	$scope.loading();
	$scope.id = $location.search()['id'];
	$scope.get("/merge/compare/"+$scope.id, function(res) {
		var data = res.Data;
        $scope.old=data.old;
        $scope.newer=data.newer;
	    $scope.loadingDone();
	});
    
	$scope.pass = function() {
        swal({
			title: "确认直接合并这篇文章吗？",
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/merge/"+$scope.id, null, function(data) {
			    window.notie.alert({
				    type: 1,
				    text: data.Message,
				    time: 4
			    });
                window.location.href = "#/merge/list";
		    });
		}, function() {
		}).catch(swal.noop);
    }

	$scope.reject = function() {
        swal({
			title: "拒绝合并理由：",
            input: 'textarea',
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false,
            inputValidator: function(value) {
            return new Promise(function(resolve, reject) {
              if (value) {
                resolve();
              } else {
                reject('请填写拒绝理由!');
              }
            });
          }
		}).then(function(reason) {
            $scope.request("/merge/reject/" + $scope.id, {reason:reason}, function(data) {
			    window.notie.alert({
				    type: 1,
				    text: data.Message,
				    time: 4
			    });
                window.location.href = "#/merge/list";
		    });
		}, function() {
		}).catch(swal.noop);
    }

	$scope.block = function() {
        swal({
			title: "确认标记为恶意修改吗？",
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/merge/block/"+$scope.id, null, function(data) {
			    window.notie.alert({
				    type: 1,
				    text: data.Message,
				    time: 4
			    });
			    self.stats = [];
			    self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		    });
		}, function() {
		}).catch(swal.noop);
    }

}]);
myApp.controller("mergeedit", ["$scope", "$http", "$timeout","$location", function ($scope, $http, $timeout,$location) {
	window.hub.stop();
	clearInterval(window.interval);
	$scope.loading();
	$scope.id = $location.search()['id'];
	$scope.get("/merge/"+$scope.id, function(res) {
		$scope.post= res.Data;
	    $scope.loadingDone();
	});
    $scope.merge= function() {
	    $scope.loading();
        $scope.request("/merge",$scope.post, function(res) {
		    window.notie.alert({
				type: 1,
				text: res.Message,
				time: 4
			});
	        $scope.loadingDone();
            window.location.href = "#/merge/list";
	    });
    }
}]);