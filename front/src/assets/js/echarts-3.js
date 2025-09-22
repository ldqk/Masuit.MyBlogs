export const income = {
  xAxis: {
    type: 'category',
    show: false
  },
  yAxis: {
    type: 'value',
    show: false
  },
  series: [{
    data: [10, 50, 36, 85, 98, 72, 79, 88, 80],
    type: 'line',
    smooth: true,
    show: false,
    symbol: 'none',
    animationDuration: 1000,
    itemStyle: {
      normal: {
        lineStyle: {
          color: '#ffffff'
        }

      }
    }
  }]
}

export const expense = {
  xAxis: {
    type: 'category',
    show: false
  },
  yAxis: {
    type: 'value',
    show: false
  },
  series: [{
    data: [50, 42, 36, 78, 56, 72, 20, 15, 35],
    type: 'line',
    smooth: true,
    show: false,
    symbol: 'none',
    animationDuration: 1000,
    itemStyle: {
      normal: {
        lineStyle: {
          color: '#ffffff'
        }

      }
    }
  }]
}

export const total = {
  xAxis: {
    type: 'category',
    show: false
  },
  yAxis: {
    type: 'value',
    show: false
  },
  series: [{
    data: [30, 45, 64, 78, 79, 80, 75, 70, 90],
    type: 'line',
    smooth: true,
    show: false,
    symbol: 'none',
    animationDuration: 1000,
    itemStyle: {
      normal: {
        lineStyle: {
          color: '#ffffff'
        }

      }
    }
  }]
}
