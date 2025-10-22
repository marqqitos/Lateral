"use client"

import { useState, useEffect } from "react"
import { TaskDashboard } from "@/components/task-dashboard"
import { AddTaskForm } from "@/components/add-task-form"
import { taskApi, Task, CreateTaskRequest, ApiError } from "@/lib/api"

export default function Home() {
  const [tasks, setTasks] = useState<Task[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Load tasks from API on mount
  useEffect(() => {
    loadTasks()
  }, [])

  const loadTasks = async () => {
    try {
      setIsLoading(true)
      setError(null)
      const response = await taskApi.fetchTasks()
      setTasks(response.tasks || [])
    } catch (err) {
      console.error('Failed to load tasks:', err)
      setError(err instanceof Error ? err.message : 'Failed to load tasks')
    } finally {
      setIsLoading(false)
    }
  }

  const addTask = async (title: string, description: string) => {
    try {
      setError(null)
      const taskData: CreateTaskRequest = { title, description: description.trim() || undefined }
      const newTask = await taskApi.createTask(taskData)
      setTasks([newTask, ...tasks])
      return { success: true }
    } catch (err) {
      console.error('Failed to create task:', err)
      const errorMessage = err instanceof Error ? err.message : 'Failed to create task'
      setError(errorMessage)
      return { success: false, error: errorMessage }
    }
  }

  const toggleTaskCompletion = async (id: number) => {
    try {
      setError(null)
      const updatedTask = await taskApi.toggleTask(id)
      setTasks(tasks.map((task) => (task.id === id ? updatedTask : task)))
    } catch (err) {
      console.error('Failed to toggle task:', err)
      setError(err instanceof Error ? err.message : 'Failed to update task')
    }
  }

  const deleteTask = async (id: number) => {
    try {
      setError(null)
      await taskApi.deleteTask(id)
      setTasks(tasks.filter((task) => task.id !== id))
    } catch (err) {
      console.error('Failed to delete task:', err)
      setError(err instanceof Error ? err.message : 'Failed to delete task')
    }
  }

  return (
    <main className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
      <div className="max-w-4xl mx-auto">
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Task Manager</h1>
          <p className="text-gray-600">Organize and track your tasks efficiently</p>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-600 text-sm">
              {error}
              <button
                onClick={loadTasks}
                className="ml-2 underline hover:no-underline"
              >
                Retry
              </button>
            </p>
          </div>
        )}

        {isLoading ? (
          <div className="text-center py-12">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <p className="mt-2 text-gray-600">Loading tasks...</p>
          </div>
        ) : (
          <div className="grid gap-6 md:grid-cols-3">
            <div className="md:col-span-1">
              <AddTaskForm onAddTask={addTask} />
            </div>

            <div className="md:col-span-2">
              <TaskDashboard tasks={tasks} onToggleCompletion={toggleTaskCompletion} onDeleteTask={deleteTask} />
            </div>
          </div>
        )}
      </div>
    </main>
  )
}
