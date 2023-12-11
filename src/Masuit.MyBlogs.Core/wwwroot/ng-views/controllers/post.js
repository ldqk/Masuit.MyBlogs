myApp.controller("postlist", ["$scope", "$http", "NgTableParams", "$timeout", function ($scope, $http, NgTableParams, $timeout) {
    var self = this;
    self.stats = [];
    self.data = {};
    $scope.kw = "";
    $scope.orderby = 1;
    $scope.CategoryId = "";
    $scope.paginationConf = {
        currentPage:  1,
        itemsPerPage: 10,
        pagesLength: 25,
        perPageOptions: [10, 15, 20, 30, 50, 100, 200],
        rememberPerPage: 'perPageItems',
        onChange: function() {
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
        }
    };
    var params = JSON.parse(localStorage.getItem("postlist-params"));
    if (params) {
        $scope.kw = params["kw"];
        $scope.orderby= params["orderby"];
        $scope.CategoryId=params["cid"];
    }

    xmSelect.render({
        el: '#orderby',
        tips: '请选择排序方式',
        model: {
             icon: 'hidden',
             label: { type: 'text' }
        },
        radio: true,
        clickClose: true,
        autoRow: true, //选项过多,自动换行
        data:[
                {name:"发表时间",value:0,selected:$scope.orderby==0},
                {name:"最后修改",value:1,selected:$scope.orderby==1},
                {name:"访问量最多",value:2,selected:$scope.orderby==2},
                {name:"支持数最多",value:4,selected:$scope.orderby==4},
                {name:"每日平均访问量(发布以来)",value:5,selected:$scope.orderby==5},
                {name:"每日平均访问量(最近一年)",value:6,selected:$scope.orderby==6},
            ],
        on: function (data) {
            if (data.arr.length>0) {
                $scope.orderby = data.arr[0].value;
                self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
            }
        }
    });

    let eventSource=new EventSource("/post/Statistic");
    eventSource.onmessage = function (event) {
        $scope.agg = JSON.parse(event.data);
        $scope.$apply();
    };
    $scope.$on('$destroy', function() {
        eventSource.close();
    });

    $http.get("/category/getcategories").then(function (res) {
        var data = res.data;
        if (data.Success) {
            data.Data=[{Name:"全部",Id:"",Children:[]}].concat(data.Data);
            var params = JSON.parse(localStorage.getItem("postlist-params"));
            if (params) {
                $scope.kw = params["kw"];
                $scope.paginationConf.currentPage= params["page"];
                for (var i = 0; i < data.Data.length; i++) {
                    for (var j = 0; j < data.Data[i].Children.length; j++) {
                        data.Data[i].Children[j].selected=data.Data[i].Children[j].Id==params["cid"];
                    }
                    data.Data[i].selected=data.Data[i].Id==params["cid"];
                }
            }

            data.Data.sort((a,b)=> (b.selected||b.Children.some(c=>c.selected||c.Children.some(cc=>cc.selected)))- (a.selected||a.Children.some(c=>c.selected||c.Children.some(cc=>cc.selected))));
            $scope.cat = data.Data;
            xmSelect.render({
                el: '#category',
                tips: '请选择分类',
                prop: {
                    name: 'Name',
                    value: 'Id',
                    children: 'Children',
                },
                model: { label: { type: 'text' } },
                radio: true,
                clickClose: true,
                tree: {
                    show: true,
                    strict: false,
                },
                filterable: true, //搜索功能
                autoRow: true, //选项过多,自动换行
                data:data.Data,
                on: function (data) {
                    if (data.arr.length>0) {
                        $scope.CategoryId = data.arr[0].Id;
                        self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
                    }
                }
            });
        } else {
            window.notie.alert({
                type: 3,
                text: '获取文章分类失败！',
                time: 4
            });
        }
    });
    $scope.request("/seminar/getall", null, function(res) {
        $scope.Seminars = res.Data;
    });

    this.GetPageData = function (page, size) {
        var params = { page, size, kw: $scope.kw, orderby: $scope.orderby, cid: $scope.CategoryId };
        $http.get(`/post/getpagedata?page=${page||1}&size=${size}&kw=${$scope.kw}&orderby=${$scope.orderby}&cid=${$scope.CategoryId}`).then(function(res) {
            $scope.paginationConf.totalItems = res.data.TotalCount;
            $("div[ng-table-pagination]").remove();
            self.tableParams = new NgTableParams({ count: 50000 }, {
                filterDelay: 0,
                dataset: res.data.Data
            });
            self.data = res.data.Data;
            Enumerable.From(res.data.Data).Select(e => e.Status).Distinct().ToArray().map(function(item, index, array) {
                self.stats.push({
                    id: item,
                    title: item
                });
            });
            self.stats = Enumerable.From(self.stats).Distinct().ToArray();
            localStorage.setItem("postlist-params",JSON.stringify(params));
            $timeout(function () {
                self.data.forEach(item => {
                    let categories = angular.copy($scope.cat);
                    categories.sort((a,b)=> (b.Id==item.CategoryId||b.Children.some(c=>c.Id==item.CategoryId||c.Children.some(cc=>cc.Id==item.CategoryId)))- (a.Id==item.CategoryId||a.Children.some(c=>c.Id==item.CategoryId||c.Children.some(cc=>cc.Id==item.CategoryId))));
                    xmSelect.render({
                        el: '#category-' + item.Id,
                        tips: '未选择分类',
                        radio: true,
                        prop: {
                            name: 'Name',
                            value: 'Id',
                            children: 'Children',
                        },
                        tree: {
                            show: true,
                            strict: false,
                            expandedKeys: true,
                        },
                        filterable: true, //搜索功能
                        data: categories,
                        initValue: [item.CategoryId],
                        on: function (data) {
                            for (var i = 0; i < data.arr.length; i++) {
                                $http.post(`/post/${item.Id}/ChangeCategory/${data.arr[i].Id}`).then(function (res) {
                                    if (data.status >= 400) {
                                        layer.msg("操作失败");
                                    }
                                });
                            }
                        }
                    });
                    xmSelect.render({
                        el: '#seminar-' + item.Id,
                        tips: '未选择专题',
                        prop: {
                            name: 'Title',
                            value: 'Id'
                        },
                        filterable: true, //搜索功能
                        autoRow: true, //选项过多,自动换行
                        data: $scope.Seminars,
                        initValue: item.Seminars,
                        on: function (data) {
                            var arr=[];
                            for (var i = 0; i < data.arr.length; i++) {
                                arr.push(data.arr[i].Id);
                            }
                            $http.post(`/post/${item.Id}/ChangeSeminar?sids=${arr.join(",")}`).then(function (res) {
                                if (data.status >= 400) {
                                    layer.msg("操作失败");
                                }
                            });
                        }
                    });
                });
            }, 1);
        });
    }

    self.takedown = function(row) {
        swal({
            title: "确认下架这篇文章吗？",
            text: row.Title,
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            showLoaderOnConfirm: true,
            animation: true,
            allowOutsideClick: false
        }).then(function() {
            $scope.request("/post/takedown/"+row.Id, null, function(data) {
                window.notie.alert({
                    type: 1,
                    text: data.Message,
                    time: 4
                });
            });
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
        }, function() {
        }).catch(swal.noop);
    }
    self.truncate = function(row) {
        swal({
            title: "确认要彻底删除这篇文章吗？",
            text: row.Title,
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            showLoaderOnConfirm: true,
            animation: true,
            allowOutsideClick: false
        }).then(function() {
            $scope.request("/post/truncate/" + row.Id, null, function(data) {
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
        $scope.request("/post/pass", row, function(data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
            self.stats = [];
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
        });
    }
    self.takeup = function(row) {
        $scope.request("/post/Takeup/"+row.Id,null, function(data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
            self.stats = [];
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
        });
    }
    self.fixtop = function(id) {
        $scope.request("/post/Fixtop/"+id,null, function(data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
            self.stats = [];
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
        });
    }
    var _timeout;
    $scope.search = function (kw) {
        if (_timeout) {
            $timeout.cancel(_timeout);
        }
        _timeout = $timeout(function () {
            $scope.kw = kw;
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
            _timeout = null;
        }, 500);
    }
    
    $scope.toggleDisableComment= function(row) {
        $scope.request(`/post/${row.Id}/DisableComment`, null, function(data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
        });
    }

    $scope.toggleDisableCopy= function(row) {
        $scope.request(`/post/${row.Id}/DisableCopy`, null, function(data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
        });
    }

    $scope.rssSwitch= function(id) {
        $scope.request("/post/"+id+"/rss-switch",null, function(data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
        });
    }

    $scope.lockedSwitch= function(id) {
        $scope.request("/post/"+id+"/locked-switch",null, function(data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
        });
    }
    
    $scope.nsfwSwitch= function(id) {
        $scope.request("/post/"+id+"/nsfw",null, function(data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
        });
    }
    
    self.insight= function(row) {
        layer.full(layer.open({
          type: 2,
          title: '文章《'+row.Title+'》洞察分析',
          maxmin: true, //开启最大化最小化按钮
          area: ['893px', '100vh'],
          content: '/'+row.Id+'/insight'
        }));
    }
    
    $scope.diffDateFromNow = function(date){
          var dateOut = new Date(date);
          var timeDiff = Math.abs(new Date().getTime() - dateOut.getTime());
          var diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24)); 
          return diffDays;
    };
    
    showCharts=function() {
        echarts.init(document.getElementById('chart')).dispose();
        var period=document.getElementById("period").value;
        window.fetch(`/post/records-chart?compare=${period>0}&period=${period}`, {
            credentials: 'include',
            method: 'GET',
            mode: 'cors'
        }).then(function (response) {
            return response.json();
        }).then(function (res) {
            var xSeries = [];
            var yCountSeries = [];
            var yUvSeries = [];
            for (let series of res) {
                var x = [];
                var yCount = [];
                var yUV = [];
                for (let item of series) {
                    x.push(new Date(Date.parse(item.Date)).toLocaleDateString());
                    yCount.push(item.Count);
                    yUV.push(item.UV);
                }
                xSeries.push(x);
                yCountSeries.push(yCount);
                yUvSeries.push(yUV);
            }
            var chartDom = document.getElementById('chart');
            var myChart = echarts.init(chartDom);
            const colors = ['#0091ee','#ccc'];
            var option = {
                color: colors,
                tooltip: {
                    trigger: 'none',
                    axisPointer: {
                        type: 'cross'
                    }
                },
                legend: {},
                grid: {
                    top: 70,
                    bottom: 50
                },
                title: {
                    left: 'center',
                    text: '最近访问趋势'
                },
                xAxis: xSeries.map(function (item, index) {
                    return {
                        type: 'category',
                        axisTick: {
                            alignWithLabel: true
                        },
                        axisLine: {
                            onZero: false,
                            lineStyle: {
                                color: colors[index]
                            }
                        },
                        axisPointer: {
                            label: {
                                formatter: function (params) {
                                    return params.value + (params.seriesData.length ? ' 访问量：' + params.seriesData[0].data+"，UV："+ params.seriesData[1].data : '');
                                }
                            }
                        },
                        data: item
                    }
                }),
                yAxis: [
                    {
                        type: 'value'
                    }
                ],
                series: yCountSeries.map(function (item, index) {
                    return {
                        type: 'line',
                        //smooth: true,
                        symbol: 'none',
                        xAxisIndex: index,
                        data: item,
                          lineStyle: {
                            type: index===1?'dashed':""
                          },
                        markPoint: {
                            data: [
                                { type: 'max', name: '最大值' },
                                { type: 'min', name: '最小值' }
                            ]
                        },
                        markLine: {
                            data: [
                                { type: 'average', name: '平均值' }
                            ]
                        }
                    }
                }).concat(yUvSeries.map(function (item, index) {
                    return {
                        type: 'line',
                        //smooth: true,
                        symbol: 'none',
                        xAxisIndex: index,
                        areaStyle: {},
                        data: item,
                          lineStyle: {
                            type: index===1?'dashed':""
                          }
                    }
                }))
            };
            myChart.setOption(option);
        });
    }
    showCharts();
}]);
myApp.controller("writeblog", ["$scope", "$http", "$timeout","$location", function ($scope, $http, $timeout,$location) {
    UEDITOR_CONFIG.initialFrameHeight=null;
    UEDITOR_CONFIG.autoHeightEnabled=true;
    UEDITOR_CONFIG.zIndex=1;
    clearInterval(window.interval);
    $scope.post = {
        Title: "",
        schedule: false,
        Content: "",
        CategoryId: 1,
        Label: "",
        Seminars: "",
        Keyword:""
    };
    
    $scope.post.Author =$scope.user.NickName || $scope.user.Username;
    $scope.post.Email = $scope.user.Email;
    var refer = $location.search()['refer'];
    if (refer) {
        $scope.get("/post/get/"+refer, function (data) {
            $scope.post = data.Data;
            delete $scope.post.Id;
            $scope.keywordsDropdown.update({data: ($scope.post.Keyword||"").split(',')});
        });
    }
    $scope.getCategory = function () {
        $http.get("/category/getcategories").then(function (res) {
            var data = res.data;
            if (data.Success) {
                $scope.cat = data.Data;
                $scope.categoryDropdown = xmSelect.render({
                    el: '#category',
                    tips: '请选择分类',
                    prop: {
                        name: 'Name',
                        value: 'Id',
                        children: 'Children',
                    },
                    model: { label: { type: 'text' } },
                    radio: true,
                    clickClose: true,
                    tree: {
                        show: true,
                        strict: false,
                        expandedKeys: true,
                    },
                    filterable: true, //搜索功能
                    autoRow: true, //选项过多,自动换行
                    data:data.Data,
                    on: function (data) {
                        if (data.arr.length>0) {
                            $scope.post.CategoryId=data.arr[0].Id;
                        }
                    }
                });
            } else {
                window.notie.alert({
                    type: 3,
                    text: '获取文章分类失败！',
                    time: 4
                });
            }
        });
    }
    $scope.getCategory();
    $scope.get("/post/gettag", function(res) {
        $scope.Tags = res.Data;
        var tags=[];
        for (var i = 0; i < res.Data.length; i++) {
            tags.push({name:res.Data[i],value:res.Data[i]});
        }

        $scope.tagDropdown =xmSelect.render({
            el: '#tags',
            tips: '请选择标签',
            toolbar: { //工具条,全选,清空,反选,自定义
                show: true,
                list: ['ALL', 'CLEAR', 'REVERSE']
            },
            data: tags,
            //initValue: ['shuiguo','shucai'],//默认初始化,也可以数据中selected属性
            filterable: true, //搜索功能
            autoRow: true, //选项过多,自动换行
            // repeat: true,//是否支持重复选择
            //max: 2,//最多选择2个
            // template({ item, sels, name, value }){
            //    //template:自定义下拉框的模板
            //     return item.name  + '<span style="position: absolute; right: 10px; color: #8799a3">'+value+'</span>'
            // },
            on: function (data) {
                var arr=[];
                for (let j = 0; j < data.arr.length; j++) {
                    arr.push(data.arr[j].value);
                }
                $scope.post.Label=arr.join(",");
            },
            filterDone(val, list) { //val: 当前搜索值, list: 过滤后的数据
                var temp={name:val,value:val}
                if (tags.find(e=>e.value==val)==undefined) {
                    tags.push(temp);
                    $scope.tagDropdown.update({data:tags});
                    $scope.tagDropdown.setValue(($scope.post.Label||"").split(','));
                }
            }
        });
    });
    $scope.request("/seminar/getall", null, function(res) {
        $scope.Seminars = res.Data;
        for (var i = 0; i < res.Data.length; i++) {
            res.Data[i].name=res.Data[i].Title;
            res.Data[i].value=res.Data[i].Id;
        }

        $scope.seminarDropdown =xmSelect.render({
            el: '#seminar',
            tips: '请选择专题',
            toolbar: { //工具条,全选,清空,反选,自定义
                show: true,
                list: ['ALL', 'CLEAR', 'REVERSE']
            },
            data: res.Data,
            //initValue: ['shuiguo','shucai'],//默认初始化,也可以数据中selected属性
            filterable: true, //搜索功能
            autoRow: true, //选项过多,自动换行
            // repeat: true,//是否支持重复选择
            //max: 2,//最多选择2个
            // template({ item, sels, name, value }){
            //    //template:自定义下拉框的模板
            //     return item.name  + '<span style="position: absolute; right: 10px; color: #8799a3">'+value+'</span>'
            // },
            on: function (data) {
                var arr=[];
                for (let j = 0; j < data.arr.length; j++) {
                    arr.push(data.arr[j].value);
                }
                $scope.post.Seminars=arr.join(",");
            }
        })
    });

     layui.config({
        base: './Assets/layui/'
    }).use(['inputTag', 'jquery'], function () {
        var $ = layui.jquery, inputTag = layui.inputTag;
        $scope.keywordsDropdown = inputTag.render({
            elem: '.keywords',
            data: [],//初始值
            permanentData: [],//不允许删除的值
            removeKeyNum: 8,//删除按键编号 默认，BackSpace 键
            createKeyNum: 13,//创建按键编号 默认，Enter 键
            beforeCreate: function (data, value) {//添加前操作，必须返回字符串才有效
                return value;
            },
            onChange: function (data, value, type) {
                $scope.post.Keyword=data.join(",");
            }
        });
    });

    //上传Word文档
    $scope.upload = function() {
        $("#docform").ajaxSubmit({
            url: "/Upload/UploadWord",
            type: "post",
            success: function(data) {
                console.log(data);
                if (data.Success) {
                    window.notie.alert({
                        type: 1,
                        text: '文档上传成功!',
                        time: 2
                    });
                    $scope.$apply(function() {
                        $scope.post.Content = data.Data.Content;
                        $scope.post.Title = data.Data.Title;
                    });
                    layer.closeAll();
                } else {
                    window.notie.alert({
                        type: 3,
                        text: data.Message,
                        time: 4
                    });
                }
            }
        });
        $scope.selectFile = false;
    }

    //文件上传
    $scope.showupload = function() {
        layui.use("layer", function() {
            var layer = layui.layer;
            layer.open({
                type: 1,
                title: '上传Word文档',
                area: ['420px', '150px'], //宽高
                content: $("#docfile")
            });
        });
    }

    //异步提交表单开始
    $scope.submit = function(post) {
        Object.keys(post).forEach(key => post[key] == undefined||post[key] == null ? delete post[key] : '');
        if (!post.Label) {
            post.Label = null;
        }
        if (post.Title.trim().length <= 2 || post.Title.trim().length > 128) {
            window.notie.alert({
                type: 3,
                text: '文章标题必须在2到128个字符以内！',
                time: 4
            });
            
            return;
        }
        $http.post("/Post/write", post).then(function(res) {
            var data = res.data;
            if (data.Success) {
                window.notie.alert({
                    type: 1,
                    text: data.Message,
                    time: 4
                });
                $scope.post.Content = "";
                $scope.post.Title = "";
                clearInterval(window.interval);
                localStorage.removeItem("write-post-draft");
            } else {
                window.notie.alert({
                    type: 3,
                    text: data.Message,
                    time: 4
                });
            }
        });
    }

    // 定时发布
    $scope.Scheduled= function() {
        if ($scope.post.schedule) {
            layui.use('laydate', function(){
              var laydate = layui.laydate;
              laydate.render({
                elem: '#timespan',
                type: 'datetime',
                calendar: true,
                min: new Date(new Date().setMinutes(new Date().getMinutes()+10)).Format("yyyy-MM-dd hh:mm:ss"),
                max: new Date(new Date().setDate(new Date().getDate()+3)).Format("yyyy-MM-dd hh:mm:ss"),
                done: function(value, date, endDate) {
                    if (value) {
                        $scope.post.timespan=value;
                    } else {
                        delete $scope.post.timespan;
                    }
                }
              });
            });
        }
    }

    //检查草稿
    if (localStorage.getItem("write-post-draft")) {
        notie.confirm({
            text: "检查到上次有未提交的草稿，是否加载？",
            submitText: "确定",
            cancelText: "取消",
            position: "bottom",
            submitCallback: function () {
                $scope.post = JSON.parse(localStorage.getItem("write-post-draft"));
                $scope.$apply();
                $timeout(function () {
                    if ($scope.post.CategoryId>0) {
                        $scope.categoryDropdown.setValue([$scope.post.CategoryId]);
                        $scope.categoryDropdown.options.data.sort((a,b)=>(b.Id==$scope.post.CategoryId||b.Children.some(c=>c.Id==$scope.post.CategoryId||c.Children.some(cc=>cc.Id==$scope.post.CategoryId)))-(a.Id==$scope.post.CategoryId||a.Children.some(c=>c.Id==$scope.post.CategoryId||c.Children.some(cc=>cc.Id==$scope.post.CategoryId))));
                    }
                    if ($scope.post.Label) {
                        $scope.tagDropdown.setValue($scope.post.Label.split(','));
                    }
                    if ($scope.post.Seminars) {
                        $scope.seminarDropdown.setValue($scope.post.Seminars.split(','));
                    }
                    if ($scope.post.Keyword) {
                        console.log($scope.keywordsDropdown);
                        $scope.keywordsDropdown.render({data:$scope.post.Keyword.split(',')})
                    }
                    $scope.Scheduled();
                }, 10);
                window.interval = setInterval(function () {
                    localStorage.setItem("write-post-draft",JSON.stringify($scope.post));
                },5000);
            },
            cancelCallback: function() {
                window.interval = setInterval(function () {
                    localStorage.setItem("write-post-draft",JSON.stringify($scope.post));
                },5000);
            }
        });
    } else {
        window.interval = setInterval(function () {
            localStorage.setItem("write-post-draft",JSON.stringify($scope.post));
        },5000);
    }

    $scope.get("/post/GetRegions?name=Regions", function(data) {
        $scope.Regions=data.Data;
    });
    $scope.get("/post/GetRegions?name=ExceptRegions", function(data) {
        $scope.ExceptRegions=data.Data;
    });
    
    // 绑定过期时间表单元素
    layui.use('laydate', function(){
        var laydate = layui.laydate;
        laydate.render({
        elem: '#expireAt',
        type: 'datetime',
        calendar: true,
        min: new Date(new Date().setMinutes(new Date().getMinutes()+10)).Format("yyyy-MM-dd hh:mm:ss"),
        done: function(value, date, endDate) {
            if (value) {
                $scope.post.expireAt=value;
            } else {
                delete $scope.post.expireAt;
            }
        }
        });
    });
}]);

myApp.controller("postedit", ["$scope", "$http", "$location", "$timeout", function ($scope, $http, $location, $timeout) {
    UEDITOR_CONFIG.initialFrameHeight=null;
    UEDITOR_CONFIG.autoHeightEnabled=true;
    UEDITOR_CONFIG.zIndex=1;
    $scope.id = $location.search()['id'];
    $scope.reserve = true;
    $scope.get("/post/get/" + $scope.id, function (data) {
        $scope.post = data.Data;
        if ($scope.post.ExpireAt) {
            $scope.post.ExpireAt=new Date($scope.post.ExpireAt).Format("yyyy-MM-dd hh:mm:ss");
        }
        $scope.get("/post/gettag", function (res) {
            $scope.Tags = res.Data;
            var tags=[];
            for (var i = 0; i < res.Data.length; i++) {
                tags.push({name:res.Data[i],value:res.Data[i]});
            }

            $scope.tagDropdown =xmSelect.render({
                el: '#tags',
                tips: '请选择标签',
                toolbar: { //工具条,全选,清空,反选,自定义
                    show: true,
                    list: ['ALL', 'CLEAR', 'REVERSE']
                },
                data: tags,
                //initValue: ['shuiguo','shucai'],//默认初始化,也可以数据中selected属性
                filterable: true, //搜索功能
                autoRow: true, //选项过多,自动换行
                // repeat: true,//是否支持重复选择
                //max: 2,//最多选择2个
                // template({ item, sels, name, value }){
                //    //template:自定义下拉框的模板
                //     return item.name  + '<span style="position: absolute; right: 10px; color: #8799a3">'+value+'</span>'
                // },
                on: function (data) {
                    var arr=[];
                    for (let j = 0; j < data.arr.length; j++) {
                        arr.push(data.arr[j].value);
                    }
                    $scope.post.Label=arr.join(",");
                },
                filterDone(val, list) { //val: 当前搜索值, list: 过滤后的数据
                    var temp={name:val,value:val}
                    if (tags.find(e=>e.value==val)==undefined) {
                        tags.push(temp);
                        $scope.tagDropdown.update({data:tags});
                        $scope.tagDropdown.setValue(($scope.post.Label||"").split(','));
                    }
                }
            });
            if ($scope.post.Label) {
                $scope.tagDropdown.setValue($scope.post.Label.split(','));
            }
        });
        $scope.request("/seminar/getall", null, function (res) {
            $scope.Seminars = res.Data;
            for (var i = 0; i < res.Data.length; i++) {
                res.Data[i].name=res.Data[i].Title;
                res.Data[i].value=res.Data[i].Id;
            }

            $scope.seminarDropdown =xmSelect.render({
                el: '#seminar',
                tips: '请选择专题',
                toolbar: { //工具条,全选,清空,反选,自定义
                    show: true,
                    list: ['ALL', 'CLEAR', 'REVERSE']
                },
                data: res.Data,
                //initValue: ['shuiguo','shucai'],//默认初始化,也可以数据中selected属性
                filterable: true, //搜索功能
                autoRow: true, //选项过多,自动换行
                // repeat: true,//是否支持重复选择
                //max: 2,//最多选择2个
                // template({ item, sels, name, value }){
                //    //template:自定义下拉框的模板
                //     return item.name  + '<span style="position: absolute; right: 10px; color: #8799a3">'+value+'</span>'
                // },
                on: function (data) {
                    var arr=[];
                    for (let j = 0; j < data.arr.length; j++) {
                        arr.push(data.arr[j].value);
                    }
                    $scope.post.Seminars=arr.join(",");
                }
            });
            if ($scope.post.Seminars) {
                $scope.seminarDropdown.setValue($scope.post.Seminars.split(','));
            }
        });
        $scope.getCategory();
        layui.config({
            base: './Assets/layui/'
        }).use(['inputTag', 'jquery'], function () {
            var $ = layui.jquery, inputTag = layui.inputTag;
            $scope.keywordsDropdown = inputTag.render({
                elem: '.keywords',
                data:  ($scope.post.Keyword||"").split(','),//初始值
                permanentData: [],//不允许删除的值
                removeKeyNum: 8,//删除按键编号 默认，BackSpace 键
                createKeyNum: 13,//创建按键编号 默认，Enter 键
                beforeCreate: function (data, value) {//添加前操作，必须返回字符串才有效
                    return value;
                },
                onChange: function (data, value, type) {
                    $scope.post.Keyword=data.join(",");
                }
            });
        });
    });
    $scope.getCategory = function () {
        $http.post("/category/getcategories", null).then(function (res) {
            var data = res.data;
            if (data.Success) {
                $scope.cat = data.Data;
                $scope.categoryDropdown = xmSelect.render({
                    el: '#category',
                    tips: '请选择分类',
                    prop: {
                        name: 'Name',
                        value: 'Id',
                        children: 'Children',
                    },
                    model: { label: { type: 'text' } },
                    radio: true,
                    clickClose: true,
                    tree: {
                        show: true,
                        strict: false,
                        expandedKeys: true,
                    },
                    filterable: true, //搜索功能
                    autoRow: true, //选项过多,自动换行
                    data:data.Data,
                    on: function (data) {
                        if (data.arr.length>0) {
                            $scope.post.CategoryId=data.arr[0].Id;
                        }
                    }
                });
                if ($scope.post.CategoryId>0) {
                        $scope.categoryDropdown.setValue([$scope.post.CategoryId]);
                        $scope.categoryDropdown.options.data.sort((a,b)=>(b.Id==$scope.post.CategoryId||b.Children.some(c=>c.Id==$scope.post.CategoryId||c.Children.some(cc=>cc.Id==$scope.post.CategoryId)))-(a.Id==$scope.post.CategoryId||a.Children.some(c=>c.Id==$scope.post.CategoryId||c.Children.some(cc=>cc.Id==$scope.post.CategoryId))));
                    }
            } else {
                window.notie.alert({
                    type: 3,
                    text: '获取文章分类失败！',
                    time: 4
                });
            }
        });
    }
    //上传Word文档
    $scope.upload = function () {
        
        $("#docform").ajaxSubmit({
            url: "/Upload/UploadWord",
            type: "post",
            success: function (data) {
                
                console.log(data);
                if (data.Success) {
                    window.notie.alert({
                        type: 1,
                        text: '文档上传成功!',
                        time: 2
                    });
                    $scope.$apply(function () {
                        $scope.post.Content = data.Data.Content;
                        $scope.post.Title = data.Data.Title;
                    });
                    layer.closeAll();
                } else {
                    window.notie.alert({
                        type: 3,
                        text: data.Message,
                        time: 4
                    });
                }
            }
        });
        $scope.selectFile = false;
    }
    //文件上传
    $scope.showupload = function () {
        layui.use("layer", function () {
            var layer = layui.layer;
            layer.open({
                type: 1,
                title: '上传Word文档',
                area: ['420px', '150px'], //宽高
                content: $("#docfile")
            });
        });
    }

    //发布
    $scope.submit = function (post) {
        Object.keys(post).forEach(key => post[key] == undefined||post[key] == null ? delete post[key] : '');
        
        if (!post.Label) {
            post.Label = null;
        }
        if (post.Title.trim().length <= 2 || post.Title.trim().length > 128) {
            window.notie.alert({
                type: 3,
                text: '文章标题必须在2到128个字符以内！',
                time: 4
            });
            
            return;
        }
        if (post.Author.trim().length <= 0 || post.Author.trim().length > 20) {
            window.notie.alert({
                type: 3,
                text: '再怎么你也应该留个合理的名字吧，非主流的我可不喜欢！',
                time: 4
            });
            
            return;
        }
        if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/
            .test(post.Email.trim())) {
            window.notie.alert({
                type: 3,
                text: '请输入正确的邮箱格式！',
                time: 4
            });
            
            return;
        }
        if (post.Content.length < 20 || post.Content.length > 1000000) {
            window.notie.alert({
                type: 3,
                text: '文章内容过短或者超长，请修改后再提交！',
                time: 4
            });
            
            return;
        }
        post.reserve = $scope.reserve;
        $http.post("/Post/edit", post).then(function (res) {
            var data = res.data;
            
            if (data.Success) {
                window.notie.alert({
                    type: 1,
                    text: data.Message,
                    time: 4
                });
                $scope.post = data.Data;
                clearInterval(window.interval);
                localStorage.removeItem("post-draft-"+$scope.id);
            } else {
                window.notie.alert({
                    type: 3,
                    text: data.Message,
                    time: 4
                });
            }
        });
    }
    
    //检查草稿
    if (localStorage.getItem("post-draft-" + $scope.id)) {
        notie.confirm({
            text: "检查到上次有未提交的草稿，是否加载？",
            submitText: "确定",
            cancelText: "取消",
            position: "bottom",
            submitCallback: function () {
                $scope.post = JSON.parse(localStorage.getItem("post-draft-" + $scope.id));
                $scope.$apply();
                if ($scope.post.CategoryId>0) {
                    $scope.categoryDropdown.setValue([$scope.post.CategoryId]);
                    $scope.categoryDropdown.options.data.sort((a,b)=>(b.Id==$scope.post.CategoryId||b.Children.some(c=>c.Id==$scope.post.CategoryId||c.Children.some(cc=>cc.Id==$scope.post.CategoryId)))-(a.Id==$scope.post.CategoryId||a.Children.some(c=>c.Id==$scope.post.CategoryId||c.Children.some(cc=>cc.Id==$scope.post.CategoryId))));
                }
                if ($scope.post.Label) {
                    $scope.tagDropdown.setValue($scope.post.Label.split(','));
                }
                if ($scope.post.Seminars) {
                    $scope.seminarDropdown.setValue($scope.post.Seminars.split(','));
                }
                if ($scope.post.Keyword) {
                    console.log($scope.keywordsDropdown);
                    $scope.keywordsDropdown.render({data:$scope.post.Keyword.split(',')})
                }
                window.interval = setInterval(function () {
                    localStorage.setItem("post-draft-"+$scope.id,JSON.stringify($scope.post));
                },5000);
            },
            cancelCallback: function() {
                window.interval = setInterval(function () {
                    localStorage.setItem("post-draft-"+$scope.id,JSON.stringify($scope.post));
                },5000);
            }
        });
    } else {
        window.interval = setInterval(function () {
            localStorage.setItem("post-draft-"+$scope.id,JSON.stringify($scope.post));
        },5000);
    }
    
    $scope.get("/post/GetRegions?name=Regions", function(data) {
        $scope.Regions=data.Data;
    });
    $scope.get("/post/GetRegions?name=ExceptRegions", function(data) {
        $scope.ExceptRegions=data.Data;
    });
    
    // 绑定过期时间表单元素
    layui.use('laydate', function(){
        var laydate = layui.laydate;
        laydate.render({
        elem: '#expireAt',
        type: 'datetime',
        calendar: true,
        min: new Date(new Date().setMinutes(new Date().getMinutes()+10)).Format("yyyy-MM-dd hh:mm:ss"),
        done: function(value, date, endDate) {
            if (value) {
                $scope.post.expireAt=value;
            } else {
                delete $scope.post.expireAt;
            }
        }
        });
    });
}]);
myApp.controller("category", ["$scope", "$http", "$timeout", function($scope, $http, $timeout) {
    $scope.category = {};
    $scope.init = function() {
        $scope.get("/category/GetCategories", function(data) {
            $scope.data = data.Data;
            $scope.collapse = true;
            $timeout(function() {
                $scope.expandAll();
            }, 0);
        });
    }
    var sourceId, destId, index, parent, sourceIndex;
    $scope.treeOptions = {
        beforeDrop: function(e) {
            index = e.dest.index;
            if (e.dest.nodesScope.$parent.$modelValue) {
                parent = e.dest.nodesScope.$parent.$modelValue; //找出父级元素
            }
        },
        dropped: function(e) {
            var dest = e.dest.nodesScope;
            destId = dest.$id;
            var pid = dest.node ? dest.node.id : null; //pid
            var prev = null;
            var next = null;
            if (index > sourceIndex) {
                next = dest.$modelValue[index + 1], prev = dest.$modelValue[index];
            } else if (index < sourceIndex) {
                next = dest.$modelValue[index], prev = dest.$modelValue[index - 1];
            } else {
                next = dest.$modelValue[index];
            }
            var current = e.source.nodeScope.$modelValue;
            if (destId == sourceId) {
                if (index == sourceIndex) {
                    //位置没改变
                    return;
                }
                //同级内改变位置，找出兄弟结点，排序号更新
                if (prev || next) {
                    //有多个子节点
                    if (next) {
                        current.ParentId = pid;
                        $scope.request("/category/save", current, function(data) {
                            window.notie.alert({
                                type: 1,
                                text: data.Message,
                                time: 3
                            });
                        });
                    } else if (prev) {
                        current.ParentId = pid;
                        $scope.request("/category/save", current, function (data) {
                            window.notie.alert({
                                type: 1,
                                text: data.Message,
                                time: 3
                            });
                        });
                    }
                }
            } else {
                //层级位置改变
                if (parent) {
                    //非顶级元素
                    //找兄弟结点
                    next = dest.$modelValue[index], prev = dest.$modelValue[index - 1];
                    if (prev || next) {
                        //有多个子节点
                        if (next) {
                            current.ParentId = parent.Id;
                            $scope.request("/category/save", current, function (data) {
                                window.notie.alert({
                                    type: 1,
                                    text: data.Message,
                                    time: 3
                                });
                            });
                        } else if (prev) {
                            current.ParentId = parent.Id;
                            $scope.request("/category/save", current, function (data) {
                                window.notie.alert({
                                    type: 1,
                                    text: data.Message,
                                    time: 3
                                });
                            });
                        }
                    } else {
                        //只有一个元素
                        current.ParentId = parent.Id;
                        $scope.request("/category/save", current, function (data) {
                            window.notie.alert({
                                type: 1,
                                text: data.Message,
                                time: 3
                            });
                        });
                    }
                } else {
                    //顶级元素
                    sourceIndex = e.source.nodesScope.$parent.index();
                    if (index < sourceIndex) {
                        next = dest.$modelValue[index + 1], prev = dest.$modelValue[index];
                    } else {
                        next = dest.$modelValue[index], prev = dest.$modelValue[index - 1];
                    }
                    if (next) {
                        current.ParentId = pid;
                        $scope.request("/category/save", current, function (data) {
                            window.notie.alert({
                                type: 1,
                                text: data.Message,
                                time: 3
                            });
                        });
                    } else if (prev) {
                        current.ParentId = pid;
                        $scope.request("/category/save", current, function (data) {
                            window.notie.alert({
                                type: 1,
                                text: data.Message,
                                time: 3
                            });
                        });
                    }
                }
                parent = null;
            }
        },
        dragStart: function(e) {
            sourceId = e.dest.nodesScope.$id;
            sourceIndex = e.dest.index;
        }
    };
    $scope.findNodes = function () {
        
    };
    $scope.visible = function (item) {
        return !($scope.query && $scope.query.length > 0 && item.Name.indexOf($scope.query) == -1);
    };
    $scope.category = {};
    $scope.newItem = function() {
        layer.open({
            type: 1,
            zIndex: 20,
            title: '修改菜单信息',
            area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',// '340px'], //宽高
            content: $("#modal"),
            success: function(layero, index) {
                $scope.category = {};
            },
            end: function() {
                $("#modal").css("display", "none");
            }
        });
    };
    $scope.subcategory = {};

    $scope.closeAll = function() {
        layer.closeAll();
        setTimeout(function() {
            $("#modal").css("display", "none");
        }, 500);
    }
    $scope.newSubItem = function (scope) {
        layer.open({
            type: 1,
            zIndex: 20,
            title: '修改菜单信息',
            area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',// '340px'], //宽高
            content: $("#modal"),
            success: function(layero, index) {
                $scope.category = {};
            },
            end: function() {
                $("#modal").css("display", "none");
            }
        });
        var nodeData = scope.$modelValue;
        $scope.subcategory = nodeData;
        $scope.category.ParentId = nodeData.Id;
    };
    $scope.expandAll = function() {
        if ($scope.collapse) {
            $scope.$broadcast('angular-ui-tree:collapse-all');
        } else {
            $scope.$broadcast('angular-ui-tree:expand-all');
        }
        $scope.collapse = !$scope.collapse;
    };
    
    $scope.del = function(scope) {
        var model = scope.$nodeScope.$modelValue;
        var select = {};
        Enumerable.From($scope.data).Where(e => e.Id != model.Id).Select(e => {
            return {
                id: e.Id,
                name: e.Name
            }
        }).Distinct().ToArray().map(function(item, index, array) {
            select[item.id] = item.name;
        });
        swal({
            title: '确定删除这个分类吗？',
            text: "删除后将该分类下的所有文章移动到：",
            input: 'select',
            inputOptions: select,
            inputPlaceholder: '请选择分类',
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            inputValidator: function(value) {
                return new Promise(function(resolve, reject) {
                    if (value == '') {
                        reject('请选择一个分类');
                    } else {
                        resolve();
                    }
                });
            }
        }).then(function(result) {
            if (result) {
                if (model.Id == 1) {
                    swal({
                        type: 'error',
                        html: "默认分类不能被删除！"
                    });
                } else {
                    $scope.request("/category/delete?id="+model.Id+"&cid="+result, null, function(data) {
                        swal({
                            type: 'success',
                            html: data.Message
                        });
                    });
                    $scope.init();
                }
            }
        }).catch(swal.noop);
    }
    
    $scope.edit= function(category) {
        $scope.category = category;
        layer.open({
            type: 1,
            zIndex: 20,
            title: '修改菜单信息',
            area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',// '340px'], //宽高
            //area: ['600px', '270px'], //宽高
            content: $("#modal"),
            success: function(layero, index) {
                $scope.category = category;
            },
            end: function() {
                $("#modal").css("display", "none");
            }
        });
    }
    
    $scope.submit = function (category) {
        if (category.Id) {
            //修改
            $scope.request("/category/save", category, function (data) {
                swal(data.Message, null, 'info');
                $scope.category = {};
                $scope.closeAll();
            });
        }else {
            //添加
            $scope.request("/category/save", category, function (data) {
                window.notie.alert({
                    type: 1,
                    text: data.Message,
                    time: 3
                });
                $scope.category = {};
                $scope.closeAll();
                $scope.init();
            });
        }
    }
    $scope.init();
}]);
myApp.controller("postpending", ["$scope", "$http", "NgTableParams", "$timeout", function ($scope, $http, NgTableParams, $timeout) {
    var self = this;
    
    $scope.kw = "";
    $scope.orderby = 1;
    $scope.paginationConf = {
        currentPage: $scope.currentPage ? $scope.currentPage : 1,
        //totalItems: $scope.total,
        itemsPerPage: 10,
        pagesLength: 25,
        perPageOptions: [10, 15, 20, 30, 50, 100, 200],
        rememberPerPage: 'perPageItems',
        onChange: function() {
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
        }
    };
    this.GetPageData = function(page, size) {
        
        $http.post("/post/GetPending", {
            page,
            size,
            search:$scope.kw
        }).then(function(res) {
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
    self.del = function(row) {
        swal({
            title: "确认删除这篇文章吗？",
            text: row.Title,
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            showLoaderOnConfirm: true,
            animation: true,
            allowOutsideClick: false
        }).then(function() {
            $scope.request("/post/delete/"+row.Id, null, function(data) {
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
        $scope.request("/post/pass", row, function(data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
            self.stats = [];
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
        });
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
    
    $scope.addToBlock= function(row) {
        swal({
            title: "确认添加恶意名单吗？",
            text: "将"+row.Email+"添加到恶意名单",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            animation: true,
            allowOutsideClick: false,
            showLoaderOnConfirm: true,
            preConfirm: function () {
                return new Promise(function (resolve, reject) {
                    $http.post("/post/block/"+row.Id).then(function(res) {
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

myApp.controller("share", ["$scope", "NgTableParams", function ($scope, NgTableParams) {
    var self = this;
    self.data = {};
    this.load = function() {
        $scope.request("/share", null, function(res) {
            self.tableParams = new NgTableParams({}, {
                filterDelay: 0,
                dataset: res.Data
            });
            shares = res.Data;
        });
    }
    self.load();
    $scope.closeAll = function() {
        layer.closeAll();
        setTimeout(function() {
            $("#modal").css("display", "none");
        }, 500);
    }
    $scope.submit = function (share) {
        if (share.Id) {
            //修改
            $scope.request("/share/update", share, function (data) {
                swal(data.Message, null, 'info');
                $scope.share = {};
                $scope.closeAll();
                self.load();
            });
        }else {
            $scope.request("/share/add", share, function (data) {
                window.notie.alert({
                    type: 1,
                    text: data.Message,
                    time: 3
                });
                $scope.share = {};
                $scope.closeAll();
                self.load();
            });
        }
    }
    self.del = function(row) {
        swal({
            title: "确认删除这个分享吗？",
            text: row.Title,
            showCancelButton: true,
            showCloseButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            showLoaderOnConfirm: true,
            animation: true,
            allowOutsideClick: false
        }).then(function() {
            $scope.request("/share/remove", {
                id: row.Id
            }, function(data) {
                window.notie.alert({
                    type: 1,
                    text: data.Message,
                    time: 4
                });
                self.load();
            });
        }, function() {
        }).catch(swal.noop);
    }
    self.edit = function (row) {
        layer.open({
            type: 1,
            zIndex: 20,
            title: '修改快速分享',
            area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',
            content: $("#modal"),
            success: function(layero, index) {
                $scope.share = row;
            },
            end: function() {
                $("#modal").css("display", "none");
            }
        });
    }
    self.add = function() {
        layer.open({
            type: 1,
            zIndex: 20,
            title: '添加快速分享',
            area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',
            content: $("#modal"),
            success: function(layero, index) {
                $scope.share = {};
            },
            end: function() {
                $("#modal").css("display", "none");
            }
        });
    }
}]);