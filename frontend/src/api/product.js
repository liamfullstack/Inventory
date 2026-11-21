const BASE = import.meta.env.VITE_API_BASE

async function request(path, options = {}) {
  let res
  try {
    res = await fetch(`${BASE}${path}`, {
      headers: { 'Content-Type': 'application/json' },
      ...options,
    })
  }  catch {
    throw new Error(`無法連線到 ${BASE || '(VITE_API_BASE 未設定)'},請確認後端是否已啟動。`)
  }
  if (!res.ok) {
    const body = await res.json().catch(() => null)
    const firstValidation = body?.errors && Object.values(body.errors).flat()[0]
    throw new Error(body?.message ?? firstValidation ?? `請求失敗 (${res.status})`)
  }
  return res.status === 204 ? null : res.json()
}

// 後端欄位 <-> 前端欄位
const fromApi = (p) => ({
  id: p.id,
  sku: p.code,
  name: p.name,
  category: p.category,
  price: p.price,
  quantity: p.stock,
})

const toApi = (p) => ({
  code: p.sku,
  name: p.name,
  category: p.category,
  price: p.price,
  stock: p.quantity,
})

export const productsApi = {
  // 後端有分頁,這裡一頁一頁抓完,讓前端維持原本的搜尋與分頁邏輯
  async list() {
    const pageSize = 100
    const all = []
    for (let page = 1; ; page++) {
      const r = await request(`/api/products?page=${page}&pageSize=${pageSize}`)
      all.push(...r.items.map(fromApi))
      if (r.items.length === 0 || all.length >= r.total) break
    }
    return all
  },

  async save(p) {
    const body = JSON.stringify(toApi(p))
    const saved = p.id
      ? await request(`/api/products/${p.id}`, { method: 'PUT', body })
      : await request('/api/products', { method: 'POST', body })
    return fromApi(saved)
  },

  async remove(id) {
    await request(`/api/products/${id}`, { method: 'DELETE' })
  },
}