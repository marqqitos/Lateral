import { taskApi, ApiError, Task, CreateTaskRequest } from '@/lib/api'

// Mock fetch globally
const mockFetch = global.fetch as jest.MockedFunction<typeof fetch>

describe('API Layer', () => {
  beforeEach(() => {
    mockFetch.mockClear()
  })

  describe('ApiError', () => {
    it('should create ApiError with status and message', () => {
      const error = new ApiError(404, 'Not found')
      expect(error.status).toBe(404)
      expect(error.message).toBe('Not found')
      expect(error.name).toBe('ApiError')
    })
  })

  describe('fetchTasks', () => {
    it('should fetch tasks successfully', async () => {
      const mockTasks: Task[] = [
        {
          id: 1,
          title: 'Test Task',
          description: 'Test Description',
          isCompleted: false,
          createdAt: '2023-01-01T00:00:00Z',
          updatedAt: '2023-01-01T00:00:00Z',
        },
      ]

      const mockResponse = {
        ok: true,
        status: 200,
        json: jest.fn().mockResolvedValue({ tasks: mockTasks, totalCount: 1 }),
      } as unknown as Response

      mockFetch.mockResolvedValueOnce(mockResponse)

      const result = await taskApi.fetchTasks()

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5082/api/tasks',
        expect.objectContaining({
          headers: { 'Content-Type': 'application/json' },
        })
      )
      expect(result.tasks).toEqual(mockTasks)
      expect(result.totalCount).toBe(1)
    })

    it('should handle fetch error', async () => {
      const mockResponse = {
        ok: false,
        status: 500,
        json: jest.fn().mockResolvedValue({ message: 'Internal server error' }),
      } as unknown as Response

      mockFetch.mockResolvedValue(mockResponse)

      await expect(taskApi.fetchTasks()).rejects.toThrow(ApiError)

      // Reset and test again with another mock
      mockFetch.mockClear()
      mockFetch.mockResolvedValue(mockResponse)
      await expect(taskApi.fetchTasks()).rejects.toThrow('Internal server error')
    })

    it('should handle network error', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'))

      await expect(taskApi.fetchTasks()).rejects.toThrow('Network error: Network error')
    })
  })

  describe('createTask', () => {
    it('should create task successfully', async () => {
      const newTask: Task = {
        id: 1,
        title: 'New Task',
        description: 'New Description',
        isCompleted: false,
        createdAt: '2023-01-01T00:00:00Z',
        updatedAt: '2023-01-01T00:00:00Z',
      }

      const createRequest: CreateTaskRequest = {
        title: 'New Task',
        description: 'New Description',
      }

      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 201,
        json: async () => newTask,
      } as Response)

      const result = await taskApi.createTask(createRequest)

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5082/api/tasks',
        expect.objectContaining({
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(createRequest),
        })
      )
      expect(result).toEqual(newTask)
    })

    it('should handle validation error', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: async () => ({ message: 'Title is required' }),
      } as Response)

      const createRequest: CreateTaskRequest = { title: '' }

      await expect(taskApi.createTask(createRequest)).rejects.toThrow(ApiError)
    })
  })

  describe('toggleTask', () => {
    it('should toggle task successfully', async () => {
      const updatedTask: Task = {
        id: 1,
        title: 'Test Task',
        description: 'Test Description',
        isCompleted: true,
        createdAt: '2023-01-01T00:00:00Z',
        updatedAt: '2023-01-01T01:00:00Z',
      }

      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => updatedTask,
      } as Response)

      const result = await taskApi.toggleTask(1)

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5082/api/tasks/1/toggle',
        expect.objectContaining({
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
        })
      )
      expect(result).toEqual(updatedTask)
    })

    it('should handle task not found error', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 404,
        json: async () => ({ message: 'Task not found' }),
      } as Response)

      await expect(taskApi.toggleTask(999)).rejects.toThrow('Task not found')
    })
  })

  describe('deleteTask', () => {
    it('should delete task successfully', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      } as Response)

      await taskApi.deleteTask(1)

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5082/api/tasks/1',
        expect.objectContaining({
          method: 'DELETE',
          headers: { 'Content-Type': 'application/json' },
        })
      )
    })

    it('should handle delete error', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 404,
        json: async () => ({ message: 'Task not found' }),
      } as Response)

      await expect(taskApi.deleteTask(999)).rejects.toThrow('Task not found')
    })
  })

  describe('Error handling', () => {
    it('should handle response without JSON error body', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => {
          throw new Error('Invalid JSON')
        },
      } as Response)

      await expect(taskApi.fetchTasks()).rejects.toThrow('HTTP error! status: 500')
    })

    it('should handle 204 No Content response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      } as Response)

      const result = await taskApi.deleteTask(1)
      expect(result).toEqual({})
    })
  })
})
