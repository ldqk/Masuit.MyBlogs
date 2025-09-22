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
      // 优化配置但禁用CSS压缩器
      config.optimization = config.optimization || {}

      // 临时禁用CSS压缩以避免构建错误
      config.optimization.minimizer = config.optimization.minimizer || []
      config.optimization.minimizer = config.optimization.minimizer.filter(plugin =>
        !plugin.constructor.name.includes('CssMinimizerPlugin')
      )

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

      // Gzip 压缩
      const CompressionPlugin = require('compression-webpack-plugin')
      config.plugins.push(
        new CompressionPlugin({
          algorithm: 'gzip',
          test: /\.(js|css|woff|woff2|svg)$/, // 匹配文件名
          threshold: 10240, // 对超过10k的数据压缩
          deleteOriginalAssets: false, // 不删除源文件
          minRatio: 0.8 // 压缩比
        })
      )

      // 将 js 文件夹添加时间戳，这样浏览器不会加载上个版本缓存的代码
      config.output.filename = `js/[name].${timeStamp}.js`
      config.output.chunkFilename = `js/[name].${timeStamp}.js`
    } else {
      // 开发环境配置
    }
  },
  chainWebpack: config => {
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
        // 'vue': 'Vue',
        // 'vue-router': 'VueRouter',
        // 'axios': 'axios',
        // 'echarts': 'echarts'
        // 注释掉CDN配置，先测试其他优化效果
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
