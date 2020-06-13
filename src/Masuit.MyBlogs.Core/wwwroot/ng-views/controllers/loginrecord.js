myApp.controller("loginrecord", ["$scope", "$http", "NgTableParams", function($scope, $http, NgTableParams) {
	window.hub.stop();
	var self = this;
	$http.post("/login/getrecent/"+$scope.user.Id).then(function(res) {
		self.tableParams = new NgTableParams({
			count: 15
		}, {
			filterDelay: 0,
			dataset: res.data.Data
		});
	});
}]);