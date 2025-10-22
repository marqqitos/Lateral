"use client"

import type { Task } from "@/app/page"
import { TaskItem } from "./task-item"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"

interface TaskDashboardProps {
  tasks: Task[]
  onToggleCompletion: (id: string) => void
  onDeleteTask: (id: string) => void
}

export function TaskDashboard({ tasks, onToggleCompletion, onDeleteTask }: TaskDashboardProps) {
  const completedCount = tasks.filter((t) => t.isCompleted).length
  const totalCount = tasks.length
  const completionPercentage = totalCount === 0 ? 0 : Math.round((completedCount / totalCount) * 100)

  const activeTasks = tasks.filter((t) => !t.isCompleted)
  const completedTasks = tasks.filter((t) => t.isCompleted)

  return (
    <div className="space-y-6">
      {/* Stats Cards */}
      <div className="grid grid-cols-3 gap-4">
        <Card className="bg-white">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-blue-600">{totalCount}</p>
              <p className="text-sm text-gray-600 mt-1">Total Tasks</p>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-white">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-orange-600">{activeTasks.length}</p>
              <p className="text-sm text-gray-600 mt-1">Active</p>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-white">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-green-600">{completionPercentage}%</p>
              <p className="text-sm text-gray-600 mt-1">Completed</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Active Tasks */}
      <Card className="bg-white">
        <CardHeader>
          <CardTitle>Active Tasks</CardTitle>
          <CardDescription>
            {activeTasks.length} task{activeTasks.length !== 1 ? "s" : ""} remaining
          </CardDescription>
        </CardHeader>
        <CardContent>
          {activeTasks.length === 0 ? (
            <p className="text-gray-500 text-center py-8">No active tasks. Great job!</p>
          ) : (
            <div className="space-y-3">
              {activeTasks.map((task) => (
                <TaskItem
                  key={task.id}
                  task={task}
                  onToggleCompletion={onToggleCompletion}
                  onDeleteTask={onDeleteTask}
                />
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Completed Tasks */}
      {completedTasks.length > 0 && (
        <Card className="bg-white">
          <CardHeader>
            <CardTitle>Completed Tasks</CardTitle>
            <CardDescription>
              {completedTasks.length} task{completedTasks.length !== 1 ? "s" : ""} completed
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {completedTasks.map((task) => (
                <TaskItem
                  key={task.id}
                  task={task}
                  onToggleCompletion={onToggleCompletion}
                  onDeleteTask={onDeleteTask}
                />
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
