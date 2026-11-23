const timeStamp = new Date().getTime()

module.exports = {
  publicPath: process.env.NODE_ENV === 'production' ? '/dashboard/' : '/',
  // 配置入口文件支持TypeScript
  pages: {
    index: {
      entry: 'src/main.ts',
      template: 'public/index.html',
      filename: 'index.html'
    }
  },
  devServer: {
    port: 8868,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        pathRewrite: { '^/api': '' },
        onProxyReq: (proxyReq, req) => {
          const address = req.socket.remoteAddress
          const local = address === '::1' || address === '127.0.0.1' || address === '::ffff:127.0.0.1'
          proxyReq.setHeader('X-Dev-Proxy-Client', local ? 'local' : 'remote')
        }
      }
    }
  },
  pluginOptions: {
    quasar: {
      importStrategy: 'kebab',
      rtlSupport: false
    }
  },
  transpileDependencies: [
    'quasar',
    'resize-detector',
    '@kangc'
  ],

  // 项目打包文件夹
  outputDir: '../src/Masuit.MyBlogs.Core/wwwroot/dashboard',

  // 静态资源文件夹 *注：当生成的资源覆写 filename 或 chunkFilename 时，assetsDir 会被忽略。
  // assetsDir: 'static',

  // 关闭 sourcemap
  productionSourceMap: false,

  // 打包的时候不使用 hash 值
  filenameHashing: false,

  // Webpack 函数式配置
  configureWebpack: config => {
    // TypeScript 支持
    config.resolve.extensions.push('.ts')

    // 体积分析
    if (process.env.use_analyzer) {
      const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
      config.plugins.push(new BundleAnalyzerPlugin())
    }

    // 生产环境配置
    if (process.env.NODE_ENV === 'production') {
      const cssMinimizer = config.optimization.minimizer.find(plugin =>
        plugin.constructor.name.includes('CssMinimizerPlugin')
      )
      if (cssMinimizer) {
        cssMinimizer.options.parallel = false
        // 部分依赖样式的选择器无法被 cssnano 解析，跳过选择器相关优化。
        const [preset, presetOptions = {}] = cssMinimizer.options.minimizer.options.preset
        cssMinimizer.options.minimizer.options.preset = [
          preset,
          { ...presetOptions, minifySelectors: false, mergeRules: false, uniqueSelectors: false }
        ]
      }

      // 优化代码分割
      config.optimization.splitChunks = {
        chunks: 'all',
        cacheGroups: {
          // Vue核心库单独打包
          vue: {
            name: 'chunk-vue',
            priority: 20,
            test: /[\\/]node_modules[\\/](vue|vue-router|pinia)[\\/]/
          },
          // UI框架单独打包
          quasar: {
            name: 'chunk-quasar',
            priority: 15,
            test: /[\\/]node_modules[\\/]quasar[\\/]/
          },
          // 大型图表库单独打包
          echarts: {
            name: 'chunk-echarts',
            priority: 15,
            test: /[\\/]node_modules[\\/](echarts|vue-echarts)[\\/]/
          },
          // 其他第三方库
          vendor: {
            name: 'chunk-vendors',
            priority: 10,
            test: /[\\/]node_modules[\\/]/,
            minChunks: 1
          },
          // 公共组件
          common: {
            name: 'chunk-common',
            priority: 5,
            minChunks: 2,
            reuseExistingChunk: true
          }
        }
      }

      // 配置 Terser 插件移除 console
      if (config.optimization && config.optimization.minimizer && config.optimization.minimizer[0]) {
        const terserPlugin = config.optimization.minimizer[0]
        if (terserPlugin.options && terserPlugin.options.terserOptions && terserPlugin.options.terserOptions.compress) {
          terserPlugin.options.terserOptions.compress.drop_console = true
          terserPlugin.options.terserOptions.compress.drop_debugger = true
        }
      }

      // 将 js 文件夹添加时间戳，这样浏览器不会加载上个版本缓存的代码
      config.output.filename = `js/[name].${timeStamp}.js`
      config.output.chunkFilename = `js/[name].${timeStamp}.js`
    } else {
      // 开发环境配置
    }
  },
  chainWebpack: config => {
    if (process.env.NODE_ENV === 'production') {
      config.plugin('copy').tap(([options]) => {
        options.patterns[0].globOptions.ignore.push('**/UEditorPlus/**')
        return [options]
      })
    }

    // 移除 prefetch 插件减少不必要的预加载
    config.plugins.delete('prefetch')
    config.plugins.delete('preload')

    // 优先处理 .d.ts 文件 - 使用 null-loader 忽略它们
    config.module
      .rule('ignore-d-ts')
      .test(/\.d\.ts$/)
      .use('null-loader')
      .loader('null-loader')
      .end()

    // TypeScript 配置
    config.module
      .rule('typescript')
      .test(/\.ts$/)
      .exclude
      .add(/node_modules/)
      .add(/\.d\.ts$/)
      .end()
      .use('ts-loader')
      .loader('ts-loader')
      .options({
        appendTsSuffixTo: [/\.vue$/],
        transpileOnly: true
      })
      .end()

    // 生产环境配置CDN外部依赖
    if (process.env.NODE_ENV === 'production') {
      config.externals({
        vue: 'Vue',
        'vue-router': 'VueRouter',
        axios: 'axios',
        echarts: 'echarts',
        dayjs: 'dayjs',
        pinia: 'Pinia',
        quasar: 'Quasar',
        'vxe-pc-ui': 'VxeUI',
        'vxe-table': 'VxeUITable',
        'xe-utils': 'XEUtils',
        'lottie-web': 'lottie'
      })
    }
  },
  css: {
    // 生产环境CSS优化
    sourceMap: false,
    extract: process.env.NODE_ENV === 'production'
      ? {
        // 合并CSS文件减少文件数量
        filename: `css/[name].${timeStamp}.css`,
        chunkFilename: `css/[name].${timeStamp}.css`,
        ignoreOrder: true // 忽略CSS顺序警告
      }
      : false
  }
}
