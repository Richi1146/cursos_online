<script setup>
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

const isAuthenticated = computed(() => {
  return !!localStorage.getItem('token')
})

const userEmail = computed(() => {
  return localStorage.getItem('user_email')
})

const logout = () => {
  localStorage.removeItem('token')
  localStorage.removeItem('user_email')
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 font-sans text-slate-900">
    <nav v-if="!route.meta.requiresGuest" class="bg-white/80 backdrop-blur-md sticky top-0 z-50 border-b border-slate-200">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-16 items-center">
          <div class="flex items-center gap-2 cursor-pointer" @click="router.push('/')">
             <div class="w-8 h-8 bg-gradient-to-tr from-blue-600 to-indigo-500 rounded-lg flex items-center justify-center text-white font-bold">
               OC
             </div>
             <span class="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-blue-700 to-indigo-600">
               OnlineCourses
             </span>
          </div>
          <div class="flex items-center gap-4" v-if="isAuthenticated">
            <span class="text-sm text-slate-500 hidden sm:block">
              {{ userEmail }}
            </span>
            <button @click="logout" class="text-sm font-medium text-slate-600 hover:text-red-600 transition-colors">
              Log out
            </button>
          </div>
        </div>
      </div>
    </nav>
    
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
  </div>
</template>

<style>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
