"use client"

import type { Task } from "@/lib/api"
import { Checkbox } from "@/components/ui/checkbox"
import { Button } from "@/components/ui/button"
import { Trash2 } from "lucide-react"

interface TaskItemProps {
  task: Task
  onToggleCompletion: (id: number) => void
  onDeleteTask: (id: number) => void
}

export function TaskItem({ task, onToggleCompletion, onDeleteTask }: TaskItemProps) {
  return (
    <div
      className={`flex items-start gap-4 p-4 rounded-lg border transition-all ${
        task.isCompleted ? "bg-gray-50 border-gray-200" : "bg-white border-gray-200 hover:border-blue-300"
      }`}
    >
      {/* Checkbox */}
      <div className="flex-shrink-0 mt-1">
        <Checkbox checked={task.isCompleted} onCheckedChange={() => onToggleCompletion(task.id)} className="w-5 h-5" />
      </div>

      {/* Task Content */}
      <div className="flex-1 cursor-pointer" onClick={() => onToggleCompletion(task.id)}>
        <h3
          className={`font-semibold transition-all ${
            task.isCompleted ? "text-gray-400 line-through" : "text-gray-900"
          }`}
        >
          {task.title}
        </h3>
        {task.description && (
          <p className={`text-sm mt-1 transition-all ${task.isCompleted ? "text-gray-300" : "text-gray-600"}`}>
            {task.description}
          </p>
        )}
      </div>

      {/* Delete Button */}
      <Button
        variant="ghost"
        size="sm"
        onClick={() => onDeleteTask(task.id)}
        className="flex-shrink-0 text-gray-400 hover:text-red-600 hover:bg-red-50"
        aria-label={`Delete task: ${task.title}`}
        data-testid={`delete-task-${task.id}`}
      >
        <Trash2 className="w-4 h-4" />
      </Button>
    </div>
  )
}
