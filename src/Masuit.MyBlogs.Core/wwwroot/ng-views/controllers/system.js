myApp.controller("system", ["$scope", "$http", function($scope, $http) {
	window.hub.stop();
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
	$scope.uploadImage = function() {
		$scope.loading();
        $("#setImageForm").ajaxSubmit({
			url: "/Upload",
			type: "post",
			success: function(data) {
				$scope.loadingDone();
				document.getElementById("setImageForm").reset();
				$scope.$apply(function () {
			     　$scope.Settings[$scope.property] = data.Data;
			    });
				layer.closeAll();
			}
		});
    };
	$scope.setImage = function(property) {
		layer.open({
			type: 1,
			zIndex: 20,
			title: '请选择一张图片',
			area: '420px', //宽高
			content: $("#setImageForm"),
			cancel: function(index, layero) {
				return true;
			}
		});
		$scope.property=property;
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
				swal(res.data.Message,'','success');
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
			swal('服务请求失败','','error');
		}).catch(swal.noop);
	}
	$scope.pathtest = function() {
		$http.post("/system/pathtest", {
			path: $scope.Settings.PathRoot
		}).then(function(res) {
			if (res.data.Success) {
				swal(res.data.Message,'','success');
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
			swal('服务请求失败','','error');
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
	window.hub.stop();
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
	window.hub.stop();
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
	window.hub.stop();
}]);
myApp.controller("task", ["$scope", "$http", function ($scope, $http) {
	window.hub.stop();
}]);
myApp.controller("firewall", ["$scope", "$http","NgTableParams","$timeout", function ($scope, $http,NgTableParams,$timeout) {
	window.hub.stop();
	var self = this;
	var data = [];
	self.data = {};
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
	this.load = function() {
		$scope.request("/system/InterceptLog", null, function(res) {
			self.tableParams = new NgTableParams({}, {
				filterDelay: 0,
				dataset: res.Data.list
			});
			data = res.Data.list;
			$scope.interceptCount=res.Data.interceptCount;
		});
	}
	self.load();
	this.clear= function() {
		swal({
			title: '确定清空拦截日志吗？',
			type: 'warning',
			showCancelButton: true,
			showCloseButton: true,
			confirmButtonColor: '#3085d6',
			cancelButtonColor: '#d33',
			confirmButtonText: '确定',
			cancelButtonText: '取消',
			preConfirm: function() {
				return new Promise(function (resolve) {
					$scope.request("/system/ClearInterceptLog", null, function (res) {
						resolve(res.Message);
					});
				});
			}
		}).then(function (msg) {
			swal(msg,'','success');
			self.load();
		}).catch(swal.noop);
	}
	$scope.EnableDenyAreaPolicy= function() {
		if($scope.Settings.EnableDenyArea=="true") {
			$scope.Settings.EnableDenyArea="false";
		} else {
			$scope.Settings.EnableDenyArea="true";
		}
	}
	$scope.getIPBlackList= function() {
		$scope.request("/system/IpBlackList",null, function (data) {
			swal({
				title:"编辑全局IP黑名单",
				text:"多个IP之间用英文逗号分隔",
				input: 'textarea',
				showCloseButton: true,
				width: 1000,
				confirmButtonColor: "#DD6B55",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
				inputValue: data.Data,
				inputClass:"height700",
				showLoaderOnConfirm: true,
				preConfirm: function(value) {
					return new Promise(function (resolve) {
						$scope.request("/system/SetIpBlackList", { content: value }, function (res) {
							resolve(res.Message);
						});
					});
				}
			}).then(function (msg) {
				swal("更新成功",'','success');
			}).catch(swal.noop);
		});
	}
	$scope.getIPWhiteList= function() {
		$scope.request("/system/IpWhiteList",null, function (data) {
			swal({
				title:"编辑全局IP白名单",
				text:"多个IP之间用英文逗号分隔",
				input: 'textarea',
				showCloseButton: true,
				width: 1000,
				confirmButtonColor: "#DD6B55",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
				inputValue: data.Data,
				inputClass:"height700",
				showLoaderOnConfirm: true,
				preConfirm: function(value) {
					return new Promise(function (resolve) {
						$scope.request("/system/SetIpWhiteList", { content: value }, function (res) {
							resolve(res.Message);
						});
					});
				}
			}).then(function (msg) {
				swal("更新成功",'','success');
			}).catch(swal.noop);
		});
	}
	$scope.getAreaIPBlackList= function() {
		$scope.request("/system/AreaIPBlackList",null, function (data) {
			layer.open({
				type: 1,
				zIndex: 20,
				title: '查看地区IP黑名单',
				area: (window.screen.width > 360 ? 360 : window.screen.width) + 'px',// '340px'], //宽高
				content: $("#modal"),
				success: function(layero, index) {
					$('.ui.dropdown.region').dropdown({
						onChange: function (value) {
							$scope.AreaIPs = $scope.AreaIPBlackList[value];
							$scope.AreaIPsCopy = $scope.AreaIPBlackList[value];
						},
						message: {
							maxSelections: '最多选择 {maxCount} 项',
							noResults: '无搜索结果！'
						}
					});
					$scope.AreaIPBlackList = data.Data;
					$scope.Areas = Object.keys(data.Data);
					$timeout(function () {
						$('.ui.dropdown.region').dropdown("set selected", [$scope.Areas[0]]);
					}, 100);
				},
				end: function() {
					$("#modal").css("display", "none");
				}
			});
		});
	}
	$scope.getIPRangeBlackList= function() {
		$scope.request("/system/GetIPRangeBlackList",null, function (data) {
			swal({
				title:"编辑IP地址段黑名单",
				text:"每行一条地址段，起始地址和结束地址用空格分隔开，其余信息也用空格分隔开",
				input: 'textarea',
				showCloseButton: true,
				width: 1000,
				confirmButtonColor: "#DD6B55",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
				inputValue: data.Data,
				inputClass:"height700",
				showLoaderOnConfirm: true,
				preConfirm: function(value) {
					return new Promise(function (resolve) {
						$scope.request("/system/SetIPRangeBlackList", { content: value }, function (res) {
							resolve(res.Message);
						});
					});
				}
			}).then(function (msg) {
				swal("更新成功",'','success');
			}).catch(swal.noop);
		});
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
	$scope.addToWhiteList= function(ip) {
		swal({
			title: "确认添加白名单吗？",
			text: "将"+ip+"添加到白名单",
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			animation: true,
			allowOutsideClick: false,
			showLoaderOnConfirm: true,
			preConfirm: function () {
				return new Promise(function (resolve, reject) {
					$http.post("/system/AddToWhiteList", {ip}, {
						'Content-Type': 'application/x-www-form-urlencoded'
					}).then(function(res) {
						resolve(res.data);
					}, function() {
						reject("请求服务器失败！");
					});
				});
			}
		}).then(function (data) {
			if (data.Success) {
				$scope.AreaIPs.remove(ip);
				swal("添加成功",'','success');
			} else {
				swal("添加失败",'','error');
			}
		}).catch(swal.noop);
	}
	$scope.searchIP= function(ip) {
		if (ip) {
			$scope.AreaIPs = _.filter($scope.AreaIPsCopy, i => i.indexOf(ip) > -1);
		} else {
			$scope.AreaIPs =$scope.AreaIPsCopy;
		}
	}
}]);