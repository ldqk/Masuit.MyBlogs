import api from "@/axios/AxiosConfig"

export interface FileManagerItem {
  name: string
  size: number
  date: string
  type: "dir" | "file"
}

interface SystemSettingResponse {
  Success?: boolean
  Message?: string
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null
}

function extractError(value: unknown): string | undefined {
  if (!isRecord(value)) {
    return undefined
  }

  const result = value.result
  if (isRecord(result) && typeof result.error === "string") {
    return result.error
  }
  if (isRecord(value.error) && typeof value.error.message === "string") {
    return value.error.message
  }
  if (typeof value.Message === "string") {
    return value.Message
  }
  return undefined
}

function getRejectedResponseError(error: unknown): Error | undefined {
  if (!isRecord(error) || !isRecord(error.response)) {
    return undefined
  }
  const message = extractError(error.response.data)
  return message ? new Error(message) : undefined
}

async function postAction<T>(action: string, values: Record<string, unknown> = {}): Promise<T> {
  let response: unknown
  try {
    response = await api.post<unknown>("/File/Handle", { action, ...values })
  } catch (error) {
    throw getRejectedResponseError(error) || error
  }

  const error = extractError(response)
  if (error) {
    throw new Error(error)
  }
  if (!isRecord(response) || !("result" in response)) {
    throw new Error("文件管理器接口返回了无效响应")
  }
  return response.result as T
}

function getHandleUrl(): string {
  const baseURL = api.defaults.baseURL || ""
  const endpoint = `${baseURL.replace(/\/+$/, "")}/File/Handle`
  return new URL(endpoint.startsWith("/") || /^https?:\/\//i.test(endpoint) ? endpoint : `/${endpoint}`, window.location.origin).toString()
}

function normalizePath(path: string[]): string {
  return `/${path.filter(Boolean).join("/")}`
}

export class FileManagerApi {
  static async list(path: string[]): Promise<FileManagerItem[]> {
    const result = await postAction<unknown>("list", { path: normalizePath(path) })
    if (!Array.isArray(result)) {
      throw new Error("文件列表响应格式不正确")
    }

    return result.map((item: unknown) => {
      if (!isRecord(item) || typeof item.name !== "string" || (item.type !== "dir" && item.type !== "file")) {
        throw new Error("文件列表包含无效项目")
      }
      return {
        name: item.name,
        size: Number(item.size) || 0,
        date: typeof item.date === "string" ? item.date : "",
        type: item.type
      }
    })
  }

  static async upload(files: File[], destination: string[], onProgress: (progress: number) => void): Promise<void> {
    const formData = new FormData()
    formData.append("destination", normalizePath(destination))
    files.forEach((file, index) => formData.append(`file-${index}`, file, file.name))

    let response: unknown
    try {
      response = await api.post<unknown>("/File/Upload", formData, {
        onUploadProgress: (event) => {
          if (event.total) {
            onProgress(Math.min(100, Math.round((event.loaded / event.total) * 100)))
          }
        }
      })
    } catch (error) {
      throw getRejectedResponseError(error) || error
    }
    const error = extractError(response)
    if (error) {
      throw new Error(error)
    }
  }

  static async rename(item: string, newItemPath: string): Promise<void> {
    await postAction("rename", { item, newItemPath })
  }

  static async copy(items: string[], newPath: string[], singleFilename?: string): Promise<void> {
    await postAction("copy", {
      items,
      newPath: normalizePath(newPath),
      ...(singleFilename ? { singleFilename } : {})
    })
  }

  static async move(items: string[], newPath: string[]): Promise<void> {
    await postAction("move", { items, newPath: normalizePath(newPath) })
  }

  static async remove(items: string[]): Promise<void> {
    await postAction("remove", { items })
  }

  static async getContent(item: string): Promise<string> {
    const result = await postAction<unknown>("getContent", { item })
    if (typeof result !== "string") {
      throw new Error("文件内容响应格式不正确")
    }
    return result
  }

  static async edit(item: string, content: string): Promise<void> {
    await postAction("edit", { item, content })
  }

  static async createFolder(path: string[]): Promise<void> {
    await postAction("createFolder", { newPath: normalizePath(path) })
  }

  static async compress(items: string[], destination: string[], compressedFilename: string): Promise<void> {
    await postAction("compress", {
      items,
      destination: normalizePath(destination),
      compressedFilename
    })
  }

  static async extract(item: string, destination: string[], folderName: string): Promise<void> {
    await postAction("extract", {
      item,
      destination: normalizePath(destination),
      folderName
    })
  }

  static async changePermissions(items: string[], perms: string, permsCode: string, recursive: boolean): Promise<void> {
    await postAction("changePermissions", { items, perms, permsCode, recursive })
  }

  static async saveRootPath(path: string): Promise<void> {
    let response: unknown
    try {
      response = await api.post<unknown>("/system/save", [{ Name: "PathRoot", Value: path }])
    } catch (error) {
      throw getRejectedResponseError(error) || error
    }
    if (!isRecord(response)) {
      throw new Error("保存根目录设置时返回了无效响应")
    }
    const result = response as SystemSettingResponse
    if (result.Success !== true) {
      throw new Error(result.Message || "保存根目录设置失败")
    }
  }

  static downloadUrl(item: string): string {
    const params = new URLSearchParams({ action: "download", path: item })
    return `${getHandleUrl()}?${params.toString()}`
  }

  static downloadMultipleUrl(items: string[], toFilename: string): string {
    const params = new URLSearchParams({ action: "downloadMultiple", toFilename })
    items.forEach((item) => params.append("items", item))
    return `${getHandleUrl()}?${params.toString()}`
  }
}
