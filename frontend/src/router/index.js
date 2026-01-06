import { createRouter, createWebHistory } from 'vue-router'
import Login from '../views/Login.vue'
import Register from '../views/Register.vue'

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/login',
            name: 'login',
            component: Login,
            meta: { requiresGuest: true }
        },
        {
            path: '/register',
            name: 'register',
            component: Register,
            meta: { requiresGuest: true }
        },
        {
            path: '/',
            name: 'home',
            redirect: '/courses', // Default to courses
        },
        {
            path: '/courses',
            name: 'courses',
            component: () => import('../views/Courses.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/courses/:id',
            name: 'course-detail',
            component: () => import('../views/CourseDetail.vue'),
            meta: { requiresAuth: true }
        }
    ]
})

router.beforeEach((to, from, next) => {
    const token = localStorage.getItem('token')

    if (to.meta.requiresAuth && !token) {
        next('/login')
    } else if (to.meta.requiresGuest && token) {
        next('/courses')
    } else {
        next()
    }
})

export default router
