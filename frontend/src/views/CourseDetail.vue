<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '../services/api'

const route = useRoute()
const router = useRouter()
const courseId = route.params.id

const course = ref(null)
const summary = ref(null)
const lessons = ref([])
const loading = ref(true)

const showAddLesson = ref(false)
const newLessonTitle = ref('')
const newLessonOrder = ref(1)

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

// Edit Course
const isEditing = ref(false)
const editTitle = ref('')

const fetchData = async () => {
    loading.value = true
    try {
        const [cRes, sRes, lRes] = await Promise.all([
            api.get(`/courses/${courseId}`),
            api.get(`/courses/${courseId}/summary`),
            api.get(`/lessons/course/${courseId}`)
        ])
        course.value = cRes.data
        summary.value = sRes.data
        lessons.value = lRes.data.sort((a, b) => a.order - b.order)
        editTitle.value = course.value.title
        
        // Auto set next order
        if(lessons.value.length > 0) {
            newLessonOrder.value = Math.max(...lessons.value.map(l => l.order)) + 1
        }
    } catch (e) {
        if(e.response && e.response.status === 404) {
            showNotify('Course not found')
            router.push('/courses')
        }
    } finally {
        loading.value = false
    }
}

const updateCourse = async () => {
    try {
        await api.put(`/courses/${courseId}`, { title: editTitle.value })
        course.value.title = editTitle.value
        isEditing.value = false
        showNotify('Course updated successfully', 'success')
    } catch(e) {
        showNotify('Failed to update course')
    }
}

const togglePublish = async () => {
    try {
        if(course.value.status === 0) {
             await api.patch(`/courses/${courseId}/publish`)
             course.value.status = 1
             showNotify('Course published!', 'success')
        } else {
             await api.patch(`/courses/${courseId}/unpublish`)
             course.value.status = 0
             showNotify('Course unpublished', 'success')
        }
    } catch(e) {
        showNotify(e.response?.data?.message || 'Failed to update status')
    }
}

const deleteCourse = async () => {
    askConfirm(
        'Delete Course', 
        'Are you sure? This action cannot be undone and all lessons will be lost.',
        async () => {
            try {
                await api.delete(`/courses/${courseId}`)
                router.push('/courses')
            } catch(e) {
                showNotify('Failed to delete course')
            }
        }
    )
}

const addLesson = async () => {
    if(!newLessonTitle.value.trim()) return
    try {
        await api.post('/lessons', {
            courseId: courseId,
            title: newLessonTitle.value,
            order: parseInt(newLessonOrder.value)
        })
        showAddLesson.value = false
        newLessonTitle.value = ''
        showNotify('Lesson added', 'success')
        fetchData() // Refresh to get correct order and lists
    } catch (e) {
         showNotify(e.response?.data?.message || 'Failed to add lesson')
    }
}

const deleteLesson = async (id) => {
    askConfirm(
        'Delete Lesson',
        'Are you sure you want to remove this lesson?',
        async () => {
            try {
                await api.delete(`/lessons/${id}`)
                showNotify('Lesson deleted', 'success')
                fetchData()
            } catch(e) {
                showNotify('Failed to delete lesson')
            }
        }
    )
}

const moveLesson = async (index, direction) => {
    // direction: -1 (up), 1 (down)
    if (index + direction < 0 || index + direction >= lessons.value.length) return

    const current = lessons.value[index]
    const target = lessons.value[index + direction]
    
    // Swap orders
    const tempOrder = current.order
    current.order = target.order
    target.order = tempOrder
    
    // Update local list to reflect swap visually immediately (optimistic)
    lessons.value.sort((a,b) => a.order - b.order) // Resort
    
    // Send full reorder request
    const payload = lessons.value.map(l => ({
        lessonId: l.id,
        newOrder: l.order
    }))
    
    try {
        await api.post('/lessons/reorder', payload, { params: { courseId } })
    } catch(e) {
        showNotify('Failed to reorder lessons')
        fetchData() // Revert
    }
}

onMounted(() => {
    fetchData()
})
</script>

<template>
  <div v-if="loading" class="flex flex-col items-center justify-center py-24">
      <div class="w-16 h-16 border-4 border-blue-500/20 border-t-blue-500 rounded-full animate-spin mb-4"></div>
      <p class="text-slate-500 font-bold uppercase tracking-widest text-xs">Loading Course Details...</p>
  </div>
  <div v-else-if="course">
     <!-- Course Header Card -->
     <div class="glass-card p-8 mb-12 relative overflow-hidden">
        <div class="absolute top-0 right-0 w-64 h-64 bg-blue-500/5 blur-[80px] rounded-full -mr-32 -mt-32"></div>
        
        <div class="relative flex flex-col md:flex-row md:items-center justify-between gap-8">
            <div class="flex-1">
                <div class="flex items-center gap-4 mb-4">
                    <span 
                        class="px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-wider border"
                        :class="course.status === 1 ? 'bg-green-500/10 text-green-400 border-green-500/20' : 'bg-slate-500/10 text-slate-400 border-slate-500/20'"
                    >
                        {{ course.status === 1 ? 'Published' : 'Draft' }}
                    </span>
                    <span class="text-slate-500 text-xs font-bold uppercase tracking-widest">
                       Updated {{ new Date(course.updatedAt).toLocaleDateString() }}
                    </span>
                </div>
                
                <div v-if="!isEditing" class="flex items-center gap-4 group">
                    <h1 class="text-4xl font-black text-white tracking-tight leading-tight">{{ course.title }}</h1>
                    <button @click="isEditing = true" class="p-2 text-slate-500 hover:text-blue-400 hover:bg-white/5 rounded-xl transition-all opacity-0 group-hover:opacity-100">
                        <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" /></svg>
                    </button>
                </div>
                <div v-else class="flex flex-col sm:flex-row gap-4">
                    <input v-model="editTitle" class="input-premium text-2xl font-bold !bg-white/10" autoFocus />
                    <div class="flex gap-2">
                       <button @click="updateCourse" class="btn-premium !py-2 !px-4 text-sm">Save</button>
                       <button @click="isEditing = false" class="px-4 py-2 rounded-xl font-bold text-slate-400 hover:bg-white/5 transition-all text-sm">Cancel</button>
                    </div>
                </div>
            </div>
            
            <div class="flex items-center gap-4">
                <button 
                    @click="togglePublish"
                    class="btn-premium !from-slate-800 !to-slate-900 border border-white/10 hover:!from-blue-600 hover:!to-indigo-600 !shadow-none hover:shadow-lg"
                    :class="{ '!from-green-600 !to-emerald-600': course.status === 0 }"
                >
                    {{ course.status === 1 ? 'Unpublish' : 'Publish Now' }}
                </button>
                <button @click="deleteCourse" class="p-3 text-slate-500 hover:text-red-400 hover:bg-red-400/10 rounded-xl transition-all" title="Delete Course">
                    <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                </button>
            </div>
        </div>
        
        <div class="mt-10 flex gap-12 border-t border-white/5 pt-8">
             <div class="flex flex-col">
                 <span class="text-3xl font-black text-white">{{ lessons.length }}</span>
                 <span class="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em]">Lessons</span>
             </div>
             <div class="flex flex-col">
                 <span class="text-3xl font-black text-white">0</span>
                 <span class="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em]">Students</span>
             </div>
             <div class="flex flex-col">
                 <span class="text-3xl font-black text-white">0%</span>
                 <span class="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em]">Completion</span>
             </div>
        </div>
     </div>
     
     <!-- Lessons Section -->
     <div class="glass-card overflow-hidden">
        <div class="p-8 border-b border-white/5 flex justify-between items-center bg-white/[0.01]">
            <div>
               <h2 class="text-2xl font-black text-white tracking-tight">Curriculum</h2>
               <p class="text-sm text-slate-500 font-medium">Manage and reorder your course content.</p>
            </div>
            <button @click="showAddLesson = true" class="btn-premium !py-2.5 !px-5 !text-xs flex items-center gap-2">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                  <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
                </svg>
                Add Lesson
            </button>
        </div>
        
        <div v-if="lessons.length === 0" class="p-20 text-center">
            <div class="w-20 h-20 bg-white/5 rounded-3xl flex items-center justify-center mx-auto mb-6">
               <svg class="h-10 w-10 text-slate-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                   <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
               </svg>
            </div>
            <h3 class="text-xl font-bold text-white mb-2">Your curriculum is empty</h3>
            <p class="text-slate-500 font-medium">Add your first lesson to start building your course.</p>
        </div>
        
        <div v-else class="divide-y divide-white/5">
            <div v-for="(lesson, index) in lessons" :key="lesson.id" class="p-6 hover:bg-white/[0.02] flex items-center justify-between group transition-all">
                <div class="flex items-center gap-6">
                    <div class="w-10 h-10 rounded-xl bg-white/5 text-blue-400 flex items-center justify-center font-black text-sm border border-white/5 group-hover:border-blue-500/30 transition-colors">
                        {{ lesson.order }}
                    </div>
                    <div>
                       <span class="block font-bold text-white text-lg group-hover:text-blue-400 transition-colors">{{ lesson.title }}</span>
                       <span class="text-xs font-bold text-slate-600 uppercase tracking-widest">Video Lesson • 10:00</span>
                    </div>
                </div>
                
                <div class="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-all translate-x-4 group-hover:translate-x-0">
                    <div class="flex bg-white/5 rounded-xl p-1 border border-white/5">
                       <button 
                           @click="moveLesson(index, -1)" 
                           :disabled="index === 0"
                           class="p-2 text-slate-500 hover:text-blue-400 disabled:opacity-20 transition-colors"
                       >
                           <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 15l7-7 7 7" /></svg>
                       </button>
                       <button 
                           @click="moveLesson(index, 1)"
                           :disabled="index === lessons.length - 1"
                           class="p-2 text-slate-500 hover:text-blue-400 disabled:opacity-20 transition-colors"
                       >
                           <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M19 9l-7 7-7-7" /></svg>
                       </button>
                    </div>
                    
                    <button @click="deleteLesson(lesson.id)" class="p-3 text-slate-600 hover:text-red-400 hover:bg-red-400/10 rounded-xl transition-all">
                       <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                    </button>
                </div>
            </div>
        </div>
     </div>
     
     <!-- Add Lesson Modal -->
     <div v-if="showAddLesson" class="fixed inset-0 z-[100] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-[#020617]/80 backdrop-blur-md" @click="showAddLesson = false"></div>
        <div class="relative glass-card w-full max-w-md p-8 shadow-2xl animate-in zoom-in-95 duration-300" @click.stop>
            <div class="flex justify-between items-center mb-8">
               <h3 class="text-2xl font-black text-white tracking-tight">New <span class="text-blue-500">Lesson</span></h3>
               <button @click="showAddLesson = false" class="text-slate-500 hover:text-white transition-colors">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
               </button>
            </div>

            <div class="space-y-6">
                <div class="grid grid-cols-3 gap-4">
                   <div class="col-span-1 space-y-2">
                      <label class="text-sm font-bold text-slate-300 ml-1">Order</label>
                      <input v-model="newLessonOrder" type="number" class="input-premium text-center" />
                   </div>
                   <div class="col-span-2 space-y-2">
                      <label class="text-sm font-bold text-slate-300 ml-1">Lesson Title</label>
                      <input v-model="newLessonTitle" type="text" class="input-premium" placeholder="Introduction to..." autoFocus @keyup.enter="addLesson" />
                   </div>
                </div>
                
                <div class="flex gap-3 pt-4">
                    <button @click="showAddLesson = false" class="flex-1 px-6 py-3 rounded-xl font-bold text-slate-400 hover:bg-white/5 transition-all">
                       Cancel
                    </button>
                    <button 
                      @click="addLesson" 
                      :disabled="!newLessonTitle" 
                      class="flex-1 btn-premium"
                    >
                        Add Lesson
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
