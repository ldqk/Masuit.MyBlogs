/**
 * 深拷贝函数
 * @param obj - 要拷贝的对象
 * @returns 深拷贝后的对象
 */
function deepClone<T>(obj: T): T {
  if (obj === null || typeof obj !== 'object') {
    return obj
  }
  
  if (obj instanceof Date) {
    return new Date(obj.getTime()) as T
  }
  
  if (obj instanceof Array) {
    const newArray: any[] = []
    for (let i = 0; i < obj.length; i++) {
      newArray[i] = deepClone(obj[i])
    }
    return newArray as T
  }
  
  const newObj: any = {}
  for (const attr in obj) {
    if (obj.hasOwnProperty(attr)) {
      newObj[attr] = deepClone(obj[attr])
    }
  }
  return newObj
}

/**
 * 获取对象的第一个值
 * @param obj - 对象
 * @returns 第一个值
 */
export function getFirst<T>(obj: Record<string, T>): T | undefined {
  for (const key in obj) {
    if (obj.hasOwnProperty(key)) {
      return obj[key]
    }
  }
  return undefined
}

export default deepClone