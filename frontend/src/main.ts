import './assets/main.css'

import 'element-plus/dist/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'

import { createApp } from 'vue'

import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'

import App from './App.vue'
import router from './router'

const app = createApp(App)

const piniaStore = createPinia()
piniaStore.use(piniaPluginPersistedstate)
app.use(piniaStore)

app.use(router)

app.mount('#app')
