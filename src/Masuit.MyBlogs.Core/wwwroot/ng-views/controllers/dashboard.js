myApp.controller("dashboard", ["$scope", "$http", "$timeout", "$location", function ($scope, $http, $timeout, $location) {
	$scope.loading();
	$scope.distinct = true;
	var loadsdata = [];
	var netdata = [];
	var iodata = [];
	var currentCpuLoad = 0;
	var currentMemUsage = 0;
	var currentTemper = 0;
	var currentRead = 0;
	var currentWrite = 0;
	var currentUp = 0;
	var currentDown = 0;
	var speed = 0;
	$scope.cpu = {};
	$scope.memory = {};
	$scope.isPush =$scope.isPush ? $scope.isPush : true;
	$scope.push = "停止";

	/**
	 * 连接WebSocket
	 */
	$scope.connectWebsocket= function() {
		window.hub=new signalR.HubConnectionBuilder().withUrl("/hubs").build();
		window.hub.start().then(() => {
		    window.hub.stream("Counter", 5000).subscribe({
			    next: (item) => {
			        loadsdata.push([item.time,item.cpuLoad,item.memoryUsage,item.temperature]);
					currentCpuLoad = item.cpuLoad;
					currentMemUsage = item.memoryUsage;
					currentTemper = item.temperature;
					$scope.cpu.CpuLoad = currentCpuLoad;
					$scope.memory.MemoryUsage = currentMemUsage;
					$scope.cpu.Temperature = currentTemper;

					iodata.push([item.time,item.diskRead,item.diskWrite]);
					currentRead = item.diskRead;
					currentWrite = item.diskWrite;
					speed = currentWrite;

					netdata.push([item.time,item.upload,item.download]);
					currentUp = item.upload;
					currentDown = item.download;
					$scope.$apply();
			    },
			    complete: () => {
			        console.log("done");
			    },
			    error: (err) => {
			        //console.error(err);
			    }
			});
		});
	};
    ifvisible.idle(function() {
		hub.stop();
	});
	ifvisible.blur(function() {
		hub.stop();
	});
	ifvisible.wakeup(function () {
		if ($scope.isPush && $location.url() ==="/home") {
			$scope.connectWebsocket();
		}
	});
	$http.post("/system/GetBaseInfo", null).then(function(res) {
		var data = res.data;
		$scope.cpu = data.cpuInfo[0];
		$scope.cores = data.cpuInfo.length;
		$scope.memory = data.ramInfo;
		$scope.disk = data.diskInfo;
		$scope.osVersion = data.osVersion;
		$scope.netInfo = data.netInfo;
		$scope.RunningTime = data.runningTime;
		$scope.BootTime = data.bootTime;
	});
	$scope.StopPush= function() {
		if ($scope.isPush) {
			hub.stop();
			$scope.push = "开始";
			$scope.isPush = false;
		} else {
			$scope.connectWebsocket();
			$scope.push = "停止";
			$scope.isPush = true;
		}
	};
    $scope.ClearPerfCounter = function () {
		swal({
			title: "清除性能计数器缓存",
			text: "确定要清除性能计数器缓存吗？",
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$http.post("/system/ClearPerfCounter").then(function (res) {
				window.notie.alert({
					type: 1,
					text: "性能计数器缓存清除完成！",
					time: 4
				});
			}, function() {
				window.notie.alert({
					type:3,
					text:'服务请求失败！',
					time:4
				});
				$scope.loadingDone();
			});
        }, function() {
		}).catch(swal.noop);
	};
    $scope.connectWebsocket();
	$.post("/system/GetHistoryList", null, function(data) {
		Highcharts.stockChart("cpu",{
			subtitle: {
				text: document.ontouchstart === undefined ?
				'鼠标拖动可以进行缩放' : '手势操作进行缩放'
			},
			chart: {
				zoomType: 'x',
				backgroundColor: 'transparent',
				animation: Highcharts.svg,
				events: {
					load: function() {
						var series1 = this.series[0],
							series2 = this.series[1],
							series3 = this.series[2];
						setInterval(function() {
							let ld = loadsdata.pop();
							if (ld) {
								series1.addPoint([ld[0], ld[1]], true, true);
								series2.addPoint([ld[0], ld[2]], true, true);
								series3.addPoint([ld[0], ld[3]], true, true);
							}
						}, 2000);
					},
					backgroundColor: 'transparent'
				}
			},
			credits: {
				enabled: false
			},
			boost: {
				useGPUTranslations: true
			},
			plotOptions: {
				series: {
					showInNavigator: true,
					marker: {
						enabled: false
					}
				}
			},
			legend: {
				align: 'right',
				verticalAlign: 'top',
				floating: true,
				x: -50,
				y: 0
			},
			title: {
				text: '资源使用率'
			},
			xAxis: {
				type: 'datetime',
				tickPixelInterval: 150
			},
			yAxis: [
				{
					title: {
						text: '使用率（%）'
					},
					min: 0,
					opposite: false
				}, {
					title: {
						text: 'CPU内核温度（℃）'
					},
					min: 30,
					max:90,
					opposite: true
				}
			],
			exporting: {
				enabled: false
			},
			scrollbar: {
				enabled: false
			},
			rangeSelector : {
				buttons: [{
					type: 'minute',
					count: 5,
					text: '5分钟'
				}, {
					type: 'minute',
					count: 15,
					text: '15分钟'
				}, {
					type: 'minute',
					count: 30,
					text: '30分钟'
				}, {
					type: 'hour',
					count: 1,
					text: '1小时'
				}, {
					type: 'hour',
					count: 2,
					text: '2小时'
				}, {
					type: 'hour',
					count: 5,
					text: '5小时'
				}, {
					type: 'hour',
					count: 12,
					text: '12小时'
				}, {
					type: 'day',
					count: 1,
					text: '1天'
				}, {
					type: 'day',
					count: 3,
					text: '3天'
				}, {
					type: 'all',
					text: 'All'
				}],
				inputEnabled: false,
				selected : 1
			},
			tooltip: {
				formatter: function() {
					return '时间点：<b>' + Highcharts.dateFormat("%H:%M:%S", this.points[0].x) + '</b><br/>' +
						'<span style="color:' + Highcharts.getOptions().colors[0] +  '">CPU使用率：<b>' + Highcharts.numberFormat(this.points[0].y, 2) + '%</b></span><br/>' +
						'<span style="color:' + Highcharts.getOptions().colors[1] +'">内存使用率：<b>' + Highcharts.numberFormat(this.points[1].y, 2) + '%</b></span><br/>' +
						'<span style="color:' + Highcharts.getOptions().colors[2] +'">CPU内核温度：<b>' + Highcharts.numberFormat(this.points[2].y, 0) + '℃</b></span><br/>';
				},
				crosshairs: true,
				shared: true
			},
			series: [{
				name: 'CPU使用率',
				type: 'areaspline',
				data: data.cpu,
				tooltip: {
					valueSuffix: ' %'
				}
			}, {
				name: '内存使用率',
				type: 'areaspline',
				data: data.mem,
				tooltip: {
					valueSuffix: ' %'
				}
			}, {
				name: 'CPU温度',
				yAxis: 1,
				data: data.temp,
				type: 'areaspline',
				tooltip: {
					valueSuffix: '℃'
				}
			}]
		});
		Highcharts.stockChart("io",{
			subtitle: {
				text: document.ontouchstart === undefined ?
				'鼠标拖动可以进行缩放' : '手势操作进行缩放'
			},
			chart: {
				zoomType: 'x',
				backgroundColor: 'transparent',
				animation: Highcharts.svg,
				events: {
					load: function() {
						var series1 = this.series[0],
							series2 = this.series[1],
							series3 = this.series[2],
							series4 = this.series[3];
						setInterval(function() {
							let io = iodata.pop();
							let net = netdata.pop();
							if (io) {
								series1.addPoint([io[0], io[1]], true, true);
								series2.addPoint([io[0], io[2]], true, true);
								series3.addPoint([net[0], net[2]], true, true);
								series4.addPoint([net[0], net[2]], true, true);
							}
						}, 2000);
					}
				}
			},
			boost: {
				useGPUTranslations: true
			},
			credits: {
				enabled: false
			},
			plotOptions: {
				series: {
					showInNavigator: true,
					marker: {
						enabled: false
					}
				}
			},
			legend: {
				align: 'right',
				verticalAlign: 'top',
				floating: true,
				x: -50,
				y: 0
			},
			title: {
				text: '网络状态 和 磁盘I/O'
			},
			rangeSelector : {
				buttons: [{
					type: 'minute',
					count: 5,
					text: '5分钟'
				}, {
					type: 'minute',
					count: 15,
					text: '15分钟'
				}, {
					type: 'minute',
					count: 30,
					text: '30分钟'
				}, {
					type: 'hour',
					count: 1,
					text: '1小时'
				}, {
					type: 'hour',
					count: 2,
					text: '2小时'
				}, {
					type: 'hour',
					count: 5,
					text: '5小时'
				}, {
					type: 'hour',
					count: 12,
					text: '12小时'
				}, {
					type: 'day',
					count: 1,
					text: '1天'
				}, {
					type: 'day',
					count: 3,
					text: '3天'
				}, {
					type: 'all',
					text: 'All'
				}],
				inputEnabled: false,
				selected : 1
			},
			xAxis: {
				type: 'datetime',
				tickPixelInterval: 150
			},
            yAxis: [
                {
                    title: {
                        text: '磁盘I/O速率(KBps)'
                    },
                    min: 0,
                    opposite: true
                }, {
                    title: {
                        text: '网络速率(KBps)'
                    },
                    min: 0,
                    opposite: false
                }
            ],
			tooltip: {
				formatter: function() {
					return '时间点：<b>' + Highcharts.dateFormat("%H:%M:%S", this.points[0].x) + '</b><br/>' +
						'<span style="color:' + Highcharts.getOptions().colors[0] + '">磁盘读：<b>' + Highcharts.numberFormat(this.points[0].y, 0) + 'KBps</b></span><br/>' +
						'<span style="color:' + Highcharts.getOptions().colors[2] + '">磁盘写：<b>' + Highcharts.numberFormat(this.points[1].y, 0) + 'KBps</b></span><br/>' +
						'<span style="color:' + Highcharts.getOptions().colors[39] + '">网络上行：<b>' + Highcharts.numberFormat(this.points[2].y, 0) + 'KBps</b></span><br/>' +
						'<span style="color:' + Highcharts.getOptions().colors[35] + '">网络下行：<b>' + Highcharts.numberFormat(this.points[3].y, 0) + 'KBps</b></span><br/>';
				},
				crosshairs: true,
				shared: true
			},
			exporting: {
				enabled: false
			},
			scrollbar: {
				enabled: false
			},
			series: [{
				name: '磁盘读',
				type: 'spline',
				data: data.read,
				yAxis:0,
				tooltip: {
					valueSuffix: 'KBps'
				}
			}, {
				name: '磁盘写',
				type: 'spline',
				data: data.write,
				yAxis:0,
				tooltip: {
					valueSuffix: 'KBps'
				}
			}, {
				name: '网络上行',
				type: 'spline',
				data: data.up,
				yAxis:1,
				tooltip: {
					valueSuffix: 'KBps'
				}
			}, {
				name: '网络下行',
				type: 'spline',
				data: data.down,
				yAxis:1,
				tooltip: {
					valueSuffix: 'KBps'
				}
			}]
		});
		$scope.loadingDone();
	});

	var gaugeOptions = {
		chart: {
			backgroundColor: 'transparent',
			type: 'solidgauge'
		},
		title: null,
		pane: {
			center: ['50%', '85%'],
			size: '140%',
			startAngle: -90,
			endAngle: 90,
			background: {
				backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || '#EEE',
				innerRadius: '60%',
				outerRadius: '100%',
				shape: 'arc'
			}
		},
		tooltip: {
			enabled: false
		},
		yAxis: {
			stops: [
				[0.1, '#55BF3B'], // green
				[0.5, '#DDDF0D'], // yellow
				[0.9, '#DF5353'] // red
			],
			lineWidth: 0,
			minorTickInterval: null,
			tickPixelInterval: 400,
			tickWidth: 0,
			title: {
				y: -70
			},
			labels: {
				y: 16
			}
		},
		plotOptions: {
			solidgauge: {
				dataLabels: {
					y: 5,
					borderWidth: 0,
					useHTML: true
				}
			}
		}
	};
	$('#container-speed').highcharts(Highcharts.merge(gaugeOptions, {
		yAxis: {
			min: 0,
			max: 100,
			title: {
				text: 'CPU使用率'
			}
		},
		boost: {
			useGPUTranslations: true
		},
		credits: {
			enabled: false
		},
		series: [{
			name: 'CPU使用率',
			data: [0],
			dataLabels: {
				format: '<div style="text-align:center"><span style="font-size:25px;color:' +
					((Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black') + '">{y:.2f}%</span><br/>'
			},
			tooltip: {
				valueSuffix: '%'
			}
		}]
	}));
	$('#container-rpm').highcharts(Highcharts.merge(gaugeOptions, {
		yAxis: {
			min: 0,
			max: 100,
			title: {
				text: '内存使用率'
			}
		},
		boost: {
			useGPUTranslations: true
		},
		credits: {
			enabled: false
		},
		series: [{
			name: '内存使用率',
			data: [0],
			dataLabels: {
				format: '<div style="text-align:center"><span style="font-size:25px;color:' +
					((Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black') + '">{y:.1f}%</span><br/>'
			},
			tooltip: {
				valueSuffix: ' %'
			}
		}]
	}));
	$('#container-temp').highcharts(Highcharts.merge(gaugeOptions, {
		yAxis: {
			min: 0,
			max: 100,
			title: {
				text: 'CPU当前温度'
			}
		},
		boost: {
			useGPUTranslations: true
		},
		credits: {
			enabled: false
		},
		series: [{
			name: 'CPU当前温度',
			data: [1],
			dataLabels: {
				format: '<div style="text-align:center"><span style="font-size:25px;color:' +
					((Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black') + '">{y}℃</span><br/>'
			},
			tooltip: {
				valueSuffix: ' ℃'
			}
		}]
	}));
	$('#container-io').highcharts(Highcharts.merge(gaugeOptions, {
		yAxis: {
			min: 0,
			max: 10000,
			title: {
				text: '磁盘I/O'
			}
		},
		boost: {
			useGPUTranslations: true
		},
		credits: {
			enabled: false
		},
		series: [{
			name: '磁盘I/O',
			data: [1],
			dataLabels: {
				format: '<div style="text-align:center"><span style="font-size:25px;color:' + ((Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black') + '">{y:.0f}KBps</span><br/>'
			},
			tooltip: {
				valueSuffix: ' KBps'
			}
		}]
	}));
	setInterval(function() {
		var chart = $('#container-speed').highcharts(), point;
		if (chart) {
			point = chart.series[0].points[0];
			point.update(currentCpuLoad);
		}
		chart = $('#container-rpm').highcharts();
		if (chart) {
			point = chart.series[0].points[0];
			point.update(currentMemUsage);
		}
		chart = $('#container-temp').highcharts();
		if (chart) {
			point = chart.series[0].points[0];
			point.update(currentTemper);
		}
		chart = $('#container-io').highcharts();
		if (chart) {
			point = chart.series[0].points[0];
			point.update(speed);
		}
	}, 1000);
}]);