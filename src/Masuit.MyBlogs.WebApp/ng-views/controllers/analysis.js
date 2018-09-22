myApp.controller("interview", ["$scope", "$http", "NgTableParams", "$timeout",
	function($scope, $http, NgTableParams, $timeout) {
		window.hub.disconnect();
		var self = this;
		$scope.loading();
		$scope.distinct = true;
		$scope.query = "";
		$scope.currentPage = 1;
		var _timeout;

		$scope.start = $.nowDate({
			DD:0
		}).substring(0, 10);
		$scope.end = $.nowDate({
			DD:1
		});
		$("#start").val($scope.start);
		$("#end").val($scope.end);
		var start = {
			format:'YYYY-MM-DD hh:mm:ss',
			minDate:'2014-06-16 23:59:59', //设定最小日期为当前日期
			isinitVal:true,
			maxDate:$.nowDate({
				DD:0
			}), //最大日期
			choosefun:function(elem, datas) {
				end.minDate = datas; //开始日选好后，重置结束日的最小日期
				end.trigger = false;
				$("#end").jeDate(end);
				$scope.start = datas;
				$("#start").val(datas);
			},
			okfun:function(elem, datas) {
				end.minDate = datas; //开始日选好后，重置结束日的最小日期
				end.trigger = false;
				$("#end").jeDate(end);
				$scope.start = datas;
				$("#start").val(datas);
			}
		};
		var end = {
			format:'YYYY-MM-DD hh:mm:ss',
			minDate:$.nowDate({
				DD:0
			}), //设定最小日期为当前日期
			isinitVal:true,
			maxDate:$.nowDate({
				DD:1
			}), //最大日期
			choosefun:function(elem, datas) {
				start.maxDate = datas; //将结束日的初始值设定为开始日的最大日期
				$scope.end = datas;
				$("#end").val(datas);
			},
			okfun:function(elem, datas) {
				start.maxDate = datas; //将结束日的初始值设定为开始日的最大日期
				$scope.end = datas;
				$("#end").val(datas);
				self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
			}
		}
		$('#start').jeDate(start);
		$('#end').jeDate(end);

		$('.field').dropdown({
			allowAdditions:false,
			onChange:function(value) {
				var state = ["OperatingSystem", "UserAgent", "BrowserType", "ISP", "HttpMethod"];
				state.map(function(item, index, array) {
					$scope[item] = false;
				});
				value.split(",").map(function(item, index, array) {
					$scope[item] = true;
				});
				self.tableParams.reload();
			}
		});
		$scope.paginationConf = {
			currentPage:1,
			itemsPerPage:10,
			pagesLength:25,
			perPageOptions:[1, 5, 10, 15, 20, 30, 40, 50],
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
			$scope.loading();
			$http.post("/interview/getpage", {
				start:$scope.start,
				end:$scope.end,
				page,
				size,
				distinct:$scope.distinct,
				search:$scope.query
			}).then(function(res) {
				if(res.data.Data) {
					$scope.paginationConf.currentPage = page;
					$scope.paginationConf.totalItems = res.data.TotalCount;
					//$("div[ng-table-pagination]").remove();
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
				$scope.loadingDone();
			});
		};
		//self.GetPageData(1, 10);
		$scope.loadingDone();
		$scope.doDistinct = function() {
			$scope.distinct = !$scope.distinct;
			if(_timeout) {
				$timeout.cancel(_timeout);
			}
			_timeout = $timeout(function() {
				self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
				$scope.analysis();
				_timeout = null;
			}, 500);
		}
		$scope.analysis = function() {
			$http.post("/interview/analysis", {
				uniq:$scope.distinct
			}).then(function(res) {
				var data = res.data.Data;
				if(!res.data.Success) {
					window.notie.alert({
						type:3,
						text:res.data.Message,
						time:4
					});
					return;
				}
				$scope.interview = data;
				window.echarts.init(document.getElementById('map')).setOption({
					tooltip:{
						trigger:'item'
					},
					visualMap:{
						min:0,
						max:Enumerable.From(data.client).Where(e => e.name != "XX").Max(e => e.value),
						left:'left',
						top:'bottom',
						text:['高', '低'], // 文本，默认为数值文本
						calculable:true
					},
					series:[
						{
							name:'访问量',
							type:'map',
							mapType:'china',
							roam:false,
							label:{
								normal:{
									show:true
								},
								emphasis:{
									show:true
								}
							},
							data:data.client
						}
					]
				});
				var china = ["北京", "天津", "上海", "重庆", "河北", "山西", "辽宁", "吉林", "黑龙江", "江苏", "浙江", "安徽", "福建", "江西", "山东",
					"河南", "湖北", "湖南", "广东", "海南", "四川", "贵州", "云南", "陕西", "甘肃", "青海", "台湾", "内蒙古", "广西", "西藏", "宁夏",
					"新疆", "香港", "澳门"];
				var sum1 = 0, sum2 = 0;
				var dril1 = {
					"name":'中国',
					"id":'中国',
					"data":[]
				};
				var dril2 = {
					"name":'海外',
					"id":'海外',
					"data":[]
				};
				for(var i = 0; i < data.client.length; i++) {
					if(china.indexOf(data.client[i].name) >= 0) {
						sum1 += data.client[i].value;
						dril1.data.push([data.client[i].name, data.client[i].value]);
					} else {
						sum2 += data.client[i].value;
						dril2.data.push([data.client[i].name, data.client[i].value]);
					}
				}
				$('#client').highcharts({
					chart:{
						type:'pie',
						plotBackgroundColor:null,
						plotBorderWidth:null,
						backgroundColor:'transparent',
						plotShadow:false
					},
					title:{
						text:'访问地区统计'
					},
					boost:{
						useGPUTranslations:true
					},
					credits:{
						enabled:false
					},
					plotOptions:{
						series:{
							dataLabels:{
								enabled:true,
								format:'<b>{point.name}</b>: {point.percentage:.2f} % - {point.y}人/次'
							}
						}
					},
					tooltip:{
						headerFormat:'<span>{series.name}</span><br>',
						pointFormat:
							'<span style="color:{point.color}">{point.name}</span>: <b>{point.percentage:.2f}%</b> - {point.y}</b>人/次<br/>'
					},
					series:[{
						name:'访问地区统计',
						colorByPoint:true,
						data:[{
							name:'中国',
							y:sum1,
							drilldown:"中国"
						}, {
							name:'海外',
							y:sum2,
							drilldown:"海外"
						}]
					}],
					drilldown:{
						series:[dril1, dril2]
					}
				});
				var groups = _.groupBy(data.browser, e => e[0].split(/\d+/)[0]);
				var series = [], drilldown = [];
				for(var key in groups) {
					var sum = 0;
					var dril = {
						"name":key,
						"id":key,
						"data":[]
					};
					groups[key].map((item, index) => {
						sum += item[1];
						//console.log(item);
						dril.data.push([item[0], item[1]])
					});
					series.push({
						name:key,
						y:sum,
						drilldown:key
					});
					drilldown.push(dril);
				}
				$('#browser').highcharts({
					chart:{
						type:'pie',
						plotBackgroundColor:null,
						plotBorderWidth:null,
						backgroundColor:'transparent',
						plotShadow:false
					},
					title:{
						text:'浏览器统计'
					},
					boost:{
						useGPUTranslations:true
					},
					credits:{
						enabled:false
					},
					plotOptions:{
						series:{
							dataLabels:{
								enabled:true,
								format:'<b>{point.name}</b>: {point.percentage:.2f} % - {point.y}人/次'
							}
						}
					},
					tooltip:{
						headerFormat:'<span>{series.name}</span><br>',
						pointFormat:
							'<span style="color:{point.color}">{point.name}</span>: <b>{point.percentage:.2f}%</b> - {point.y}</b>人/次<br/>'
					},
					series:[{
						name:'浏览器类型',
						colorByPoint:true,
						data:series
					}],
					drilldown:{
						series:drilldown
					}
				});
				$('#alldata').highcharts('StockChart', {
					credits:{
						enabled:false
					},
					boost:{
						useGPUTranslations:true
					},
					rangeSelector:{
						buttons:[{
							type:'day',
							count:7,
							text:'1周'
						}, {
							type:'day',
							count:15,
							text:'半个月'
						}, {
							type:'month',
							count:1,
							text:'1个月'
						}, {
							type:'month',
							count:3,
							text:'3个月'
						}, {
							type:'month',
							count:6,
							text:'6个月'
						}, {
							type:'ytd',
							text:'YTD'
						}, {
							type:'year',
							count:1,
							text:'1年'
						}, {
							type:'all',
							count:1,
							text:"所有"
						}],
						selected:2,
						inputEnabled:false
					},
					tooltip:{
						dateTimeLabelFormats:{
							millisecond:'%H:%M:%S.%L',
							second:'%H:%M:%S',
							minute:'%H:%M',
							hour:'%H:%M',
							day:'%Y-%m-%d',
							week:'%m-%d',
							month:'%Y-%m',
							year:'%Y'
						},
						formatter:function() {
							try {
								return '时间点：<b>' + Highcharts.dateFormat("%Y-%m-%d", this.points[0].x) + '</b><br/>' +
									'<span style="color:' + Highcharts.getOptions().colors[0] + '">直接访问量：<b>' +
									this.points[0].y +
									'人/天</b></span><br/>' +
									'<span style="color:' + Highcharts.getOptions().colors[39] + '">独立访客量：<b>' +
									this.points[1].y +
									'人/天</b></span><br/>' +
									'<span style="color:' + Highcharts.getOptions().colors[35] + '">新增独立访客：<b>' +
									this.points[2].y +
									'人/天</b></span><br/>' +
									'<span style="color:' + Highcharts.getOptions().colors[26] + '">跳出率：<b>' +
									Highcharts.numberFormat(this.points[3].y, 2) +
									'%</b></span><br/>';
							} catch(e) {
								return '时间点：<b>' + Highcharts.dateFormat("%Y-%m-%d", this.points[0].x) + '</b><br/>' +
									'<span style="color:' + Highcharts.getOptions().colors[0] + '">直接访问量：<b>' +
									this.points[0].y +
									'人/天</b></span><br/>' +
									'<span style="color:' + Highcharts.getOptions().colors[39] + '">独立访客量：<b>' +
									this.points[1].y +
									'人/天</b></span><br/>' +
									'<span style="color:' + Highcharts.getOptions().colors[35] + '">新增独立访客：<b>' +
									this.points[2].y +
									'人/天</b></span><br/>';
							}
						},
						crosshairs:true,
						shared:true
					},
					scrollbar:{
						enabled:false
					},
					title:{
						text:'历史访客记录走势图'
					},
					xAxis:{
						type:'datetime',
						dateTimeLabelFormats:{
							millisecond:'%H:%M:%S.%L',
							second:'%H:%M:%S',
							minute:'%H:%M',
							hour:'%H:%M',
							day:'%m-%d',
							week:'%m-%d',
							month:'%Y-%m',
							year:'%Y'
						}
					},
					yAxis:[
						{
							title:{
								text:'访问量'
							},
							min:0,
							opposite:false
						}, {
							title:{
								text:'跳出率（%）'
							},
							min:0,
							max:100,
							opposite:true
						}
					],
					plotOptions:{
						series:{
							showInNavigator:true,
							marker:{
								enabled:false
							}
						}
					},
					series:[{
						name:'直接访问量',
						data:data.pv,
						type:'areaspline',
						fillColor:{
							linearGradient:{
								x1:0,
								y1:0,
								x2:0,
								y2:1
							},
							stops:[
								[0, Highcharts.getOptions().colors[0]],
								[1, Highcharts.Color(Highcharts.getOptions().colors[0]).setOpacity(0).get('rgba')]
							]
						}
					}, {
						name:'独立访客量',
						data:data.uv,
						type:'areaspline',
						fillColor:{
							linearGradient:{
								x1:0,
								y1:0,
								x2:0,
								y2:1
							},
							stops:[
								[0, Highcharts.getOptions().colors[39]],
								[1, Highcharts.Color(Highcharts.getOptions().colors[39]).setOpacity(0).get('rgba')]
							]
						}
					}, {
						name:'独立访客',
						data:data.iv,
						type:'areaspline',
						fillColor:{
							linearGradient:{
								x1:0,
								y1:0,
								x2:0,
								y2:1
							},
							stops:[
								[0, Highcharts.getOptions().colors[35]],
								[1, Highcharts.Color(Highcharts.getOptions().colors[35]).setOpacity(0).get('rgba')]
							]
						}
					}, {
						name:'跳出率',
						data:data.BounceRateAggregate,
						type:'areaspline',
						yAxis:1,
						fillColor:{
							linearGradient:{
								x1:0,
								y1:0,
								x2:0,
								y2:1
							},
							stops:[
								[0, Highcharts.getOptions().colors[26]],
								[1, Highcharts.Color(Highcharts.getOptions().colors[26]).setOpacity(0).get('rgba')]
							]
						}
					}]
				});
			});
		}
		$scope.analysis();
		$scope.details = function(id) {
			layer.open({
				type:1,
				zIndex:20,
				title:'访客浏览路径',
				offset:window.screen.height * 0.02 + "px",
				area:document.body.clientWidth * 0.8 + "px",
				content:$("#modal"),
				success:function(layero, index) {
					$scope.request("/interview/InterviewDetails", {
						id
					}, function(data) {
						if(data.Success) {
							$scope.viewer = data.Data.interview;
							$scope.viewdetails = data.Data.details;
							self.ViewDetails = new NgTableParams({
								count:10
							}, {
								filterDelay:0,
								dataset:data.Data.details
							});
						}
					});
				},
				end:function() {
					$("#modal").css("display", "none");
				}
			});
		}
		$scope.addToBlackList = function(ip) {
			swal({
				title: "确认添加黑名单吗？",
				text: "将"+ip+"添加到黑名单",
				showCancelButton: true,
				confirmButtonColor: "#DD6B55",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
				animation: true,
				allowOutsideClick: false,
				showLoaderOnConfirm: true,
				preConfirm: function () {
					return new Promise(function (resolve, reject) {
						$http.post("/system/AddToBlackList", {ip}, {
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
					swal("添加成功",'','success');
				} else {
					swal("添加失败",'','error');
				}
			}).catch(swal.noop);
		}
	}]);
myApp.controller("searchAnalysis", ["$scope", "$http", "NgTableParams", "$timeout",
	function($scope, $http, NgTableParams, $timeout) {
		window.hub.disconnect();
		var self = this;
		$scope.loading();
		$scope.query = "";
		$scope.currentPage = 1;
		var _timeout;
		$http.post("/search/HotKey").then(function(res) {
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
			perPageOptions:[1, 5, 10, 15, 20, 30, 40, 50],
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
			$scope.loading();
			$http.post("/search/SearchList", {
				page,
				size,
				search:$scope.query
			}).then(function(res) {
				if(res.data.TotalCount > 0) {
					$scope.paginationConf.currentPage = page;
					$scope.paginationConf.totalItems = res.data.TotalCount;
					//$("div[ng-table-pagination]").remove();
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
				$scope.loadingDone();
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
				$scope.request("/search/delete", {
					id:row.Id
				}, function(data) {
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
		$scope.addToBlackList = function(ip) {
			swal({
				title: "确认添加黑名单吗？",
				text: "将"+ip+"添加到黑名单",
				showCancelButton: true,
				confirmButtonColor: "#DD6B55",
				confirmButtonText: "确定",
				cancelButtonText: "取消",
				animation: true,
				allowOutsideClick: false,
				showLoaderOnConfirm: true,
				preConfirm: function () {
					return new Promise(function (resolve, reject) {
						$http.post("/system/AddToBlackList", {ip}, {
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
					swal("添加成功",'','success');
				} else {
					swal("添加失败",'','error');
				}
			}).catch(swal.noop);
		}
	}]);