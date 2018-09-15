myApp.controller("system", ["$scope", "$http", function($scope, $http) {
	window.hub.disconnect();
	$scope.request("/system/getsettings", null, function(data) {
		var settings = {};
		Enumerable.From(data.Data).Select(e => {
			return {
				name: e.Name,
				value: e.Value
			}
		}).Distinct().ToArray().map(function(item, index, array) {
			settings[item.name] = item.value;
		});
		$scope.Settings = settings;
	});
	$scope.setImage = function(property) {
		swal({
			title: '请选择一张图片',
			input: 'file',
			showCloseButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false,
			inputAttributes: {
				accept: 'image/*'
			},
			preConfirm: function(value) {
				return new Promise(function(resolve) {
					if (value) {
						var reader = new FileReader;
						reader.onload = function (e) {
							$http.post("/upload/DecodeDataUri", {
								data: e.target.result
							}).then(function (res) {
								resolve(res.data.Data);
							}, function (error, status, result) {

							});
						};
						reader.readAsDataURL(value);
					} else {
						reject('请选择图片');
					}
				});
			},
			inputValidator: function(value) {
				return new Promise(function(resolve, reject) {
					if (value) {
						resolve();
					} else {
						reject('请选择图片');
					}
				});
			}
		}).then(function(data) {
			$scope.$apply(function() {
				$scope.Settings[property] = data;
			});
		}).catch(swal.noop);
	}
	$scope.save = function() {
		swal({
			title: '确认保存吗？',
			type: 'warning',
			showCloseButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false,
			preConfirm: function() {
				return new Promise(function(resolve, reject) {
					var result = [];
					for (var key in $scope.Settings) {
						if ($scope.Settings.hasOwnProperty(key)) {
							result.push({
								Name: key,
								Value: $scope.Settings[key]
							});
						}
					}
					$http.post("/system/save", {
						sets: JSON.stringify(result)
					}, {
						'Content-Type': 'application/x-www-form-urlencoded'
					}).then(function(res) {
						resolve(res.data);
					}, function() {
						reject("请求服务器失败！");
					});
				});
			}
		}).then(function(data) {
			swal({
				type: data.Success ? "success" : "error",
				text: data.Message
			});
		}).catch(swal.noop);
	}
	$scope.mailtest= function() {
		$http.post("/system/mailtest", {
			smtp: $scope.Settings.SMTP,
			user: $scope.Settings.EmailFrom,
			pwd: $scope.Settings.EmailPwd,
			port: $scope.Settings.SmtpPort,
			to: $scope.Settings.ReceiveEmail
		}).then(function(res) {
			if (res.data.Success) {
				swal(
					res.data.Message,
					'',
					'success'
				);
			} else {
				swal({
					input: 'textarea',
					showCloseButton: true,
					width: 800,
					confirmButtonColor: "#DD6B55",
					confirmButtonText: "确定",
					inputValue: res.data.Message,
					inputClass: "height700"
				});
			}
		}, function() {
			swal(
				'服务请求失败',
				'',
				'error'
			);
		}).catch(swal.noop);
	}
	$scope.pathtest = function() {
		$http.post("/system/pathtest", {
			path: $scope.Settings.PathRoot
		}).then(function(res) {
			if (res.data.Success) {
				swal(
					res.data.Message,
					'',
					'success'
				);
			} else {
				swal({
					input: 'textarea',
					showCloseButton: true,
					width: 800,
					confirmButtonColor: "#DD6B55",
					confirmButtonText: "确定",
					inputValue: res.data.Message,
					inputClass: "height700"
				});
			}
		}, function() {
			swal(
				'服务请求失败',
				'',
				'error'
			);
		}).catch(swal.noop);
	}
	$scope.DisabledEmailBroadcast= function() {
		if($scope.Settings.DisabledEmailBroadcast=="true") {
			$scope.Settings.DisabledEmailBroadcast="false";
		} else {
			$scope.Settings.DisabledEmailBroadcast="true";
		}
	}
}]);
myApp.controller("log", ["$scope", "$http", function ($scope, $http) {
	window.hub.disconnect();
	$scope.getfiles= function() {
		$scope.request("/dashboard/GetLogfiles", null, function(data) {
			$scope.files = data.Data;
		});
	}
	$scope.getfiles();
	$scope.view= function(file) {
		$scope.request("/dashboard/catlog", { filename: file }, function (data) {
			swal({
				input: 'textarea',
				showCloseButton: true,
				width: 1000,
				confirmButtonColor: "#DD6B55",
				confirmButtonText: "确定",
				inputValue: data.Data,
				inputClass:"height700"
			});
		});
	}
	$scope.delete= function(file) {
		swal({
			title: '确定删除吗？',
			type: 'warning',
			showCancelButton: true,
			showCloseButton: true,
			confirmButtonColor: '#3085d6',
			cancelButtonColor: '#d33',
			confirmButtonText: '确定',
			cancelButtonText: '取消',
			preConfirm: function() {
				return new Promise(function (resolve) {
					$scope.request("/dashboard/deletefile", { filename: file }, function (res) {
						$scope.getfiles();
						resolve(res.Message);
					});
				});
			}
		}).then(function (msg) {
			swal(
				msg,
				'',
				'success'
			);
		}).catch(swal.noop);
	}
}]);
myApp.controller("email", ["$scope", "$http", function ($scope, $http) {
	window.hub.disconnect();
	$scope.getfiles = function () {
		$scope.request("/file/Getfiles", {path:"/template"}, function (data) {
			$scope.files = data.Data;
		});
	}
	$scope.getfiles();
	$scope.view = function (file) {
		$scope.request("/file/read", { filename: file }, function (data) {
			swal({
				input: 'textarea',
				showCloseButton: true,
				width: 1000,
				showCancelButton: true,
				confirmButtonColor: '#3085d6',
				cancelButtonColor: '#d33',
				confirmButtonText: '确定',
				cancelButtonText: '取消',
				inputValue: data.Data,
				inputClass: "height700",
				preConfirm: function (value) {
					return new Promise(function (resolve, reject) {
						if (value) {
							$scope.request("/file/save", { filename: file, content: value }, function (res) {
								if (res.Success) {
								resolve(res.Message);
								} else {
									reject(res.Message);
								}
							});
						} else {
							reject('请输入内容');
						}
					});
				}
			}).then(function (msg) {
				swal(
					msg,
					'',
					'success'
				);
			}).catch(swal.noop);
		});
	}
}]);
myApp.controller("file", ["$scope", "$http", function ($scope, $http) {
	window.hub.disconnect();
}]);
myApp.controller("task", ["$scope", "$http", function ($scope, $http) {
	window.hub.disconnect();
}]);