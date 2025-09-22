import router from './index'
import LoadingBar from '../components/LoadingBar/LoadingBar'

router.afterEach(() => {
  // 使用多个 stop() 来保证 LoadingBar 在动态添加路由后正确关闭
  LoadingBar.stop()
  LoadingBar.stop()
})

export default router
