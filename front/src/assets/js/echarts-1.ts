/**
 * ECharts 图表配置 - 1
 * 包含渐变色配置和图表选项
 */

// 颜色列表配置
interface ColorStop {
  offset: number
  color: string
}

interface LinearGradient {
  type: 'linear'
  x: number
  y: number
  x2: number
  y2: number
  colorStops: ColorStop[]
  globalCoord: boolean
}

const colorList: LinearGradient[] = [{
  type: 'linear',
  x: 0,
  y: 0,
  x2: 1,
  y2: 1,
  colorStops: [{
    offset: 0,
    color: 'rgba(51,192,205,0.01)' // 0% 处的颜色
  },
  {
    offset: 1,
    color: 'rgba(51,192,205,0.57)' // 100% 处的颜色
  }
  ],
  globalCoord: false // 缺省为 false
},
{
  type: 'linear',
  x: 1,
  y: 0,
  x2: 0,
  y2: 1,
  colorStops: [{
    offset: 0,
    color: 'rgba(115,172,255,0.02)' // 0% 处的颜色
  },
  {
    offset: 1,
    color: 'rgba(115,172,255,0.67)' // 100% 处的颜色
  }
  ],
  globalCoord: false
}]

// ECharts 选项接口
interface EChartsOption {
  grid?: any
  tooltip?: any
  legend?: any
  xAxis?: any
  yAxis?: any
  series?: any[]
  dataZoom?: any[]
  // 可根据需要添加更多属性
}

/**
 * 获取 ECharts 配置选项
 * @returns ECharts 配置对象
 */
export function getEChartsOption1(): EChartsOption {
  // 这里应该包含完整的 ECharts 配置
  // 为了简化，只返回基本结构
  return {
    grid: {},
    tooltip: {},
    legend: {},
    xAxis: {},
    yAxis: {},
    series: []
  }
}

export { colorList }
export type { EChartsOption, LinearGradient, ColorStop }