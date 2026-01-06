<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '../services/api'

const router = useRouter()
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)
const error = ref('')

const handleRegister = async () => {
  if (password.value !== confirmPassword.value) {
    error.value = 'Passwords do not match.'
    return
  }

  loading.value = true
  error.value = ''
  
  try {
    await api.post('/auth/register', {
      email: email.value,
      password: password.value
    })
    
    // Redirect to login after successful registration
    router.push('/login')
  } catch (err) {
    if (err.response && err.response.data && err.response.data.message) {
      error.value = err.response.data.message
    } else {
      error.value = 'Failed to register. Please try again.'
    }
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="flex items-center justify-center min-h-[calc(100vh-160px)]">
    <div class="w-full max-w-md">
      <!-- Header -->
      <div class="text-center mb-10">
        <h1 class="text-4xl font-black text-white mb-3 tracking-tight">
          Join the <span class="text-blue-500">Future</span>
        </h1>
        <p class="text-slate-400 font-medium">Start your learning journey today.</p>
      </div>

      <!-- Register Card -->
      <div class="glass-card p-8 sm:p-10 shadow-2xl shadow-blue-500/5">
        <form @submit.prevent="handleRegister" class="space-y-6">
          <div v-if="error" class="p-4 bg-red-500/10 border border-red-500/20 rounded-xl text-red-400 text-sm font-medium animate-shake">
            {{ error }}
          </div>

          <div class="space-y-2">
            <label class="text-sm font-bold text-slate-300 ml-1">Email Address</label>
            <input 
              v-model="email" 
              type="email" 
              required 
              class="input-premium"
              placeholder="name@example.com"
            />
          </div>

          <div class="space-y-2">
            <label class="text-sm font-bold text-slate-300 ml-1">Password</label>
            <input 
              v-model="password" 
              type="password" 
              required 
              class="input-premium"
              placeholder="••••••••"
            />
          </div>

          <div class="space-y-2">
            <label class="text-sm font-bold text-slate-300 ml-1">Confirm Password</label>
            <input 
              v-model="confirmPassword" 
              type="password" 
              required 
              class="input-premium"
              placeholder="••••••••"
            />
          </div>

          <button 
            type="submit" 
            :disabled="loading"
            class="btn-premium w-full flex items-center justify-center gap-2 group"
          >
            <span v-if="loading" class="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
            <span v-else>Create Account</span>
            <svg v-if="!loading" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 group-hover:translate-x-1 transition-transform" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10.293 3.293a1 1 0 011.414 0l6 6a1 1 0 010 1.414l-6 6a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-4.293-4.293a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
        </form>

        <div class="mt-8 pt-8 border-t border-white/5 text-center">
          <p class="text-slate-400 text-sm font-medium">
            Already have an account? 
            <router-link to="/login" class="text-blue-400 font-bold hover:text-blue-300 transition-colors ml-1">
              Sign in
            </router-link>
          </p>
        </div>
      </div>
      
      <!-- Footer Info -->
      <p class="text-center mt-8 text-slate-600 text-xs font-bold uppercase tracking-[0.2em]">
        Join +10,000 Students
      </p>
    </div>
  </div>
</template>
