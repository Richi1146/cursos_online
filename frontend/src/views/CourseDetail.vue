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
            alert('Course not found')
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
    } catch(e) {
        alert('Failed to update course')
    }
}

const togglePublish = async () => {
    try {
        if(course.value.status === 0) {
             await api.patch(`/courses/${courseId}/publish`)
             course.value.status = 1
        } else {
             await api.patch(`/courses/${courseId}/unpublish`)
             course.value.status = 0
        }
    } catch(e) {
        alert(e.response?.data?.message || 'Failed to update status')
    }
}

const deleteCourse = async () => {
    if(!confirm('Are you sure? This action cannot be undone.')) return
    try {
        await api.delete(`/courses/${courseId}`)
        router.push('/courses')
    } catch(e) {
        alert('Failed to delete course')
    }
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
        fetchData() // Refresh to get correct order and lists
    } catch (e) {
         alert(e.response?.data?.message || 'Failed to add lesson')
    }
}

const deleteLesson = async (id) => {
    if(!confirm('Delete this lesson?')) return
    try {
        await api.delete(`/lessons/${id}`)
        fetchData()
    } catch(e) {
        alert('Failed to delete lesson')
    }
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
        alert('Failed to reorder')
        fetchData() // Revert
    }
}

onMounted(() => {
    fetchData()
})
</script>

<template>
  <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>
  <div v-else-if="course">
     <!-- Header -->
     <div class="bg-white rounded-xl shadow-sm border border-slate-100 p-6 mb-6">
        <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div class="flex-1">
                <div class="flex items-center gap-3 mb-2">
                    <span 
                        class="px-2.5 py-0.5 rounded-full text-xs font-semibold"
                        :class="course.status === 1 ? 'bg-green-100 text-green-700' : 'bg-slate-100 text-slate-700'"
                    >
                        {{ course.status === 1 ? 'Published' : 'Draft' }}
                    </span>
                    <span class="text-slate-400 text-sm">Last updated: {{ new Date(course.updatedAt).toLocaleDateString() }}</span>
                </div>
                
                <div v-if="!isEditing" class="flex items-center gap-2 group">
                    <h1 class="text-3xl font-bold text-slate-900">{{ course.title }}</h1>
                    <button @click="isEditing = true" class="text-slate-400 hover:text-blue-600 opacity-0 group-hover:opacity-100 transition-opacity">
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" /></svg>
                    </button>
                </div>
                <div v-else class="flex gap-2">
                    <input v-model="editTitle" class="text-3xl font-bold text-slate-900 border-b-2 border-blue-500 focus:outline-none w-full" autoFocus />
                    <button @click="updateCourse" class="text-blue-600 font-semibold px-3 py-1 bg-blue-50 rounded text-sm">Save</button>
                    <button @click="isEditing = false" class="text-slate-500 hover:text-slate-700 px-2">Cancel</button>
                </div>
            </div>
            
            <div class="flex items-center gap-3">
                <button 
                    @click="togglePublish"
                    class="px-4 py-2 rounded-lg font-semibold text-sm transition-colors border"
                    :class="course.status === 1 ? 'border-red-200 text-red-600 hover:bg-red-50' : 'bg-green-600 text-white hover:bg-green-700 border-transparent'"
                >
                    {{ course.status === 1 ? 'Unpublish' : 'Publish Course' }}
                </button>
                <button @click="deleteCourse" class="px-4 py-2 rounded-lg font-semibold text-xs text-red-600 hover:bg-red-50">
                    Delete
                </button>
            </div>
        </div>
        
        <div class="mt-6 flex gap-6 text-sm text-slate-600 border-t pt-4">
             <div>
                 <span class="block text-xl font-bold text-slate-900">{{ lessons.length }}</span>
                 <span>Lessons</span>
             </div>
             <!-- Metrics placeholder -->
             <div>
                <span class="block text-xl font-bold text-slate-900">0</span>
                <span>Students</span>
             </div>
        </div>
     </div>
     
     <!-- Lessons -->
     <div class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
        <div class="p-6 border-b border-slate-100 flex justify-between items-center bg-slate-50/50">
            <h2 class="text-lg font-bold text-slate-900">Lessons Content</h2>
            <button @click="showAddLesson = true" class="text-sm bg-white border border-slate-200 shadow-sm px-3 py-1.5 rounded-md font-medium text-slate-700 hover:bg-slate-50">
                + Add Lesson
            </button>
        </div>
        
        <div v-if="lessons.length === 0" class="p-12 text-center text-slate-500">
            No lessons yet. Add one to publish the course.
        </div>
        
        <ul v-else class="divide-y divide-slate-100">
            <li v-for="(lesson, index) in lessons" :key="lesson.id" class="p-4 hover:bg-slate-50 flex items-center justify-between group transition-colors">
                <div class="flex items-center gap-4">
                    <span class="w-8 h-8 rounded-full bg-slate-100 text-slate-500 flex items-center justify-center font-mono text-xs font-bold">
                        {{ lesson.order }}
                    </span>
                    <span class="font-medium text-slate-900">{{ lesson.title }}</span>
                </div>
                
                <div class="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button 
                        @click="moveLesson(index, -1)" 
                        :disabled="index === 0"
                        class="p-1 text-slate-400 hover:text-blue-600 disabled:opacity-30"
                        title="Move Up"
                    >
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" /></svg>
                    </button>
                    <button 
                        @click="moveLesson(index, 1)"
                        :disabled="index === lessons.length - 1"
                        class="p-1 text-slate-400 hover:text-blue-600 disabled:opacity-30"
                        title="Move Down"
                    >
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                    </button>
                    <div class="w-px h-4 bg-slate-200 mx-2"></div>
                    <button @click="deleteLesson(lesson.id)" class="text-red-500 hover:text-red-700 text-xs font-semibold px-2">Delete</button>
                </div>
            </li>
        </ul>
     </div>
     
     <!-- Add Lesson Modal -->
     <div v-if="showAddLesson" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-sm p-4" @click="showAddLesson = false">
        <div class="bg-white rounded-xl shadow-xl w-full max-w-md p-6" @click.stop>
            <h3 class="text-lg font-bold text-slate-900 mb-4">Add New Lesson</h3>
            <div class="space-y-4">
                <div>
                   <label class="block text-sm font-medium text-slate-700">Order</label>
                   <input v-model="newLessonOrder" type="number" class="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border p-2" />
                </div>
                <div>
                   <label class="block text-sm font-medium text-slate-700">Lesson Title</label>
                   <input v-model="newLessonTitle" type="text" class="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border p-2" placeholder="Introduction to..." autoFocus />
                </div>
            </div>
            <div class="mt-6 flex justify-end gap-3">
                <button @click="showAddLesson = false" class="text-slate-600 hover:text-slate-800 font-medium text-sm">Cancel</button>
                <button @click="addLesson" :disabled="!newLessonTitle" class="bg-blue-600 text-white rounded-md px-4 py-2 font-semibold text-sm hover:bg-blue-700 disabled:opacity-50">Add Lesson</button>
            </div>
        </div>
     </div>
  </div>
</template>
