"use client"

import { useState, useEffect } from "react"
import { TaskDashboard } from "@/components/task-dashboard"
import { AddTaskForm } from "@/components/add-task-form"

export interface Task {
  id: string
  title: string
  description: string
  isCompleted: boolean
}

export default function Home() {
  const [tasks, setTasks] = useState<Task[]>([])
  const [isLoading, setIsLoading] = useState(true)

  // Load tasks from localStorage on mount
  useEffect(() => {
    const savedTasks = localStorage.getItem("tasks")
    if (savedTasks) {
      setTasks(JSON.parse(savedTasks))
    }
    setIsLoading(false)
  }, [])

  // Save tasks to localStorage whenever they change
  useEffect(() => {
    if (!isLoading) {
      localStorage.setItem("tasks", JSON.stringify(tasks))
    }
  }, [tasks, isLoading])

  const addTask = (title: string, description: string) => {
    const newTask: Task = {
      id: Date.now().toString(),
      title,
      description,
      isCompleted: false,
    }
    setTasks([newTask, ...tasks])
  }

  const toggleTaskCompletion = (id: string) => {
    setTasks(tasks.map((task) => (task.id === id ? { ...task, isCompleted: !task.isCompleted } : task)))
  }

  const deleteTask = (id: string) => {
    setTasks(tasks.filter((task) => task.id !== id))
  }

  return (
    <main className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
      <div className="max-w-4xl mx-auto">
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Task Manager</h1>
          <p className="text-gray-600">Organize and track your tasks efficiently</p>
        </div>

        <div className="grid gap-6 md:grid-cols-3">
          <div className="md:col-span-1">
            <AddTaskForm onAddTask={addTask} />
          </div>

          <div className="md:col-span-2">
            <TaskDashboard tasks={tasks} onToggleCompletion={toggleTaskCompletion} onDeleteTask={deleteTask} />
          </div>
        </div>
      </div>
    </main>
  )
}
