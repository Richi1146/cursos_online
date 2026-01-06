<script setup>
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

const isAuthenticated = computed(() => {
  // Use route to trigger re-computation
  return !!route.path && !!localStorage.getItem('token')
})

const userEmail = computed(() => {
  return localStorage.getItem('user_email')
})

const userInitial = computed(() => {
  const email = userEmail.value
  return email ? email.charAt(0).toUpperCase() : '?'
})

const logout = () => {
  localStorage.removeItem('token')
  localStorage.removeItem('user_email')
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-[#020617] font-sans text-slate-50 selection:bg-blue-500/30">
    <!-- Navbar -->
    <nav v-if="!route.meta.requiresGuest" class="sticky top-0 z-50 border-b border-white/5 bg-[#020617]/80 backdrop-blur-xl">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-20 items-center">
          <!-- Logo -->
          <div class="flex items-center gap-3 cursor-pointer group" @click="router.push('/')">
             <div class="w-10 h-10 bg-gradient-to-br from-blue-500 to-purple-600 rounded-xl flex items-center justify-center text-white font-black shadow-lg shadow-blue-500/20 group-hover:scale-110 transition-transform duration-300">
               OC
             </div>
             <div class="flex flex-col leading-tight">
               <span class="text-xl font-extrabold tracking-tight text-white">
                 Online<span class="text-blue-500">Courses</span>
               </span>
               <span class="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Learning Platform</span>
             </div>
          </div>

          <!-- User Actions -->
          <div class="flex items-center gap-6" v-if="isAuthenticated">
            <div class="flex items-center gap-3 px-4 py-2 bg-white/5 rounded-2xl border border-white/10 hover:bg-white/10 transition-colors cursor-default group">
              <div class="w-8 h-8 bg-gradient-to-tr from-blue-600 to-blue-400 rounded-lg flex items-center justify-center text-xs font-black text-white uppercase shadow-inner">
                {{ userInitial }}
              </div>
              <span class="text-sm font-semibold text-slate-300 hidden sm:block group-hover:text-white transition-colors">
                {{ userEmail }}
              </span>
            </div>
            
            <button @click="logout" class="group flex items-center gap-2 text-sm font-bold text-slate-400 hover:text-red-400 transition-all">
              <span class="px-3 py-2 rounded-xl group-hover:bg-red-500/10 transition-colors">Log out</span>
            </button>
          </div>
        </div>
      </div>
    </nav>
    
    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <router-view v-slot="{ Component }">
        <transition 
          enter-active-class="transition duration-300 ease-out"
          enter-from-class="transform translate-y-4 opacity-0"
          enter-to-class="transform translate-y-0 opacity-100"
          leave-active-class="transition duration-200 ease-in"
          leave-from-class="transform translate-y-0 opacity-100"
          leave-to-class="transform translate-y-4 opacity-0"
          mode="out-in"
        >
          <component :is="Component" />
        </transition>
      </router-view>
    </main>

    <!-- Background Decoration -->
    <div class="fixed top-0 left-1/2 -translate-x-1/2 w-full h-full -z-10 overflow-hidden pointer-events-none">
      <div class="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-blue-600/10 blur-[120px] rounded-full"></div>
      <div class="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] bg-purple-600/10 blur-[120px] rounded-full"></div>
    </div>
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
