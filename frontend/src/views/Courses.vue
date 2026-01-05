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
    console.error('Failed to fetch courses', error)
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
        fetchCourses() // Refresh
        // Optionally redirect to detail
        // router.push(`/courses/${response.data.id}`)
    } catch (e) {
        alert('Failed to create course')
    } finally {
        createLoading.value = false
    }
}

const deleteCourse = async (id) => {
    if(!confirm('Are you sure you want to delete this course?')) return
    try {
        await api.delete(`/courses/${id}`)
        fetchCourses()
    } catch(e) {
        alert('Failed to delete course')
    }
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
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-3xl font-bold tracking-tight text-slate-900">Courses</h1>
        <p class="mt-2 text-sm text-slate-500">Manage your online courses and lessons.</p>
      </div>
      <div class="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
        <button 
           @click="showCreateModal = true"
           class="block rounded-lg bg-blue-600 px-4 py-2.5 text-center text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600 transition-all"
        >
          New Course
        </button>
      </div>
    </div>

    <!-- Filters -->
    <div class="mt-8 flex flex-col sm:flex-row gap-4">
        <div class="relative flex-grow">
            <div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                <svg class="h-5 w-5 text-slate-400" viewBox="0 0 20 20" fill="currentColor">
                   <path fill-rule="evenodd" d="M9 3.5a5.5 5.5 0 100 11 5.5 5.5 0 000-11zM2 9a7 7 0 1112.452 4.391l3.328 3.329a.75.75 0 11-1.06 1.06l-3.329-3.328A7 7 0 012 9z" clip-rule="evenodd" />
                </svg>
            </div>
            <input 
                v-model.lazy="search"
                type="text" 
                class="block w-full rounded-md border-0 py-2 pl-10 text-slate-900 ring-1 ring-inset ring-slate-300 placeholder:text-slate-400 focus:ring-2 focus:ring-inset focus:ring-blue-600 sm:text-sm sm:leading-6 shadow-sm"
                placeholder="Search courses..."
            />
        </div>
        <div class="w-full sm:w-48">
            <select v-model="statusFilter" class="block w-full rounded-md border-0 py-2 text-slate-900 ring-1 ring-inset ring-slate-300 focus:ring-2 focus:ring-inset focus:ring-blue-600 sm:text-sm sm:leading-6 shadow-sm">
                <option value="">All Statuses</option>
                <option :value="0">Draft</option>
                <option :value="1">Published</option>
            </select>
        </div>
    </div>

    <!-- Grid -->
    <div v-if="loading" class="mt-8 flex justify-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
    </div>

    <div v-else class="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <div 
           v-for="course in courses" 
           :key="course.id" 
           class="relative flex flex-col bg-white rounded-xl shadow-[0_2px_15px_-3px_rgba(0,0,0,0.07),0_10px_20px_-2px_rgba(0,0,0,0.04)] ring-1 ring-slate-100 hover:shadow-lg transition-all duration-300 overflow-hidden group"
        >
           <div class="p-6 flex-1">
              <div class="flex justify-between items-start mb-4">
                   <span 
                     class="inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ring-1 ring-inset"
                     :class="getStatusColor(course.status)"
                   >
                     {{ getStatusLabel(course.status) }}
                   </span>
                   <div class="relative group-hover:opacity-100 opacity-0 transition-opacity">
                      <button @click.stop="deleteCourse(course.id)" class="text-slate-400 hover:text-red-500">
                         <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                           <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                         </svg>
                      </button>
                   </div>
              </div>
              <h3 class="text-xl font-semibold text-slate-900 group-hover:text-blue-600 transition-colors cursor-pointer" @click="router.push(`/courses/${course.id}`)">
                 {{ course.title }}
              </h3>
              <p class="mt-2 text-sm text-slate-500 line-clamp-2">
                 Created on {{ new Date(course.createdAt).toLocaleDateString() }}
              </p>
           </div>
           
           <div class="bg-slate-50 px-6 py-4 border-t border-slate-100 flex justify-between items-center">
              <button 
                @click="router.push(`/courses/${course.id}`)"
                class="text-sm font-semibold text-blue-600 hover:text-blue-800 transition-colors"
              >
                Manage &rarr;
              </button>
              <span class="text-xs text-slate-400">ID: {{ course.id.substring(0,8) }}...</span>
           </div>
        </div>
    </div>
    
    <!-- Empty State -->
    <div v-if="!loading && courses.length === 0" class="mt-12 text-center py-12 bg-white rounded-xl border border-dashed border-slate-300">
        <svg class="mx-auto h-12 w-12 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
        </svg>
        <h3 class="mt-2 text-sm font-semibold text-slate-900">No courses found</h3>
        <p class="mt-1 text-sm text-slate-500">Get started by creating a new course.</p>
        <div class="mt-6">
            <button @click="showCreateModal = true" class="inline-flex items-center rounded-md bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600">
                <svg class="-ml-0.5 mr-1.5 h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm.75-11.25a.75.75 0 00-1.5 0v2.5h-2.5a.75.75 0 000 1.5h2.5v2.5a.75.75 0 001.5 0v-2.5h2.5a.75.75 0 000-1.5h-2.5v-2.5z" clip-rule="evenodd" />
                </svg>
                Create Course
            </button>
        </div>
    </div>
    
    <!-- Modal -->
    <div v-if="showCreateModal" class="relative z-50" aria-labelledby="modal-title" role="dialog" aria-modal="true">
        <div class="fixed inset-0 bg-slate-900/30 backdrop-blur-sm transition-opacity" @click="showCreateModal = false"></div>
        <div class="fixed inset-0 z-10 w-screen overflow-y-auto">
            <div class="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
                <div class="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg" @click.stop>
                    <div class="bg-white px-4 pb-4 pt-5 sm:p-6 sm:pb-4">
                        <h3 class="text-base font-semibold leading-6 text-slate-900" id="modal-title">Create New Course</h3>
                        <div class="mt-4">
                            <label class="block text-sm font-medium text-slate-700">Course Title</label>
                            <input v-model="newCourseTitle" type="text" class="mt-1 block w-full rounded-md border-0 py-2 px-3 text-slate-900 shadow-sm ring-1 ring-inset ring-slate-300 focus:ring-2 focus:ring-inset focus:ring-blue-600 sm:text-sm sm:leading-6" placeholder="e.g. Advanced Vue.js Patterns">
                        </div>
                    </div>
                    <div class="bg-slate-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
                        <button @click="createCourse" :disabled="createLoading || !newCourseTitle.trim()" type="button" class="inline-flex w-full justify-center rounded-md bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 sm:ml-3 sm:w-auto disabled:opacity-70">
                            {{ createLoading ? 'Creating...' : 'Create' }}
                        </button>
                        <button @click="showCreateModal = false" type="button" class="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-slate-900 shadow-sm ring-1 ring-inset ring-slate-300 hover:bg-slate-50 sm:mt-0 sm:w-auto">Cancel</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>
