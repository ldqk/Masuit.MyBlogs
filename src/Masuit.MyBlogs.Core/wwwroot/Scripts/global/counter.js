const { createApp, ref, onMounted, watch, computed } = Vue;
const { createDiscreteApi } = naive;
const { message, dialog } = createDiscreteApi(["message", "dialog"]);
window.message = message;
window.dialog = dialog;
createApp({
  setup() {
    const servers = ref([]);
    const server = ref('');
    return {
      servers,
      server
    };
  },
  methods: {
    async getServers() {
      this.servers = await axios.get("/system/GetServers").then(function (response) {
        return response.data.map(item => { return { label: item, value: item }; });
      });
      this.server = this.servers.length > 0 ? this.servers[0].value : '';
    },
    async clearMemory() {
      await axios.post("/dashboard/clear-memory");
      message.success("操作成功");
    },
    async refreshMemory() {
      await axios.post("/dashboard/refresh-memory");
      message.success("操作成功");
    }
  },
  mounted() {
    this.getServers();
    showLine(this.server);
  },
  watch: {
    server(newVal, oldVal) {
      showLine(this.server);
    }
  },
}).use(naive).mount('#app');
function showSpeed() {
  var myChart = echarts.init(document.getElementById("container"));
  myChart.setOption({
    series: [{
      type: 'gauge',
      anchor: {
        show: true,
        showAbove: true,
        size: 18,
        itemStyle: {
          color: '#FAC858'
        }
      },
      pointer: {
        icon: 'path://M2.9,0.7L2.9,0.7c1.4,0,2.6,1.2,2.6,2.6v115c0,1.4-1.2,2.6-2.6,2.6l0,0c-1.4,0-2.6-1.2-2.6-2.6V3.3C0.3,1.9,1.4,0.7,2.9,0.7z',
        width: 8,
        length: '80%',
        offsetCenter: [0, '8%']
      },

      progress: {
        show: true,
        overlap: true,
        roundCap: true
      },
      axisLine: {
        roundCap: true
      },
      data: [{
        value: 0,
        name: 'CPU',
        title: {
          offsetCenter: ['-25%', '80%']
        },
        detail: {
          offsetCenter: ['-25%', '95%']
        }
      },
      {
        value: 0,
        name: '内存',
        title: {
          offsetCenter: ['25%', '80%']
        },
        detail: {
          offsetCenter: ['25%', '95%']
        }
      }, {
        value: 0,
        name: 'CPU(网站)',
        title: {
          offsetCenter: ['-75%', '80%']
        },
        detail: {
          offsetCenter: ['-75%', '95%']
        }
      },
      {
        value: 0,
        name: '内存(网站)',
        title: {
          offsetCenter: ['75%', '80%']
        },
        detail: {
          offsetCenter: ['75%', '95%']
        }
      }],
      title: {
        fontSize: 14
      },
      detail: {
        width: 40,
        height: 14,
        fontSize: 14,
        color: '#fff',
        backgroundColor: 'auto',
        borderRadius: 3,
        formatter: '{value}%'
      }
    }]
  });
  return myChart;
}

function showIO(data) {
  var myChart = echarts.init(document.getElementById("container-io"));
  myChart.setOption({
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        animation: false
      }
    },
    dataZoom: [{
      type: 'inside',
      start: 70,
      end: 100,
      minValueSpan: 100
    }, {
      start: 70,
      end: 100,
      minValueSpan: 100
    }],
    xAxis: {
      type: 'time',
      interval: 20000,
      axisLabel: {
        formatter: function (value) {
          var dt = new Date(value);
          return dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds()
        }
      },
      splitLine: {
        show: false
      }
    },
    yAxis: [{
      name: '磁盘',
      type: 'value',
      splitLine: {
        show: false
      }
    },
    {
      name: '网络',
      type: 'value',
      splitLine: {
        show: false
      }
    }
    ],
    legend: {
      data: ['磁盘读(KBps)', '磁盘写(KBps)', "网络上行(KBps)", "网络下行(KBps)"],
      top: 0
    },
    series: [{
      name: '磁盘读(KBps)',
      type: 'line',
      showSymbol: false,
      hoverAnimation: false,
      data: data.read
    }, {
      name: '磁盘写(KBps)',
      type: 'line',
      showSymbol: false,
      hoverAnimation: false,
      data: data.write
    }, {
      name: '网络上行(KBps)',
      yAxisIndex: 1,
      type: 'line',
      showSymbol: false,
      hoverAnimation: false,
      data: data.up
    }, {
      name: '网络下行(KBps)',
      yAxisIndex: 1,
      type: 'line',
      showSymbol: false,
      hoverAnimation: false,
      data: data.down
    }]
  });
  return myChart;
}

function showLine(ip) {
  clearInterval(window.counterInterval);
  window.fetch("/system/GetCounterHistory?ip=" + ip, {
    credentials: 'include',
    method: 'GET',
    mode: 'cors'
  }).then(function (response) {
    return response.json();
  }).then(function (data) {
    var myChart = echarts.init(document.getElementById("container-cpu"));
    myChart.setOption({
      visualMap: [{
        show: false,
        type: 'continuous',
        seriesIndex: 0
      }],
      tooltip: {
        trigger: 'axis',
        axisPointer: {
          animation: false
        }
      },
      dataZoom: [{
        type: 'inside',
        start: 70,
        end: 100,
        minValueSpan: 100
      }, {
        start: 70,
        end: 100,
        minValueSpan: 100
      }],
      xAxis: {
        type: 'time',
        interval: 20000,
        axisLabel: {
          formatter: function (value) {
            var dt = new Date(value);
            return dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds()
          }
        },
        splitLine: {
          show: false
        }
      },
      yAxis: {
        type: 'value',
        boundaryGap: [0, '100%'],
        splitLine: {
          show: false
        },
        max: 100
      },
      legend: {
        data: ['CPU使用率', '网站CPU使用率', '内存使用率', '网站内存使用率'],
        top: 0
      },
      series: [{
        name: 'CPU使用率',
        type: 'line',
        showSymbol: false,
        hoverAnimation: false,
        data: data.cpu,
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
      }, {
        name: '网站CPU使用率',
        type: 'line',
        showSymbol: false,
        hoverAnimation: false,
        data: data.processCpu
      }, {
        name: '内存使用率',
        type: 'line',
        showSymbol: false,
        hoverAnimation: false,
        data: data.mem,
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
      }, {
        name: '网站内存使用率',
        type: 'line',
        showSymbol: false,
        hoverAnimation: false,
        data: data.processMem
      }]
    });
    var rateChart = showSpeed();
    var ioChart = showIO(data);
    DotNet.invokeMethodAsync('Masuit.MyBlogs.Core', 'GetTotalMemory').then(res => {
      window.counterInterval = setInterval(function () {
        DotNet.invokeMethodAsync('Masuit.MyBlogs.Core', 'GetCurrentPerformanceCounter').then(item => {
          data.cpu.push([item.time, item.cpuLoad.toFixed(2)]);
          data.mem.push([item.time, (item.memoryUsage / res * 100).toFixed(2)]);
          data.processCpu.push([item.time, item.processCpuLoad.toFixed(2)]);
          data.processMem.push([item.time, (item.processMemoryUsage / res * 100).toFixed(2)]);
          data.read.push([item.time, item.diskRead.toFixed(2)]);
          data.write.push([item.time, item.diskWrite.toFixed(2)]);
          data.up.push([item.time, item.upload.toFixed(2)]);
          data.down.push([item.time, item.download.toFixed(2)]);
          myChart.setOption({
            series: [{
              data: data.cpu
            }, {
              data: data.processCpu
            }, {
              data: data.mem
            }, {
              data: data.processMem
            }]
          });
          ioChart.setOption({
            series: [{
              data: data.read
            }, {
              data: data.write
            }, {
              data: data.up
            }, {
              data: data.down
            }]
          });
          let option = rateChart.getOption();
          option.series[0].data[0].value = item.cpuLoad.toFixed(2);
          option.series[0].data[1].value = (item.memoryUsage / res * 100).toFixed(2);
          option.series[0].data[2].value = item.processCpuLoad.toFixed(2);
          option.series[0].data[3].value = (item.processMemoryUsage / res * 100).toFixed(2);
          rateChart.setOption(option, true);
        });
      }, 2000);
    })
  }).catch(function (e) {
    console.error(e);
  });
}
