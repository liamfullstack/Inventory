const seed = [
{id:1,sku:'IT-001',name:'無線鍵盤',category:'電腦周邊',price:1290,quantity:42},
{id:2,sku:'IT-002',name:'USB-C 集線器',category:'電腦周邊',price:890,quantity:5},
{id:3,sku:'NW-001',name:'Gigabit 網路交換器',category:'網路設備',price:1690,quantity:18},
{id:4,sku:'NW-002',name:'Cat 6 網路線',category:'網路設備',price:150,quantity:96},
{id:5,sku:'ST-001',name:'NVMe SSD 1TB',category:'儲存設備',price:2490,quantity:0},
{id:6,sku:'ST-002',name:'外接硬碟 2TB',category:'儲存設備',price:2190,quantity:12}]
const key='stockroom-products-v1'
const base=(import.meta.env.VITE_API_URL || '').replace(/\/$/,'')
function read(){return JSON.parse(localStorage.getItem(key) || JSON.stringify(seed))}
function write(items){localStorage.setItem(key,JSON.stringify(items))}
async function request(path='',method='GET',data){const r=await fetch(`${base}/api/products${path}`,{method,headers:{'Content-Type':'application/json'},...(data?{body:JSON.stringify(data)}:{})});if(!r.ok)throw new Error(`API 錯誤 (${r.status})`);return r.status===204?null:r.json()}
export const productsApi={
 async list(){return base?request():read()},
 async save(item){if(base){return item.id?request(`/${item.id}`,'PUT',item):request('','POST',item)}const items=read();if(item.id){write(items.map(p=>p.id===item.id?item:p))}else{write([...items,{...item,id:Math.max(0,...items.map(p=>p.id))+1}])}},
 async remove(id){if(base)return request(`/${id}`,'DELETE');write(read().filter(p=>p.id!==id))}
}
