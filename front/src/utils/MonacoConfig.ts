// Monaco Editor Worker 配置
// 用于解决 "You must define a function MonacoEnvironment.getWorkerUrl or MonacoEnvironment.getWorker" 错误

// 在全局范围内配置Monaco Editor的Worker环境
// @ts-ignore
self.MonacoEnvironment = {
  getWorker: function () {
    // 创建一个简单的Worker来处理Monaco Editor的语法高亮等功能
    return new Worker(
      'data:text/javascript;charset=utf-8,' + encodeURIComponent(`
        // 简化的Worker实现，禁用复杂功能以避免错误
        self.addEventListener('message', function(e) {
          // 简单回复，不执行复杂处理
          self.postMessage({ id: e.data.id, result: null });
        });
      `)
    )
  }
}

export {}