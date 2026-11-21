<script setup>
import { ref, computed, onMounted } from "vue";
import { productsApi } from "./api/product";
const items = ref([]),
  query = ref(""),
  category = ref(""),
  filter = ref("all"),
  page = ref(1),
  modal = ref(false),
  busy = ref(false),
  error = ref(""),
  formError = ref(""),
  deleting = ref(null);
const blank = () => ({ sku: "", name: "", category: "電腦周邊", price: 0, quantity: 0 });
const form = ref(blank()),
  categories = computed(() => [...new Set(items.value.map((p) => p.category))].sort());
const low = (p) => p.quantity > 0 && p.quantity <= 10;
const status = (p) => (p.quantity === 0 ? "缺貨" : low(p) ? "低庫存" : "正常");
const money = (n) =>
  new Intl.NumberFormat("zh-TW", {
    style: "currency",
    currency: "TWD",
    maximumFractionDigits: 0,
  }).format(n);
const stats = computed(() => [
  { label: "商品種類", value: items.value.length, unit: "項" },
  {
    label: "庫存總數",
    value: items.value.reduce((s, p) => s + p.quantity, 0),
    unit: "件",
  },
  {
    label: "庫存總值",
    value: money(items.value.reduce((s, p) => s + p.quantity * p.price, 0)),
    unit: "",
  },
  {
    label: "待補貨商品",
    value: items.value.filter((p) => p.quantity <= 10).length,
    unit: "項",
  },
]);
const filtered = computed(() =>
  items.value.filter(
    (p) =>
      `${p.name} ${p.sku}`.toLowerCase().includes(query.value.toLowerCase().trim()) &&
      (!category.value || p.category === category.value) &&
      (filter.value === "all" ||
        (filter.value === "low" && low(p)) ||
        (filter.value === "out" && p.quantity === 0))
  )
);
const pages = computed(() => Math.max(1, Math.ceil(filtered.value.length / 8))),
  current = computed(() => Math.min(page.value, pages.value)),
  visible = computed(() =>
    filtered.value.slice((current.value - 1) * 8, current.value * 8)
  );
async function load() {
  try {
    items.value = await productsApi.list();
    error.value = "";
  } catch (e) {
    error.value = e.message;
  }
}
onMounted(load);
function open(p) {
  form.value = p ? { ...p } : blank();
  formError.value = "";
  modal.value = true;
}
async function save() {
  const p = {
    ...form.value,
    sku: form.value.sku.trim(),
    name: form.value.name.trim(),
    category: form.value.category.trim(),
  };
  if (
    !p.sku ||
    !p.name ||
    !p.category ||
    !Number.isFinite(p.price) ||
    p.price < 0 ||
    !Number.isInteger(p.quantity) ||
    p.quantity < 0
  ) {
    formError.value = "請輸入完整資料；價格不得為負，庫存須為非負整數。";
    return;
  }
  if (
    items.value.some((x) => x.id !== p.id && x.sku.toLowerCase() === p.sku.toLowerCase())
  ) {
    formError.value = "商品編號不可重複。";
    return;
  }
  busy.value = true;
  try {
    await productsApi.save(p);
    modal.value = false;
    await load();
  } catch (e) {
    formError.value = e.message;
  } finally {
    busy.value = false;
  }
}
async function remove() {
  busy.value = true;
  try {
    await productsApi.remove(deleting.value.id);
    deleting.value = null;
    await load();
  } catch (e) {
    error.value = e.message;
  } finally {
    busy.value = false;
  }
}
</script>
<template>
  <div class="layout">
    <aside>
      <div class="brand"><span class="logo">S</span> Stockroom</div>
      <small>WORKSPACE / 庫存管理</small
      ><button
        class="nav active"
        @click="
          filter = 'all';
          page = 1;
        "
      >
        ▦　商品總覽</button
      ><button
        class="nav"
        @click="
          filter = 'low';
          page = 1;
        "
      >
        ◷　低庫存商品</button
      ><button
        class="nav"
        @click="
          filter = 'out';
          page = 1;
        "
      >
        □　缺貨商品
      </button>
      <div class="asidefoot">
        INVENTORY SYSTEM<br /><small>Vue 3 · Demo workspace</small>
      </div>
    </aside>
    <main>
      <header>
        <div>
          <div class="eyebrow">WAREHOUSE / PRODUCTS</div>
          <h1>商品庫存管理</h1>
          <p>管理商品資訊、追蹤庫存狀態與庫存價值。</p>
        </div>
        <button class="primary" @click="open()">＋ 新增商品</button>
      </header>
      <section class="stats">
        <article v-for="s in stats" :key="s.label">
          <span>{{ s.label }}</span
          ><strong
            >{{ s.value }} <small>{{ s.unit }}</small></strong
          >
        </article>
      </section>
      <div v-if="error" role="alert" class="error">{{ error }}</div>
      <section class="panel">
        <div class="panelhead">
          <h2>
            商品清單 <span>{{ filtered.length }} 筆</span>
          </h2>
          <div class="filters">
            <input
              v-model="query"
              @input="page = 1"
              placeholder="搜尋名稱或商品編號"
              aria-label="搜尋商品"
            /><select v-model="category" @change="page = 1" aria-label="商品分類">
              <option value="">所有分類</option>
              <option v-for="c in categories" :key="c">{{ c }}</option></select
            ><select v-model="filter" @change="page = 1" aria-label="庫存狀態">
              <option value="all">所有狀態</option>
              <option value="low">低庫存</option>
              <option value="out">缺貨</option>
            </select>
          </div>
        </div>
        <div class="tablewrap">
          <table>
            <thead>
              <tr>
                <th>商品編號</th>
                <th>商品名稱</th>
                <th>分類</th>
                <th>單價</th>
                <th>庫存數量</th>
                <th>狀態</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in visible" :key="p.id">
                <td class="sku">{{ p.sku }}</td>
                <td>
                  <b>{{ p.name }}</b>
                </td>
                <td>{{ p.category }}</td>
                <td>{{ money(p.price) }}</td>
                <td>{{ p.quantity }}</td>
                <td>
                  <span
                    class="badge"
                    :class="p.quantity === 0 ? 'out' : low(p) ? 'low' : 'ok'"
                    >{{ status(p) }}</span
                  >
                </td>
                <td class="actions">
                  <button @click="open(p)">編輯</button
                  ><button class="dangertext" @click="deleting = p">刪除</button>
                </td>
              </tr>
              <tr v-if="!visible.length">
                <td colspan="7" class="empty">沒有符合條件的商品</td>
              </tr>
            </tbody>
          </table>
        </div>
        <footer>
          <span>共 {{ filtered.length }} 項商品</span>
          <div>
            <button :disabled="current === 1" @click="page = current - 1">上一頁</button>
            {{ current }} / {{ pages }}
            <button :disabled="current === pages" @click="page = current + 1">
              下一頁
            </button>
          </div>
        </footer>
      </section>
      <p class="note">低庫存門檻：1–10 件；0 件為缺貨。庫存總值依商品單價估算。</p>
    </main>
  </div>
  <div v-if="modal" class="overlay" @click.self="!busy && (modal = false)">
    <section class="dialog" role="dialog" aria-modal="true" aria-label="商品表單">
      <h2>{{ form.id ? "編輯商品" : "新增商品" }}</h2>
      <form @submit.prevent="save">
        <label>商品編號<input v-model="form.sku" required /></label
        ><label>商品名稱<input v-model="form.name" required /></label
        ><label
          >分類<input v-model="form.category" required list="categories" /><datalist
            id="categories"
          >
            <option v-for="c in categories" :key="c" :value="c" /></datalist
        ></label>
        <div class="pair">
          <label
            >單價（NT$）<input
              type="number"
              v-model.number="form.price"
              min="0"
              step="1"
              required /></label
          ><label
            >庫存數量<input
              type="number"
              v-model.number="form.quantity"
              min="0"
              step="1"
              required
          /></label>
        </div>
        <p v-if="formError" class="error" role="alert">{{ formError }}</p>
        <div class="dialogactions">
          <button type="button" :disabled="busy" @click="modal = false">取消</button
          ><button class="primary" :disabled="busy">儲存商品</button>
        </div>
      </form>
    </section>
  </div>
  <div v-if="deleting" class="overlay" @click.self="!busy && (deleting = null)">
    <section class="dialog" role="dialog" aria-modal="true" aria-label="確認刪除">
      <h2>刪除商品？</h2>
      <p>確定刪除「{{ deleting.name }}」？此操作無法復原。</p>
      <div class="dialogactions">
        <button :disabled="busy" @click="deleting = null">取消</button
        ><button class="danger" :disabled="busy" @click="remove">確認刪除</button>
      </div>
    </section>
  </div>
</template>
