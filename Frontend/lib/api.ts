const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5082'

export interface Task {
  id: number
  title: string
  description?: string
  isCompleted: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateTaskRequest {
  title: string
  description?: string
}

export interface TaskListResponse {
  tasks: Task[]
  totalCount: number
}

class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message)
    this.name = 'ApiError'
  }
}

async function apiRequest<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`
  
  const config: RequestInit = {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  }

  try {
    const response = await fetch(url, config)
    
    if (!response.ok) {
      let errorMessage = `HTTP error! status: ${response.status}`
      try {
        const errorData = await response.json()
        errorMessage = errorData.message || errorMessage
      } catch {
        // If error response isn't JSON, use default message
      }
      throw new ApiError(response.status, errorMessage)
    }

    // Handle 204 No Content responses (like DELETE)
    if (response.status === 204) {
      return {} as T
    }

    return await response.json()
  } catch (error) {
    if (error instanceof ApiError) {
      throw error
    }
    // Network or other errors
    throw new Error(`Network error: ${error instanceof Error ? error.message : 'Unknown error'}`)
  }
}

export const taskApi = {
  // Get all tasks
  async fetchTasks(): Promise<TaskListResponse> {
    return apiRequest<TaskListResponse>('/api/tasks')
  },

  // Create a new task
  async createTask(data: CreateTaskRequest): Promise<Task> {
    return apiRequest<Task>('/api/tasks', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  // Toggle task completion status
  async toggleTask(id: number): Promise<Task> {
    return apiRequest<Task>(`/api/tasks/${id}/toggle`, {
      method: 'PATCH',
    })
  },

  // Delete a task
  async deleteTask(id: number): Promise<void> {
    return apiRequest<void>(`/api/tasks/${id}`, {
      method: 'DELETE',
    })
  },
}

export { ApiError }
