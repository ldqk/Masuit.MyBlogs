import api from "@/axios/AxiosConfig"

export interface PostData {
  Title: string
  Author: string
  Email: string
  Content: string
  ProtectContent?: string
  ProtectContentMode: number
  ProtectContentRegions?: string
  ProtectContentLimitMode?: number
  ProtectPassword?: string
  CategoryId: number
  Labels: string[]
  Keyword?: string
  Seminars: string[]
  DisableCopy: boolean
  DisableComment: boolean
  schedule: boolean
  timespan?: string
  Redirect?: string
  IsNsfw: boolean
  ExpireAt?: Date
  LimitMode: number
  Regions?: string
  ExceptRegions?: string
  Reserve: boolean
}

export interface Category {
  Id: number
  CategoryName: string
}

export interface Tag {
  Id: number
  TagName: string
}

export interface Seminar {
  Id: number
  Title: string
}

export interface ApiResponse<T = any> {
  Success: boolean
  Message?: string
  Data?: T
}

/**
 * 文章 API 类
 */
export class PostApi {
  /**
   * 获取分类列表
   */
  static async getCategories(): Promise<ApiResponse<Category[]>> {
    const response = await api.get('/category/getcategories')
    return response
  }

  /**
   * 获取标签列表
   */
  static async getTags(): Promise<ApiResponse<Tag[]>> {
    const response = await api.get('/post/gettag')
    return response
  }

  /**
   * 获取专题列表
   */
  static async getSeminars(): Promise<ApiResponse<Seminar[]>> {
    const response = await api.get('/seminar/getall')
    return response
  }

  /**
   * 发布文章
   */
  static async writePost(postData: PostData): Promise<ApiResponse> {
    const response = await api.post('/Post/write', postData)
    return response
  }

  /**
   * 修改文章
   */
  static async editPost(postData: PostData): Promise<ApiResponse> {
    const response = await api.post('/Post/edit', postData)
    return response
  }

  /**
   * 编辑文章
   */
  static async editPost(postData: PostData & { Id: number, reserve?: boolean }): Promise<ApiResponse> {
    const response = await api.post('/Post/edit', postData)
    return response
  }

  /**
   * 获取文章详情
   */
  static async getPost(id: number): Promise<ApiResponse<PostData & { Id: number }>> {
    const response = await api.get(`/post/get/${id}`)
    return response
  }

  /**
   * 上传 Word 文档
   */
  static async uploadWord(file: File): Promise<ApiResponse<{ Content: string }>> {
    const formData = new FormData()
    formData.append('upload', file)

    const response = await api.post('/Upload/UploadWord', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    return response
  }

  /**
   * 获取地区列表
   */
  static async getRegions(name: string = 'Regions'): Promise<ApiResponse<string[]>> {
    const response = await api.get(`/post/GetRegions?name=${name}`)
    return response
  }
}