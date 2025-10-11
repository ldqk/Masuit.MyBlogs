import 'codemirror/lib/codemirror.css'

type CodeMirrorInstance = any

let loadingPromise: Promise<CodeMirrorInstance> | null = null
let modesLoaded = false

const loadModeModules = async () => {
  if (modesLoaded) {
    return
  }

  await Promise.all([
    import(/* webpackChunkName: "codemirror-mode-xml" */ 'codemirror/mode/xml/xml'),
    import(/* webpackChunkName: "codemirror-mode-javascript" */ 'codemirror/mode/javascript/javascript'),
    import(/* webpackChunkName: "codemirror-mode-css" */ 'codemirror/mode/css/css'),
    import(/* webpackChunkName: "codemirror-mode-htmlmixed" */ 'codemirror/mode/htmlmixed/htmlmixed')
  ])

  modesLoaded = true
}

const ensureCodeMirror = async () => {
  if (loadingPromise) {
    return loadingPromise
  }

  loadingPromise = import(/* webpackChunkName: "codemirror-core" */ 'codemirror')
    .then((module) => {
      const CodeMirror = (module as { default?: CodeMirrorInstance }).default ?? (module as CodeMirrorInstance)
        ; (window as any).CodeMirror = CodeMirror
      return CodeMirror
    })
    .then(async (CodeMirror) => {
      if (CodeMirror?.modes?.htmlmixed) {
        return CodeMirror
      }

      await loadModeModules()
      return CodeMirror
    })
    .catch((error) => {
      loadingPromise = null
      throw error
    })

  return loadingPromise
}

export const loadCodeMirror = async () => {
  if ((window as any).CodeMirror?.modes?.htmlmixed) {
    return (window as any).CodeMirror
  }

  const codeMirror = await ensureCodeMirror()

  if (!codeMirror?.modes?.htmlmixed) {
    await loadModeModules()
  }

  return codeMirror
}

export default loadCodeMirror
