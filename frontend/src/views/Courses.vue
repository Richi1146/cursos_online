<script setup>
import { ref, onMounted, watch } from 'vue'
import api from '../services/api'
import { useRouter } from 'vue-router'

const router = useRouter()
const courses = ref([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(9) // 3x3 grid
const search = ref('')
const statusFilter = ref('') // '' (all), '0' (draft), '1' (published)
const loading = ref(false)

const showCreateModal = ref(false)
const newCourseTitle = ref('')
const createLoading = ref(false)

// Custom UI State
const notification = ref({ show: false, message: '', type: 'error' })
const confirmModal = ref({ show: false, title: '', message: '', action: null })

const showNotify = (message, type = 'error') => {
    notification.value = { show: true, message, type }
    setTimeout(() => { notification.value.show = false }, 4000)
}

const askConfirm = (title, message, action) => {
    confirmModal.value = { show: true, title, message, action }
}

const fetchCourses = async () => {
  loading.value = true
  try {
    const params = {
        page: page.value,
        pageSize: pageSize.value,
        q: search.value
    }
    if (statusFilter.value !== '') {
        params.status = statusFilter.value
    }
    
    const response = await api.get('/courses/search', { params })
    courses.value = response.data.items
    totalCount.value = response.data.totalCount
  } catch (error) {
    showNotify('Failed to fetch courses')
  } finally {
    loading.value = false
  }
}

const createCourse = async () => {
    if(!newCourseTitle.value.trim()) return
    createLoading.value = true
    try {
        const response = await api.post('/courses', { title: newCourseTitle.value })
        showCreateModal.value = false
        newCourseTitle.value = ''
        showNotify('Course created successfully!', 'success')
        fetchCourses() // Refresh
    } catch (e) {
        showNotify('Failed to create course')
    } finally {
        createLoading.value = false
    }
}

const deleteCourse = async (id) => {
    askConfirm(
        'Delete Course',
        'Are you sure you want to delete this course? All associated lessons will be removed.',
        async () => {
            try {
                await api.delete(`/courses/${id}`)
                showNotify('Course deleted', 'success')
                fetchCourses()
            } catch(e) {
                showNotify('Failed to delete course')
            }
        }
    )
}

watch([page, search, statusFilter], () => {
    fetchCourses()
})

const getStatusColor = (status) => {
    // 0: Draft, 1: Published
    return status === 1 
        ? 'bg-green-100 text-green-700 ring-green-600/20' 
        : 'bg-slate-100 text-slate-700 ring-slate-600/20'
}

const getStatusLabel = (status) => {
    return status === 1 ? 'Published' : 'Draft'
}

onMounted(() => {
    fetchCourses()
})
</script>

<template>
  <div>
    <!-- Hero Section -->
    <div class="relative mb-16">
      <div class="sm:flex sm:items-center sm:justify-between">
        <div class="sm:flex-auto">
          <h1 class="text-5xl font-black tracking-tight text-white mb-4">
            Explore <span class="text-gradient">Knowledge</span>
          </h1>
          <p class="text-lg text-slate-400 font-medium max-w-2xl">
            Master new skills with our professional courses. Learn from experts and build your future today.
          </p>
        </div>
        <div class="mt-8 sm:ml-16 sm:mt-0 sm:flex-none">
          <button 
             @click="showCreateModal = true"
             class="btn-premium flex items-center gap-2"
          >
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
            </svg>
            Create Course
          </button>
        </div>
      </div>
    </div>

    <!-- Filters & Search -->
    <div class="flex flex-col sm:flex-row gap-4 mb-12">
        <div class="relative flex-grow group">
            <div class="absolute inset-y-0 left-0 flex items-center pl-5 pointer-events-none">
                <svg class="h-5 w-5 text-slate-500 group-focus-within:text-blue-500 transition-colors" viewBox="0 0 20 20" fill="currentColor">
                   <path fill-rule="evenodd" d="M9 3.5a5.5 5.5 0 100 11 5.5 5.5 0 000-11zM2 9a7 7 0 1112.452 4.391l3.328 3.329a.75.75 0 11-1.06 1.06l-3.329-3.328A7 7 0 012 9z" clip-rule="evenodd" />
                </svg>
            </div>
            <input 
                v-model.lazy="search"
                type="text" 
                class="input-premium pl-16"
                placeholder="Search for courses, topics, or skills..."
            />
        </div>
        <div class="w-full sm:w-64 relative">
            <select v-model="statusFilter" class="input-premium appearance-none cursor-pointer pr-10">
                <option value="" class="bg-slate-900 text-slate-100">All Statuses</option>
                <option :value="0" class="bg-slate-900 text-slate-100">Draft</option>
                <option :value="1" class="bg-slate-900 text-slate-100">Published</option>
            </select>
            <div class="absolute inset-y-0 right-0 flex items-center pr-4 pointer-events-none text-slate-500">
                <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.5a.75.75 0 01-1.08 0l-4.25-4.5a.75.75 0 01.02-1.06z" clip-rule="evenodd" />
                </svg>
            </div>
        </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex flex-col items-center justify-center py-24">
        <div class="w-16 h-16 border-4 border-blue-500/20 border-t-blue-500 rounded-full animate-spin mb-4"></div>
        <p class="text-slate-500 font-bold uppercase tracking-widest text-xs">Loading Courses...</p>
    </div>

    <!-- Course Grid -->
    <div v-else class="grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-3">
        <div 
           v-for="course in courses" 
           :key="course.id" 
           class="glass-card group flex flex-col h-full overflow-hidden"
        >
           <!-- Course Image Placeholder / Decoration -->
           <div class="h-48 w-full bg-gradient-to-br from-slate-800 to-slate-900 relative overflow-hidden">
              <div class="absolute inset-0 opacity-20 group-hover:opacity-30 transition-opacity">
                 <div class="absolute top-0 left-0 w-full h-full bg-[radial-gradient(circle_at_center,_var(--tw-gradient-stops))] from-blue-500/20 via-transparent to-transparent"></div>
              </div>
              <div class="absolute top-4 left-4">
                 <span 
                   class="px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-wider border"
                   :class="course.status === 1 ? 'bg-green-500/10 text-green-400 border-green-500/20' : 'bg-slate-500/10 text-slate-400 border-slate-500/20'"
                 >
                   {{ getStatusLabel(course.status) }}
                 </span>
              </div>
              <div class="absolute inset-0 flex items-center justify-center">
                 <div class="w-16 h-16 bg-white/5 rounded-2xl flex items-center justify-center backdrop-blur-md border border-white/10 group-hover:scale-110 transition-transform duration-500">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                    </svg>
                 </div>
              </div>
           </div>

           <div class="p-6 flex-1 flex flex-col">
              <h3 class="text-xl font-bold text-white mb-3 group-hover:text-blue-400 transition-colors line-clamp-2 leading-tight">
                 {{ course.title }}
              </h3>
              <p class="text-sm text-slate-400 font-medium mb-6 line-clamp-2">
                 Master the fundamentals and advanced concepts of {{ course.title.split(' ')[0] }}.
              </p>
              
              <div class="mt-auto pt-6 border-t border-white/5 flex items-center justify-between">
                 <div class="flex items-center gap-2">
                    <div class="w-6 h-6 rounded-full bg-blue-500/20 flex items-center justify-center">
                       <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3 text-blue-400" viewBox="0 0 20 20" fill="currentColor">
                          <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z" clip-rule="evenodd" />
                       </svg>
                    </div>
                    <span class="text-[10px] font-bold text-slate-500 uppercase tracking-widest">
                       {{ new Date(course.createdAt).toLocaleDateString() }}
                    </span>
                 </div>
                 
                 <div class="flex items-center gap-3">
                    <button @click.stop="deleteCourse(course.id)" class="p-2 text-slate-600 hover:text-red-400 hover:bg-red-400/10 rounded-lg transition-all opacity-0 group-hover:opacity-100">
                       <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                         <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                       </svg>
                    </button>
                    <button 
                      @click="router.push(`/courses/${course.id}`)"
                      class="px-4 py-2 bg-blue-500/10 text-blue-400 text-xs font-black uppercase tracking-widest rounded-lg hover:bg-blue-500 hover:text-white transition-all"
                    >
                      Enter
                    </button>
                 </div>
              </div>
           </div>
        </div>
    </div>
    
    <!-- Empty State -->
    <div v-if="!loading && courses.length === 0" class="mt-12 text-center py-24 glass-card border-dashed border-white/10">
        <div class="w-20 h-20 bg-white/5 rounded-3xl flex items-center justify-center mx-auto mb-6">
           <svg class="h-10 w-10 text-slate-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
               <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
           </svg>
        </div>
        <h3 class="text-2xl font-bold text-white mb-2">No courses found</h3>
        <p class="text-slate-400 font-medium mb-8">Ready to share your knowledge with the world?</p>
        <button @click="showCreateModal = true" class="btn-premium inline-flex items-center gap-2">
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm.75-11.25a.75.75 0 00-1.5 0v2.5h-2.5a.75.75 0 000 1.5h2.5v2.5a.75.75 0 001.5 0v-2.5h2.5a.75.75 0 000-1.5h-2.5v-2.5z" clip-rule="evenodd" />
            </svg>
            Create Your First Course
        </button>
    </div>
    
    <!-- Modal -->
    <div v-if="showCreateModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-[#020617]/80 backdrop-blur-md" @click="showCreateModal = false"></div>
        <div class="relative glass-card w-full max-w-lg p-8 shadow-2xl animate-in zoom-in-95 duration-300" @click.stop>
            <div class="flex justify-between items-center mb-8">
               <h3 class="text-2xl font-black text-white tracking-tight">New <span class="text-blue-500">Course</span></h3>
               <button @click="showCreateModal = false" class="text-slate-500 hover:text-white transition-colors">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
               </button>
            </div>
            
            <div class="space-y-6">
                <div class="space-y-2">
                    <label class="text-sm font-bold text-slate-300 ml-1">Course Title</label>
                    <input 
                      v-model="newCourseTitle" 
                      type="text" 
                      class="input-premium" 
                      placeholder="e.g. Advanced Vue.js Patterns"
                      @keyup.enter="createCourse"
                    >
                </div>
                
                <div class="flex gap-3 pt-4">
                    <button @click="showCreateModal = false" class="flex-1 px-6 py-3 rounded-xl font-bold text-slate-400 hover:bg-white/5 transition-all">
                       Cancel
                    </button>
                    <button 
                      @click="createCourse" 
                      :disabled="createLoading || !newCourseTitle.trim()" 
                      class="flex-1 btn-premium"
                    >
                        {{ createLoading ? 'Creating...' : 'Create Course' }}
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- Notifications -->
    <Transition
        enter-active-class="transform ease-out duration-300 transition"
        enter-from-class="translate-y-2 opacity-0 sm:translate-y-0 sm:translate-x-2"
        enter-to-class="translate-y-0 opacity-100 sm:translate-x-0"
        leave-active-class="transition ease-in duration-100"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
     >
        <div v-if="notification.show" class="fixed bottom-8 right-8 z-[200] w-full max-w-sm">
            <div class="glass-card !bg-slate-900/90 border-l-4 p-4 shadow-2xl" :class="notification.type === 'success' ? 'border-green-500' : 'border-red-500'">
                <div class="flex items-center gap-3">
                    <div v-if="notification.type === 'success'" class="text-green-500">
                        <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                    </div>
                    <div v-else class="text-red-500">
                        <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                    </div>
                    <p class="text-sm font-bold text-white">{{ notification.message }}</p>
                </div>
            </div>
        </div>
     </Transition>

     <!-- Confirm Modal -->
     <div v-if="confirmModal.show" class="fixed inset-0 z-[150] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-[#020617]/80 backdrop-blur-md" @click="confirmModal.show = false"></div>
        <div class="relative glass-card w-full max-w-md p-8 shadow-2xl animate-in zoom-in-95 duration-300" @click.stop>
            <h3 class="text-2xl font-black text-white tracking-tight mb-4">{{ confirmModal.title }}</h3>
            <p class="text-slate-400 font-medium mb-8">{{ confirmModal.message }}</p>
            <div class="flex gap-3">
                <button @click="confirmModal.show = false" class="flex-1 px-6 py-3 rounded-xl font-bold text-slate-400 hover:bg-white/5 transition-all">
                    Cancel
                </button>
                <button 
                  @click="() => { confirmModal.action(); confirmModal.show = false; }" 
                  class="flex-1 bg-red-500 hover:bg-red-600 text-white rounded-xl font-bold transition-all shadow-lg shadow-red-500/20"
                >
                    Confirm
                </button>
            </div>
        </div>
     </div>
  </div>
</template>
